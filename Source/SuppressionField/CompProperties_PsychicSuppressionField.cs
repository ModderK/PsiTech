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
    public class CompProperties_PsychicSuppressionField : CompProperties {
        
        public float MinEffect = -2f;
        public float MaxEffect = 0f;
        public float EffectStep = 0.1f;
        public float EffectChangeSpeedPerSecond = 0.01f;

        public float MinRadius = 1f;
        public float MaxRadius = 5f;
        public float RadiusChangeSpeedPerSecond = 0.02f;

        public float BasePowerConsumption = 50f;
        public float PowerPerCellEffect = 2f;

        public CompProperties_PsychicSuppressionField() {
            compClass = typeof (CompPsychicSuppressionField);
        }
        
    }
}