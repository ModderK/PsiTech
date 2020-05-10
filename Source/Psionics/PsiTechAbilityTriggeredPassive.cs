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
using System.Linq;
using PsiTech.Misc;
using PsiTech.Utility;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PsiTech.Psionics {
    public class PsiTechAbilityTriggeredPassive : PsiTechAbility {
        
        private int cooldownTicker;

        public override void AbilityTick() {
            if (cooldownTicker > 0) {
                cooldownTicker--;
            }
        }
        
        public void TriggerAbility(Pawn instigator) {

            if (!Tracker.CanUseEnergy(Def.EnergyPerUse) || cooldownTicker > 0) return;

            var success = false;
            switch(Def.Target){
                case TargetType.Single:
                    var target = Def.TargetValidator.SelectBestTargetFromLists(User,
                        User.Map.mapPawns.AllPawns.Where(pawn => Def.TargetValidator.IsValidTarget(User, pawn)).ToList());
                    if (target == null || !Rand.Chance(SuccessChanceOnTarget(target))) break;
                    TryThrowMoteOnTarget(target);
                    TryPickAndDoEffect(target);
                    success = true;
                    break;
                
                case TargetType.AllAvailable:
                    var targets =
                        User.Map.mapPawns.AllPawns.Where(pawn => Def.TargetValidator.IsValidTarget(User, pawn));
                    foreach (var targ in targets) {
                        if (targ == null || !Rand.Chance(SuccessChanceOnTarget(targ))) continue;
                        TryThrowMoteOnTarget(targ);
                        TryPickAndDoEffect(targ);
                        success = true;
                    }
                    break;
                
                case TargetType.Attacker:
                    if (instigator == null || !Def.TargetValidator.IsValidTarget(User, instigator) ||
                        !Rand.Chance(SuccessChanceOnTarget(instigator))) break;
                    TryThrowMoteOnTarget(instigator);
                    TryPickAndDoEffect(instigator);
                    success = true;
                    break;
                    
                default:
                    Log.Warning("PsiTech tried to process a triggered passive with an unsupported target type " + Def.Target);
                    break;
            }

            Tracker.UseEnergy(Def.EnergyPerUse);
            cooldownTicker = CooldownTicks;

            if (success) {
                Def.SoundDefSuccessOnCaster?.PlayOneShot(new TargetInfo(User.Position, User.Map));
                Def.SoundDefSuccessOnTarget?.PlayOneShot(new TargetInfo(instigator.Position, instigator.Map));
            }
            else {
                Def.SoundDefFailure.PlayOneShot(new TargetInfo(User.Position, User.Map));
                TryThrowFailMoteOnUser();
            }

        }
        
        public override float SuccessChanceOnTarget(Pawn target) {
            return Def.AlwaysHits
                ? 1f
                : Mathf.Clamp(
                    Def.BaseSuccessChance * Tracker.GetTotalModifierPassive() *
                    target.PsiTracker().GetTotalModifierSensitivity(), 0, 1);
        }

        // Not a usable gizmo, just shows the cooldown/energy
        public override IEnumerable<Gizmo> GetGizmos() {

            if (User.IsColonist && !User.Drafted) yield break;

            var command = new CommandIndicator {
                defaultLabel = Def.label,
                defaultDesc = Def.GizmoDesc,
                icon = ContentFinder<Texture2D>.Get(Def.PathToIcon),
            };
            
            if (Tracker.Essence < 1e-4) {
                command.Disable(EssenceDisableKey.Translate(User.LabelShort, User.health.hediffSet.CountAddedAndImplantedParts()));
            }else if (Tracker.PsychicSensitivity < 1e-4) {
                command.Disable(SensitivityDisableKey.Translate(User.LabelShort, Tracker.PsychicSensitivity));
            }
            else if (cooldownTicker > 0) { // We're on cooldown
                command.Disable(OnCooldownKey.Translate(Def.label, cooldownTicker.TicksToHours().ToString("0.0")));
            }
            else if (!Tracker.CanUseEnergy(Def.EnergyPerUse)) { // Not enough energy to cast
                command.Disable(NotEnoughEnergyKey.Translate(Def.label, Tracker.CurrentEnergy.ToString("0.0"), Def.EnergyPerUse));
            }

            yield return command;
        }
        
        public override void ResetAbility() {
            cooldownTicker = 0;
        }
        
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref cooldownTicker, "CooldownTicker");
        }

    }

    public enum TriggerType {
        AlmostDead,
        Attacked
    }

    public enum TargetType {
        Single,
        AllAvailable,
        Attacker
    }
}