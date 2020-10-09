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
using UnityEngine;
using Verse;

namespace PsiTech.Utility {
    public static class EssenceCalculationUtility {

        public static float CalculateEssencePenalty(this HediffSet set) {
            var sum = set.hediffs.Sum(hediff => PsiTechSettings.GetPenaltyForPart(hediff.def)) *
                      PsiTechSettings.Get().EssenceLossMultiplier;
            return Mathf.Min(sum, 1f);
        }

        public static IEnumerable<(HediffDef hediff, float impact)> GetAllEssencePenalties(this HediffSet set) {
            foreach (var hediff in set.hediffs) {
                var impact = -PsiTechSettings.GetPenaltyForPart(hediff.def) *
                              PsiTechSettings.Get().EssenceLossMultiplier;
                
                if(impact == 0f) continue;
                
                yield return (hediff.def, impact);
            }
        }

        public static float EssencePenaltyForDisplay(this HediffSet set) {
            return -set.hediffs.Sum(hediff => PsiTechSettings.GetPenaltyForPart(hediff.def)) *
                      PsiTechSettings.Get().EssenceLossMultiplier;
        }
        
    }
}