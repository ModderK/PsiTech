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
using PsiTech.Psionics;
using PsiTech.Training;
using Verse;
using Verse.AI;

namespace PsiTech.Utility {
    public class PsiTechManager : GameComponent {

        private Dictionary<Pawn, PsiTechTracker> trackers = new Dictionary<Pawn, PsiTechTracker>();

        private Dictionary<Thing, PsiTechEquipmentTracker> equipmentTrackers =
            new Dictionary<Thing, PsiTechEquipmentTracker>();

        private List<BuildingPsiTechTrainer> trainers = new List<BuildingPsiTechTrainer>();

        private Dictionary<string, DeadTracker> deadTrackers = new Dictionary<string, DeadTracker>();

        // For ticking
        private List<PsiTechTracker> trackersForTick = new List<PsiTechTracker>();

        // Required for saving dictionary
        private List<Pawn> pawnsForScribe;
        private List<PsiTechTracker> trackersForScribe;
        private List<Thing> thingsForScribe;
        private List<PsiTechEquipmentTracker> equipmentTrackersForScribe;
        private List<string> thingIdsForScribe;
        private List<DeadTracker> deadTrackersForScribe;

        // For giving IDs
        private int nextTrackerId;
        private int nextAbilityId;

        public PsiTechManager(Game game) {
            ExtensionMethods.Manager = this;

            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                PsiTechMapTargetPawnsUtility.TargetPawnUtilities.Clear();
#if VER13
                BlindnessHelper.ClearCache();
#endif
            }
        }

        public PsiTechTracker this[Pawn pawn] {
            get {
                if (pawn == null) return null;
                if (trackers.TryGetValue(pawn, out var tracker)) return tracker;

                tracker = new PsiTechTracker(pawn, GetNextTrackerId());
                trackers.Add(pawn, tracker);
                tracker.InitializeCaches();
                tracker.Notify_EssenceDirty();
                return tracker;
            }
        }

        public int GetNextTrackerId() {
            return nextTrackerId++;
        }

        public int GetNextAbilityId() {
            return nextAbilityId++;
        }

        public PsiTechEquipmentTracker this[Thing thing] {
            get {
                if (thing == null) return null;
                if (equipmentTrackers.TryGetValue(thing, out var tracker)) return tracker;

                tracker = new PsiTechEquipmentTracker(thing);
                equipmentTrackers.Add(thing, tracker);
                return tracker;
            }
        }

        public void RegisterTrainer(BuildingPsiTechTrainer building) {
            if (!trainers.Contains(building)) {
                trainers.Add(building);
            }
        }

        public void UnregisterTrainer(BuildingPsiTechTrainer building) {
            if (trainers.Contains(building)) {
                trainers.Remove(building);
            }
        }

        public BuildingPsiTechTrainer GetOpenTrainerForPawn(Pawn pawn) {
            return trainers.Find(trainer =>
                trainer.IsOperating && trainer.InnerPawn == null && pawn.CanReserve((LocalTargetInfo)trainer));
        }

        public override void GameComponentTick() {
            foreach (var tracker in trackersForTick) {
                tracker.TrackerTick();
            }

            trainers.ForEach(trainer => trainer.BuildingTick());
        }

        public void Notify_PawnAwakened(PsiTechTracker tracker) {
            trackersForTick.Add(tracker);
        }

        public void Notify_EssenceCostsChanged() {
            foreach (var tracker in trackers) {
                tracker.Value.Notify_EssenceDirty();
            }
        }

        public void Notify_PawnDied(Pawn pawn) {
            if (!trackers.TryGetValue(pawn, out var tracker)) return;

            trackersForTick.Remove(tracker);
        }
        
        public void Notify_PawnResurrected(Pawn pawn) {
            if (!deadTrackers.TryGetValue(pawn.ThingID, out var deadTracker)) return;
            
            // There's a dead tracker stored for this pawn, add it back to the active tracker set
            deadTracker.Tracker.AttachToPawn(pawn);
            if (trackers.ContainsKey(pawn)) { // Pawn already has a blank tracker, replace
                trackers[pawn] = deadTracker.Tracker;
            }
            else { // No tracker, add ours
                trackers.Add(pawn, deadTracker.Tracker);
            }
            deadTracker.Tracker.InitializeCaches();
            deadTracker.Tracker.Notify_EssenceDirty();
            Notify_PawnAwakened(deadTracker.Tracker);
            
            // Clean out this entry from the dead trackers dictionary
            deadTrackers.Remove(pawn.ThingID);
        }

