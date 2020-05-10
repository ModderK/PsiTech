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

using PsiTech.Interface;
using UnityEngine;
using Verse;

namespace PsiTech.AutocastManagement {
    public abstract class AutocastCondition_Boolean : AutocastCondition {

        public override float Height => 92f;

        public override void Draw(Rect inRect, AbilityWindow window) {
            Widgets.DrawBoxSolid(inRect, new Color(21f/256f, 25f/256f, 29f/256f));

            var drawBox = inRect.ContractedBy(5f);

            var xAnchor = drawBox.x;
            var yAnchor = drawBox.y;
            
            // Draw top matter
            DrawTopMatter(drawBox.x, ref yAnchor, drawBox.width);

            // Draw invert option
            DrawInvertOption(ref xAnchor, yAnchor);

            yAnchor += OptionHeight + YSeparation;

            // Draw bottom matter
            Text.Anchor = TextAnchor.MiddleCenter;
            DrawBottomMatter(drawBox.x, yAnchor, drawBox.width, window);
            
            Text.Anchor = TextAnchor.UpperLeft;

            
        }

    }
}