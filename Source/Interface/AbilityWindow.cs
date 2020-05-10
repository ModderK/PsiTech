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
using PsiTech.AutocastManagement;
using PsiTech.Psionics;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace PsiTech.Interface {
    public class AbilityWindow : Window {

        private readonly Pawn pawn;

        public override Vector2 InitialSize => new Vector2(1130f, 600f);
        
        private readonly List<TabRecord> tabs = new List<TabRecord>();
        private PsiTechAbility curAbility;
        private List<AutocastConditionDef> conditionsForAdd = new List<AutocastConditionDef>();
        private Vector2 scrollPosition;
        private bool lastAutocastSetting;
        private AbilityPriority lastPrioritySetting;
        private int lastConditionsCount;
        private readonly List<AutocastCondition> conditionsToRemove = new List<AutocastCondition>();

        private const string HideAutocastToggleGizmoKey = "PsiTech.Interface.HideAutocastToggleGizmo";
        private const string AutocastLabelKey = "PsiTech.Interface.AutocastLabel";
        private const string PriorityLabelKey = "PsiTech.Interface.PriorityLabel";
        private const string PriorityHighKey = "PsiTech.Interface.PriorityHigh";
        private const string PriorityNormalKey = "PsiTech.Interface.PriorityNormal";
        private const string PriorityLowKey = "PsiTech.Interface.PriorityLow";
        private const string AutocastConditionsKey = "PsiTech.Interface.AutocastConditions";
        private const string AddAutocastCondition = "PsiTech.Interface.AddAutocastCondition";
        private const string AutocastFilterKey = "PsiTech.Interface.AutocastFilter";

        private const float TabVerticalOffset = 32f;
        
        private const float YSeparation = 5f;
        private const float XSeparation = 5f;
        
        private const float TopBarHeight = 30f;
        private const float AutocastLabelWidth = 90f;
        private const float PriorityLabelWidth = 80f;
        private const float AutocastCheckSize = 24f;

        private const float BoxTitleHeight = 25f;
        private const float AddConditionButtonWidth = 200f;

        private const float ConditionsRebalanceWidth = 0f;

        public AbilityWindow(Pawn selPawn) {
            pawn = selPawn;
            Setup();
        }

        private void Setup() {
            forcePause = true;
            doCloseButton = false;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            soundAppear = SoundDefOf.InfoCard_Open;
            soundClose = SoundDefOf.InfoCard_Close;
            
            var abilities = pawn.PsiTracker().Abilities.Where(ability => ability.Def.Autocastable).ToList();
            curAbility = abilities.First();
            
            RebuildTabCache();
            RebuildConditionCache();
            
            lastAutocastSetting = curAbility.Autocast;
            lastPrioritySetting = curAbility.Priority;
            lastConditionsCount = curAbility.AutocastConditions.Count;
        }

        public override void DoWindowContents(Rect inRect) {

            if (Event.current.type == EventType.Layout) return;

            // Deal with all the things that have changed //
            if (conditionsToRemove.Count != 0) {
                conditionsToRemove.ForEach(condition => curAbility.AutocastConditions.Remove(condition));
                conditionsToRemove.Clear();
            }

            if (lastAutocastSetting != curAbility.Autocast || lastPrioritySetting != curAbility.Priority) {
                RebuildTabCache();
                lastAutocastSetting = curAbility.Autocast;
                lastPrioritySetting = curAbility.Priority;
            }

            if (lastConditionsCount != curAbility.AutocastConditions.Count) {
                RebuildConditionCache();
                lastConditionsCount = curAbility.AutocastConditions.Count;
            }
            
            // Draw gizmo hide toggle //
            Text.Font = GameFont.Medium;
            
            Widgets.Label(new Rect(inRect.x + 10f, inRect.y + 10f, 285f, TopBarHeight), HideAutocastToggleGizmoKey.Translate());
            Widgets.Checkbox(inRect.x + 300f, inRect.y + 10f + (TopBarHeight - AutocastCheckSize) / 2, ref pawn.PsiTracker().HideAutocastToggleGizmo);
            
            // Draw menu and tabs //
            Text.Font = GameFont.Small;

            var tabBox = new Rect(inRect.x, inRect.y + 50f, inRect.width, inRect.height - 50f);
            tabBox.yMin += TabVerticalOffset;
            var drawBox = tabBox.ContractedBy(10f);
            
            Widgets.DrawMenuSection(tabBox);
            TabDrawer.DrawTabs(tabBox, tabs);
            
            // First vertical section - autocast toggle and priority select //
            var xAnchor = drawBox.x;
            var yAnchor = drawBox.y;

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            
            // Autocast label & checkbox - left side
            Widgets.Label(new Rect(xAnchor, yAnchor, AutocastLabelWidth, TopBarHeight), AutocastLabelKey.Translate());
            xAnchor += AutocastLabelWidth + XSeparation;
            Widgets.Checkbox(xAnchor, yAnchor + (TopBarHeight - AutocastCheckSize) / 2, ref curAbility.Autocast);
            
            // Priority label & dropdown - right side
            Widgets.Label(new Rect(drawBox.xMax - PriorityLabelWidth, yAnchor, PriorityLabelWidth, TopBarHeight), PriorityLabelKey.Translate());
            Widgets.Dropdown(new Rect(drawBox.xMax - 2*PriorityLabelWidth, yAnchor, PriorityLabelWidth, TopBarHeight), curAbility.Priority,
                priority => priority.ToString(), priority => GeneratePriorityOptions(curAbility),
                LocalizePriority(curAbility.Priority));

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            
            // Conditions and Filters prep
            yAnchor += TopBarHeight + YSeparation;
            var subMenuWidth = (drawBox.width - XSeparation) / 2;
            var subMenuHeight = drawBox.height - (TopBarHeight + YSeparation);

            var conditionsBox = new Rect(drawBox.x, yAnchor, subMenuWidth + ConditionsRebalanceWidth, subMenuHeight);
            var filterBox = new Rect(drawBox.x + subMenuWidth + ConditionsRebalanceWidth + XSeparation, yAnchor,
                subMenuWidth - ConditionsRebalanceWidth, subMenuHeight);
            
            Widgets.DrawMenuSection(conditionsBox);
            Widgets.DrawMenuSection(filterBox);
            
            // Conditions box //
            var condBoxCont = conditionsBox.ContractedBy(5f);
            xAnchor = condBoxCont.x;
            yAnchor = condBoxCont.y;

            Text.Anchor = TextAnchor.MiddleLeft;
            
            Widgets.Label(new Rect(xAnchor, yAnchor, condBoxCont.width, BoxTitleHeight), AutocastConditionsKey.Translate());

            // Add condition button
            if (Widgets.ButtonText(new Rect(condBoxCont.xMax - AddConditionButtonWidth, yAnchor, AddConditionButtonWidth, BoxTitleHeight), AddAutocastCondition.Translate())) {
                var options = new List<FloatMenuOption>();
                foreach (var condition in conditionsForAdd) {
                    options.Add(GenerateAutocastConditionOption(condition));
                }

                if (!options.NullOrEmpty()) {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                    
            }

            yAnchor += BoxTitleHeight + YSeparation;
            Text.Anchor = TextAnchor.UpperLeft;
            
            // Find the total needed height and available height, see if we need to draw a scroll bar
            var needed = curAbility.AutocastConditions.Sum(condition => condition.Height + YSeparation);
            var available = condBoxCont.height - (BoxTitleHeight + YSeparation);

            if (needed > available) { // Need to draw a scrollbar
                var outRect = new Rect(xAnchor, yAnchor, condBoxCont.width, available);
                var viewRect = new Rect(0, 0, condBoxCont.width - 16f, needed);
                var scrollAnchor = 0f;
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
                foreach (var condition in curAbility.AutocastConditions) {
                    var height = condition.Height;
                    condition.Draw(new Rect(0, scrollAnchor, condBoxCont.width - 21f, height), this);
                    scrollAnchor += height + YSeparation;
                }
                Widgets.EndScrollView();
            }
            else { // No need to draw a scrollbar
                foreach (var condition in curAbility.AutocastConditions) {
                    var height = condition.Height;
                    condition.Draw(new Rect(xAnchor, yAnchor, condBoxCont.width, height), this);
                    yAnchor += height + YSeparation;
                }
            }
            
            // Filter box //
            var filterBoxCont = filterBox.ContractedBy(5f);
            xAnchor = filterBoxCont.x;
            yAnchor = filterBoxCont.y;

            Text.Anchor = TextAnchor.MiddleLeft;
            
            Widgets.Label(new Rect(xAnchor, yAnchor, condBoxCont.width, BoxTitleHeight), AutocastFilterKey.Translate());
            yAnchor += BoxTitleHeight + YSeparation;

            var filterDrawBox = new Rect(xAnchor, yAnchor, filterBoxCont.width, filterBoxCont.height - BoxTitleHeight - YSeparation);
            
            curAbility.AutocastFilter.Draw(filterDrawBox);

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public void ScheduleConditionForRemoval(AutocastCondition condition) {
            conditionsToRemove.Add(condition);
        }

        private void RebuildTabCache() {
            tabs.Clear();
            
            var abilities = pawn.PsiTracker().Abilities.Where(ability => ability.Def.Autocastable).ToList();
            foreach (var ability in abilities) {
                string title;
                if (ability.Autocast) {
                    title = "[A] " + ability.Def.LabelCap;
                    
                    switch (ability.Priority) {
                        case AbilityPriority.High:
                            title += " +";
                            break;
                        case AbilityPriority.Low:
                            title += " -";
                            break;
                    }
                }
                else {
                    title = ability.Def.LabelCap;
                }

                tabs.Add(new TabRecord(title, () => curAbility = ability, () => curAbility == ability));
            }
        }

        private void RebuildConditionCache() {
            conditionsForAdd = DefDatabase<AutocastConditionDef>.AllDefsListForReading.ListFullCopy();
            conditionsForAdd.RemoveAll(condition =>
                curAbility.AutocastConditions.Any(existing => existing.GetType() == condition.ConditionClass));
        }

        private static IEnumerable<Widgets.DropdownMenuElement<string>> GeneratePriorityOptions(PsiTechAbility ability) {

            foreach (AbilityPriority priorityType in Enum.GetValues(typeof(AbilityPriority))) {

                yield return new Widgets.DropdownMenuElement<string> {
                    payload = "",
                    option = new FloatMenuOption(LocalizePriority(priorityType),
                        () => { ability.Priority = priorityType; })
                };

            }

        }
        
        private FloatMenuOption GenerateAutocastConditionOption(AutocastConditionDef entry) {
            void Action() {
                if (!(Activator.CreateInstance(entry.ConditionClass) is AutocastCondition instance)) {
                    Log.Error("PsiTech tried to instantiate an autocast conditions of class " + entry.ConditionClass +
                              " and failed. This indicated a misconfigured autocast condition def.");
                }
                else {
                    instance.User = pawn;
                    instance.Ability = curAbility;
                    instance.Def = entry;
                    curAbility.AutocastConditions.Add(instance);
                    RebuildConditionCache();
                }
            }

            var floatMenuOption = new FloatMenuOption(entry.label, Action) {
                extraPartWidth = 29f,
                extraPartOnGUI = rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, entry)
            };
            
            return floatMenuOption;
        }

        private static string LocalizePriority(AbilityPriority priority) {

            switch (priority) {
                case AbilityPriority.High:
                    return PriorityHighKey.Translate();
                    
                case AbilityPriority.Normal:
                    return PriorityNormalKey.Translate();
                
                case AbilityPriority.Low:
                    return PriorityLowKey.Translate();
                
                default:
                    return "";
                
            }
        }
        
    }
}