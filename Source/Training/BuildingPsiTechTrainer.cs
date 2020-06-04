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

using System.Collections.Generic;
using System.Linq;
using PsiTech.Utility;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PsiTech.Training {
    public class BuildingPsiTechTrainer : Building_CryptosleepCasket {
        
        
        private const string NoTrainingQueuedKey = "PsiTech.Training.CannotUseNoTrainingQueued";
        private const string NotOperatingKey = "PsiTech.Training.CannotUseNotOperating";
        private const string EnterPsychicTrainerAwaken = "PsiTech.Training.EnterPsychicTrainerAwaken";
        private const string EnterPsychicTrainerTraining = "PsiTech.Training.EnterPsychicTrainerTraining";

        private CompPowerTrader powerTrader;
        private CompBreakdownable breakdownable;

        public bool IsOperating => (powerTrader?.PowerOn ?? true) && !(breakdownable?.BrokenDown ?? false);
        public Pawn InnerPawn => innerContainer.FirstOrDefault() as Pawn;
        
        
        private const int TickRate = 60;

        public CompPsiTechTrainer Trainer;

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            powerTrader = GetComp<CompPowerTrader>();
            breakdownable = GetComp<CompBreakdownable>();
            Current.Game.GetComponent<PsiTechManager>().RegisterTrainer(this);
            Trainer = GetComp<CompPsiTechTrainer>();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish) {
            base.DeSpawn(mode);
            Current.Game.GetComponent<PsiTechManager>().UnregisterTrainer(this);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn) {
            if (innerContainer.Count != 0) yield break;

            if (myPawn.IsQuestLodger()) {
                yield return new FloatMenuOption(
                    "CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);
            }
            else if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly)) {
                var failer = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield return failer;
            }
            else if (!IsOperating) {
                var failer = new FloatMenuOption(NotOperatingKey.Translate(), null);
                yield return failer;
            }
            else if (myPawn.PsiTracker().Activated && !myPawn.PsiTracker().TrainingQueue.Any()) {
                var failer = new FloatMenuOption(NoTrainingQueuedKey.Translate(), null);
                yield return failer;
            }
            else {
                var jobDef = JobDefOf.EnterCryptosleepCasket;
                var jobStr = myPawn.PsiTracker().TrainingQueue.Any()
                    ? EnterPsychicTrainerTraining.Translate()
                    : EnterPsychicTrainerAwaken.Translate();

                void JobAction() {
                    var job = JobMaker.MakeJob(jobDef, this);
                    myPawn.jobs.TryTakeOrderedJob(job);
                }

                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, JobAction), myPawn, this);
            }
        }

        public void BuildingTick() {
            if ((Find.TickManager.TicksGame + GetHashCode()) % TickRate != 0) return;
            
            Trainer.CompTick();
        }

        public override void EjectContents() {
            var filthSlime = ThingDefOf.Filth_Slime;
            foreach (var thing in innerContainer) {
                if (!(thing is Pawn pawn)) continue;
                
                PawnComponentsUtility.AddComponentsForSpawn(pawn);
                pawn.filth.GainFilth(filthSlime);
                Trainer.ResetComp(pawn);
                if (!pawn.PsiTracker().Activated) continue;

                pawn.PsiTracker().TrainingSuspended = true;
            }

            if (!Destroyed) SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(Position, Map)));
            
            innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
            contentsKnown = true;
        }

    }
}