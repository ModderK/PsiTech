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
using PsiTech.Psionics;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PsiTech.Interface {
    public class PawnPsiCard {

        public static Vector2 PsiCardSize => PsiCardSizeBase + new Vector2((_showingTrainingQueue ? TrainingQueueSize.x : 0f), 0f);
        private static readonly Vector2 PsiCardSizeBase = new Vector2(450f, 470f);
        private static readonly Vector2 TrainingQueueSize = new Vector2(250f, 470f);
        
        private const string PsiEnergyKey = "PsiTech.Interface.PsiEnergy";
        private const string PsiEnergyTip = "PsiTech.Interface.PsiEnergyTip";
        private const string EnergyNodesKey = "PsiTech.Interface.EnergyNodes";
        private const string EnergyNodesTipKey = "PsiTech.Interface.EnergyNodesTip";
        private const string FocusNodesKey = "PsiTech.Interface.FocusNodes";
        private const string FocusNodesTipKey = "PsiTech.Interface.FocusNodesTip";
        private const string EssenceKey = "PsiTech.Interface.Essence";
        private const string EssenceTipKey = "PsiTech.Interface.EssenceTip";
        private const string PsychicSensitivityKey = "PsiTech.Interface.PsychicSensitivity";
        private const string PsychicSensitivityTipKey = "PsiTech.Interface.PsychicSensitivityTip";
        private const string AbilityModifierKey = "PsiTech.Interface.AbilityModifier";
        private const string AbilityModifierTipKey = "PsiTech.Interface.AbilityModifierTip";
        private const string Tier1AbilitiesKey = "PsiTech.Interface.Tier1Abilities";
        private const string Tier1AbilitiesTipKey = "PsiTech.Interface.Tier1AbilitiesTip";
        private const string Tier2AbilitiesKey = "PsiTech.Interface.Tier2Abilities";
        private const string Tier2AbilitiesTipKey = "PsiTech.Interface.Tier2AbilitiesTip";
        private const string Tier3AbilitiesKey = "PsiTech.Interface.Tier3Abilities";
        private const string Tier3AbilitiesTipKey = "PsiTech.Interface.Tier3AbilitiesTip";
        private const string EmptyAbilitySlotKey = "PsiTech.Interface.EmptyAbilitySlot";
        private const string LockedAbilitySlotPluralKey = "PsiTech.Interface.LockedAbilitySlotPlural";
        private const string LockedAbilitySlotSingularKey = "PsiTech.Interface.LockedAbilitySlotSingular";
        private const string NoTrainersAvailableKey = "PsiTech.Interface.NoAvailableTrainers";
        private const string AbilityListKey = "PsiTech.Interface.AbilityList";
        private const string ShowTrainingQueueKey = "PsiTech.Interface.ShowTrainingQueue";
        private const string HideTrainingQueueKey = "PsiTech.Interface.HideTrainingQueue";
        private const string TrainingQueueKey = "PsiTech.Interface.TrainingQueue";
        private const string QueuedAbilityFormatKey = "PsiTech.Interface.QueuedAbility";
        private const string EnergyTrainingKey = "PsiTech.Interface.EnergyTraining";
        private const string FocusTrainingKey = "PsiTech.Interface.FocusTraining";
        private const string RemoveAbilityKey = "PsiTech.Interface.RemoveAbility";

        private const float XMargin = 10f;
        private const float YPadding = 2f;
        private const float YSeparationForSections = 10f;

        private const float NodeTitleWidth = 90f;
        private const float EnergyIconSize = 32f;
        private const float EnergyIconBarSeparation = 15f;
        private const float EnergyBarHeight = 20f;
        private const float EnergyBarWidth = 200f;
        private static float[] EnergyBarMarkers => new[]{0.25f, 0.5f, 0.75f};
        private const float EnergyBarFocusSeparation = 15f;
        private const float FocusIconWidth = 52f;
        private const float FocusIconHeight = 32f;
        private const float ExtraEnergyNodePadding = 27f;
        private const float ExtraFocusNodePadding = 12f;

        private const float EssenceWidth = 115f;
        private const float PsychicSensitivityWidth = 175f;
        private const float AbilityModifierWidth = 140f;

        private const float AbilityListButtonWidth = 212.5f;
        private const float AbilityListButtonHeight = 25f;

        private const float AbilitySlotWidth = 212.5f;
        private const float AbilitySlotHeight = 25f;
        private const float AbilitySlotsPerRow = 2;
        private const float AbilitySlotSeparation = 5f;

        private const float TrainingQueueEntryHeight = 50f;
        
        private static bool _showingTrainingQueue;
        private static Vector2 _trainingQueueScrollAnchor;
        private static readonly List<TrainingQueueMoveEntry> MoveEntriesToResolve = new List<TrainingQueueMoveEntry>();
        private static readonly List<TrainingQueueEntry> EntriesToRemove = new List<TrainingQueueEntry>();

        private static bool _devInstantTraining;
        private static bool _devShowAllAbilities;
        
        public static void DrawPsiCard (Rect rect, Pawn pawn, bool inTrainer) {
            
            if (Event.current.type == EventType.Layout) return;

            var yAnchor = rect.y + YPadding;
            var xAnchor = rect.x + XMargin;
            var cachedFont = Text.Font;
            
            //* Debug options at the top *//
            Text.Font = GameFont.Small;
            if (PsiTechManager.PsiTechDebug && Prefs.DevMode) {
                Widgets.CheckboxLabeled(new Rect(rect.x, rect.y - 17f, 160f, 25f), "Dev: Instant Training", ref _devInstantTraining);
                Widgets.CheckboxLabeled(new Rect(rect.x + 170f, rect.y - 17f, 180f, 25f), "Dev: Show All Abilities", ref _devShowAllAbilities);
            }
            
            //* First horizontal area - energy nodes, energy, focus nodes *//
            
            // Labels
            Text.Font = GameFont.Small;
            
            Widgets.Label(new Rect(xAnchor, yAnchor, NodeTitleWidth, 25), EnergyNodesKey.Translate());
            xAnchor += NodeTitleWidth + EnergyIconBarSeparation;
            
            Widgets.Label(new Rect(xAnchor, yAnchor, EnergyBarWidth, 25), PsiEnergyKey.Translate());
            xAnchor += EnergyBarWidth + EnergyBarFocusSeparation;
            
            Widgets.Label(new Rect(xAnchor, yAnchor, NodeTitleWidth, 25), FocusNodesKey.Translate());

            xAnchor = rect.x + XMargin;
            yAnchor += 25;
            
            // Psi Energy Nodes
            // Oh how horrible! Nested switch expressions!
            var energyIcon = pawn.PsiTracker().EnergyLevel switch {
                1 => pawn.PsiTracker().QueuedEnergy switch {
                    1 => PsiTechUiTextureHelper.EnergyNodes2pre1,
                    2 => PsiTechUiTextureHelper.EnergyNodes2pre2,
                    _ => PsiTechUiTextureHelper.EnergyNodes1
                },
                2 => pawn.PsiTracker().QueuedEnergy switch {
                    1 => PsiTechUiTextureHelper.EnergyNodes3pre,
                    _ => PsiTechUiTextureHelper.EnergyNodes2
                },
                3 => PsiTechUiTextureHelper.EnergyNodes3,
                _ => throw new ArgumentOutOfRangeException()
            };

            var energyNodesRect = new Rect(xAnchor + ExtraEnergyNodePadding, yAnchor, EnergyIconSize, EnergyIconSize);
            if (Widgets.ButtonImage(energyNodesRect, energyIcon) && (PsiTechManager.PsiTechDebug || pawn.IsColonist)) {
                if (_devInstantTraining) {
                    pawn.PsiTracker().EnergyLevel += 1;
                }
                else if (pawn.PsiTracker().CanAddEnergyToTrainingQueue()) {
                    if (!inTrainer) {
                        var trainer = Current.Game.GetComponent<PsiTechManager>().GetOpenTrainerForPawn(pawn);
                        if (trainer == null) {
                            Messages.Message(NoTrainersAvailableKey.Translate(), MessageTypeDefOf.RejectInput, false);
                        }
                    }

                    pawn.PsiTracker().AddEnergyToTrainingQueue();
                }
            }

            TooltipHandler.TipRegion(energyNodesRect,
                new TipSignal(() => EnergyNodesTipKey.Translate(pawn.PsiTracker().EnergyLevel),
                    energyNodesRect.GetHashCode()));
            
            xAnchor += NodeTitleWidth + EnergyIconBarSeparation;

            // Psi Energy Bar
            var energyBarRect = new Rect(xAnchor, yAnchor + (EnergyIconSize - EnergyBarHeight) / 2, EnergyBarWidth,
                EnergyBarHeight);
            var energyBar = Widgets.FillableBar(energyBarRect, pawn.PsiTracker().EnergyPercentOfMax());

            TooltipHandler.TipRegion(energyBarRect,
                new TipSignal(
                    () => PsiEnergyTip.Translate(pawn.PsiTracker().CurrentEnergy.ToString("0.0"),
                        pawn.GetStatValue(PsiTechDefOf.PTMaxPsiEnergy)), energyBarRect.GetHashCode()));
            
            xAnchor += EnergyBarWidth + EnergyBarFocusSeparation;

            foreach (var marker in EnergyBarMarkers) {
                DrawBarThreshold(energyBar, marker, pawn);
            }
            
            // Focus Nodes
            var focusIcon = pawn.PsiTracker().FocusLevel switch {
                1 => pawn.PsiTracker().QueuedFocus switch {
                    1 => PsiTechUiTextureHelper.FocusNodes2pre1,
                    2 => PsiTechUiTextureHelper.FocusNodes2pre2,
                    _ => PsiTechUiTextureHelper.FocusNodes1
                },
                2 => pawn.PsiTracker().QueuedEnergy switch {
                    1 => PsiTechUiTextureHelper.FocusNodes3pre,
                    _ => PsiTechUiTextureHelper.FocusNodes2
                },
                3 => PsiTechUiTextureHelper.FocusNodes3,
                _ => throw new ArgumentOutOfRangeException()
            };

            xAnchor += 5f;
            var focusNodesRect = new Rect(xAnchor + ExtraFocusNodePadding, yAnchor, FocusIconWidth, FocusIconHeight);
            if (Widgets.ButtonImage(focusNodesRect, focusIcon) && (PsiTechManager.PsiTechDebug || pawn.IsColonist)) {
                if (_devInstantTraining) {
                    pawn.PsiTracker().FocusLevel += 1;
                }
                else if (pawn.PsiTracker().CanAddFocusToTrainingQueue()) {
                    if (!inTrainer) {
                        var trainer = Current.Game.GetComponent<PsiTechManager>().GetOpenTrainerForPawn(pawn);
                        if (trainer == null) {
                            Messages.Message(NoTrainersAvailableKey.Translate(), MessageTypeDefOf.RejectInput, false);
                        }
                    }

                    pawn.PsiTracker().AddFocusToTrainingQueue();
                }
            }
            
            TooltipHandler.TipRegion(focusNodesRect,
                new TipSignal(() => FocusNodesTipKey.Translate(pawn.PsiTracker().FocusLevel),
                    focusNodesRect.GetHashCode()));
            
            //* Second horizontal area - essence, psychic sensitivity, ability modifier *//
            
            yAnchor += FocusIconHeight + YSeparationForSections;
            xAnchor = rect.x + XMargin;

            // Essence
            var essenceRect = new Rect(xAnchor, yAnchor, EssenceWidth, 25);
            Widgets.Label(essenceRect, EssenceKey.Translate(pawn.PsiTracker().Essence.ToStringPercent()));
            xAnchor += EssenceWidth;

            TooltipHandler.TipRegion(essenceRect,
                new TipSignal(() => EssenceTipKey.Translate(PsiTechSettings.Get().EssenceLossPerPart.ToStringPercent()),
                    essenceRect.GetHashCode()));

            // Psychic Sensitivity
            var psychicSensitivityRect = new Rect(xAnchor, yAnchor, PsychicSensitivityWidth, 25);
            Widgets.Label(psychicSensitivityRect, PsychicSensitivityKey.Translate(pawn.PsiTracker().PsychicSensitivity.ToStringPercent()));
            xAnchor += PsychicSensitivityWidth;

            TooltipHandler.TipRegion(psychicSensitivityRect,
                new TipSignal(() => PsychicSensitivityTipKey.Translate(), psychicSensitivityRect.GetHashCode()));

            // Ability Modifier
            var abilityModifierRect = new Rect(xAnchor, yAnchor, AbilityModifierWidth, 25);
            Widgets.Label(abilityModifierRect, AbilityModifierKey.Translate(pawn.PsiTracker().AbilityModifier.ToStringPercent()));

            TooltipHandler.TipRegion(abilityModifierRect,
                new TipSignal(() => AbilityModifierTipKey.Translate(), abilityModifierRect.GetHashCode()));

            var abilitySlots = pawn.PsiTracker().UnlockedAbilitySlots();
            
            //* Third horizontal area - tier 1 abilities *//
            yAnchor += 25f + YSeparationForSections;
            xAnchor = rect.x + XMargin;

            var labelRect = new Rect(xAnchor, yAnchor, 250f, 25f);
            Widgets.Label(labelRect, Tier1AbilitiesKey.Translate());
            yAnchor += 25f + YPadding;
            
            TooltipHandler.TipRegion(labelRect,
                new TipSignal(() => Tier1AbilitiesTipKey.Translate(), labelRect.GetHashCode()));

            yAnchor = DrawAbilitySlotsGrid(new Vector2(xAnchor, yAnchor), abilitySlots[0],
                PsiTechTracker.Tier1Abilities, 1, pawn, inTrainer);

            //* Fourth horizontal area - tier 2 abilities *//
            yAnchor += YSeparationForSections;
            xAnchor = rect.x + XMargin;

            labelRect = new Rect(xAnchor, yAnchor, 250f, 25f);
            Widgets.Label(labelRect, Tier2AbilitiesKey.Translate());
            yAnchor += 25f + YPadding;
            
            TooltipHandler.TipRegion(labelRect,
                new TipSignal(() => Tier2AbilitiesTipKey.Translate(), labelRect.GetHashCode()));
            
            yAnchor = DrawAbilitySlotsGrid(new Vector2(xAnchor, yAnchor), abilitySlots[1],
                PsiTechTracker.Tier2Abilities, 2, pawn, inTrainer);

            //* Fifth horizontal area - tier 3 abilities *//
            yAnchor += YSeparationForSections;
            xAnchor = rect.x + XMargin;

            labelRect = new Rect(xAnchor, yAnchor, 250f, 25f);
            Widgets.Label(labelRect, Tier3AbilitiesKey.Translate());
            yAnchor += 25f + YPadding;
            
            TooltipHandler.TipRegion(labelRect,
                new TipSignal(() => Tier3AbilitiesTipKey.Translate(), labelRect.GetHashCode()));
            
            yAnchor = DrawAbilitySlotsGrid(new Vector2(xAnchor, yAnchor), abilitySlots[2],
                PsiTechTracker.Tier3Abilities, 3, pawn, inTrainer);
            
            //* Sixth horizontal area - ability window button and training queue toggle *//
            yAnchor += 2 * YSeparationForSections;
            if ((PsiTechManager.PsiTechDebug || pawn.IsColonist) && pawn.PsiTracker().CanUseActiveAbilities() &&
                pawn.PsiTracker().Abilities.Any(ability => ability.Def.Autocastable)) {
                xAnchor = rect.x + XMargin;
                Text.Anchor = TextAnchor.MiddleCenter;

                var buttonRect = new Rect(xAnchor, yAnchor, AbilityListButtonWidth, AbilityListButtonHeight);
                if (Widgets.ButtonText(buttonRect, AbilityListKey.Translate())) {
                    Find.WindowStack.Add(new AbilityWindow(pawn));
                }
            }
            
            if (pawn.IsColonist && pawn.PsiTracker().TrainingQueue.Any()) {
                xAnchor = rect.x + XMargin + AbilityListButtonWidth + AbilitySlotSeparation;
                Text.Anchor = TextAnchor.MiddleCenter;

                var buttonRect = new Rect(xAnchor, yAnchor, AbilityListButtonWidth, AbilityListButtonHeight);
                var key = _showingTrainingQueue ? HideTrainingQueueKey : ShowTrainingQueueKey;
                if (Widgets.ButtonText(buttonRect, key.Translate())) {
                    _showingTrainingQueue = !_showingTrainingQueue;
                }
            }
            else {
                _showingTrainingQueue = false;
            }
            
            //* "Addendum": Displaying the training queue *//
            if (_showingTrainingQueue) {
                var trainingQueueRect = new Rect(new Vector2(rect.xMax - TrainingQueueSize.x, rect.y), TrainingQueueSize);
                Widgets.DrawLine(new Vector2(rect.xMax - TrainingQueueSize.x, rect.y - 17f),
                    new Vector2(rect.xMax - TrainingQueueSize.x, rect.yMax + 17f),
                    new Color(97f / 255f, 108f / 255f, 122f / 255f), 0.5f);
                DrawTrainingQueue(trainingQueueRect, pawn);
            }

            Text.Font = cachedFont;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void DrawTrainingQueue(Rect drawBox, Pawn pawn) {
            Text.Anchor = TextAnchor.MiddleLeft;
            
            // Training Queue label
            Widgets.Label(new Rect(drawBox.x + 20f, drawBox.y - 10f, drawBox.width, 22f), TrainingQueueKey.Translate());
            
            var needed = (TrainingQueueEntryHeight + 5f) * pawn.PsiTracker().TrainingQueue.Count;
            var viewRect = new Rect(0, 0, drawBox.width - 16f, needed);
            var outRect = new Rect(drawBox.x, drawBox.y + 16f, drawBox.width, drawBox.height - 16f);
            var scrollAnchor = 0f;
            Widgets.BeginScrollView(outRect, ref _trainingQueueScrollAnchor, viewRect);
            foreach (var entry in pawn.PsiTracker().TrainingQueue) {
                DrawTrainingQueueEntry(new Rect(16f, scrollAnchor, drawBox.width - 37f, TrainingQueueEntryHeight),
                    entry, pawn);
                scrollAnchor += TrainingQueueEntryHeight + 5f;
            }
            Widgets.EndScrollView();

            // Resolve move entries and delete entries since you can't modify a collection while enumerating.
            // I guess you could just copy instead but that's mildly inefficient
            foreach (var entry in MoveEntriesToResolve) {
                if (entry.MovingUp) {
                    pawn.PsiTracker().MoveTrainingEntryUp(entry.Entry);
                }
                else {
                    pawn.PsiTracker().MoveTrainingEntryDown(entry.Entry);
                }
            }
            MoveEntriesToResolve.Clear();

            foreach (var entry in EntriesToRemove) {
                pawn.PsiTracker().RemoveTrainingEntry(entry);
            }
            EntriesToRemove.Clear();
        }

        private static void DrawTrainingQueueEntry(Rect drawBox, TrainingQueueEntry entry, Pawn pawn) {
            Widgets.DrawBoxSolid(drawBox, new Color(53f/255f, 54f/255f, 55f/255f));

            string label;
            switch (entry.Type) {
                case TrainingType.Ability:
                    label = entry.Def.label;
                    break;
                case TrainingType.Focus:
                    label = FocusTrainingKey.Translate();
                    break;
                case TrainingType.Energy:
                    label = EnergyTrainingKey.Translate();
                    break;
                case TrainingType.Remove:
                    label = RemoveAbilityKey.Translate(entry.Def.label);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var inRect = drawBox.ContractedBy(5f);
            var baseColor = Color.white;

            // Draw reorder arrows
            if (pawn.PsiTracker().CanMoveTrainingEntryUp(entry)) {
                var upArrow = new Rect(inRect.x, inRect.y, 24f, 20f);
                if (Widgets.ButtonImage(upArrow, PsiTechUiTextureHelper.ReorderUp, baseColor)) {
                    var moveEntry = new TrainingQueueMoveEntry {
                        Entry = entry,
                        MovingUp = true
                    };
                    MoveEntriesToResolve.Add(moveEntry);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
            }

            if (pawn.PsiTracker().CanMoveTrainingEntryDown(entry)) {
                var downArrow = new Rect(inRect.x, inRect.y + 20f, 24f, 20f);
                if (Widgets.ButtonImage(downArrow, PsiTechUiTextureHelper.ReorderDown, baseColor)) {
                    var moveEntry = new TrainingQueueMoveEntry {
                        Entry = entry,
                        MovingUp = false
                    };
                    MoveEntriesToResolve.Add(moveEntry);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
            }

            // Draw ability name
            Widgets.Label(new Rect(inRect.x + 29f, inRect.y, inRect.width - 58f, inRect.height), label);
            
            // Draw delete button if not locked
            // This means people need to be manually ejected to end training, and I'm alright with that
            if (!entry.Locked) {
                var delButton = new Rect(drawBox.xMax - 24f, drawBox.y, 24f, 24f);
                if (Widgets.ButtonImage(delButton, PsiTechUiTextureHelper.DeleteX, baseColor,
                    baseColor * GenUI.SubtleMouseoverColor)) {
                    EntriesToRemove.Add(entry);
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
            }

            // Draw info button
            if (entry.Def != null) {
                Widgets.InfoCardButton(drawBox.xMax - 24f, drawBox.yMax - 24f, entry.Def);
            }

        }
        
        private static float DrawAbilitySlotsGrid(Vector2 startPos, int unlockedSlots, int totalSlots, int tier, Pawn pawn, bool inTrainer) {
            var yAnchor = startPos.y;
            var xAnchor = startPos.x;
            var drawn = 0;
            while (drawn < totalSlots) {
                for (var k = 0; k < AbilitySlotsPerRow; k++) {
                    if (drawn < unlockedSlots) {

                        string label;
                        
                        var ability = pawn.PsiTracker().AbilityInSlot(drawn, tier);
                        var queued = pawn.PsiTracker().QueuedAbilityInSlot(drawn, tier);
                        if (queued != null) {
                            label = QueuedAbilityFormatKey.Translate(queued.label);
                        }
                        else {
                            label = ability == null ? (string)EmptyAbilitySlotKey.Translate() : ability.Def.label;
                        }
                        
                        DrawAbilitySlot(new Rect(xAnchor, yAnchor, AbilitySlotWidth, AbilitySlotHeight), label, drawn,
                            tier, false, pawn, inTrainer, ability?.Def);
                    }
                    else {
                        var nodes = PsiTechTracker.LevelsNeededForSlot(drawn, tier) - pawn.PsiTracker().TotalLevel;
                        var key = nodes == 1 ? LockedAbilitySlotSingularKey : LockedAbilitySlotPluralKey;
                        DrawAbilitySlot(new Rect(xAnchor, yAnchor, AbilitySlotWidth, AbilitySlotHeight),
                            key.Translate(nodes), drawn, tier, true, pawn,
                            inTrainer);
                    }

                    drawn++;
                    xAnchor += AbilitySlotWidth + AbilitySlotSeparation;
                }

                xAnchor = startPos.x;
                yAnchor += AbilitySlotHeight + AbilitySlotSeparation;
            }

            return yAnchor;
        }

        private static void DrawAbilitySlot(Rect slotRect, string slotText, int slotNum, int tier, bool locked,
            Pawn pawn, bool inTrainer, PsiTechAbilityDef def = null) {
            Widgets.DrawMenuSection(slotRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            var color = GUI.color;

            if (locked) {
                Widgets.Label(slotRect, slotText);
            }
            else {
                GUI.color = Widgets.NormalOptionColor;

                float infoSize;
                if (def != null && !pawn.PsiTracker().TrainingEntryExists(slotNum, tier)) {
                    infoSize = Widgets.InfoCardButtonSize;
                    Widgets.InfoCardButton(slotRect.xMax - infoSize, slotRect.y + (slotRect.height - 24f) / 2f, def);
                }
                else {
                    infoSize = 0f;
                }

                var buttonRect = new Rect(slotRect.x, slotRect.y, slotRect.width - infoSize, slotRect.height);
                Widgets.Label(buttonRect, slotText);
                if (Widgets.ButtonText(buttonRect, "", false) && (PsiTechManager.PsiTechDebug || pawn.IsColonist)) {
                    
                    var options = new List<FloatMenuOption>();
                    
                    // Add remove ability option
                    if (def != null) {
                        void Remove() {
                            if (_devInstantTraining) {
                                pawn.PsiTracker().TryRemoveAbility(def);
                            }
                            else {
                                if (!inTrainer) {
                                    var trainer = Current.Game.GetComponent<PsiTechManager>()
                                        .GetOpenTrainerForPawn(pawn);
                                    if (trainer == null) {
                                        Messages.Message(NoTrainersAvailableKey.Translate(), MessageTypeDefOf.CautionInput, false);
                                    }
                                }

                                pawn.PsiTracker().AddRemoveToTrainingQueue(slotNum, def);
                            }
                        }

                        options.Add(new FloatMenuOption(RemoveAbilityKey.Translate(def.label), Remove));
                    }
                    
                    // Abilities available to be trained
                    var available = pawn.PsiTracker().AvailableAbilitiesForDisplay(tier, _devShowAllAbilities);
                    foreach (var ability in available) {
                        options.Add(GenerateAbilityOption(ability, slotNum, pawn, inTrainer));
                    }

                    if (!options.NullOrEmpty()) {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                    
                }

            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = color;
        }

        private static FloatMenuOption GenerateAbilityOption(AbilityDisplayEntry entry, int slotNum, Pawn pawn, bool inTrainer) {
            Action action = null;
            if (_devInstantTraining || entry.Trainable) {
                action = () => {
                    if (_devInstantTraining) {
                        pawn.PsiTracker().AddAbility(slotNum, entry.Def);
                    }
                    else {
                        if (!inTrainer) {
                            var trainer = Current.Game.GetComponent<PsiTechManager>().GetOpenTrainerForPawn(pawn);
                            if (trainer == null) {
                                Messages.Message(NoTrainersAvailableKey.Translate(), MessageTypeDefOf.CautionInput,
                                    false);
                            }
                        }

                        pawn.PsiTracker().AddAbilityToTrainingQueue(slotNum, entry.Def);
                    }
                };
            }

            var floatMenuOption = new FloatMenuOption(entry.Def.label, action) {
                extraPartWidth = 29f,
                extraPartOnGUI = rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, entry.Def)
            };
            
            return floatMenuOption;
        }

        private static void DrawBarThreshold(Rect barRect, float threshPct, Pawn pawn) {
            var width = (double) barRect.width <= 60.0 ? 1f : 2f;
            var position =
                new Rect(
                    (float) (barRect.x + barRect.width * threshPct - (width - 1.0)),
                    barRect.y + barRect.height / 2f, width, barRect.height / 2f);
            Texture2D texture2D;
            if (threshPct < pawn.PsiTracker().EnergyPercentOfMax()) {
                texture2D = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else {
                texture2D = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }

            GUI.DrawTexture(position, texture2D);
            GUI.color = Color.white;
        }

    }

    public struct TrainingQueueMoveEntry {
        public TrainingQueueEntry Entry;
        public bool MovingUp;
    }
}