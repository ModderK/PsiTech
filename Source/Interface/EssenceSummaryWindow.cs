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

using System.Globalization;
using System.Linq;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace PsiTech.Interface {
    public class EssenceSummaryWindow : Window {

        private readonly Pawn pawn;
        
        public override Vector2 InitialSize => new Vector2(400f, 500f);
        
        private Vector2 listScrollAnchor = new Vector2(0f, 0f);

        private const string SummaryKey = "PsiTech.Interface.EssenceSummary";
        private const string PenaltySumKey = "PsiTech.Interface.TotalEssencePenalty";

        private const float NumberWidth = 50f;
        private const float DefaultHeight = 22f;
        private const float YSeparation = 5f;
        
        public EssenceSummaryWindow(Pawn selPawn) {
            pawn = selPawn;
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
            if (Event.current.type == EventType.Layout) return;
            
            // Prep boxes and such
            var drawRect = inRect.ContractedBy(10f);
            var xAnchor = drawRect.x;
            var yAnchor = drawRect.y;
            
            // Title bar
            Widgets.Label(new Rect(xAnchor, yAnchor, drawRect.width, DefaultHeight),
                SummaryKey.Translate(pawn.NameFullColored));
            yAnchor += DefaultHeight + YSeparation;
            
            Widgets.DrawLineHorizontal(inRect.x, yAnchor, inRect.width);
            yAnchor += YSeparation;
            
            // Entries
            var drawEntries = pawn.health.hediffSet.GetAllEssencePenalties().ToList();
            var needed = (DefaultHeight + YSeparation) * drawEntries.Count;
            var listHeight = drawRect.height - (DefaultHeight + 2 * YSeparation) * 2;
            var outRect = new Rect(xAnchor, yAnchor, drawRect.width, listHeight);
            var viewRect = new Rect(0f, 0f, drawRect.width - 16f, needed);
            var scrollAnchor = 0f;
            Widgets.BeginScrollView(outRect, ref listScrollAnchor, viewRect);
            foreach (var (hediff, impact) in drawEntries) {
                Widgets.Label(new Rect(16f, scrollAnchor, drawRect.width - NumberWidth - 37f, DefaultHeight), hediff.LabelCap);
                Widgets.Label(new Rect(drawRect.xMax - NumberWidth - 21f, scrollAnchor, NumberWidth, DefaultHeight),
                    impact.ToStringPercent());
                scrollAnchor += DefaultHeight + YSeparation;
            }
            Widgets.EndScrollView();

            yAnchor += listHeight + YSeparation;
            Widgets.DrawLineHorizontal(inRect.x, yAnchor, inRect.width);
            yAnchor += YSeparation;

            Widgets.Label(new Rect(xAnchor + 16f, yAnchor, drawRect.width, DefaultHeight),
                PenaltySumKey.Translate());
            Widgets.Label(new Rect(drawRect.xMax - NumberWidth, yAnchor, NumberWidth, DefaultHeight),
                pawn.health.hediffSet.EssencePenaltyForDisplay().ToStringPercent());

        }


    }
}