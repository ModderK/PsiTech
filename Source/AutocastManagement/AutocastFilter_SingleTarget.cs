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
using System.Linq;
using PsiTech.Utility;
using UnityEngine;
using Verse;

namespace PsiTech.AutocastManagement {
    public class AutocastFilter_SingleTarget : AutocastFilter {

        public float MinSuccessChance;
        private string minSuccessBuffer;

        public AutocastFilterSelector Selector = new AutocastFilterSelector_Closest {
            Def = DefDatabase<AutocastFilterSelectorDef>.AllDefsListForReading.Find(def =>
                def.SelectorClass == typeof(AutocastFilterSelector_Closest))
        };

        public bool InvertSelector;
        private List<AdditionalTargetFilter> additionalFilters = new List<AdditionalTargetFilter>();
        private List<AdditionalTargetFilter> filtersToRemove = new List<AdditionalTargetFilter>();
        private List<AdditionalTargetFilterDef> filtersForAdd = new List<AdditionalTargetFilterDef>();
        private Vector2 scrollPosition;
        private int lastFiltersCount;

        private const string MinimumSuccessChanceKey = "PsiTech.AutocastManagement.MinimumSuccessChance";
        private const string TargetRangeKey = "PsiTech.AutocastManagement.TargetRange";
        private const string TargetSelectorKey = "PsiTech.AutocastManagement.TargetSelector";
        private const string FilterTargetTypeKey = "PsiTech.AutocastManagement.FilterTargetType";
        private const string InvertKey = "PsiTech.AutocastManagement.Invert";
        private const string AdditionalTargetFiltersKey = "PsiTech.Interface.AdditionalTargetFilters";
        private const string AddAdditionalTargetFilterKey = "PsiTech.Interface.AddAdditionalTargetFilter";

        private const float MinimumSuccessLabelWidth = 180f;
        private const float MinimumSuccessFillableWidth = 100f;
        
        private const float TargetRangeLabelWidth = 180f;
        private const float TargetRangeFillableWidth = 100f;

        private const float TargetTypeLabelWidth = 100f;
        private const float TargetTypeDropdownWidth = 120f;

        private const float TargetSelectorLabelWidth = 100f;
        private const float TargetSelectorSelectionWidth = 200f;
        
        private const float InvertWidth = 40f;
        private const float InvertCheckSize = 20f;
        
        private const float FilterTitleHeight = 25f;
        private const float AddFilterButtonWidth = 200f;

        public AutocastFilter_SingleTarget() {
            RebuildFilterCache();
        }
        
        public override Pawn GetBestTarget(List<Pawn> targets) {
            targets.RemoveAll(target => !TargetRange.Contains(target.Position.DistanceTo(User.Position)));
            targets.RemoveAll(target => Ability.SuccessChanceOnTarget(target) < MinSuccessChance);
            FilterForTargetType(ref targets);
            targets.RemoveAll(target => additionalFilters.Any(filter => !filter.TargetMeetsFilter(target)));

            return targets.Any() ? Selector.SelectBestTarget(User, targets, Ability, InvertSelector) : null;
        }

