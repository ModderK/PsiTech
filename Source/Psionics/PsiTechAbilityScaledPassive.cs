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

using RimWorld;
using Verse;

namespace PsiTech.Psionics {
    public class PsiTechAbilityScaledPassive : PsiTechAbility {
        
        public override float GetOffsetOfStat(StatDef stat) {
            var offset = Def.StatOffsets?.Find(mod => mod.stat == stat)?.value ?? 0;
            
            return stat == StatDefOf.PsychicSensitivity ? offset : offset * Tracker.GetTotalModifierPassive();
        }

        public override float GetFactorOfStat(StatDef stat) {
            var factor = Def.StatFactors?.Find(mod => mod.stat == stat)?.value ?? 1;
            
            return stat == StatDefOf.PsychicSensitivity ? factor : 1f + (factor - 1f) * Tracker.GetTotalModifierPassive();
        }
        
        public override float GetOffsetOfCapacity(PawnCapacityDef cap) {
            return (Def.CapMods.Find(capMod => capMod.capacity == cap)?.offset ?? 0) * Tracker.GetTotalModifierPassive();
        }

        public override float GetFactorOfCapacity(PawnCapacityDef cap) {
            return 1f + ((Def.CapMods.Find(capMod => capMod.capacity == cap)?.postFactor ?? 1) - 1f) *
                   Tracker.GetTotalModifierPassive();
        }
    }
}