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

using PsiTech.Utility;
using RimWorld;
using Verse;
using Verse.AI;

namespace PsiTech.AI {
    // I have no idea why this is a ThinkNode.
    // What is this architecture anyway, Tynan?
    public class JobGiver_TrainInTube : ThinkNode {
        
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams) {
            if (!pawn.PsiTracker().ShouldTrain()) return ThinkResult.NoJob;
            
            var availableTrainer = Current.Game.GetComponent<PsiTechManager>().GetOpenTrainerForPawn(pawn);
            return availableTrainer == null
                ? ThinkResult.NoJob
                : new ThinkResult(JobMaker.MakeJob(JobDefOf.EnterCryptosleepCasket, (LocalTargetInfo) availableTrainer),
                    this, new JobTag?());
        }

    }
}