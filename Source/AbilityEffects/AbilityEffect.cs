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

using PsiTech.Utility;
using Verse;

namespace PsiTech.AbilityEffects {
    public abstract class AbilityEffect {

        public string Title;
        public string Description;
        public float Weight;

        public bool IsPassive = false;
        
        public abstract bool TryDoEffectOnPawn(Pawn user, Pawn target);

        public virtual float GetModifier(Pawn user, Pawn target) {
            return IsPassive ? user.PsiTracker().GetTotalModifierPassive() : user.PsiTracker().GetTotalModifierActive();
        }
        
        public virtual string ExtraListingString() {
            return "";
        }

    }
}