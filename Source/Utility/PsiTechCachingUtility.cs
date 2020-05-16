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

        public static readonly HashSet<StatDef> CachedAffectedStats = new HashSet<StatDef>();
        public static readonly List<ThingDef> CachedCryptosleepDefs = new List<ThingDef>();
        
        static PsiTechCachingUtility() {
            
            // Cache stats abilites effects
            var abilities = DefDatabase<PsiTechAbilityDef>.AllDefsListForReading;

            foreach (var ability in abilities) {
                foreach (var stat in ability.StatOffsets) {
                    if (CachedAffectedStats.Contains(stat.stat)) continue;
                    
                    CachedAffectedStats.Add(stat.stat);
                }
                
                foreach (var stat in ability.StatFactors) {
                    if (CachedAffectedStats.Contains(stat.stat)) continue;
                    
                    CachedAffectedStats.Add(stat.stat);
                }
            }
            
            // Cache psychic sensitivity for the suppression field just in case
            if (!CachedAffectedStats.Contains(StatDefOf.PsychicSensitivity)) {
                CachedAffectedStats.Add(StatDefOf.PsychicSensitivity);
            }
            
            // Cache stats that can be affected on weapons
            var equipmentEffects = DefDatabase<EquipmentEnhancementDef>.AllDefsListForReading;

            // This should only ever run once
            foreach (var effects in equipmentEffects) {
                CachedAffectedStats.AddRange(effects.RangedMods.Select(mod => mod.stat));
                CachedAffectedStats.AddRange(effects.MeleeMods.Select(mod => mod.stat));
            }
            
            // Secret optimization/fix - build a custom cache of possible cryptosleep casket types
            var things = DefDatabase<ThingDef>.AllDefsListForReading;
            foreach (var thing in things) {
                if (!typeof(Building_CryptosleepCasket).IsAssignableFrom(thing.thingClass) ||
                    typeof(BuildingPsiTechTrainer).IsAssignableFrom(thing.thingClass)) continue;
                
                CachedCryptosleepDefs.Add(thing);
            }
            
        }
        
    }
}