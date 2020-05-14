/*
 *  Copyright 2020, K
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

namespace PsiTech.SuppressionField {
    public class Building_PsychicSuppressionField : Building {

        private CompPsychicSuppressionField comp => this.TryGetComp<CompPsychicSuppressionField>();

        public override void DrawExtraSelectionOverlays() {
            base.DrawExtraSelectionOverlays();
            if (Map == null) return;
            
            GenDraw.DrawRadiusRing(Position, comp.InConfigurationWindow ? comp.TargetRadius : comp.CurrentRadius);
        }
    }
}