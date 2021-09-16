/*
 *  Copyright 2021, K
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
using PsiTech.Psionics;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace PsiTech.Interface {
    public class EnemyAbilitiesWindow : Window {
        
        public override Vector2 InitialSize => new Vector2(450f, 720f);

        private string search = "";
        private Vector2 listScrollAnchor = new Vector2(0f, 0f);

        private const string SearchKey = "PsiTech.Interface.Search";
        private const string ResetKey = "PsiTech.Interface.ResetEnemyAbilities";
        
        private const float XSeparation = 5f;
        private const float YSeparation = 5f;
        private const float DefaultContraction = 5f;
        private const float DefaultHeight = 22f;

        private const float SearchLabelWidth = 150f;

        private const float HediffListHeight = 600f;
        
        private const float EntryBoxWidth = 100f;
        
        public EnemyAbilitiesWindow() {
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

        public override void DoWindowContents(Rect inRect) {
            var drawRect = inRect.ContractedBy(DefaultContraction);
            var xAnchor = drawRect.x;
            var yAnchor = drawRect.y;

            // Search bar
            Widgets.Label(new Rect(xAnchor, yAnchor, SearchLabelWidth, DefaultHeight), SearchKey.Translate());
            xAnchor += SearchLabelWidth;
            search = Widgets.TextField(new Rect(xAnchor, yAnchor, drawRect.width - SearchLabelWidth, DefaultHeight),
                search);
            xAnchor = drawRect.x;
            yAnchor += DefaultHeight + YSeparation;
            
            Widgets.DrawLineHorizontal(inRect.x, yAnchor, inRect.width);
            yAnchor += YSeparation;

            // Entries
            var drawEntries = PsiTechSettings.DisabledEnemyAbilities.Where(entry =>
                    (entry.Key?.label ?? entry.Key?.defName ?? "").ToLower().Contains(search.ToLower()))
                .ToList();
            var needed = (DefaultHeight + YSeparation) * drawEntries.Count;
            var outRect = new Rect(xAnchor, yAnchor, drawRect.width, HediffListHeight);
            var viewRect = new Rect(0f, 0f, drawRect.width - 16f, needed);
            var scrollAnchor = 0f;
            Widgets.BeginScrollView(outRect, ref listScrollAnchor, viewRect);
            foreach (var entry in drawEntries) {
                DrawEntry(entry.Key, new Rect(16f, scrollAnchor, drawRect.width - 37f, DefaultHeight));
                scrollAnchor += DefaultHeight + YSeparation;
            }
            Widgets.EndScrollView();
            
            yAnchor += HediffListHeight + YSeparation;
            Widgets.DrawLineHorizontal(inRect.x, yAnchor, inRect.width);
            yAnchor += YSeparation;
            
            // Reset button
            if (Widgets.ButtonText(new Rect(xAnchor, yAnchor, drawRect.width, drawRect.yMax - yAnchor),
                ResetKey.Translate())) {
                PsiTechSettings.ResetDisabledAbilities();
            }
        }

        private static void DrawEntry(PsiTechAbilityDef def, Rect drawRect) {
            var xAnchor = drawRect.x;
            var yAnchor = drawRect.y;
            var height = drawRect.height;

            // Label
            var width = drawRect.width - EntryBoxWidth;
            if (Widgets.ButtonText(new Rect(xAnchor, yAnchor, width, height), def.LabelCap, false)) {
                Find.WindowStack.Add(new Dialog_InfoCard(def));
            }
            xAnchor += width + XSeparation;

            // Entry
            Text.Anchor = TextAnchor.MiddleRight;
            // Invert since we save if something is disabled, which is rather counterintuitive in the menu
            // Checkbox is - checked = enabled, unchecked = disabled.
            var value = !PsiTechSettings.DisabledEnemyAbilities[def];
            Widgets.Checkbox(new Vector2(xAnchor, yAnchor), ref value);
            PsiTechSettings.DisabledEnemyAbilities[def] = !value;

            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}