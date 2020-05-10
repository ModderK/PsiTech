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
using UnityEngine;
using Verse;

namespace PsiTech.AutocastManagement {
    public abstract class AdditionalTargetFilter : IExposable {
        
        public Pawn User;
        public PsiTechAbility Ability;
        public AdditionalTargetFilterDef Def;
        public bool Inverted;

        private const string InvertKey = "PsiTech.AutocastManagement.Invert";
        private const string RemoveKey = "PsiTech.AutocastManagement.Remove";

        private const float TitleHeight = 25f;

        private const float RemoveButtonHeight = 25f;
        private const float RemoveButtonWidth = 80f;
        
        protected const float OptionHeight = 22f;
        private const float InvertWidth = 40f;
        private const float InvertCheckSize = 20f;
        
        protected const float YSeparation = 5f;
        protected const float XSeparation = 5f;
        
        public abstract bool TargetMeetsFilter(Pawn target);

        public virtual float Height => 100f;
        public abstract void Draw(Rect inRec, AutocastFilter_SingleTarget filter);

        protected void DrawTopMatter(float xAnchor, ref float yAnchor, float width) {
            
            // Draw title
            Text.Anchor = TextAnchor.MiddleLeft;
            var titleWidth = (width - XSeparation) / 2;
            Widgets.Label(new Rect(xAnchor, yAnchor, titleWidth, TitleHeight), Def.LabelCap);

            yAnchor += TitleHeight + YSeparation;
        }

        protected void DrawInvertOption(ref float xAnchor, float yAnchor) {
            Widgets.Label(new Rect(xAnchor, yAnchor, InvertWidth, OptionHeight), InvertKey.Translate());
            xAnchor += InvertWidth + XSeparation;
            Widgets.Checkbox(xAnchor, yAnchor + (OptionHeight - InvertCheckSize) / 2, ref Inverted, InvertCheckSize);

            xAnchor += InvertCheckSize + XSeparation;
        }
        
        protected void DrawBottomMatter(float xAnchor, float yAnchor, float width, AutocastFilter_SingleTarget filter) {

            // Draw info button
            Widgets.InfoCardButton(xAnchor, yAnchor, Def);
            
            // Draw remove button
            var buttonRect = new Rect(xAnchor + width - RemoveButtonWidth, yAnchor, RemoveButtonWidth, RemoveButtonHeight);
            if (Widgets.ButtonText(buttonRect, RemoveKey.Translate())) {
                filter.MarkAdditionalFilterForRemoval(this);
            }
        }

        protected virtual void PostExpose(){}

        public void ExposeData() {
            Scribe_References.Look(ref User, "User");
            Scribe_References.Look(ref Ability, "Ability");
            Scribe_Defs.Look(ref Def, "Def");
            Scribe_Values.Look(ref Inverted, "Inverted");
            PostExpose();
        }
        
    }
}