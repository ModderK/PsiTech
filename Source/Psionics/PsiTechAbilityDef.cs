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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PsiTech.AbilityEffects;
using PsiTech.AutocastManagement;
using PsiTech.TargetValidators;
using PsiTech.Utility;
using RimWorld;
using Verse;

namespace PsiTech.Psionics {
    public class PsiTechAbilityDef : Def {

        public Type AbilityClass;

        public string GizmoDesc;
        public string PathToIcon;

        public List<ResearchProjectDef> RequiredResearch = new List<ResearchProjectDef>();
        public List<PsiTechAbilityDef> RequiredAbilities = new List<PsiTechAbilityDef>();
        public List<PsiTechAbilityDef> ConflictingAbilities = new List<PsiTechAbilityDef>();
        public int Tier;
        public float TrainingTimeDays;
        public bool Autocastable = false;
        public bool Violent = false;

        // Stats that get shown
        public float EnergyPerUse = 0;
        public float EnergyPerSecondActive = 0;
        public float Range = 0;
        public float CooldownSeconds = 0;
        public float BaseSuccessChance = 0;
        
        public List<StatModifier> StatOffsets = new List<StatModifier>();
        public List<StatModifier> StatFactors = new List<StatModifier>();
        
        public List<AbilityEffect> PossibleEffects = new List<AbilityEffect>();

        public List<PawnCapacityModifier> CapMods = new List<PawnCapacityModifier>();
        
        // "Internal" stuff
        public bool AlwaysHits = false;
        public float BaseSkillTransfer = 0;
        public SoundDef SoundDefSuccessOnCaster;
        public SoundDef SoundDefSuccessOnTarget;
        public SoundDef SoundDefFailure;
        public ThingDef MoteOnTarget;
        public ThingDef MoteOnUserSuccess;
        public ThingDef MoteOnUserFailure;
        public ThingDef MoteSuccessPointer;
        public ThingDef MoteLink;
        public int LinkPulseTicks = 0;
        public int CastTimeTicks = 0;
        public SimpleCurve AdditionalDifficultyCurve;
        public TargetValidator TargetValidator = new TargetValidatorAnyButUser();
        public Type AutocastFilterClass;
        public FilterTargetType DefaultFilterTargetType = FilterTargetType.Enemies;
        public float AddedValueForThreat;
        public float AbilityCostForRaid;
        
        // For triggered abilities
        public TriggerType Trigger;
        public TargetType Target; 
        
