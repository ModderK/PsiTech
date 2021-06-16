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

using System;
using System.Collections.Generic;
using PsiTech.Psionics;
using PsiTech.Utility;
using Verse;

namespace PsiTech.Training {
    public class CompPsiTechTrainer : ThingComp {

        public CompPropertiesPsiTechBuilding Properties => props as CompPropertiesPsiTechBuilding;

        private const string TrainingSummaryKey = "PsiTech.Training.TrainingSummary";
        private const string TrainingSummaryAwakeningKey = "PsiTech.Training.TrainingSummaryAwakening";
        private const string RemovingSummaryKey = "PsiTech.Training.RemovingSummary";
        private const string EnergyTrainingKey = "PsiTech.Interface.EnergyTraining";
        private const string FocusTrainingKey = "PsiTech.Interface.FocusTraining";
        
        private BuildingPsiTechTrainer Building => parent as BuildingPsiTechTrainer;
        
        private float timeLeft = -1;
        private TrainingQueueEntry curEntry;
        private const int DayToSeconds = 1000;

        public Pawn InnerPawn => Building.InnerPawn;
        
        public override void CompTick() {
            base.CompTick();

            if (!Building.IsOperating) return; // Have to be working to do anything
            
            // Check if we were in a training cycle but the pawn was ejected
            // This shouldn't happen but doesn't hurt to check
            if (timeLeft > 0 && InnerPawn == null) {
                timeLeft = -1;
            }

            // All other operation needs a pawn
            if (InnerPawn == null) return;
            
            // Check if we need to start a new timer
            if (timeLeft < 0) {
                if (!InnerPawn.PsiTracker().Activated) { // Are we awakening a pawn or training a skill?
                    timeLeft = PsiTechTracker.ActivationTimeSeconds;
                    InnerPawn.PsiTracker().TrainingSuspended = false;
                }
                else if(InnerPawn.PsiTracker().TryBeginNextTrainingEntry(out curEntry)) {
                    timeLeft = curEntry.TrainingTimeSeconds;
                    InnerPawn.PsiTracker().TrainingSuspended = false;
                }
                else {
                    Building.EjectContents();
                }
            }

            // Decrease the time left each second
            if (timeLeft > 0) {
                timeLeft -= PsiTechSettings.Get().TrainingSpeedMultiplier;
            }

            // Check if we've completed a training cycle
            if (timeLeft > 0) return;
            
            // Finish training
            timeLeft = -1;
            InnerPawn.PsiTracker().FinishTraining();
            if (InnerPawn.PsiTracker().TrainingQueue.Any()) return;
            
            Building.EjectContents();
        }

        public override void PostExposeData() {
            base.PostExposeData();
            
            Scribe_Values.Look(ref timeLeft, "timeLeft");
            Scribe_Deep.Look(ref curEntry, "curEntry");
        }

        public void ResetComp(Pawn pawn) {
            timeLeft = -1;
            pawn.PsiTracker().ClearTrainingQueueLock();
            curEntry = new TrainingQueueEntry();
        }

        public override string CompInspectStringExtra() {
            if (InnerPawn == null) return "";
            
            if (curEntry.Type == TrainingType.Ability && curEntry.Def == null) { // We're awakening
                return TrainingSummaryAwakeningKey.Translate(
                    (timeLeft / (PsiTechSettings.Get().TrainingSpeedMultiplier * DayToSeconds))
                    .ToStringDecimalIfSmall());
            }

            if (curEntry.Type == TrainingType.Remove) { // We're removing and want to use the special remove format
                return RemovingSummaryKey.Translate(curEntry.Def.label,
                    (timeLeft / (PsiTechSettings.Get().TrainingSpeedMultiplier * DayToSeconds))
                    .ToStringDecimalIfSmall());
            }

            string label = curEntry.Type switch {
                TrainingType.Ability => curEntry.Def.label,
                TrainingType.Focus => FocusTrainingKey.Translate(),
                TrainingType.Energy => EnergyTrainingKey.Translate(),
                _ => throw new ArgumentOutOfRangeException()
            };

            return TrainingSummaryKey.Translate(label,
                (timeLeft / (PsiTechSettings.Get().TrainingSpeedMultiplier * DayToSeconds)).ToStringDecimalIfSmall());
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            if (!Prefs.DevMode || !PsiTechManager.PsiTechDebug) yield break;
            
            yield return new Command_Action {
                defaultLabel = "DEBUG Finish training",
                defaultDesc = "",
                action = () => timeLeft = 1
            };
        }
    }
}