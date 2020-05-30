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

using System.Collections.Generic;
using System.Linq;
using PsiTech.Utility;
using UnityEngine;
using Verse;

namespace PsiTech.AutocastManagement {
    public class AutocastFilter_Burst : AutocastFilter {
        
        public int MinTargetsInRange;
        private string minTargetsBuffer;
        
        private const string MinimumTargetsInRangeKey = "PsiTech.AutocastManagement.MinimumTargetsInRange";
        private const string CountedTargetRangeKey = "PsiTech.AutocastManagement.CountedTargetRange";
        private const string FilterCountedTargetType = "PsiTech.AutocastManagement.FilterCountedTargetType";
        
        private const float MinimumTargetsLabelWidth = 180f;
        private const float MinimumTargetsFillableWidth = 200f;
        
        private const float TargetRangeLabelWidth = 220f;
        private const float TargetRangeFillableWidth = 100f;
        
        private const float TargetTypeLabelWidth = 140f;
        private const float TargetTypeDropdownWidth = 120f;
        
        public override Pawn GetBestTarget(IEnumerable<Pawn> targets) {
            return GetValidTargetsCount(targets) >= MinTargetsInRange ? new Pawn() : null;
        }

        private int GetValidTargetsCount(IEnumerable<Pawn> targets) {
            return targets.Count(target =>
                TargetRange.Contains(target.Position.DistanceTo(User.Position)) && TargetMatchesTargetType(target) &&
                AdditionalFilters.All(filter => filter.TargetMeetsFilter(target)));
        }
        
        public override void Draw(Rect inRect) {
            
            ResolveRemovedFilters();
            
            Widgets.DrawBoxSolid(new Rect(inRect.x, inRect.y, inRect.width, 87f), new Color(21f/256f, 25f/256f, 29f/256f));

            var drawBox = inRect.ContractedBy(5f);
            
            var xAnchor = drawBox.x;
            var yAnchor = drawBox.y;
            
            // Draw minimum targets in range
            Widgets.Label(new Rect(xAnchor, yAnchor, MinimumTargetsLabelWidth, OptionHeight), MinimumTargetsInRangeKey.Translate());
            xAnchor += MinimumTargetsLabelWidth + XSeparation;
            Widgets.IntEntry(new Rect(xAnchor, yAnchor, MinimumTargetsFillableWidth, OptionHeight), ref MinTargetsInRange, ref minTargetsBuffer);

            xAnchor = drawBox.x;
            yAnchor += OptionHeight + YSeparation;
            
            // Draw target range
            Widgets.Label(new Rect(xAnchor, yAnchor, TargetRangeLabelWidth, OptionHeight), CountedTargetRangeKey.Translate());
            xAnchor += TargetRangeLabelWidth + XSeparation;
            Widgets.IntRange(new Rect(xAnchor, yAnchor, TargetRangeFillableWidth, OptionHeight), GetHashCode(), ref TargetRange, 0,
                Mathf.CeilToInt(Ability.Def.Range));

            xAnchor = drawBox.x;
            yAnchor += OptionHeight + YSeparation;
            
            // Draw target type dropdown
            Widgets.Label(new Rect(xAnchor, yAnchor, TargetTypeLabelWidth, OptionHeight), FilterCountedTargetType.Translate());
            xAnchor += TargetTypeLabelWidth + XSeparation;
            Widgets.Dropdown(new Rect(xAnchor, yAnchor, TargetTypeDropdownWidth, OptionHeight), FilterTargetType,
                type => type.ToString(), priority => GenerateTargetTypeOptions(),
                LocalizeTargetType(FilterTargetType));
            
            // Draw additional filters
            xAnchor = drawBox.x;
            yAnchor += OptionHeight + 2 * YSeparation;
            DrawAdditionalFilters(drawBox, xAnchor, yAnchor);

        }

        protected override void PostExpose() {
            Scribe_Values.Look(ref MinTargetsInRange, "MinTargetsInRange");
        }
    }
}