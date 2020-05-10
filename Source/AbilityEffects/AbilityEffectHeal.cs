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
 *  Foobar is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with PsiTech. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System.Linq;
using System.Text;
using Verse;

namespace PsiTech.AbilityEffects {
    public class AbilityEffectHeal : AbilityEffect {

        public float BaseHeal;

        private const string BaseHealKey = "PsiTech.AbilityEffects.BaseHeal";
        
        public override bool TryDoEffectOnPawn(Pawn user, Pawn target) {

            var damageable = target.health.hediffSet.hediffs
                .Where(hediff => hediff is Hediff_Injury injury && !injury.IsPermanent()).ToList();

            var totalHeal = BaseHeal * GetModifier(user, target);
            while (totalHeal > 0 && damageable.Any()) {
                var injury = damageable.RandomElementByWeight(x => x.Part.coverageAbs);
                var healOnInjury = injury.Severity;
                if (healOnInjury > totalHeal) {
                    healOnInjury = totalHeal;
                }
                injury.Heal(healOnInjury);
                damageable.Remove(injury);
                totalHeal -= healOnInjury;
            }

            return true;

        }
        
        public override string ExtraListingString() {
            var sb = new StringBuilder();
            sb.AppendLine(BaseHealKey.Translate(BaseHeal));
            return sb.ToString();
        }
        
    }
}