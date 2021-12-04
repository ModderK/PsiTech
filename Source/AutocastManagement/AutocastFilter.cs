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
using PsiTech.Psionics;
using RimWorld;
using UnityEngine;
using Verse;

namespace PsiTech.AutocastManagement {
    public abstract class AutocastFilter : IExposable {
        
        public PsiTechAbility Ability;
        public Pawn User;
        public FilterTargetType FilterTargetType;
        public IntRange TargetRange;
        
        private const string FilterTargetTypeEnemiesKey = "PsiTech.AutocastManagement.FilterTargetTypeEnemies";
        private const string FilterTargetTypeHostilesKey = "PsiTech.AutocastManagement.FilterTargetTypeHostiles";
        private const string FilterTargetTypeFriendliesKey = "PsiTech.AutocastManagement.FilterTargetTypeFriendlies";
        private const string FilterTargetTypeAnyKey = "PsiTech.AutocastManagement.FilterTargetTypeAny";
        
        private const string AdditionalTargetFiltersKey = "PsiTech.Interface.AdditionalTargetFilters";
        private const string AddAdditionalTargetFilterKey = "PsiTech.Interface.AddAdditionalTargetFilter";
        
        private const float FilterTitleHeight = 25f;
        private const float AddFilterButtonWidth = 200f;
        
        protected const float YSeparation = 5f;
        protected const float XSeparation = 5f;
        
        protected const float OptionHeight = 22f;
        
        protected List<AdditionalTargetFilter> AdditionalFilters = new List<AdditionalTargetFilter>();
        private readonly List<AdditionalTargetFilter> filtersToRemove = new List<AdditionalTargetFilter>();
        private List<AdditionalTargetFilterDef> filtersForAdd = new List<AdditionalTargetFilterDef>();
        private Vector2 scrollPosition;
        private int lastFiltersCount;

        protected AutocastFilter() {
            RebuildFilterCache();
        }
        
        public abstract Pawn GetBestTarget(IEnumerable<Pawn> targets);
        
        public abstract void Draw(Rect inRect);

        protected void ResolveRemovedFilters() {
            // Remove filters that are marked for removal
            filtersToRemove.ForEach(filter => AdditionalFilters.Remove(filter));
            filtersToRemove.Clear();
            
            // Rebuild cache if needed
            if (AdditionalFilters.Count != lastFiltersCount) {
                RebuildFilterCache();
                lastFiltersCount = AdditionalFilters.Count;
            }
        }
        
        protected void DrawAdditionalFilters(Rect drawBox, float xAnchor, float yAnchor) {
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
            var needed = AdditionalFilters.Sum(condition => condition.Height + YSeparation);
            var available = filterBox.height;

            if (needed > available) { // Need to draw a scrollbar
                var outRect = new Rect(xAnchor, yAnchor, filterBox.width, available);
                var viewRect = new Rect(0, 0, filterBox.width - 16f, needed);
                var scrollAnchor = 0f;
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
                foreach (var filter in AdditionalFilters) {
                    var height = filter.Height;
                    filter.Draw(new Rect(0, scrollAnchor, filterBox.width - 21f, height), this);
                    scrollAnchor += height + YSeparation;
                }
                Widgets.EndScrollView();
            }
            else { // No need to draw a scrollbar
                foreach (var filter in AdditionalFilters) {
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
            AdditionalFilters.Add(filter);
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
                    AdditionalFilters.Add(instance);
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
                AdditionalFilters.Any(existing => existing.GetType() == condition.FilterClass));
        }

        protected bool TargetMatchesTargetType(Pawn target) {
            return FilterTargetType switch {
                FilterTargetType.Enemies => target.HostileTo(User) && target.Faction.HostileTo(User.Faction),
                FilterTargetType.Hostiles => target.HostileTo(User),
                FilterTargetType.Friendlies => target.Faction.AllyOrNeutralTo(User.Faction),
                FilterTargetType.Any => true,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        protected IEnumerable<Widgets.DropdownMenuElement<string>> GenerateTargetTypeOptions() {

            foreach (FilterTargetType targetType in Enum.GetValues(typeof(FilterTargetType))) {

                yield return new Widgets.DropdownMenuElement<string> {
                    payload = "",
                    option = new FloatMenuOption(LocalizeTargetType(targetType),
                        () => { FilterTargetType = targetType; })
                };

            }

        }
        
        protected static string LocalizeTargetType(FilterTargetType type) {
            
            switch(type) {
                
                case FilterTargetType.Enemies:
                    return FilterTargetTypeEnemiesKey.Translate();
                
                case FilterTargetType.Hostiles:
                    return FilterTargetTypeHostilesKey.Translate();
                
                case FilterTargetType.Friendlies:
                    return FilterTargetTypeFriendliesKey.Translate();
                
                case FilterTargetType.Any:
                    return FilterTargetTypeAnyKey.Translate();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected virtual void PostExpose(){ }
        
        public void ExposeData() {
            Scribe_References.Look(ref Ability, "Ability");
            Scribe_References.Look(ref User, "User");
            Scribe_Values.Look(ref FilterTargetType, "FilterTargetType");
            Scribe_Values.Look(ref TargetRange, "TargetRange");
            Scribe_Collections.Look(ref AdditionalFilters, "additionalFilters", LookMode.Deep);
            
            // For save compatibility, create the list if null
            AdditionalFilters ??= new List<AdditionalTargetFilter>();
            
            PostExpose();
        }
        
    }
    
    public enum FilterTargetType {
        Enemies,
        Hostiles,
        Friendlies,
        Any
    }
}