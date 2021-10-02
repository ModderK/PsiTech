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
using PsiTech.Misc;
using PsiTech.Psionics;
using UnityEngine;
using Verse;

namespace PsiTech.Utility {
    public class PsiTechSettings : ModSettings {

        [TweakValue("!PsiTechDebug")]
        public static bool PsiTechDebug;

        // Apparently there's a callback for tweakvalues changing
        private static void PsiTechDebug_Changed() {
            Get().Write();
        }

        //Use PsiTechSettings.Get().setting to refer to settings
        public bool EnablePsychicFactionRaids = true;
        public bool IgnorePsychicFactionRaidConditions = false;
        public float PsychicFactionRaidSizeMultiplier => psychicFactionRaidSizeMultiplierInternal / 100f;
        public float EssenceLossMultiplier => essenceLossMultiplierInternal / 100f;
        public float TrainingSpeedMultiplier => trainingSpeedMultiplierInternal / 100f;
        public bool PatchAllRaces;
        public bool PrisonerCastingDisabled;

        public const float DefaultEssenceLoss = 0.1f;
        public static readonly Dictionary<HediffDef, float> EssenceLossesPerPart = new Dictionary<HediffDef, float>();
        private static Dictionary<string, float> _essenceLossesForSaving = new Dictionary<string, float>();

        public static Dictionary<PsiTechAbilityDef, bool> DisabledEnemyAbilities =
            new Dictionary<PsiTechAbilityDef, bool>();
        
        private int psychicFactionRaidSizeMultiplierInternal = 100;
        private int essenceLossMultiplierInternal = 100;
        private int trainingSpeedMultiplierInternal = 100;

        private string raidSizeBuffer;
        private string essenceLossBuffer;
        private string trainingSpeedMultiplierBuffer;
        
        private const string EnablePsychicFactionRaidsKey = "PsiTech.Utility.EnablePsychicFactionRaids";
        private const string IgnorePsychicFactionRaidConditionsKey =
            "PsiTech.Utility.IgnorePsychicFactionRaidConditions";
        private const string IgnorePsychicFactionRaidConditionsDescKey =
            "PsiTech.Utility.IgnorePsychicFactionRaidConditionsDesc";
        private const string PsychicFactionRaidSizeMultiplierKey = "PsiTech.Utility.PsychicFactionRaidSizeMultiplier";
        private const string PsychicFactionRaidSizeMultiplierDescKey =
            "PsiTech.Utility.PsychicFactionRaidSizeMultiplierDesc";
        private const string ConfigureEnemyAbilitiesKey = "PsiTech.Utility.ConfigureEnemyAbilities";
        private const string EssenceLossMultiplierKey = "PsiTech.Utility.EssenceLossMultiplier";
        private const string EssenceLossMultiplierDescKey = "PsiTech.Utility.EssenceLossMultiplierDesc";
        private const string EssenceLossConfigurationKey = "PsiTech.Utility.EssenceLossConfiguration";
        private const string TrainingSpeedMultiplierKey = "PsiTech.Utility.TrainingSpeedMultiplier";
        private const string TrainingSpeedMultiplierDescKey = "PsiTech.Utility.TrainingSpeedMultiplierDesc";
        private const string PatchAllRacesKey = "PsiTech.Utility.PatchAllRaces";
        private const string PatchAllRacesDescKey = "PsiTech.Utility.PatchAllRacesDesc";
        private const string DisablePrisonerCastingKey = "PsiTech.Utility.DisablePrisonerCasting";
        private const string DisablePrisonerCastingDescKey = "PsiTech.Utility.DisablePrisonerCastingDesc";
        
        public static PsiTechSettings Get() {
            return LoadedModManager.GetMod<PsiTech>().GetSettings<PsiTechSettings>();
        }

