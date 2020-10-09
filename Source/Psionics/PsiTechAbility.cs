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
using System.Linq;
using JetBrains.Annotations;
using PsiTech.AutocastManagement;
using PsiTech.Misc;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PsiTech.Psionics {
    public abstract class PsiTechAbility : IExposable, ILoadReferenceable {

        public PsiTechAbilityDef Def;
        public int Slot;
        public PsiTechTracker Tracker;
        public Pawn User;
        public int LoadId;

        private Verb_Psionic verb;
        protected Verb_Psionic Verb =>
            verb ??= new Verb_Psionic {
                Ability = this,
                caster = User,
            };

        // Autocast and AI control variables
        public bool Autocast;
        public AbilityPriority Priority = AbilityPriority.Normal;
        public List<AutocastCondition> AutocastConditions = new List<AutocastCondition>();
        public AutocastFilter AutocastFilter;
        public bool CanAutocast => Def.Autocastable && Autocast && Tracker.AutocastEnabled;

        protected int CooldownTicks => Mathf.RoundToInt(Def.CooldownSeconds.SecondsToTicks() *
                                       User.GetStatValue(PsiTechDefOf.PTPsiCooldownMultiplier));

        protected const string NotEnoughEnergyKey = "PsiTech.Psionics.NotEnoughEnergy";
        protected const string OnCooldownKey = "PsiTech.Psionics.OnCooldown";
        protected const string EssenceDisableKey = "PsiTech.Psionics.EssenceDisable";
        protected const string SensitivityDisableKey = "PsiTech.Psionics.SensitivityDisable";
        protected const string ProjectionDisabledKey = "PsiTech.Psionics.ProjectionDisabled";
        protected const string CancelActiveKey = "PsiTech.Psionics.CancelActive";
        protected const string CancelActiveDescKey = "PsiTech.Psionics.CancelActiveDesc";
        protected const string ResistedKey = "PsiTech.Psionics.Resisted";

        protected const string CancelIconPath = "Abilities/CancelAbility";

        public string GetUniqueLoadID() {
            return Def.defName + "_" + LoadId;
        }
        
        public virtual void AbilityTick() {}

        [CanBeNull]
        public virtual Job GetAutocastJob(Pawn target) {
            return null;
        }
        
        public virtual void DoAbility() {}
        
        public virtual void DoAbilityOnTarget(Pawn target) {}

        public virtual bool CanHitTarget(Pawn target) {
            return true;
        }

        public virtual float SuccessChanceOnTarget(Pawn target) {
            return 1f;
        }

        public virtual float GetOffsetOfStat(StatDef stat) {
            return 0f;
        }

        public virtual float GetFactorOfStat(StatDef stat) {
            return 1f;
        }

        public virtual float GetOffsetOfCapacity(PawnCapacityDef cap) {
            return 0f;
        }

        public virtual float GetFactorOfCapacity(PawnCapacityDef cap) {
            return 1f;
        }

        public virtual IEnumerable<(StatDef stat, float offset)> GetAllStatOffsets() {
            yield break;
        }

        public virtual IEnumerable<(StatDef stat, float factor)> GetAllStatFactors() {
            yield break;
        }

        
        public virtual IEnumerable<(PawnCapacityDef capacity, float offset, float factor)> GetAllCapacityMods() {
            yield break;
        }

        protected virtual bool TryPickAndDoEffect(Pawn target) {
            if (!Def.PossibleEffects.Any()) return false;
            
            var weightsTotal = Def.PossibleEffects.Sum(ef => ef.Weight);
            var rand = Rand.Value * weightsTotal;

            foreach (var possible in Def.PossibleEffects) {
                rand -= possible.Weight;
                if (rand <= 0) {
                    return possible.TryDoEffectOnPawn(User, target);
                }
            }

            return false;
        }
        
        protected virtual bool TryPickAndDoEffectOnUser() {
            if (!Def.PossibleEffectsOnUser.Any()) return false;
            
            var weightsTotal = Def.PossibleEffectsOnUser.Sum(ef => ef.Weight);
            var rand = Rand.Value * weightsTotal;

            foreach (var possible in Def.PossibleEffectsOnUser) {
                rand -= possible.Weight;
                if (rand <= 0) {
                    return possible.TryDoEffectOnPawn(User, User);
                }
            }

            return false;
        }

        protected virtual void TryThrowMoteOnTarget(Pawn target) {
            if (Def.MoteOnTarget == null) return;
            
            var mote = (Mote) ThingMaker.MakeThing(Def.MoteOnTarget);
            mote.Scale = Rand.Range(1.25f, 1.75f);
            mote.exactPosition = target.DrawPos;
            GenSpawn.Spawn(mote, target.Position, target.Map);
        }

        protected virtual void TryThrowSuccessMoteOnUser() {
            if (Def.MoteOnUserSuccess == null) return;
            
            var mote = (Mote) ThingMaker.MakeThing(Def.MoteOnUserSuccess);
            mote.Scale = Rand.Range(1.25f, 1.75f);
            mote.exactPosition = User.DrawPos;
            GenSpawn.Spawn(mote, User.Position, User.Map);
        }

        protected virtual void TryThrowMoteDualAttachedToTarget(Pawn target) {
            if (Def.MoteLink == null) return;
            
            var mote = (MoteLink) ThingMaker.MakeThing(Def.MoteLink);
            mote.Scale = Rand.Range(0.9f, 1.1f);
            mote.SetupLink(ContentFinder<Texture2D>.Get(Def.MoteLink.graphicData.texPath), User.DrawPos, target.DrawPos);
            GenSpawn.Spawn(mote, User.Position, User.Map);
        }

        protected virtual void TryThrowMoteSuccessPointerToTarget(Pawn target) {
            if (Def.MoteSuccessPointer == null) return;
            
            var mote = (MotePointer) ThingMaker.MakeThing(Def.MoteSuccessPointer);
            mote.Scale = Rand.Range(1.9f, 2.1f);
            mote.SetupLink(ContentFinder<Texture2D>.Get(Def.MoteSuccessPointer.graphicData.texPath), User.DrawPos, target.DrawPos);
            GenSpawn.Spawn(mote, User.Position, User.Map);
        }

        protected virtual void TryThrowFailMoteOnUser() {
            if (Def.MoteOnUserFailure == null) return;
            
            var mote = (Mote) ThingMaker.MakeThing(Def.MoteOnUserFailure);
            mote.Scale = Rand.Range(1.25f, 1.75f);
            mote.exactPosition = User.DrawPos + Vector3.forward * 0.7f;
            GenSpawn.Spawn(mote, User.Position, User.Map);
        }

        public virtual IEnumerable<Gizmo> GetGizmos() {
            yield break;
        }
        
        public virtual void ResetAbility() { }

        public virtual bool CanCast() {
            return false;
        }

        public virtual AutocastEntry TryGetAutocastEntry(IEnumerable<Pawn> targets) {

            if (!CanCast() || AutocastConditions.Any(condition => !condition.CanDoAutocast())) {
                return new AutocastEntry{
                    Ability = this,
                    Target = null
                };
            }

            var possibleTargets = targets.Where(target => Def.TargetValidator.IsValidTarget(User, target) && target.Position.InHorDistOf(User.Position, Def.Range));
            return new AutocastEntry{
                Ability = this,
                Target = AutocastFilter.GetBestTarget(possibleTargets)
            };
        }

        public virtual void ExposeData() {
            Scribe_Defs.Look(ref Def, "Def");
            Scribe_Values.Look(ref Slot, "Slot");
            Scribe_References.Look(ref Tracker, "Tracker");
            Scribe_References.Look(ref User, "User");
            Scribe_Values.Look(ref LoadId, "LoadId");
            
            Scribe_Values.Look(ref Autocast, "Autocast");
            Scribe_Values.Look(ref Priority, "Priority");
            Scribe_Collections.Look(ref AutocastConditions, "AutocastCondtions", LookMode.Deep);
            Scribe_Deep.Look(ref AutocastFilter, "AutocastFilter");
        }
    }

    public enum AbilityPriority {
        High,
        Normal,
        Low
    }
}