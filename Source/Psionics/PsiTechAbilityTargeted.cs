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
using PsiTech.Misc;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PsiTech.Psionics {
    public class PsiTechAbilityTargeted : PsiTechAbility {
        
        protected int CooldownTicker;

        public override void AbilityTick() {
            if (CooldownTicker > 0) {
                CooldownTicker--;
                Tracker.Notify_GizmosDirty();
            }
        }

        public override Job GetAutocastJob(Pawn target) {
            var job = JobMaker.MakeJob(PsiTechDefOf.PTSingleTargetPsionic, User, target);
            job.verbToUse = Verb;
            return job;
        }

        public override void DoAbilityOnTarget(Pawn target) {
            Tracker.UseEnergy(Def.EnergyPerUse, true);
            CooldownTicker = CooldownTicks;

            if (!Rand.Chance(SuccessChanceOnTarget(target))) {
                MoteMaker.ThrowText(target.Position.ToVector3(), target.Map, ResistedKey.Translate(), 1.9f);
                Def.SoundDefFailure.PlayOneShot(new TargetInfo(User.Position, User.Map));
                TryThrowFailMoteOnUser();
                return;
            }
            
            TryThrowMoteOnTarget(target);
            TryThrowMoteSuccessPointerToTarget(target);
            Def.SoundDefSuccessOnCaster?.PlayOneShot(new TargetInfo(User.Position, User.Map));
            Def.SoundDefSuccessOnTarget?.PlayOneShot(new TargetInfo(target.Position, target.Map));
            TryPickAndDoEffect(target);
            TryPickAndDoEffectOnUser();
        }

        public override bool CanHitTarget(Pawn target) {
            return User.Position.InHorDistOf(target.Position, Def.Range);
        }

        public override float SuccessChanceOnTarget(Pawn target) {
            return Def.AlwaysHits
                ? 1f
                : Mathf.Clamp(
                    Def.BaseSuccessChance * Tracker.GetTotalModifierActive() *
                    target.PsiTracker().GetTotalModifierSensitivity(), 0, 1);
        }

        public override IEnumerable<Gizmo> GetGizmos() {
            
            if (User.IsColonist && !User.Drafted) yield break;

            Command command;
            if (User.IsColonist) {
                command = new CommandTargetedPsionic {
                    Ability = this,
                    Caster = User,
                    defaultLabel = Def.label,
                    defaultDesc = Def.GizmoDesc,
                    icon = ContentFinder<Texture2D>.Get(Def.PathToIcon),
                    targetingParams = TargetingParameters.ForAttackAny(),
                };
            }
            else {
                command = new CommandIndicator {
                    Ability = this,
                    Caster = User,
                    defaultLabel = Def.label,
                    defaultDesc = Def.GizmoDesc,
                    icon = ContentFinder<Texture2D>.Get(Def.PathToIcon)
                };
            }
            
            if (Tracker.Essence < 1e-4) {
                command.Disable(EssenceDisableKey.Translate(User.LabelShort, User.health.hediffSet.CountAddedAndImplantedParts()));
            }else if (Tracker.PsychicSensitivity < 1e-4) {
                command.Disable(SensitivityDisableKey.Translate(User.LabelShort, Tracker.PsychicSensitivity));
            }
            else if (!Tracker.CanUseActiveAbilities()) { // Pawn has projection disabled
                command.Disable(ProjectionDisabledKey.Translate(User.LabelShort));
            }
            else if (CooldownTicker > 0) { // We're on cooldown
                command.Disable(OnCooldownKey.Translate(Def.label, CooldownTicker.TicksToHours().ToString("0.0")));
            }
            else if (!Tracker.CanUseEnergy(Def.EnergyPerUse)) { // Not enough energy to cast
                command.Disable(NotEnoughEnergyKey.Translate(Def.label, Tracker.CurrentEnergy.ToString("0.0"), Def.EnergyPerUse));
            }
            else if(command is CommandTargetedPsionic psi){ // We've got enough energy and we're off cooldown
                psi.action = target => {
#if VER13
                    if (!User.Position.InHorDistOf(target.Cell, Def.Range) || target.Pawn == null ||
                        !Def.TargetValidator.IsValidTarget(User, target.Pawn)) return;
                    // We're in range and have a valid target
                    var job = JobMaker.MakeJob(PsiTechDefOf.PTSingleTargetPsionic, User, target.Pawn);
#else
                    if (!User.Position.InHorDistOf(target.Position, Def.Range) || !(target is Pawn pawn) ||
                        !Def.TargetValidator.IsValidTarget(User, pawn)) return;
                    // We're in range and have a valid target
                    var job = JobMaker.MakeJob(PsiTechDefOf.PTSingleTargetPsionic, User, pawn);
#endif
                    job.verbToUse = Verb;
                    User.jobs.TryTakeOrderedJob(job);
                };
            }

            yield return command;
        }
        
        public override void ResetAbility() {
            CooldownTicker = 0;
        }

        public override bool CanCast() {
            return CooldownTicker == 0 && User.PsiTracker().CanUseEnergy(Def.EnergyPerUse);
        }
		
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref CooldownTicker, "CooldownTicker");
		}
    }
}