        public void DoWindowContents(Rect rect) {
            var options = new Listing_Standard();
            options.Begin(rect);

            // Psychic raids
            options.CheckboxLabeled(EnablePsychicFactionRaidsKey.Translate(), ref EnablePsychicFactionRaids);

            options.CheckboxLabeled(IgnorePsychicFactionRaidConditionsKey.Translate(),
                ref IgnorePsychicFactionRaidConditions, IgnorePsychicFactionRaidConditionsDescKey.Translate());
            
            // Psychic raid size
            options.Label(PsychicFactionRaidSizeMultiplierKey.Translate(), -1f,
                PsychicFactionRaidSizeMultiplierDescKey.Translate());
            var oldSizeMult = psychicFactionRaidSizeMultiplierInternal;
            options.IntEntry(ref psychicFactionRaidSizeMultiplierInternal, ref raidSizeBuffer);
            psychicFactionRaidSizeMultiplierInternal = Mathf.Clamp(psychicFactionRaidSizeMultiplierInternal, 30, 300);
            raidSizeBuffer = psychicFactionRaidSizeMultiplierInternal.ToString();
            if (psychicFactionRaidSizeMultiplierInternal != oldSizeMult) {
                UpdateRaidSizeMultiplier(PsychicFactionRaidSizeMultiplier);
            }
            
            if (options.ButtonText(ConfigureEnemyAbilitiesKey.Translate())) {
                EnsureAllAbilitiesInitialized();
                Find.WindowStack.Add(new EnemyAbilitiesWindow());
            }
            
            options.GapLine();

            // Essence loss
            options.Label(EssenceLossMultiplierKey.Translate(), -1f, EssenceLossMultiplierDescKey.Translate());
            var oldMult = essenceLossMultiplierInternal;
            options.TextFieldNumeric(ref essenceLossMultiplierInternal, ref essenceLossBuffer, 0, 1000);
            if (essenceLossMultiplierInternal != oldMult) {
                Current.Game?.GetComponent<PsiTechManager>().Notify_EssenceCostsChanged();
            }

            if (options.ButtonText(EssenceLossConfigurationKey.Translate())) {
                EnsureAllHediffsInitialized();
                Find.WindowStack.Add(new EssenceConfigurationWindow());
            }
            
            options.GapLine();
            
            // Training speed
            options.Label(TrainingSpeedMultiplierKey.Translate(), -1f, TrainingSpeedMultiplierDescKey.Translate());
            options.TextFieldNumeric(ref trainingSpeedMultiplierInternal, ref trainingSpeedMultiplierBuffer, 1, 1000);
            
            options.GapLine();
            
            // Patch races
            options.CheckboxLabeled(PatchAllRacesKey.Translate(), ref PatchAllRaces, PatchAllRacesDescKey.Translate());
            
            options.GapLine();
            
            // Disable prisoner casting
            options.CheckboxLabeled(DisablePrisonerCastingKey.Translate(), ref PrisonerCastingDisabled,
                DisablePrisonerCastingDescKey.Translate());
            
            options.End();
        }
		
        public override void ExposeData() {
            // Copy essence losses dictionary to string saving dictionary
            // This is so that if a HediffDef is removed, we can keep the setting in case the mod that added it is added
            // back into the game
            if (Scribe.mode == LoadSaveMode.Saving) {
                CopyToSavingDictionary();
            }
            
            Scribe_Values.Look(ref PsiTechDebug, "PsiTechDebug");
            Scribe_Values.Look(ref EnablePsychicFactionRaids, "EnablePsychicFactionRaids", true);
            Scribe_Values.Look(ref IgnorePsychicFactionRaidConditions, "IgnorePsychicFactionRaidConditions");
            Scribe_Values.Look(ref psychicFactionRaidSizeMultiplierInternal, "psychicFactionRaidSizeMultiplierInternal",
                100);
            Scribe_Values.Look(ref essenceLossMultiplierInternal, "essenceLossMultiplierInternal", 100);
            Scribe_Values.Look(ref trainingSpeedMultiplierInternal, "trainingSpeedMultiplierInternal", 100);
            Scribe_Values.Look(ref PatchAllRaces, "PatchAllRaces");
            Scribe_Values.Look(ref PrisonerCastingDisabled, "PrisonerCastingDisabled");
            Scribe_Collections.Look(ref _essenceLossesForSaving, "_essenceLossesForSaving", LookMode.Value,
                LookMode.Value);
            Scribe_Collections.Look(ref DisabledEnemyAbilities, "DisabledEnemyAbilities", LookMode.Def,
                LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                InitializeEssenceLossesDatabase();
                UpdateRaidSizeMultiplier(PsychicFactionRaidSizeMultiplier);

                DisabledEnemyAbilities ??= new Dictionary<PsiTechAbilityDef, bool>();
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

        private static void UpdateRaidSizeMultiplier(float multiplier) {
            DefDatabase<PsiTechPawnKindDef>.AllDefsListForReading.ForEach(def => def.UpdateCombatPower(multiplier));
        }
        
        public static void ResetEssenceLosses() {
            EssenceLossesPerPart.Clear();
            foreach (var def in DefDatabase<HediffDef>.AllDefsListForReading) {
                var value = DefIsArtificial(def) ? DefaultEssenceLoss : 0f;
                EssenceLossesPerPart.Add(def, value);
            }
        }

        public static void ResetDisabledAbilities() {
            foreach (var entry in DisabledEnemyAbilities.Keys.ToList()) {
                DisabledEnemyAbilities[entry] = false;
            }
        }

        private static void EnsureAllHediffsInitialized() {
            foreach (var def in DefDatabase<HediffDef>.AllDefsListForReading) {
                if (def == null || EssenceLossesPerPart.ContainsKey(def)) continue;

                var value = 0f;
                if (!(_essenceLossesForSaving?.TryGetValue(def.defName, out value) ?? false)) {
                    value = DefIsArtificial(def) ? DefaultEssenceLoss : 0f;
                }
                EssenceLossesPerPart.Add(def, value);
            }
        }

        private static void EnsureAllAbilitiesInitialized() {
            foreach (var def in DefDatabase<PsiTechPawnKindDef>.AllDefsListForReading) {
                if (def?.AbilityPool == null) continue;

                var neededAbilities = def.AbilityPool.Where(ability => !DisabledEnemyAbilities.Keys.Contains(ability));
                
                foreach (var ability in neededAbilities) {
                    DisabledEnemyAbilities.Add(ability, false);
                }
            }

            // Sort to tier, then alphabetical order
            DisabledEnemyAbilities = DisabledEnemyAbilities.OrderBy(entry => entry.Key.Tier)
                .ThenBy(entry => entry.Key.label).ToDictionary(entry => entry.Key, entry => entry.Value);
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