        public override void ExposeData() {

            // The other nuclear option - clear the dictionaries and lists before a load
            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                trackers.Clear();
                equipmentTrackers.Clear();
                deadTrackers.Clear();
                pawnsForScribe?.Clear();
                trackersForScribe?.Clear();
                thingsForScribe?.Clear();
                equipmentTrackersForScribe?.Clear();
                thingIdsForScribe?.Clear();
                deadTrackersForScribe?.Clear();
            }

            // Find any trackers that are associated with now-dead pawns, and move them to the dead dictionary if a
            // corpse exists. The corpse determines when we should stop saving the tracker - if there's no corpse,
            // the pawn can't be revived.
            if (Scribe.mode == LoadSaveMode.Saving) {
                foreach (var entry in trackers) {
                    if (entry.Key?.Corpse == null || !entry.Value.Activated) continue;

                    var deadTracker = new DeadTracker {
                        Corpse = entry.Key.Corpse,
                        Tracker = entry.Value
                    };
                    // Remove all pawn references before we store this tracker in the dead dictionary
                    deadTracker.Tracker.DetachFromPawn();
                    deadTrackers.Add(entry.Key.ThingID, deadTracker);
                }
            }

            // Clean up dictionaries before saving + "the nuclear option" - eliminate all trackers that aren't activated
            trackers.RemoveAll(entry => (entry.Key?.Destroyed ?? true) || !entry.Value.Activated);

            // Various checks to make sure we only save things that are load referencable. Some things aren't deep-saved
            // elsewhere, but we can't know what.
            equipmentTrackers.RemoveAll(entry =>
                (entry.Key?.Destroyed ?? true) || !entry.Value.IsPsychic || !(entry.Key is ThingWithComps));
            
            // Clean out any dead trackers where the corpse is destroyed or null.
            if (Scribe.mode == LoadSaveMode.Saving) {
                deadTrackers.RemoveAll(entry => entry.Value.Corpse?.Destroyed ?? true);
            }

            Scribe_Collections.Look(ref trackers, "Trackers", LookMode.Reference, LookMode.Deep, ref pawnsForScribe,
                ref trackersForScribe);
            Scribe_Collections.Look(ref equipmentTrackers, "Equipment", LookMode.Reference, LookMode.Deep,
                ref thingsForScribe, ref equipmentTrackersForScribe);
            Scribe_Collections.Look(ref deadTrackers, "DeadTrackers", LookMode.Value, LookMode.Deep,
                ref thingIdsForScribe, ref deadTrackersForScribe);

            Scribe_Values.Look(ref nextTrackerId, "nextTrackerId");
            Scribe_Values.Look(ref nextAbilityId, "nextAbilityId");

            // A sort of save recovery thing if all our fields are lost, which can happen sometimes for unknown reason,
            // likely related to conflicts of some kind breaking the saving process.
            if (trackers == null) {
                trackers = new Dictionary<Pawn, PsiTechTracker>();
                equipmentTrackers ??= new Dictionary<Thing, PsiTechEquipmentTracker>();
                deadTrackers ??= new Dictionary<string, DeadTracker>();
                nextTrackerId = 0;
                nextAbilityId = 0;
                Log.Error("PsiTech has recovered from catastrophic data loss. All psion and equipment data has been " +
                          "lost. The save has been corrupted, try loading autosaves if available.");
            }

            // Scrub any disappearing pawns for safety
            // Add all awakened pawns to the tick list after load
            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                trackers.RemoveAll(entry => entry.Key == null);

                foreach (var entry in trackers.Where(entry => entry.Value.Activated)) {
                    trackersForTick.Add(entry.Value);
                }
            }
        }
        
        private class DeadTracker : IExposable {
            public Corpse Corpse;
            public PsiTechTracker Tracker;

            public void ExposeData() {
                Scribe_References.Look(ref Corpse, "Corpse");
                Scribe_Deep.Look(ref Tracker, "Tracker");
            }
        }
    }
}