        public override void Draw(Rect inRect) {
            
            // Remove filters that are marked for removal
            filtersToRemove.ForEach(filter => additionalFilters.Remove(filter));
            filtersToRemove.Clear();
            
            // Rebuild cache if needed
            if (additionalFilters.Count != lastFiltersCount) {
                RebuildFilterCache();
                lastFiltersCount = additionalFilters.Count;
            }
            
            
            Widgets.DrawBoxSolid(new Rect(inRect.x, inRect.y, inRect.width, 113f), new Color(21f/256f, 25f/256f, 29f/256f));

            var drawBox = inRect.ContractedBy(5f);
            
            var xAnchor = drawBox.x;
            var yAnchor = drawBox.y;
            
            // Draw minimum success chance
            Widgets.Label(new Rect(xAnchor, yAnchor, MinimumSuccessLabelWidth, OptionHeight), MinimumSuccessChanceKey.Translate());
            xAnchor += MinimumSuccessLabelWidth + XSeparation;
            Widgets.TextFieldPercent(new Rect(xAnchor, yAnchor, MinimumSuccessFillableWidth, OptionHeight), ref MinSuccessChance, ref minSuccessBuffer);

            xAnchor = drawBox.x;
            yAnchor += OptionHeight + YSeparation;
            
            // Draw target range
            Widgets.Label(new Rect(xAnchor, yAnchor, TargetRangeLabelWidth, OptionHeight), TargetRangeKey.Translate());
            xAnchor += TargetRangeLabelWidth + XSeparation;
            Widgets.IntRange(new Rect(xAnchor, yAnchor, TargetRangeFillableWidth, OptionHeight), GetHashCode(), ref TargetRange, 0,
                Mathf.CeilToInt(Ability.Def.Range));

            xAnchor = drawBox.x;
            yAnchor += OptionHeight + YSeparation;
            
            // Draw target type dropdown
            Widgets.Label(new Rect(xAnchor, yAnchor, TargetTypeLabelWidth, OptionHeight), FilterTargetTypeKey.Translate());
            xAnchor += TargetTypeLabelWidth + XSeparation;
            Widgets.Dropdown(new Rect(xAnchor, yAnchor, TargetTypeDropdownWidth, OptionHeight), FilterTargetType,
                type => type.ToString(), priority => GenerateTargetTypeOptions(),
                LocalizeTargetType(FilterTargetType));
            
            xAnchor = drawBox.x;
            yAnchor += OptionHeight + YSeparation;
            
            // Draw target selector dropdown
            Widgets.Label(new Rect(xAnchor, yAnchor, TargetSelectorLabelWidth, OptionHeight), TargetSelectorKey.Translate());
            xAnchor += TargetSelectorLabelWidth + XSeparation;
            
            if (Widgets.ButtonText(new Rect(xAnchor, yAnchor, TargetSelectorSelectionWidth, OptionHeight), Selector.Def.LabelCap)) {
                var options = new List<FloatMenuOption>();
                foreach (var condition in DefDatabase<AutocastFilterSelectorDef>.AllDefsListForReading) {
                    options.Add(GenerateSelectorOption(condition));
                }

                if (!options.NullOrEmpty()) {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }

            xAnchor += TargetSelectorSelectionWidth + XSeparation;
            Widgets.Label(new Rect(xAnchor, yAnchor, InvertWidth, OptionHeight), InvertKey.Translate());
            xAnchor += InvertWidth + XSeparation;
            Widgets.Checkbox(xAnchor, yAnchor + (OptionHeight - InvertCheckSize) / 2, ref InvertSelector, InvertCheckSize);

            // Draw additional filters label and add button
            xAnchor = drawBox.x;
            yAnchor += OptionHeight + 2 * YSeparation;
            
            Text.Anchor = TextAnchor.MiddleLeft;
            
            Widgets.Label(new Rect(xAnchor, yAnchor, drawBox.width, FilterTitleHeight), AdditionalTargetFiltersKey.Translate());

            // Add filter button
            if (Widgets.ButtonText(
                new Rect(drawBox.xMax - AddFilterButtonWidth, yAnchor, AddFilterButtonWidth, FilterTitleHeight),
                AddAdditionalTargetFilterKey.Translate())) {
                var options = new List<FloatMenuOption>();
                foreach (var filter in filtersForAdd) {
                    options.Add(GenerateAdditionalFilterOption(filter));
                }

                if (!options.NullOrEmpty()) {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                    
            }

            yAnchor += FilterTitleHeight + YSeparation;

            var filterBox = new Rect(xAnchor, yAnchor, drawBox.width,
                drawBox.height - 113f - FilterTitleHeight - 2 * YSeparation);
            
            // Find the total needed height and available height, see if we need to draw a scroll bar
            var needed = additionalFilters.Sum(condition => condition.Height + YSeparation);
            var available = filterBox.height;

            if (needed > available) { // Need to draw a scrollbar
                var outRect = new Rect(xAnchor, yAnchor, filterBox.width, available);
                var viewRect = new Rect(0, 0, filterBox.width - 16f, needed);
                var scrollAnchor = 0f;
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
                foreach (var filter in additionalFilters) {
                    var height = filter.Height;
                    filter.Draw(new Rect(0, scrollAnchor, filterBox.width - 21f, height), this);
                    scrollAnchor += height + YSeparation;
                }
                Widgets.EndScrollView();
            }
            else { // No need to draw a scrollbar
                foreach (var filter in additionalFilters) {
                    var height = filter.Height;
                    filter.Draw(new Rect(xAnchor, yAnchor, filterBox.width, height), this);
                    yAnchor += height + YSeparation;
                }
            }
            
        }

        public void MarkAdditionalFilterForRemoval(AdditionalTargetFilter filter) {
            filtersToRemove.Add(filter);
        }

        public void AddAdditionalFilter(AdditionalTargetFilter filter) {
            additionalFilters.Add(filter);
        }
        
        private FloatMenuOption GenerateAdditionalFilterOption(AdditionalTargetFilterDef entry) {
            void Action() {
                if (!(Activator.CreateInstance(entry.FilterClass) is AdditionalTargetFilter instance)) {
                    Log.Error("PsiTech tried to instantiate an AdditionalTargetFilter of type " + entry.FilterClass +
                              " and failed. This indicates a misconfigured additional filter def.");
                }
                else {
                    instance.User = User;
                    instance.Ability = Ability;
                    instance.Def = entry;
                    additionalFilters.Add(instance);
                    RebuildFilterCache();
                }
            }

            var floatMenuOption = new FloatMenuOption(entry.label, Action) {
                extraPartWidth = 29f,
                extraPartOnGUI = rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, entry)
            };
            
            return floatMenuOption;
        }
        
        private void RebuildFilterCache() {
            filtersForAdd = DefDatabase<AdditionalTargetFilterDef>.AllDefsListForReading.ListFullCopy();
            filtersForAdd.RemoveAll(condition =>
                additionalFilters.Any(existing => existing.GetType() == condition.FilterClass));
        }

        private FloatMenuOption GenerateSelectorOption(AutocastFilterSelectorDef entry) {
            void Action() {
                if (!(Activator.CreateInstance(entry.SelectorClass) is AutocastFilterSelector instance)) {
                    Log.Error("PsiTech tried to instantiate an AutocastFilterSelector of type " + entry.SelectorClass +
                              " and failed. This indicates a misconfigured selector def.");
                }
                else {
                    instance.Def = entry;
                    Selector = instance;
                }
            }

            var floatMenuOption = new FloatMenuOption(entry.label, Action) {
                extraPartWidth = 29f,
                extraPartOnGUI = rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, entry)
            };
            
            return floatMenuOption;
        }

        protected override void PostExpose() {
            Scribe_Values.Look(ref MinSuccessChance, "MinSuccessChance");
            Scribe_Deep.Look(ref Selector, "Selector");
            Scribe_Values.Look(ref InvertSelector, "InvertSelector");
            Scribe_Collections.Look(ref additionalFilters, "additionalFilters", LookMode.Deep);
            
            // For save compatibility, create the list if null
            additionalFilters ??= new List<AdditionalTargetFilter>();
        }
    }
    
    public enum FilterTargetType {
        Enemies,
        Hostiles,
        Friendlies,
        Any
    }
}