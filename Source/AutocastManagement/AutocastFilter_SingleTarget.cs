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

        private bool anyValidTargets;

        private const string MinimumSuccessChanceKey = "PsiTech.AutocastManagement.MinimumSuccessChance";
        private const string TargetRangeKey = "PsiTech.AutocastManagement.TargetRange";
        private const string TargetSelectorKey = "PsiTech.AutocastManagement.TargetSelector";
        private const string FilterTargetTypeKey = "PsiTech.AutocastManagement.FilterTargetType";
        private const string InvertKey = "PsiTech.AutocastManagement.Invert";

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

        public AutocastFilter_SingleTarget() { }
        
        public override Pawn GetBestTarget(IEnumerable<Pawn> targets) {
            anyValidTargets = false;
            var validTargets = GetValidTargets(targets);
            return anyValidTargets ? Selector.SelectBestTarget(User, validTargets, Ability, InvertSelector) : null;
        }

        // The reason that this is this way is because iterators are very fast - but they can't have out parameters.
        // Instead, we need to provide a place outside the method to pass whether we found a valid target.
        // Why do this at all? It avoids a very expensive ToList call that we would otherwise need to prevent multiple
        // enumeration.
        private IEnumerable<Pawn> GetValidTargets(IEnumerable<Pawn> targets) {
            foreach (var target in targets) {
                if(!TargetRange.Contains(target.Position.DistanceTo(User.Position)) ||
                   Ability.SuccessChanceOnTarget(target) < MinSuccessChance ||
                   !TargetMatchesTargetType(target) ||
                   AdditionalFilters.Any(filter => !filter.TargetMeetsFilter(target))) continue;

                anyValidTargets = true;
                yield return target;
            }
        }

        public override void Draw(Rect inRect) {

            ResolveRemovedFilters();

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

            // Draw additional filters
            xAnchor = drawBox.x;
            yAnchor += OptionHeight + 2 * YSeparation;
            DrawAdditionalFilters(drawBox, xAnchor, yAnchor);

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
        }
    }
}