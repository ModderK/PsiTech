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
    public class AbilityEffectDamagePart : AbilityEffect {
        
        public float BaseDamage;
        public DamageDef DamageType;
        public BodyPartDef Part;
        
        private const string BaseDamageKey = "PsiTech.AbilityEffects.BaseDamage";
        private const string DamageTypeKey = "PsiTech.AbilityEffects.DamageType";
        private const string TargetPartKey = "PsiTech.AbilityEffects.TargetPart";
        
        public override bool TryDoEffectOnPawn(Pawn user, Pawn target) {
            
            var part = target.health.hediffSet.GetNotMissingParts().First(p => p.def == Part);

            if (part == null) return false;

            var totalDamage = BaseDamage * GetModifier(user, target);
            target.TakeDamage(new DamageInfo(DamageType, totalDamage, 0.0f, -1f, user, part));

            return true;
        }
        
        public override string ExtraListingString() {
            var sb = new StringBuilder();
            sb.AppendLine(BaseDamageKey.Translate(BaseDamage));
            sb.AppendLine(DamageTypeKey.Translate(DamageType.LabelCap));
            sb.AppendLine(TargetPartKey.Translate(Part.LabelCap));
            return sb.ToString();
        }

    }
}