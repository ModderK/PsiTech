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
using PsiTech.Psionics;
using PsiTech.Training;
using Verse;
using Verse.AI;

namespace PsiTech.Utility {
    public class PsiTechManager : GameComponent {
        
        [TweakValue("!PsiTechDebug")]
        public static bool PsiTechDebug = false;
        
        private Dictionary<Pawn, PsiTechTracker> trackers = new Dictionary<Pawn, PsiTechTracker>();
        private Dictionary<Thing, PsiTechEquipmentTracker> equipmentTrackers = new Dictionary<Thing, PsiTechEquipmentTracker>();

        private List<BuildingPsiTechTrainer> trainers = new List<BuildingPsiTechTrainer>();

        // Required for saving dictionary
        private List<Pawn> pawnsForScribe;
        private List<PsiTechTracker> trackersForScribe;
        private List<Thing> thingsForScribe;
        private List<PsiTechEquipmentTracker> equipmentTrackersForScribe;
        
        // For giving IDs
        private int nextTrackerId;
        private int nextAbilityId;

        public PsiTechManager(Game game) {
            ExtensionMethods.Manager = this;

            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                PsiTechMapTargetPawnsUtility.TargetPawnUtilities.Clear();
            }
        }

        public PsiTechTracker this[Pawn pawn] {
            get {
                if (pawn == null) return null;
                if (trackers.TryGetValue(pawn, out var tracker)) return tracker;
                
                tracker = new PsiTechTracker(pawn, GetNextTrackerId());
                trackers.Add(pawn, tracker);
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
            return trainers.Find(trainer => trainer.IsOperating && trainer.InnerPawn == null && pawn.CanReserve((LocalTargetInfo)trainer));
        }

        public override void GameComponentTick() {
            foreach (var entry in trackers) {
                if (!entry.Value.Activated) continue;
                
                entry.Value.TrackerTick();
            }
            trainers.FindAll(trainer => trainer.Spawned).ForEach(trainer => trainer.BuildingTick());
        }

        public override void ExposeData() {
            
            // The other nuclear option - clear the dictionaries and lists before a load
            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                trackers.Clear();
                equipmentTrackers.Clear();
                pawnsForScribe?.Clear();
                trackersForScribe?.Clear();
                thingsForScribe?.Clear();
                equipmentTrackersForScribe?.Clear();
            }
            
            // Clean up dictionaries before saving + "the nuclear option" - eliminate all trackers that aren't activated
            trackers.RemoveAll(entry => (entry.Key?.Destroyed ?? true) || !entry.Value.Activated);
            
            // Various checks to make sure we only save things that are load referencable. Some things aren't deep-saved
            // elsewhere, but we can't know what.
            equipmentTrackers.RemoveAll(entry =>
                (entry.Key?.Destroyed ?? true) || !entry.Value.IsPsychic || !(entry.Key is ThingWithComps thing) ||
                thing.TryGetComp<CompEquippable>() == null);

            Scribe_Collections.Look(ref trackers, "Trackers", LookMode.Reference, LookMode.Deep, ref pawnsForScribe,
                ref trackersForScribe);
            Scribe_Collections.Look(ref equipmentTrackers, "Equipment", LookMode.Reference, LookMode.Deep,
                ref thingsForScribe, ref equipmentTrackersForScribe);
            
            Scribe_Values.Look(ref nextTrackerId, "nextTrackerId");
            Scribe_Values.Look(ref nextAbilityId, "nextAbilityId");
        }
    }
}