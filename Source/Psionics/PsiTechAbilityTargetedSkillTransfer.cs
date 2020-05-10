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
using PsiTech.Utility;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PsiTech.Psionics {
    public class PsiTechAbilityTargetedSkillTransfer : PsiTechAbilityTargeted {
        
        private bool active;
        private Pawn linkedPawn;
        private int drawTimer;
        
        public override void AbilityTick() {
            base.AbilityTick();

            if (!active) return;

            drawTimer--;
            if (drawTimer <= 0) {
                TryThrowMoteDualAttachedToTarget(linkedPawn);
                drawTimer = Def.LinkPulseTicks;
            }

            if (Tracker.CanUseEnergy(Def.EnergyPerSecondActive / 60f) &&
                User.Position.InHorDistOf(linkedPawn.Position, Def.Range)) {
                Tracker.UseEnergy(Def.EnergyPerSecondActive / 60f);
            }
            else {
                EndAbility();
            }
        }

        private void EndAbility() {
            if (!active) return;
            
            active = false;
            PsiTechSkillTransferUtility.RemoveTransferPairWithInitiator(User);
            linkedPawn = null;
            drawTimer = 0;
        }
        
        public override void DoAbilityOnTarget(Pawn target) {
            Tracker.UseEnergy(Def.EnergyPerUse);
            CooldownTicker = CooldownTicks;

            if (!Rand.Chance(SuccessChanceOnTarget(target))) {
                Def.SoundDefFailure.PlayOneShot(new TargetInfo(User.Position, User.Map));
                TryThrowFailMoteOnUser();
                return;
            }

            TryThrowMoteOnTarget(target);
            TryThrowMoteSuccessPointerToTarget(target);
            Def.SoundDefSuccessOnCaster?.PlayOneShot(new TargetInfo(User.Position, User.Map));
            Def.SoundDefSuccessOnTarget?.PlayOneShot(new TargetInfo(target.Position, target.Map));

            var statTransfer =
                Mathf.Min(Def.BaseSkillTransfer * Tracker.GetTotalModifierActive() * target.PsiTracker().GetTotalModifierActive(), 1);
            active = PsiTechSkillTransferUtility.TryAddNewTransferPair(new TransferPair
                {Initiator = User, Receiver = target, SkillTransferAmount = statTransfer});
            if (active) linkedPawn = target;
            }

        public override IEnumerable<Gizmo> GetGizmos() {
            
            if (!active) {
                foreach (var gizmo in base.GetGizmos()) yield return gizmo;
            }
            else {
                yield return new Command_Action() {
                    defaultLabel = CancelActiveKey.Translate(Def.label),
                    defaultDesc = CancelActiveDescKey.Translate(Def.label),
                    icon = ContentFinder<Texture2D>.Get(CancelIconPath),
                    action = EndAbility
                }; 
            }
        }
		
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref active, "active");
            Scribe_Deep.Look(ref linkedPawn, "linkedPawn");
            Scribe_Values.Look(ref drawTimer, "drawTimer");
		}
    }
}