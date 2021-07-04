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
using RimWorld;
using Verse;

namespace PsiTech.Psionics {
    public class PsiTechAbilityUnscaledPassive : PsiTechAbility {
        
        public override float GetOffsetOfStat(StatDef stat) {
            return Def.StatOffsets.Find(mod => mod.stat == stat)?.value ?? 0;
        }

        public override float GetFactorOfStat(StatDef stat) {
            return Def.StatFactors.Find(mod => mod.stat == stat)?.value ?? 1;
        }

        public override float GetOffsetOfCapacity(PawnCapacityDef cap) {
            return Def.CapMods.Find(capMod => capMod.capacity == cap)?.offset ?? 0;
        }

        public override float GetFactorOfCapacity(PawnCapacityDef cap) {
            return Def.CapMods.Find(capMod => capMod.capacity == cap)?.postFactor ?? 1;
        }

        public override IEnumerable<(StatDef stat, float offset)> GetAllStatOffsets() {
            foreach (var offset in Def.StatOffsets) {
                yield return (offset.stat, offset.value);
            }
        }
        
        public override IEnumerable<(StatDef stat, float factor)> GetAllStatFactors() {
            foreach (var factor in Def.StatFactors) {
                yield return (factor.stat, factor.value);
            }
        }

        public override IEnumerable<(PawnCapacityDef capacity, float offset, float factor)> GetAllCapacityMods() {
            foreach (var mod in Def.CapMods) {
                yield return (mod.capacity, mod.offset, mod.postFactor);
            }
        }
    }
}