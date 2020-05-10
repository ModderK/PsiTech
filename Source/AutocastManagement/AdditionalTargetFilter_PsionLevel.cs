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

using PsiTech.Utility;
using Verse;

namespace PsiTech.AutocastManagement {
    public class AdditionalTargetFilter_PsionLevel : AdditionalTargetFilter_ThresholdInt {
        
        protected override string ThresholdKey => "PsiTech.AutocastManagement.PsionLevel";
        protected override float ThresholdLabelWidth => 140f;

        protected override int MinValue => 2;
        protected override int MaxValue => 6;

        public AdditionalTargetFilter_PsionLevel() {
            Threshold = 2;
        }

        public override bool TargetMeetsFilter(Pawn target) {
            return (target.PsiTracker().TotalLevel >= Threshold) ^ Inverted;
        }
        
    }
}