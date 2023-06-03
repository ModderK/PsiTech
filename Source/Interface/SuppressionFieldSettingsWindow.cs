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

using PsiTech.SuppressionField;
using RimWorld;
using UnityEngine;
using Verse;

namespace PsiTech.Interface {
    public class SuppressionFieldSettingsWindow : Window {

        private readonly CompPsychicSuppressionField comp;
        
        public override Vector2 InitialSize => new Vector2(350f, 200f);
        
        private const string TargetEffectKey = "PsiTech.Interface.TargetEffect";
        private const string TargetRadiusKey = "PsiTech.Interface.TargetRadius";
        private const string PowerConsumptionKey = "PsiTech.Interface.PowerConsumption";
        
        private const float YSeparation = 5f;
        
        public SuppressionFieldSettingsWindow(CompPsychicSuppressionField selComp) {
            comp = selComp;
            comp.InConfigurationWindow = true;
            Setup();
        }
        
        private void Setup() {
            forcePause = true;
            doCloseButton = false;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            soundAppear = SoundDefOf.InfoCard_Open;
            soundClose = SoundDefOf.InfoCard_Close;
        }

        public override void PreClose() {
            comp.RadiusCached = false;
            comp.InConfigurationWindow = false;
            comp.UpdateField();
            base.PreClose();
        }

        public override void DoWindowContents(Rect inRect) {
            if (Event.current.type == EventType.Layout) return;
            
            // Prep boxes and such
            var drawBox = inRect.ContractedBy(10f);
            var xAnchor = drawBox.x;
            var yAnchor = drawBox.y;
            
            // Draw current sensitivity label
            Widgets.Label(new Rect(xAnchor, yAnchor, drawBox.width, 22f),
                TargetEffectKey.Translate(comp.TargetEffect.ToStringPercent()));
            yAnchor += 22f + YSeparation;
            
            // Draw effect slider. All these various negative signs and other strangeness are to make the slider value
            // positive, since apparently having a negative slider value broke in 1.4.
            var effectRect = new Rect(xAnchor, yAnchor, drawBox.width, 22f);
            var tempTargetEffect = Widgets.HorizontalSlider(effectRect, -comp.TargetEffect, comp.Props.MaxEffect,
                -comp.Props.MinEffect, false, null, null, null, comp.Props.EffectStep);
            comp.TargetEffect = -tempTargetEffect;
            yAnchor += 22f + 2*YSeparation;
            
            // Draw current radius label
            Widgets.Label(new Rect(xAnchor, yAnchor, drawBox.width, 22f),
                TargetRadiusKey.Translate(comp.TargetRadius));
            yAnchor += 22f + YSeparation;
            
            // Draw radius slider
            var radiusRect = new Rect(xAnchor, yAnchor, drawBox.width, 22f);
            comp.TargetRadius = Widgets.HorizontalSlider(radiusRect, comp.TargetRadius, comp.Props.MinRadius,
                comp.Props.MaxRadius, false, null, null, null, 0.5f);
            yAnchor += 22f + 2*YSeparation;

            // Draw power consumption
            Widgets.Label(new Rect(xAnchor, yAnchor, drawBox.width, 22f),
                PowerConsumptionKey.Translate(comp.PredictedPowerConsumption()));
        }
        
    }
}