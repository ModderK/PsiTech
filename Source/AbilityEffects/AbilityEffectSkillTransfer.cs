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

using System.Text;
using Verse;

namespace PsiTech.AbilityEffects {
    // This ability effect doesn't actually do anything! It's only for display.
    // The state tracker logic is implemented in the PsiTechAbilityTargetedSkillTransfer class.
    public class AbilityEffectSkillTransfer : AbilityEffect {

        public float BaseSkillTransfer;

        private const string BaseSkillTransferKey = "PsiTech.AbilityEffects.BaseSkilTransfer";
        
        public override bool TryDoEffectOnPawn(Pawn user, Pawn target) {
            Log.Error("PsiTech tried to use the ability transfer effect. This should never happen.");
            return false;
        }
        
        public override string ExtraListingString() {
            var sb = new StringBuilder();
            sb.AppendLine(BaseSkillTransferKey.Translate(BaseSkillTransfer.ToStringPercent()));
            return sb.ToString();
        }
    }
}