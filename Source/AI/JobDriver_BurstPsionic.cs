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

using System.Collections.Generic;
using PsiTech.Psionics;
using Verse;
using Verse.AI;

namespace PsiTech.AI {
    public class JobDriver_BurstPsionic : JobDriver {

        private float totalTicksRequired = 99999f;
        private const float DamageInterruptMin = 1f;
        private bool jobInterrupted;
        private Verb_Psionic Verb => pawn.CurJob.verbToUse as Verb_Psionic;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            yield return new Toil{
                initAction = delegate {
                    ticksLeftThisToil = Verb.Ability.Def.CastTimeTicks;
                    totalTicksRequired = ticksLeftThisToil;
                    pawn.stances.SetStance(new Stance_PsiWarmup(ticksLeftThisToil, pawn.Position + IntVec3.South * 2,
                        true));
                },

                tickAction = delegate {

                    if (jobInterrupted) {
                        EndJobWith(JobCondition.InterruptForced);
                    }

                    if (ticksLeftThisToil > 0) return;

                    Verb.DoCast(null);
                    EndJobWith(JobCondition.Succeeded);
                },

                defaultCompleteMode = ToilCompleteMode.Never
            };
        }

        public override void Notify_DamageTaken(DamageInfo dinfo) {
            base.Notify_DamageTaken(dinfo);

            if (dinfo.Amount < DamageInterruptMin) return;

            jobInterrupted = true;
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref totalTicksRequired, "TotalTicksRequired");
        }
    }
}