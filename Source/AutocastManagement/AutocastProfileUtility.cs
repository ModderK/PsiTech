/*
 *  Copyright 2019, 2020, K
 * 
 *  This file is part of PsiTech.
 *
 *  PsiTech is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  PsiTech is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with PsiTech. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using PsiTech.Psionics;
using Verse;

namespace PsiTech.AutocastManagement {
    
    [StaticConstructorOnStartup]
    public static class AutocastProfileUtility {

        private static Dictionary<PsiTechAbilityDef, List<AutocastProfileDef>> profileCache =
            new Dictionary<PsiTechAbilityDef, List<AutocastProfileDef>>();

        static AutocastProfileUtility() {

            // Cache profiles
            foreach (var profile in DefDatabase<AutocastProfileDef>.AllDefsListForReading) {
                if (profileCache.TryGetValue(profile.Ability, out var existing)) {
                    existing.Add(profile);
                    continue;
                }
                
                var newCache = new List<AutocastProfileDef>{profile};
                profileCache.Add(profile.Ability, newCache);
            }
        }

        public static void SelectAndConfigureAutocastProfile(ref PsiTechAbility ability) {
            if (!profileCache.TryGetValue(ability.Def, out var profiles)) {
                Log.Warning("PsiTech tried to retrieve an autocast profile for " + ability.Def.defName + ", which doesn't have any configured.");
                return;
            }
            
            var profile = profiles.RandomElement();
            
            ability.AutocastFilter.User = ability.User;
            ability.AutocastFilter.Ability = ability;
            ability.AutocastFilter.FilterTargetType = profile.TargetType;
            ability.AutocastFilter.TargetRange = profile.TargetRange;
            switch (ability.AutocastFilter) {
                case AutocastFilter_SingleTarget single: {
                    single.MinSuccessChance = profile.MinSuccessChance;

                    if (!(Activator.CreateInstance(profile.Selector.SelectorClass) is AutocastFilterSelector instance)) {
                        Log.Error("PsiTech tried to instantiate an AutocastFilterSelector of type " +
                                    profile.Selector.SelectorClass +
                                    " and failed. This indicates a misconfigured filter selector def or profile.");
                    }
                    else {
                        instance.Def = profile.Selector;
                        single.Selector = instance;
                        single.InvertSelector = profile.InvertSelector;
                    }

                    foreach (var filterStruct in profile.AdditionalFilterProfiles) {
                        var filter = Activator.CreateInstance(filterStruct.Def.FilterClass) as AdditionalTargetFilter;

                        if (filter == null) {
                            Log.Error("PsiTech tried to instantiate an AdditionalTargetFilter of type " +
                                      profile.Selector.SelectorClass +
                                      " and failed. This indicates a misconfigured additional filter def or profile.");
                            continue;
                        }

                        filter.Ability = ability;
                        filter.Def = filterStruct.Def;
                        filter.User = ability.User;
                    
                        AdditionalTargetFilter final;
                        if (filter is AdditionalTargetFilter_Boolean boolean) {
                            boolean.Inverted = filterStruct.Invert;
                            final = boolean;
                        }else if (filter is AdditionalTargetFilter_ThresholdInt integer) {
                            integer.Inverted = filterStruct.Invert;
                            integer.Threshold = (int)filterStruct.Threshold;
                            final = integer;
                        }else if (filter is AdditionalTargetFilter_ThresholdPercent percent) {
                            percent.Inverted = filterStruct.Invert;
                            percent.Threshold = filterStruct.Threshold;
                            final = percent;
                        }
                        else {
                            Log.Error("PsiTech tried to create an additional filter for profile " + profile.defName + " but the type was unrecognized.");
                            continue;
                        }
                    
                        single.AddAdditionalFilter(final);
                    }

                    break;
                }
                case AutocastFilter_Burst burst:
                    burst.MinTargetsInRange = profile.MinTargetsInRange;
                    break;
                default:
                    Log.Error("PsiTech tried to implement an autocast profile for " + ability.Def.defName + ", which doesn't have an AutocastFilterClass.");
                    break;
            }
        }
        
    }
}