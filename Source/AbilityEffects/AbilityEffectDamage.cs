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
    public class AbilityEffectDamage : AbilityEffect {

        public float BaseDamage;
        public DamageDef DamageType;

        private const string BaseDamageKey = "PsiTech.AbilityEffects.BaseDamage";
        private const string DamageTypeKey = "PsiTech.AbilityEffects.DamageType";
        
        public override bool TryDoEffectOnPawn(Pawn user, Pawn target) {
            
            var damageable = target.health.hediffSet.GetNotMissingParts().Where(part => !part.def.conceptual).ToList();

            var totalDamage = BaseDamage * GetModifier(user, target);
            while (totalDamage > 0) {
                var part = damageable.RandomElementByWeight(x => x.coverageAbs);
                var damageOnPart = totalDamage;
                if (totalDamage > 1f) {
                    damageOnPart = Rand.Range(0, totalDamage);
                }
                target.TakeDamage(new DamageInfo(DamageType, damageOnPart, 0.0f, -1f, user, part));
                totalDamage -= damageOnPart;
            }

            return true;

        }

        public override string ExtraListingString() {
            var sb = new StringBuilder();
            sb.AppendLine(BaseDamageKey.Translate(BaseDamage));
            sb.AppendLine(DamageTypeKey.Translate(DamageType.label));
            return sb.ToString();
        }
    }
}