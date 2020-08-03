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
using PsiTech.Interface;
using UnityEngine;
using Verse;

namespace PsiTech.Utility {
    public class PsiTechSettings : ModSettings {

        //Use PsiTechSettings.Get().setting to refer to settings
        public bool EnablePsychicFactionRaids = true;
        public float EssenceLossMultiplier => essenceLossMultiplierInternal / 100f;
        public float TrainingSpeedMultiplier => trainingSpeedMultiplierInternal / 100f;
        public bool PatchAllRaces;

        private const float DefaultEssenceLoss = 0.1f;
        public static readonly Dictionary<HediffDef, float> EssenceLossesPerPart = new Dictionary<HediffDef, float>();
        private static Dictionary<string, float> _essenceLossesForSaving = new Dictionary<string, float>();

        private int essenceLossMultiplierInternal = 100;
        private int trainingSpeedMultiplierInternal = 100;

        private string essenceLossBuffer;
        private string trainingSpeedMultiplierBuffer;
        
        private const string EnablePsychicFactionRaidsKey = "PsiTech.Utility.EnablePsychicFactionRaids";
        private const string EssenceLossMultiplierKey = "PsiTech.Utility.EssenceLossMultiplier";
        private const string EssenceLossMultiplierDescKey = "PsiTech.Utility.EssenceLossMultiplierDesc";
        private const string EssenceLossConfigurationKey = "PsiTech.Utility.EssenceLossConfiguration";
        private const string TrainingSpeedMultiplierKey = "PsiTech.Utility.TrainingSpeedMultiplier";
        private const string TrainingSpeedMultiplierDescKey = "PsiTech.Utility.TrainingSpeedMultiplierDesc";
        private const string PatchAllRacesKey = "PsiTech.Utility.PatchAllRaces";
        private const string PatchAllRacesDescKey = "PsiTech.Utility.PatchAllRacesDesc";
        
        public static PsiTechSettings Get() {
            return LoadedModManager.GetMod<PsiTech>().GetSettings<PsiTechSettings>();
        }

        public void DoWindowContents(Rect rect) {
            var options = new Listing_Standard();
            options.Begin(rect);

            // Psychic raids
            options.CheckboxLabeled(EnablePsychicFactionRaidsKey.Translate(), ref EnablePsychicFactionRaids);
            
            options.GapLine();

            // Essence loss
            options.Label(EssenceLossMultiplierKey.Translate(), -1f, EssenceLossMultiplierDescKey.Translate());
            var oldMult = essenceLossMultiplierInternal;
            options.TextFieldNumeric(ref essenceLossMultiplierInternal, ref essenceLossBuffer, 0, 1000);
            if (essenceLossMultiplierInternal != oldMult) {
                Current.Game.GetComponent<PsiTechManager>().Notify_EssenceCostsChanged();
            }

            if (options.ButtonText(EssenceLossConfigurationKey.Translate())) {
                Find.WindowStack.Add(new EssenceConfigurationWindow());
            }
            
            options.GapLine();
            
            // Training speed
            options.Label(TrainingSpeedMultiplierKey.Translate(), -1f, TrainingSpeedMultiplierDescKey.Translate());
            options.TextFieldNumeric(ref trainingSpeedMultiplierInternal, ref trainingSpeedMultiplierBuffer, 1, 1000);
            
            options.GapLine();
            
            // Patch races
            options.CheckboxLabeled(PatchAllRacesKey.Translate(), ref PatchAllRaces, PatchAllRacesDescKey.Translate());
            
            options.End();
        }
		
        public override void ExposeData() {
            // Copy essence losses dictionary to string saving dictionary
            // This is so that if a HediffDef is removed, we can keep the setting in case the mod that added it is added
            // back into the game
            if (Scribe.mode == LoadSaveMode.Saving) {
                CopyToSavingDictionary();
            }
            
            Scribe_Values.Look(ref EnablePsychicFactionRaids, "EnablePsychicFactionRaids", true);
            Scribe_Values.Look(ref essenceLossMultiplierInternal, "essenceLossMultiplierInternal", 100);
            Scribe_Values.Look(ref trainingSpeedMultiplierInternal, "trainingSpeedMultiplierInternal", 100);
            Scribe_Values.Look(ref PatchAllRaces, "PatchAllRaces");
            Scribe_Collections.Look(ref _essenceLossesForSaving, "_essenceLossesForSaving", LookMode.Value,
                LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                InitializeEssenceLossesDatabase();
            }
        }

        public void InitializeEssenceLossesDatabase() {
            if (!(_essenceLossesForSaving?.Any() ?? false)) { // Create new essence losses dictionary
                ResetEssenceLosses();
                CopyToSavingDictionary();
            }
            else { 
                // Parse strings from saved essence losses collection and insert them into the working dictionary 
                // Iterate over the entire database in case hediffs have been added
                EssenceLossesPerPart.Clear();
                foreach (var def in DefDatabase<HediffDef>.AllDefsListForReading) {
                    if (!_essenceLossesForSaving.TryGetValue(def.defName, out var value)) {
                        value = DefIsArtificial(def) ? DefaultEssenceLoss : 0f;
                    }
                    EssenceLossesPerPart.Add(def, value);
                }
            }
        }

        public static float GetPenaltyForPart(HediffDef def) {
            if (!EssenceLossesPerPart.TryGetValue(def, out var value)) {
                value = DefIsArtificial(def) ? DefaultEssenceLoss : 0f;
                EssenceLossesPerPart.Add(def, value);
            }

            return value;
        }

        public static void ResetEssenceLosses() {
            EssenceLossesPerPart.Clear();
            foreach (var def in DefDatabase<HediffDef>.AllDefsListForReading) {
                var value = DefIsArtificial(def) ? DefaultEssenceLoss : 0f;
                EssenceLossesPerPart.Add(def, value);
            }
        }

        private static void CopyToSavingDictionary() {
            _essenceLossesForSaving ??= new Dictionary<string, float>();
            foreach (var entry in EssenceLossesPerPart) {
                if (_essenceLossesForSaving.ContainsKey(entry.Key.defName)) {
                    _essenceLossesForSaving[entry.Key.defName] = entry.Value;
                }
                else {
                    _essenceLossesForSaving.Add(entry.Key.defName, entry.Value);
                }
            }
        }

        private static bool DefIsArtificial(HediffDef def) {
            return typeof(Hediff_Implant).IsAssignableFrom(def.hediffClass);
        }
        
    }
}