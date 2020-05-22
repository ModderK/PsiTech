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

using Verse;

namespace PsiTech.AI {
    public class Stance_PsiWarmup : Stance_Busy {

        private bool targetStartedDowned;
        private bool showPie;

        // For scribe
        public Stance_PsiWarmup() { }
        
        public Stance_PsiWarmup(int ticks, LocalTargetInfo target, bool showPie = false) : base(ticks, target, null) {
            this.showPie = showPie;

            if (focusTarg != null && focusTarg.HasThing && focusTarg.Thing is Pawn pawn) {
                targetStartedDowned = pawn.Downed;
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetStartedDowned, "targetStartDowned");
            Scribe_Values.Look(ref showPie, "showPie");
        }
        
        public override void StanceDraw()
        {
            if (!Find.Selector.IsSelected(stanceTracker.pawn) || !showPie) return;
            
            GenDraw.DrawAimPie(stanceTracker.pawn, focusTarg, (int) (ticksLeft * (double) pieSizeFactor), 0.2f);
        }

        public override void StanceTick()
        {
            if (!targetStartedDowned && focusTarg != null && focusTarg.HasThing && focusTarg.Thing is Pawn pawn &&
                pawn.Downed) {
                stanceTracker.SetStance(new Stance_Mobile());
            } else {
                base.StanceTick();
            }
        }

    }
}