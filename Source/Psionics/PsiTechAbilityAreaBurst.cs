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
using PsiTech.Misc;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PsiTech.Psionics {
    public class PsiTechAbilityAreaBurst : PsiTechAbility {
        
        private int cooldownTicker;

        private IEnumerable<Pawn> PawnsToAffect =>
            User.Map.PotentialPsiTargets().Where(target => Def.TargetValidator.IsValidTarget(User, target));

        public override void AbilityTick() {
            if (cooldownTicker <= 0) return;
            
            cooldownTicker--;
            Tracker.Notify_GizmosDirty();
        }
        
        public override Job GetAutocastJob(Pawn target) {
            var job = JobMaker.MakeJob(PsiTechDefOf.PTBurstPsionic, User);
            job.verbToUse = Verb;
            return job;
        }

        public override void DoAbility() {
            Tracker.UseEnergy(Def.EnergyPerUse, true);
            cooldownTicker = CooldownTicks;

            var cachedToAffect = PawnsToAffect.ToList().ListFullCopy();
            foreach (var pawn in cachedToAffect) {
                if (!pawn.Position.InHorDistOf(User.Position, Def.Range)) continue;

                var stackMod = 0f;
                var didEffect = false;
                while (Rand.Chance(SuccessChanceOnTarget(pawn) - stackMod)){
                    TryPickAndDoEffect(pawn);
                    stackMod += 1.0f;
                    didEffect = true;
                    if (pawn.Dead) break;
                }

                if (!didEffect) {
                    MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, ResistedKey.Translate(), 1.9f);
                }
            }
            
            Def.SoundDefSuccessOnCaster?.PlayOneShot(new TargetInfo(User.Position, User.Map));
            TryThrowSuccessMoteOnUser();
            TryPickAndDoEffectOnUser();
        }

        public override float SuccessChanceOnTarget(Pawn target) {
            var min = Def.AlwaysHits ? 1f : 0f;
            return Mathf.Clamp(Def.BaseSuccessChance * Tracker.GetTotalModifierActive() * target.PsiTracker().GetTotalModifierSensitivity(), min, Mathf.Infinity);
        }
        
        protected override void TryThrowSuccessMoteOnUser() {
            if (Def.MoteOnUserSuccess == null) return;
            
            var mote = (Mote) ThingMaker.MakeThing(Def.MoteOnUserSuccess);
            mote.Scale = Rand.Range(15f, 20f);
            mote.exactPosition = User.DrawPos;
            GenSpawn.Spawn(mote, User.Position, User.Map);
        }

        public override IEnumerable<Gizmo> GetGizmos() {

            if (User.IsColonist && !User.Drafted) yield break;

            Command command;
            if (User.IsColonist) {
                command = new CommandPsionic {
                    Ability = this,
                    Caster = User,
                    defaultLabel = Def.label,
                    defaultDesc = Def.GizmoDesc,
                    icon = ContentFinder<Texture2D>.Get(Def.PathToIcon)
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
            else if (cooldownTicker > 0) { // We're on cooldown
                command.Disable(OnCooldownKey.Translate(Def.label, cooldownTicker.TicksToHours().ToString("0.0")));
            }
            else if (!Tracker.CanUseEnergy(Def.EnergyPerUse)) { // Not enough energy to cast
                command.Disable(NotEnoughEnergyKey.Translate(Def.label, Tracker.CurrentEnergy.ToString("0.0"), Def.EnergyPerUse));
            }
            else if (command is CommandPsionic psi){ // We've got enough energy and we're off cooldown
                var job = JobMaker.MakeJob(PsiTechDefOf.PTBurstPsionic, User);
                job.verbToUse = Verb;
                psi.action = () => { User.jobs.TryTakeOrderedJob(job); };
            }

            yield return command;
        }

        public override void ResetAbility() {
            cooldownTicker = 0;
        }
        
        public override bool CanCast() {
            return cooldownTicker == 0 && User.PsiTracker().CanUseEnergy(Def.EnergyPerUse);
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref cooldownTicker, "CooldownTicker");
        }
    }
}