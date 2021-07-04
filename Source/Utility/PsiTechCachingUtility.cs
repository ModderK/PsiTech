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

using System.Collections.Generic;
using System.Linq;
using PsiTech.Misc;
using PsiTech.Psionics;
using PsiTech.Training;
using RimWorld;
using Verse;

namespace PsiTech.Utility {
    [StaticConstructorOnStartup]
    public static class PsiTechCachingUtility {

        private static bool[] _cachedAffectedStats;
        public static readonly List<ThingDef> CachedCryptosleepDefs = new List<ThingDef>();
        
        static PsiTechCachingUtility() {
            
            // Initialize cache array
            _cachedAffectedStats = new bool[DefDatabase<StatDef>.DefCount];
            
            // Cache stats abilities effects
            var abilities = DefDatabase<PsiTechAbilityDef>.AllDefsListForReading;

            foreach (var ability in abilities) {
                foreach (var stat in ability.StatOffsets) {
                    _cachedAffectedStats[stat.stat.index] = true;
                }
                
                foreach (var stat in ability.StatFactors) {
                    _cachedAffectedStats[stat.stat.index] = true;
                }
            }
            
            // Cache psychic sensitivity for the suppression field just in case
            _cachedAffectedStats[StatDefOf.PsychicSensitivity.index] = true;
            
            // Cache stats that can be affected on weapons
            var equipmentEffects = DefDatabase<EquipmentEnhancementDef>.AllDefsListForReading;

            // This should only ever run once
            foreach (var effects in equipmentEffects) {
                foreach (var stat in effects.RangedMods) {
                    _cachedAffectedStats[stat.stat.index] = true;
                }
                foreach (var stat in effects.MeleeMods) {
                    _cachedAffectedStats[stat.stat.index] = true;
                }
                foreach (var stat in effects.ShellMods) {
                    _cachedAffectedStats[stat.stat.index] = true;
                }
                foreach (var stat in effects.OverheadMods) {
                    _cachedAffectedStats[stat.stat.index] = true;
                }
            }
            
            // Secret optimization/fix - build a custom cache of possible cryptosleep casket types
            var things = DefDatabase<ThingDef>.AllDefsListForReading;
            foreach (var thing in things) {
                if (!typeof(Building_CryptosleepCasket).IsAssignableFrom(thing.thingClass) ||
                    typeof(BuildingPsiTechTrainer).IsAssignableFrom(thing.thingClass)) continue;
                
                CachedCryptosleepDefs.Add(thing);
            }
            
            // Fix the enhancement ThingFilter
            var enhancementRecipe = DefDatabase<RecipeDef>.GetNamed("PTUpgradeApparelPsychic");
            var offsetStat = DefDatabase<StatDef>.GetNamed("PsychicSensitivityOffset", false); // Why does this exist
            foreach (var thing in DefDatabase<ThingDef>.AllDefsListForReading) {
                if (!thing.IsApparel ||
                    thing.apparel.layers.Any(layer =>
                        layer == ApparelLayerDefOf.Overhead || layer == ApparelLayerDefOf.Shell) &&
                    (thing.equippedStatOffsets?.All(mod =>
                         mod.stat != StatDefOf.PsychicSensitivity && mod.stat != offsetStat ||
                         0.05f <= mod.value && mod.value <= 0.05f) ??
                     true)) continue;
                // We also allow clothing with 5% or less impact on psychic sensitivity, primarily so that 
                // prestige Royalty armor variants can be enhanced.

                enhancementRecipe.fixedIngredientFilter.SetAllow(thing, false);
            }
        }

        public static bool EverAffectsStat(StatDef stat) {
            return _cachedAffectedStats[stat.index];
        }
    }
}