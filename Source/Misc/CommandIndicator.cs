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

using PsiTech.Psionics;
using Verse;

namespace PsiTech.Misc {
    public class CommandIndicator : Command {
        
        public PsiTechAbility Ability;
        public Pawn Caster;
        
        public override void GizmoUpdateOnMouseover() {
            if (Ability == null || Ability.Def.Range < 0.1f || Find.CurrentMap == null) return;
            
            GenDraw.DrawRadiusRing(Caster.Position, Ability.Def.Range);
        }
        
    }
}