using System.Linq;
using UnityEngine;
using Verse;

namespace PsiTech.Utility {
    public static class EssenceCalculationUtility {

        public static float CalculateEssencePenalty(this HediffSet set) {
            var sum = set.hediffs.Sum(hediff => PsiTechSettings.EssenceLossesPerPart[hediff.def]) *
                      PsiTechSettings.Get().EssenceLossMultiplier;
            return Mathf.Min(sum, 1f);
        }
        
    }
}