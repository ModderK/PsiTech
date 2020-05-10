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

using Verse;

namespace PsiTech.Utility {
    public static class PsiTechRenderUtility {
        
        // Fields for drawing targeting circles
        private static Pawn _castingPawn;
        private static float _range;

        public static void PsiTechRenderUtilityUpdate() {
            TryDrawTargetingCircle();
        }
        
        public static void StartTargetingDraw(Pawn pawn, float abilityRange) {
            _castingPawn = pawn;
            _range = abilityRange;
        }

        private static void TryDrawTargetingCircle() {
            if(_castingPawn == null || _range < 0.1f || Find.CurrentMap == null) return;
            
            if (_castingPawn != null && !Find.Targeter.IsTargeting) {
                _castingPawn = null;
                return;
            }

            GenDraw.DrawRadiusRing(_castingPawn.Position, _range);
        }
        
    }
}