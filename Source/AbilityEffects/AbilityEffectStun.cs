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

using System.Text;
using Verse;

namespace PsiTech.AbilityEffects {
    public class AbilityEffectStun : AbilityEffect {

        public float BaseStunSeconds;

        private const string BaseStunKey = "PsiTech.AbilityEffects.BaseStun";
        
        public override bool TryDoEffectOnPawn(Pawn user, Pawn target) {

            var stunDuration = (int) (BaseStunSeconds * 60f * GetModifier(user, target));
            target.stances.stunner.StunFor(stunDuration, user);

            return true;

        }

        public override string ExtraListingString() {
            var sb = new StringBuilder();
            sb.AppendLine(BaseStunKey.Translate(BaseStunSeconds));
            return sb.ToString();
        }
    }
}