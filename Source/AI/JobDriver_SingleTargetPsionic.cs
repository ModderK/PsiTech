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
using PsiTech.Psionics;
using RimWorld;
using Verse;
using Verse.AI;

namespace PsiTech.AI {
    public class JobDriver_SingleTargetPsionic : JobDriver {

        private float totalTicksRequired = 99999f;
        private Verb_Psionic Verb => pawn.CurJob.verbToUse as Verb_Psionic;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.B);
            yield return new Toil{
                initAction = delegate {
                    ticksLeftThisToil = Verb.Ability.Def.CastTimeTicks;
                    totalTicksRequired = ticksLeftThisToil;

                    var target = TargetThingB as Pawn;
                    if (!target?.Dead ?? false) {
                        pawn.stances.SetStance(new Stance_PsiWarmup(ticksLeftThisToil, TargetB, true));
                    }
                    else {
                        pawn.pather.StopDead();
                        EndJobWith(JobCondition.Incompletable);
                    }
                },

                tickAction = delegate {

                    if (!TargetA.IsValid) {
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }

                    var target = TargetThingB as Pawn;
                    if (target?.Dead ?? true) {
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }

                    if (!Verb.Ability.CanHitTarget(target)) {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    if (ticksLeftThisToil > 0) return;

                    Verb.DoCast(TargetB.Thing as Pawn);
                    EndJobWith(JobCondition.Succeeded);

                },

                defaultCompleteMode = ToilCompleteMode.Never
            };
        }
        
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref totalTicksRequired, "TotalTicksRequired");
        }
    }
}