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
using System.Reflection;
using PsiTech.AutocastManagement;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PsiTech.Psionics {
    public class PsiTechTracker : IExposable, ILoadReferenceable {
        private const int TickRate = 60;
        private const int DayToSeconds = 1000;

        private const string AutocastLabel = "PsiTech.Psionics.AutocastLabel";
        private const string AutocastDesc = "PsiTech.Psionics.AutocastDesc";
        private const string AutocastIcon = "Abilities/Autocast";

        private Pawn pawn;
        private int loadId;

        private bool activated;
        public bool Activated => activated;
        public const int ActivationTimeSeconds = 2 * DayToSeconds;
        public const int RemoveTimeSeconds = DayToSeconds / 2;
        public bool AutocastEnabled = true;
        public bool HideAutocastToggleGizmo;

        public PsiTechAbility CurrentAbility;

        public bool TrainingSuspended;
        public List<TrainingQueueEntry> TrainingQueue = new List<TrainingQueueEntry>();
        private int nextTrainingId;

        public float AbilityModifier;
        private float abilityModifier => PsychicSensitivity * Essence;

        public float Essence;

        public float PsychicSensitivity {
            get {
                if (pawn == null) return 0f;
                if (hasCachedSensitivity) return psychicSensitivity;

                psychicSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                hasCachedSensitivity = true;
                return psychicSensitivity;
            }
        }

        private float psychicSensitivity;
        private bool hasCachedSensitivity;

        private float ProjectionAbility {
            get {
                if (pawn == null) return 0f;
                if (hasCachedProjection) return projectionAbility;

                projectionAbility = pawn.GetStatValue(PsiTechDefOf.PTPsiProjectionAbility);
                hasCachedProjection = true;
                return projectionAbility;
            }
        }

        private float projectionAbility;
        private bool hasCachedProjection;

        private float PsiDefense {
            get {
                if (pawn == null) return 0f;
                if (hasCachedDefense) return psiDefense;

                psiDefense = pawn.GetStatValue(PsiTechDefOf.PTPsiDefence);
                hasCachedDefense = true;
                return psiDefense;
            }
        }

        private float psiDefense;
        private bool hasCachedDefense;

        private int focusLevel = 1;
        private const int MaxFocusLevel = 3;
        private const float RegenPerFocusLevel = 0.5f;
        public float BaseRegen => RegenPerFocusLevel * focusLevel;

        public int FocusLevel {
            get => focusLevel;
            set {
                focusLevel = value;

                if (focusLevel < 0) focusLevel = 0;
                if (focusLevel > MaxFocusLevel) focusLevel = MaxFocusLevel;
                
                RebuildCaches();
            }
        }

        private int energyLevel = 1;
        private const int MaxEnergyLevel = 3;
        private const float EnergyPerEnergyLevel = 50f;
        private float BaseMaxEnergy => EnergyPerEnergyLevel * energyLevel;

        public int EnergyLevel {
            get => energyLevel;
            set {
                energyLevel = value;

                if (energyLevel < 0) energyLevel = 0;
                if (energyLevel > MaxEnergyLevel) energyLevel = MaxEnergyLevel;
                
                RebuildCaches();
            }
        }

        public int TotalLevel => EnergyLevel + FocusLevel;

        private float currentEnergy;
        public float CurrentEnergy => currentEnergy;
        private float lastCurrentEnergy;

        public List<PsiTechAbility> Abilities = new List<PsiTechAbility>();
        public const int Tier1Abilities = 8;
        public const int Tier2Abilities = 4;
        public const int Tier3Abilities = 2;

        private static int[][] LevelForSlot = {new[] {2, 2, 3, 3, 4, 4, 4, 4}, new[] {3, 4, 5, 5}, new[] {4, 5}};

        private bool lastCachedDebug;
        private bool lastCachedDraft;
        private bool lastCachedHideAutocastGizmo;
        private float lastCachedAbilityModifier;
        private float lastCachedProjectionStat;
        private bool hasCachedGizmos;

        private float[,] cachedStatMods;
        private float[,] cachedCapacities;
        private float[] cachedUnmodifiedCapacities;
        private IEnumerable<Gizmo> cachedGizmos = Enumerable.Empty<Gizmo>();

        public PsiTechTracker(Pawn pawn, int id) {
            this.pawn = pawn;
            loadId = id;
        }

        // Scribe requires a no-args constructor
        public PsiTechTracker() { }

        public void DetachFromPawn() {
            pawn = null;
            Abilities.ForEach(ability => ability.DetachFromUser());
        }

        public void AttachToPawn(Pawn newPawn) {
            pawn = newPawn;
            Abilities.ForEach(ability => ability.AttachToUser(newPawn));
        }
        
        public void InitializeCaches() {
            var statCount = DefDatabase<StatDef>.DefCount;
            cachedStatMods = new float[statCount,2];
            for (var k = 0; k < cachedStatMods.GetLength(0); k++) {
                cachedStatMods[k,1] = 1f;
            }

            var capCount = DefDatabase<PawnCapacityDef>.DefCount;
            cachedCapacities = new float[capCount,2];
            for (var k = 0; k < cachedCapacities.GetLength(0); k++) {
                cachedCapacities[k,1] = 1f;
            }

            var capacities = DefDatabase<PawnCapacityDef>.AllDefs;
            cachedUnmodifiedCapacities = new float[capCount];
            
            // Trigger capacity recache to get initial values
            pawn.health.capacities.Notify_CapacityLevelsDirty();
            foreach (var capacity in capacities) {
                pawn.health.capacities.GetLevel(capacity);
            }
        }

        public void TrackerTick() {
            if (pawn == null) return;
            Abilities.ForEach(ability => ability.AbilityTick());

            if ((Find.TickManager.TicksGame + loadId) % TickRate != 0) return;

            if (lastCachedDebug != (Prefs.DevMode && PsiTechSettings.PsiTechDebug)) {
                hasCachedGizmos = false;
                lastCachedDebug = Prefs.DevMode && PsiTechSettings.PsiTechDebug;
            }

            // There's no better way to invalidate our caches unfortunately, since there's no notify methods
            ClearStatCaches();

            var current = abilityModifier;
            if (current != AbilityModifier) {
                AbilityModifier = current;
                RebuildCaches();
            }

            RegenEnergy();
        }

        public float EnergyPercentOfMax() {
            return CurrentEnergy / pawn?.GetStatValue(PsiTechDefOf.PTMaxPsiEnergy) ?? 0f;
        }

        public void MaxEnergy() {
            currentEnergy = pawn?.GetStatValue(PsiTechDefOf.PTMaxPsiEnergy) ?? 0f;
            hasCachedGizmos = false;
        }

        public int[] UnlockedAbilitySlots() {
            return SlotsForLevel(TotalLevel);
        }

        public int[] QueueableAbilitySlots() {
            var level = TotalLevel + TrainingQueue.Count(EntryIsLevelling);

            return SlotsForLevel(level);
        }

        private static int[] SlotsForLevel(int level) {
            switch (level) {
                case 2:
                    return new[] {Tier1Abilities / 4, 0, 0};

                case 3:
                    return new[] {Tier1Abilities / 2, Tier2Abilities / 4, 0};

                case 4:
                    return new[] {Tier1Abilities, Tier2Abilities / 2, Tier3Abilities / 2};

                case 5:
                    return new[] {Tier1Abilities, Tier2Abilities, Tier3Abilities};

                case 6:
                    return new[] {Tier1Abilities, Tier2Abilities, Tier3Abilities};

                default:
                    Log.Warning("PsiTech tried to process an unrecognized level number.");
                    return new[] {Tier1Abilities, Tier2Abilities, Tier3Abilities};
            }
        }

        public static int LevelsNeededForSlot(int slot, int tier) {
            return LevelForSlot[tier - 1][slot];
        }

        public bool HasAvailableSlot(int tier) {
            return UnlockedAbilitySlots()[tier - 1] > Abilities.Count(ability => ability.Def.Tier == tier);
        }

        public List<AbilityDisplayEntry> AvailableAbilitiesForDisplay(int tier, bool ignoreRequiredResearch) {
            var abilities = DefDatabase<PsiTechAbilityDef>.AllDefs.Where(def =>
                def.Tier == tier && !Abilities.Any(ability => ability.Def == def) &&
                !TrainingQueue.Any(entry => entry.Def == def) &&
                (ignoreRequiredResearch || def.RequiredResearch.All(project => project.IsFinished)) &&
                !(pawn.WorkTagIsDisabled(WorkTags.Violent) && def.Violent));

            return abilities.Select(ability => new AbilityDisplayEntry
                {Def = ability, Trainable = IsAbilityTrainable(ability)}).ToList();
        }

        private bool IsAbilityTrainable(PsiTechAbilityDef def) {
            return def.RequiredAbilities.All(req =>
                Abilities.Any(ability => ability.Def == req) || TrainingQueue.Any(entry => entry.Def == req));
        }

        public PsiTechAbility AbilityInSlot(int slot, int tier) {
            return Abilities.Find(ability => ability.Def.Tier == tier && ability.Slot == slot);
        }

        public PsiTechAbilityDef QueuedAbilityInSlot(int slot, int tier) {
            return TrainingQueue.Find(entry =>
                entry.Type == TrainingType.Ability && entry.Def.Tier == tier && entry.Slot == slot).Def;
        }

        public bool HasAbility(PsiTechAbilityDef def) {
            return Abilities.Any(ability => ability.Def == def);
        }

        public bool HasConflictingAbility(PsiTechAbilityDef def) {
            if (def.ConflictingAbilities.NullOrEmpty()) return false;

            return !Abilities.Any(ability => def.ConflictingAbilities.Contains(ability.Def));
        }

        public PsiTechAbility AddAbility(PsiTechAbilityDef def, bool silent = false) {
            var existing = Abilities.Where(ability => ability.Def.Tier == def.Tier).ToList();

            int slots;
            switch (def.Tier) {
                case 1:
                    slots = Tier1Abilities;
                    break;

                case 2:
                    slots = Tier2Abilities;
                    break;

                case 3:
                    slots = Tier3Abilities;
                    break;

                default:
                    Log.Warning("PsiTech tried to add a tier " + def.Tier + " ability, which couldn't be processed.");
                    return null;
            }

            var slot = -1;
            for (var k = 0; k < slots; k++) {
                var slotCheck = k; // Gotta stop those closures
                if (existing.Any(ability => ability.Slot == slotCheck)) continue;

                slot = k;
                break;
            }

            if (slot != -1) return AddAbility(slot, def);
            if (!silent) {
                Log.Warning("PsiTech tried to add ability " + def.defName + " to pawn " + pawn +
                            " but no slots were available. Overwriting first ability.");
            }

            slot = 0;

            return AddAbility(slot, def);
        }

        public PsiTechAbility AddAbility(int slot, PsiTechAbilityDef def) {
            var abilityInSlot = Abilities.Find(existingAbility =>
                existingAbility.Def.Tier == def.Tier && existingAbility.Slot == slot);
            if (abilityInSlot != null) Abilities.Remove(abilityInSlot);

            def.ConflictingAbilities.ForEach(conflict => Abilities.RemoveAll(existing => existing.Def == conflict));

            var ability = (PsiTechAbility) Activator.CreateInstance(def.AbilityClass);
            ability.Def = def;
            ability.Slot = slot;
            ability.Tracker = this;
            ability.User = pawn;
            ability.LoadId = Current.Game.GetComponent<PsiTechManager>().GetNextAbilityId();
            if (def.Autocastable) {
                ability.AutocastFilter = Activator.CreateInstance(def.AutocastFilterClass) as AutocastFilter;

                if (ability.AutocastFilter == null) {
                    Log.Error(
                        "PsiTech tried to create a default autocast filter but got null, indicating a misconfigured ability def (def " +
                        def.defName + ")");
                }
                else {
                    ability.AutocastFilter.Ability = ability;
                    ability.AutocastFilter.User = pawn;
                    ability.AutocastFilter.FilterTargetType = def.DefaultFilterTargetType;
                    ability.AutocastFilter.TargetRange = new IntRange(0, Mathf.CeilToInt(ability.Def.Range));
                }
            }

            Abilities.Add(ability);

            RebuildCaches();
            ClearStatCaches();
            hasCachedGizmos = false;

            return ability;
        }

        public void TryRemoveAbility(PsiTechAbilityDef def) {
            var abilityToRemove = Abilities.Find(existingAbility => existingAbility.Def == def);
            if (abilityToRemove != null) {
                Abilities.Remove(abilityToRemove);
                hasCachedGizmos = false;
                RebuildCaches();
                ClearStatCaches();
            }
        }

        public bool ShouldTrain() {
            return TrainingQueue.Any() && !TrainingSuspended;
        }

        public bool CanMoveTrainingEntryUp(TrainingQueueEntry entry) {
            var index = TrainingQueue.FindIndex(existing => existing.Id == entry.Id);

            if (index == -1) {
                Log.Error("PsiTech was asked to find a training entry for ability " + entry.Def.defName + " on pawn " +
                          pawn + " but none existed. This should never happen.");
                return false;
            }

            // We're not already at the top
            // We're not a locked ability
            // We're not below a locked ability (this is only for the top ability while training)
            // We're not trying to move above a required ability
            // We're not trying to move above a focus or energy node that unlocked the slot we're in
            if (index == 0 || entry.Locked || TrainingQueue[index - 1].Locked ||
                (entry.Def?.RequiredAbilities.Contains(TrainingQueue[index - 1].Def) ?? false) ||
                (entry.Type == TrainingType.Ability && EntryIsLevelling(TrainingQueue[index - 1]) &&
                 QueuedNodesAbove(index) <= QueuedNodesNeededAbove(entry.Slot, entry.Def?.Tier ?? 1))) return false;

            return true;
        }

        public bool CanMoveTrainingEntryDown(TrainingQueueEntry entry) {
            var index = TrainingQueue.FindIndex(existing => existing.Id == entry.Id);

            if (index == -1) {
                Log.Error("PsiTech was asked to find a training entry for ability " + entry.Def.defName + " on pawn " +
                          pawn + " but none existed. This should never happen.");
                return false;
            }

            // We're not already at the bottom
            // We're not a locked ability
            // We're not above a locked ability (this is only for the top ability while training, not really necessary)
            // We're not trying a required ability trying to move below something that needs us
            // We're not a focus or energy node trying to move below an ability in a slot we unlock
            if (index == TrainingQueue.Count - 1 || entry.Locked || TrainingQueue[index + 1].Locked ||
                (TrainingQueue[index + 1].Def?.RequiredAbilities.Contains(entry.Def) ?? false) ||
                (EntryIsLevelling(entry) && TrainingQueue[index + 1].Type == TrainingType.Ability &&
                 QueuedNodesAbove(index + 1) <= QueuedNodesNeededAbove(TrainingQueue[index + 1].Slot,
                     TrainingQueue[index + 1].Def?.Tier ?? 1))) return false;

            return true;
        }

        public bool TrainingEntryExists(int slot, int tier) {
            return TrainingQueue.Any(entry =>
                entry.Type == TrainingType.Ability && entry.Def?.Tier == tier && entry.Slot == slot);
        }

        public void RemoveTrainingEntry(TrainingQueueEntry entry) {
            TrainingQueue.Remove(entry);
            if (EntryIsLevelling(entry)) {
                var toRemove = TrainingQueue.Where((existing, k) =>
                    existing.Type == TrainingType.Ability &&
                    QueuedNodesAbove(k) < QueuedNodesNeededAbove(existing.Slot, existing.Def.Tier)).ToList();

                toRemove.ForEach(RemoveTrainingEntry);
            }
            else if (entry.Type == TrainingType.Ability) {
                foreach (var req in TrainingQueue.Where(existing =>
                        existing.Type == TrainingType.Ability && existing.Def.RequiredAbilities.Contains(entry.Def))
                    .ToList().ListFullCopy()) {
                    RemoveTrainingEntry(req);
                }
            }
        }

        private static bool EntryIsLevelling(TrainingQueueEntry entry) {
            return entry.Type == TrainingType.Energy || entry.Type == TrainingType.Focus;
        }

        private int QueuedNodesAbove(int index) {
            return TrainingQueue.GetRange(0, index).Count(EntryIsLevelling);
        }

        private int QueuedNodesNeededAbove(int slot, int tier) {
            var needed = LevelsNeededForSlot(slot, tier) - TotalLevel;
            return Mathf.Clamp(needed, 0, int.MaxValue);
        }

        public void MoveTrainingEntryUp(TrainingQueueEntry entry) {
            var index = TrainingQueue.FindIndex(existing => existing.Id == entry.Id);

            var old = TrainingQueue[index - 1];

            TrainingQueue[index - 1] = entry;
            TrainingQueue[index] = old;
        }

        public void MoveTrainingEntryDown(TrainingQueueEntry entry) {
            var index = TrainingQueue.FindIndex(existing => existing.Id == entry.Id);

            var old = TrainingQueue[index + 1];

            TrainingQueue[index + 1] = entry;
            TrainingQueue[index] = old;
        }

        public void AddAbilityToTrainingQueue(int slot, PsiTechAbilityDef def) {
            // Check for existing queued ability and remove
            var existing = TrainingQueue.Find(queued => queued.Def?.Tier == def.Tier && queued.Slot == slot);
            if (existing.Def != null) {
                RemoveTrainingEntry(existing);
            }

            var entry = new TrainingQueueEntry {
                Type = TrainingType.Ability,
                Def = def,
                Id = nextTrainingId++,
                TrainingTimeSeconds = (int) (def.TrainingTimeDays * DayToSeconds),
                Slot = slot
            };
            TrainingQueue.Add(entry);
        }

        public void AddRemoveToTrainingQueue(int slot, PsiTechAbilityDef defToRemove) {
            // Check for existing queued ability and remove
            // Not much of a point in training something just to remove it, right?
            var existing = TrainingQueue.Find(queued => queued.Def?.Tier == defToRemove.Tier && queued.Slot == slot);
            if (existing.Def != null) {
                RemoveTrainingEntry(existing);
            }

            var entry = new TrainingQueueEntry {
                Type = TrainingType.Remove,
                Def = defToRemove,
                Id = nextTrainingId++,
                TrainingTimeSeconds = RemoveTimeSeconds,
                Slot = slot
            };
            TrainingQueue.Add(entry);
        }

        public void AddFocusToTrainingQueue() {
            var entry = new TrainingQueueEntry {
                Type = TrainingType.Focus,
                Id = nextTrainingId++,
            };
            TrainingQueue.Add(entry);
        }

        public void AddEnergyToTrainingQueue() {
            var entry = new TrainingQueueEntry {
                Type = TrainingType.Energy,
                Id = nextTrainingId++,
            };
            TrainingQueue.Add(entry);
        }

        private int TrainingTimeForFocusEnergy() {
            return (int) (Mathf.Pow(2, TotalLevel - 2) * DayToSeconds);
        }

        public int QueuedFocus => TrainingQueue.Count(entry => entry.Type == TrainingType.Focus);

        public bool CanAddFocusToTrainingQueue() {
            return FocusLevel + QueuedFocus < MaxFocusLevel;
        }

        public int QueuedEnergy => TrainingQueue.Count(entry => entry.Type == TrainingType.Energy);

        public bool CanAddEnergyToTrainingQueue() {
            return EnergyLevel + QueuedEnergy < MaxEnergyLevel;
        }

        public bool TryBeginNextTrainingEntry(out TrainingQueueEntry entry) {

            entry = TrainingQueue.FirstOrDefault();

            if (entry.Type == TrainingType.Ability && entry.Def == null) return false;

            if (EntryIsLevelling(entry)) {
                entry.TrainingTimeSeconds = TrainingTimeForFocusEnergy();
            }

            entry.Locked = true;
            TrainingQueue[0] = entry;
            return true;
        }

        public void ClearTrainingQueueLock() {
            var entry = TrainingQueue.FirstOrDefault();

            if (entry.Type == TrainingType.Ability && entry.Def == null) return;

            entry.Locked = false;
            TrainingQueue[0] = entry;
        }

        public void FinishTraining() {
            
            // Bit of a hack - dirty essence to make sure that it's up to date after training
            Notify_EssenceDirty();
            
            if (!Activated) {
                ActivateTracker();
                return;
            }

            var entry = TrainingQueue.FirstOrDefault();

            if (entry.Type == TrainingType.Ability && entry.Def == null) {
                Log.Warning("PsiTech tracker was asked to finish training for pawn " + pawn +
                            " when nothing was being trained. This should never happen.");
                return;
            }

            switch (entry.Type) {
                case TrainingType.Ability:
                    AddAbility(entry.Slot, entry.Def);
                    break;
                case TrainingType.Focus:
                    FocusLevel += 1;
                    break;
                case TrainingType.Energy:
                    EnergyLevel += 1;
                    break;
                case TrainingType.Remove:
                    TryRemoveAbility(entry.Def);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TrainingQueue.Remove(entry);
        }

        public void ActivateTracker() {
            activated = true;
            Current.Game.GetComponent<PsiTechManager>().Notify_PawnAwakened(this);
        }

        public float TotalAddedValueForThreat() {
            return Abilities.Sum(ability => ability.Def.AddedValueForThreat);
        }

        public float GetTotalOffsetOfStat(StatDef stat) {
            return cachedStatMods[stat.index, 0];
        }

        public float GetTotalFactorOfStat(StatDef stat) {
            return cachedStatMods[stat.index, 1];
        }

        public float GetTotalOffsetOfCapacity(PawnCapacityDef cap) {
            return cachedCapacities[cap.index, 0];
        }

        public float GetTotalFactorOfCapacity(PawnCapacityDef cap) {
            return cachedCapacities[cap.index, 1];
        }

        public void SetBaseCapacity(PawnCapacityDef cap, float value) {
            cachedUnmodifiedCapacities[cap.index] = value;

#if VER13
            // Clear entry in blindness cache if we changed the pawn's base sight
            if (cap != PawnCapacityDefOf.Sight) return;

            BlindnessHelper.ClearEntryForPawn(pawn);
#endif
        }

        public float GetBaseCapacity(PawnCapacityDef cap) {
            return cachedUnmodifiedCapacities[cap.index];
        }
        
        public IEnumerable<PsiTechAbility> GetAllAbilitiesImpactingCapacity(PawnCapacityDef cap) {
            return Abilities.Where(ability =>
                ability.GetOffsetOfCapacity(cap) != 0 || ability.GetFactorOfCapacity(cap) != 1);
        }

        // Modifier for casting of active abilities
        public float GetTotalModifierActive() {
            return AbilityModifier * ProjectionAbility;
        }

        public bool CanUseActiveAbilities() {
            return GetTotalModifierActive() > 1e-04;
        }

        // Modifier on effects/casting of passive abilities and triggered passives
        public float GetTotalModifierPassive() {
            return AbilityModifier;
        }

        // Total sensitivity modifier, used for defense rolls
        public float GetTotalModifierSensitivity() {
            return Mathf.Clamp(
                PsychicSensitivity * ProjectionAbility * (1 - PsiDefense) * Mathf.Max(Essence, 0.5f), 0,
                Mathf.Infinity);
        }

        // Normalized sensitivity modifier, essentially, what percentage of total psychic sensitivity we have
        // Used for some specific defense-type things where we shouldn't also be scaling by psychic sensitivity
        // I hate it
        public float GetTotalModifierSensitivityNormalized() {
            return Mathf.Clamp(ProjectionAbility * (1 - PsiDefense) * Mathf.Max(Essence, 0.5f), 0, Mathf.Infinity);
        }

        public void UseEnergy(float amount, bool silent = false) {
            currentEnergy -= amount;
            hasCachedGizmos = false;
            if (currentEnergy > 0) return;

            currentEnergy = 0;
            if (silent) return;
            
            Log.Warning("PsiTech ability used more energy than available in the pool of pawn " + pawn.Name);
        }

        public bool CanUseEnergy(float amount) {
            return currentEnergy >= amount;
        }

        public List<AutocastEntry> GetAbilitiesToAutocast() {
            var autocastable = Abilities.Where(ability => ability.CanAutocast).ToList();

            // Valid targets
            var targets = pawn.Map.PotentialPsiTargets();
            if (targets == null) return null;

            // Get entries
            var entries = new List<AutocastEntry>();
            foreach (var ability in autocastable) {
                entries.Add(ability.TryGetAutocastEntry(targets));
            }

            // Remove entries without targets
            entries.RemoveAll(entry => entry.Target == null);

            // Priority handling
            var highestPriority = AbilityPriority.Low;
            foreach (var entry in entries) {
                if (entry.Ability.Priority == AbilityPriority.High) {
                    highestPriority = AbilityPriority.High;
                    break;
                }

                if (entry.Ability.Priority == AbilityPriority.Normal && highestPriority == AbilityPriority.Low) {
                    highestPriority = AbilityPriority.Normal;
                }
            }

            entries.RemoveAll(entry => entry.Ability.Priority != highestPriority);

            return entries;
        }

        public Job GetAutocastJob() {
            if (pawn.Downed || pawn.stances.FullBodyBusy || !pawn.jobs.IsCurrentJobPlayerInterruptible()) return null;

            var entries = GetAbilitiesToAutocast();

            if (!entries.Any()) return null;

            // Select ability to autocast and autocast it
            var entry = entries.RandomElement();
            return entry.Ability.GetAutocastJob(entry.Target);
        }

        private void RegenEnergy() {
            if (pawn == null) return;
            currentEnergy += pawn.GetStatValue(PsiTechDefOf.PTPsiEnergyRegeneration);

            if (currentEnergy < 0) currentEnergy = 0;
            var maxEnergy = pawn.GetStatValue(PsiTechDefOf.PTMaxPsiEnergy);
            if (currentEnergy > maxEnergy)
                currentEnergy = maxEnergy;

            if (currentEnergy != lastCurrentEnergy) {
                lastCurrentEnergy = currentEnergy;
                hasCachedGizmos = false;
            }
        }

        public void Notify_EssenceDirty() {
            Essence = 1f - pawn?.health.hediffSet.CalculateEssencePenalty() ?? 0f;
        }

        public void Notify_GizmosDirty() {
            hasCachedGizmos = false;
        }
        
        public IEnumerable<Gizmo> GetGizmos() {
            // Ensure responsiveness to drafting and undrafting when paused
            if (pawn.Drafted != lastCachedDraft) hasCachedGizmos = false;

            // Ensure responsiveness to changing the gizmo status when paused
            if (HideAutocastToggleGizmo != lastCachedHideAutocastGizmo) hasCachedGizmos = false;

            // And all the stats and such
            if (lastCachedAbilityModifier != AbilityModifier ||
                lastCachedProjectionStat != ProjectionAbility)
                hasCachedGizmos = false;

            if (hasCachedGizmos) return cachedGizmos;

            // Rebuild cache if needed
            var gizmos = new List<Gizmo>();

            if (!HideAutocastToggleGizmo && pawn.IsColonistPlayerControlled && CanUseActiveAbilities() &&
                Abilities.Any(ability => ability.Def.Autocastable)) {
                var gizmo = new Command_Toggle {
                    defaultLabel = AutocastLabel.Translate(),
                    defaultDesc = AutocastDesc.Translate(),
                    isActive = () => AutocastEnabled,
                    toggleAction = () => AutocastEnabled = !AutocastEnabled,
                    icon = ContentFinder<Texture2D>.Get(AutocastIcon)
                };

                gizmos.Add(gizmo);
            }

            foreach (var ability in Abilities) {
                var abilityGizmos = ability.GetGizmos();
                if (abilityGizmos == null) continue;

                gizmos.AddRange(abilityGizmos);
            }

            if (PsiTechSettings.PsiTechDebug && Prefs.DevMode) {
                gizmos.AddRange(DebugGizmos());
            }

            cachedGizmos = gizmos;
            lastCachedDraft = pawn.Drafted;
            lastCachedHideAutocastGizmo = HideAutocastToggleGizmo;
            lastCachedAbilityModifier = AbilityModifier;
            lastCachedProjectionStat = ProjectionAbility;
            hasCachedGizmos = true;

            return gizmos;
        }

        private IEnumerable<Gizmo> DebugGizmos() {
            yield return new Command_Action {
                defaultLabel = "DEBUG Dump total pawn threat",
                defaultDesc = "",
                action =
                    () => Log.Message(pawn + " total threat: " +
                                      (pawn.MarketValue +
                                       pawn.equipment.AllEquipmentListForReading.Sum(equip => equip.MarketValue) +
                                       TotalAddedValueForThreat()))
            };

            if (!Activated) {
                yield return new Command_Action {
                    defaultLabel = "DEBUG Enable Psi",
                    defaultDesc = "",
                    action = () => {
                        ActivateTracker();
                        hasCachedGizmos = false;
                    }
                };
            }
            else {
                yield return new Command_Action {
                    defaultLabel = "DEBUG Max Energy",
                    defaultDesc = "",
                    action = MaxEnergy
                };

                yield return new Command_Action {
                    defaultLabel = "DEBUG Reset cooldowns",
                    defaultDesc = "",
                    action = () => {
                        Abilities.ForEach(ability => ability.ResetAbility());
                        hasCachedGizmos = false;
                    }
                };

                yield return new Command_Action {
                    defaultLabel = "DEBUG Max psion",
                    defaultDesc = "",
                    action = MaxPsion
                };
            }
        }

        private void MaxPsion() {
            // Juice base stats
            energyLevel = MaxEnergyLevel;
            focusLevel = MaxFocusLevel;

            // Reset abilities before rolling - prevent conflicts with existing abilities
            Abilities.Clear();

            // Roll abilities for each slot
            for (var k = 0; k < Tier1Abilities; k++) {
                var available = AvailableAbilitiesForDisplay(1, true)
                    .Where(ability => !HasConflictingAbility(ability.Def));
                AddAbility(k, available.RandomElement().Def);
            }

            for (var k = 0; k < Tier2Abilities; k++) {
                var available = AvailableAbilitiesForDisplay(2, true)
                    .Where(ability => !HasConflictingAbility(ability.Def));
                AddAbility(k, available.RandomElement().Def);
            }

            for (var k = 0; k < Tier3Abilities; k++) {
                var available = AvailableAbilitiesForDisplay(3, true)
                    .Where(ability => !HasConflictingAbility(ability.Def));
                AddAbility(k, available.RandomElement().Def);
            }
        }

        public void RebuildCaches() {
            if (pawn == null) return;
            
            pawn.health.capacities.Notify_CapacityLevelsDirty();
            
            for (var k = 0; k < cachedStatMods.GetLength(0); k++) {
                cachedStatMods[k,0] = 0f;
                cachedStatMods[k,1] = 1f;
            }

            for (var k = 0; k < cachedCapacities.GetLength(0); k++) {
                cachedCapacities[k,0] = 0f;
                cachedCapacities[k,1] = 1f;
            }

            foreach (var ability in Abilities) {
                foreach (var (stat, offset) in ability.GetAllStatOffsets()) {
                    cachedStatMods[stat.index, 0] += offset;
                }
                foreach (var (stat, factor) in ability.GetAllStatFactors()) {
                    cachedStatMods[stat.index, 1] *= factor;
                }
                
                foreach (var (capacity, offset, factor) in ability.GetAllCapacityMods()) {
                    cachedCapacities[capacity.index, 0] += offset;
                    cachedCapacities[capacity.index, 1] *= factor;
                }
            }
            
            cachedStatMods[PsiTechDefOf.PTPsiEnergyRegeneration.index, 0] += BaseRegen;
            cachedStatMods[PsiTechDefOf.PTMaxPsiEnergy.index, 0] += BaseMaxEnergy;
        }

        public void ClearStatCaches() {
            hasCachedProjection = false;
            hasCachedSensitivity = false;
            hasCachedDefense = false;
        }

        public void ForceSetAbilityModifier() {
            AbilityModifier = abilityModifier;
        }

    public string GetUniqueLoadID() {
            return "PsiTechTracker_" + loadId;
        }

        public void ExposeData() {
            Scribe_References.Look(ref CurrentAbility, "CurrentAbility");

            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref loadId, "loadId");
            Scribe_Values.Look(ref activated, "Activated");
            Scribe_Values.Look(ref AutocastEnabled, "AutocastEnabled", true);
            Scribe_Values.Look(ref HideAutocastToggleGizmo, "HideAutocastToggleGizmo");

            Scribe_Values.Look(ref TrainingSuspended, "TrainingSuspended");
            Scribe_Collections.Look(ref TrainingQueue, "trainingQueue", LookMode.Deep);
            Scribe_Values.Look(ref nextTrainingId, "NextTrainingId");

            Scribe_Values.Look(ref focusLevel, "FocusLevel");
            Scribe_Values.Look(ref energyLevel, "EnergyLevel");
            Scribe_Values.Look(ref currentEnergy, "CurrentEnergy");

            Scribe_Collections.Look(ref Abilities, "Abilities", LookMode.Deep);

            // Initialize caches after load (must be deferred to prevent infinite recursion)
            // So we don't have to tick to build the fill the cache after a load
            if (pawn != null && Scribe.mode == LoadSaveMode.PostLoadInit) {
                InitializeCaches();
                AbilityModifier = abilityModifier;
                Notify_EssenceDirty();
                RebuildCaches();
            }
        }

        // This is pretty terrible, but it is what it is
        // Trigger intercepting occurs in various Harmony patches stuck in places they don't belong
        // Those patches call these methods, which perform trigger distribution
        public void TriggerAlmostDead(Pawn instigator) {
            if (!Activated) return;

            Abilities
                .Where(ability => ability is PsiTechAbilityTriggeredPassive triggered &&
                                  triggered.Def.Trigger == TriggerType.AlmostDead)
                .Select(ability => (PsiTechAbilityTriggeredPassive) ability).ToList()
                .ForEach(triggered => triggered.TriggerAbility(instigator));
        }

        public void TriggerAttacked(Pawn instigator) {
            if (!Activated) return;

            Abilities
                .Where(ability => ability is PsiTechAbilityTriggeredPassive triggered &&
                                  triggered.Def.Trigger == TriggerType.Attacked)
                .Select(ability => (PsiTechAbilityTriggeredPassive) ability).ToList()
                .ForEach(triggered => triggered.TriggerAbility(instigator));
        }
    }

    public struct AbilityDisplayEntry {
        public PsiTechAbilityDef Def;
        public bool Trainable;
    }

    public struct AutocastEntry {
        public PsiTechAbility Ability;
        public Pawn Target;
    }

    public struct TrainingQueueEntry : IExposable {
        public TrainingType Type;
        public PsiTechAbilityDef Def;
        public int Id;
        public int TrainingTimeSeconds;
        public int Slot;
        public bool Locked;

        public void ExposeData() {
            Scribe_Values.Look(ref Type, "Type");
            Scribe_Defs.Look(ref Def, "Def");
            Scribe_Values.Look(ref Id, "Id");
            Scribe_Values.Look(ref TrainingTimeSeconds, "TrainingTimeSeconds");
            Scribe_Values.Look(ref Slot, "Slot");
            Scribe_Values.Look(ref Locked, "Locked");
        }
    }

    public enum TrainingType {
        Ability,
        Focus,
        Energy,
        Remove
    }
}