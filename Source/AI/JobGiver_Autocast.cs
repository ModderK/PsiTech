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
using Verse.AI;

namespace PsiTech.AI {
    public class JobGiver_Autocast : ThinkNode_JobGiver {
        public override float GetPriority(Pawn pawn) {

            if (!pawn.PsiTracker().Activated || !pawn.PsiTracker().AutocastEnabled) return -100f;

            return 100f;

        }

        protected override Job TryGiveJob(Pawn pawn) {
            if (pawn.IsColonist && pawn.Drafted) return null;
            
            return pawn.PsiTracker().GetAutocastJob();
        }
        
    }
}