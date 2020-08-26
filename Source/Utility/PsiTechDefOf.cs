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

using PsiTech.Psionics;
using RimWorld;
using Verse;

namespace PsiTech.Utility {
    
    [DefOf]
    public class PsiTechDefOf {
        
        // Stat categories
        public static StatCategoryDef PTAbilityStats;
        public static StatCategoryDef PTEffects;
        public static StatCategoryDef PTEffectsOnUser;
        public static StatCategoryDef PTStatOffsets;
        public static StatCategoryDef PTStatFactors;
        public static StatCategoryDef PTCapMods;
        
        // Stats
        public static StatDef PTPsiEnergyRegeneration;
        public static StatDef PTMaxPsiEnergy;
        public static StatDef PTPsiProjectionAbility;
        public static StatDef PTPsiDefence;
        public static StatDef PTPsiWeaponSynchronicity;
        public static StatDef PTPsiCooldownMultiplier;
        public static StatDef PTDamageDodgeChance;
        public static StatDef PTDamageMultiplier;
        
        // Jobs
        public static JobDef PTBurstPsionic;
        public static JobDef PTSingleTargetPsionic;
        
        // Recipes
        public static RecipeDef PTUpgradeWeaponPsychic;
        public static RecipeDef PTUpgradeApparelPsychic;
        public static RecipeDef PTWorkbenchCreateAthenium;
        
        // Abilities
        public static PsiTechAbilityDef PTPsiForging;
        public static PsiTechAbilityDef PTPsiRally;
        public static PsiTechAbilityDef PTPsiStorm;
        public static PsiTechAbilityDef PTPerfectedCapacitance;
        public static PsiTechAbilityDef PTPerfectedSynchronicity;
        
        // Hediffs
        public static HediffDef PTMeleeMastery;
        
        // Factions
        public static FactionDef PTPsionic;
        
        // Research projects
        public static ResearchProjectDef PTProjectionTheory;

        static PsiTechDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(PsiTechDefOf));
        }

    }
}