        private const string RequiredResearchKey = "PsiTech.Psionics.RequiredResearch";
        private const string RequiredResearchDescKey = "PsiTech.Psionics.RequiredResearchDesc";
        private const string RequiredAbilitiesKey = "PsiTech.Psionics.RequiredAbility";
        private const string RequiredAbilitiesDescKey = "PsiTech.Psionics.RequiredAbilityDesc";
        private const string ConflictingAbilitiesKey = "PsiTech.Psionics.ConflictingAbility";
        private const string ConflictingAbilitiesDescKey = "PsiTech.Psionics.ConflictingAbilityDesc";
        private const string TrainingTimeDaysKey = "PsiTech.Psionics.TrainingTimeDays";
        private const string TrainingTimeDaysDescKey = "PsiTech.Psionics.TrainingTimeDaysDesc";
        private const string EnergyPerUseKey = "PsiTech.Psionics.EnergyPerUse";
        private const string EnergyPerUseDescKey = "PsiTech.Psionics.EnergyPerUseDesc";
        private const string EnergyPerSecondActiveKey = "PsiTech.Psionics.EnergyPerSecondActive";
        private const string EnergyPerSecondActiveDescKey = "PsiTech.Psionics.EnergyPerSecondActiveDesc";
        private const string RangeKey = "PsiTech.Psionics.Range";
        private const string RangeDescKey = "PsiTech.Psionics.RangeDesc";
        private const string CooldownSecondsKey = "PsiTech.Psionics.CooldownSeconds";
        private const string CooldownSecondsDescKey = "PsiTech.Psionics.CooldownSecondsDesc";
        private const string BaseSuccessChanceKey = "PsiTech.Psionics.BaseSuccessChance";
        private const string BaseSuccessChanceDescKey = "PsiTech.Psionics.BaseSuccessChanceDesc";
        private const string EffectProbabilityKey = "PsiTech.Psionics.EffectProbability";

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req) {

            if (!RequiredResearch.NullOrEmpty()) {
                foreach (var project in RequiredResearch) {
                    yield return new StatDrawEntry(StatCategoryDefOf.Basics, RequiredResearchKey.Translate(),
                        project.LabelCap, RequiredResearchDescKey.Translate(), 4);
                }
            }
            
            if (!RequiredAbilities.NullOrEmpty()) {
                foreach (var ability in RequiredAbilities) {
                    yield return new StatDrawEntry(StatCategoryDefOf.Basics, RequiredAbilitiesKey.Translate(),
                        ability.LabelCap, RequiredAbilitiesDescKey.Translate(), 3);
                }
            }

            if (!ConflictingAbilities.NullOrEmpty()) {
                foreach (var ability in ConflictingAbilities) {
                    yield return new StatDrawEntry(StatCategoryDefOf.Basics, ConflictingAbilitiesKey.Translate(),
                        ability.LabelCap, ConflictingAbilitiesDescKey.Translate(), 2);
                }
            }

            yield return new StatDrawEntry(StatCategoryDefOf.Basics, TrainingTimeDaysKey.Translate(),
                TrainingTimeDays.ToString(CultureInfo.InvariantCulture), TrainingTimeDaysDescKey.Translate(), 1);

            if (EnergyPerUse != 0) {
                yield return new StatDrawEntry(PsiTechDefOf.PTAbilityStats, EnergyPerUseKey.Translate(),
                    EnergyPerUse.ToString(CultureInfo.InvariantCulture), EnergyPerUseDescKey.Translate(), 99);
            }

            if (EnergyPerSecondActive != 0) {
                yield return new StatDrawEntry(PsiTechDefOf.PTAbilityStats, EnergyPerSecondActiveKey.Translate(),
                    EnergyPerSecondActive.ToString(CultureInfo.InvariantCulture), EnergyPerSecondActiveDescKey.Translate(), 98);
            }

            if (Range != 0) {
                yield return new StatDrawEntry(PsiTechDefOf.PTAbilityStats, RangeKey.Translate(),
                    Range.ToString(CultureInfo.InvariantCulture), RangeDescKey.Translate(), 97);
            }

            if (CooldownSeconds != 0) {
                yield return new StatDrawEntry(PsiTechDefOf.PTAbilityStats, CooldownSecondsKey.Translate(),
                    CooldownSeconds.ToString(CultureInfo.InvariantCulture), CooldownSecondsDescKey.Translate(), 96);
            }
            
            if (BaseSuccessChance != 0) {
                yield return new StatDrawEntry(PsiTechDefOf.PTAbilityStats, BaseSuccessChanceKey.Translate(),
                    BaseSuccessChance.ToStringPercent(), BaseSuccessChanceDescKey.Translate(), 95);
            }

            if (!PossibleEffects.NullOrEmpty()) {
                foreach (var effect in PossibleEffects) {
                    yield return new StatDrawEntry(PsiTechDefOf.PTEffects, effect.Title, "",
                        GenerateEffectValueString(effect), (int) Math.Round(effect.Weight * 10));
                }
            }

            if (!StatOffsets.NullOrEmpty()) {
                foreach (var offset in StatOffsets) {
                    yield return new StatDrawEntry(PsiTechDefOf.PTStatOffsets, offset.stat.LabelCap,
                        offset.value.ToStringByStyle(offset.stat.ToStringStyleUnfinalized, ToStringNumberSense.Offset),
                        offset.stat.description, 0);
                }
            }
            
            if (!StatFactors.NullOrEmpty()) {
                foreach (var factor in StatFactors) {
                    yield return new StatDrawEntry(PsiTechDefOf.PTStatFactors, factor.stat, factor.value,
                        StatRequest.ForEmpty(), ToStringNumberSense.Factor);
                }
            }

            if (!CapMods.NullOrEmpty()) {
                foreach (var mod in CapMods) {
                    if (mod.offset != 0)
                        yield return new StatDrawEntry(PsiTechDefOf.PTCapMods, mod.capacity.LabelCap,
                            "+" + mod.offset.ToStringPercent(), mod.capacity.description, 0);
                    if (mod.postFactor != 1)
                        yield return new StatDrawEntry(PsiTechDefOf.PTCapMods, mod.capacity.LabelCap,
                            "x" + mod.postFactor.ToStringPercent(), mod.capacity.description, 0);
                }
            }
        }

        private string GenerateEffectValueString(AbilityEffect effect) {
            var sb = new StringBuilder();
            sb.AppendLine(effect.Title);
            sb.AppendLine();
            sb.AppendLine(effect.Description);
            sb.AppendLine();
            sb.AppendLine(EffectProbabilityKey.Translate(GetProbabilityOfEffect(effect).ToStringPercent()));
            sb.AppendLine();
            sb.AppendLine(effect.ExtraListingString());
            
            return sb.ToString();
        }

        private float GetProbabilityOfEffect(AbilityEffect effect) {
            return effect.Weight / PossibleEffects.Sum(possible => possible.Weight);
        }
    }
}