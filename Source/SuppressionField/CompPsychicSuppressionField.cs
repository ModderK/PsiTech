/*
 *  Copyright 2020, K
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
using PsiTech.Interface;
using RimWorld;
using UnityEngine;
using Verse;

namespace PsiTech.SuppressionField {
    public class CompPsychicSuppressionField : ThingComp {
        private float currentEffect;
        public float CurrentRadius;

        public float TargetEffect;
        public float TargetRadius;

        public bool InConfigurationWindow;
        public bool RadiusCached;

        private List<IntVec3> cachedCells;
        
        private IEnumerable<Gizmo> cachedGizmos;
        private bool hasCachedGizmos;

        private SuppressionFieldManager Manager => parent.Map?.GetComponent<SuppressionFieldManager>();
        private CompPowerTrader PowerTrader => parent.TryGetComp<CompPowerTrader>();
        private CompBreakdownable Breakdownable => parent.TryGetComp<CompBreakdownable>();
        public CompProperties_PsychicSuppressionField Props => props as CompProperties_PsychicSuppressionField;

        private bool IsOperating => (PowerTrader?.PowerOn ?? true) && !(Breakdownable?.BrokenDown ?? false);
        
        private const string CurrentEffectKey = "PsiTech.SuppressionField.CurrentEffect";
        private const string CurrentRadiusKey = "PsiTech.SuppressionField.CurrentRadius";
        private const string ConfigureSuppressionFieldKey = "PsiTech.SuppressionField.ConfigureSuppressionField";
        private const string ConfigureSuppressionFieldDescKey = "PsiTech.SuppressionField.ConfigureSuppressionFieldDesc";

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            Manager?.RegisterField(this);
            UpdatePower();

            if (respawningAfterLoad) return;
            
            // We've got to do this here because we can't resolve the props at compile-time
            currentEffect = Props.MaxEffect;
            CurrentRadius = Props.MinRadius;
            TargetEffect = Props.MaxEffect;
            TargetRadius = Props.MinRadius;
        }

        public override void PostDeSpawn(Map map) {
            base.PostDeSpawn(map);
            Manager?.UnregisterField(this);
            UpdatePower();
        }

        public override void CompTick() {

            if (!parent.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) return;
            
            // Reset on power loss or breakdown, must be recharged
            if (!IsOperating) {
                currentEffect = Props.MaxEffect;
                CurrentRadius = Props.MinRadius;
                return;
            }
            
            if (currentEffect == TargetEffect && CurrentRadius == TargetRadius) return;

            // Update effect
            var effectDiff = TargetEffect - currentEffect;
            if (currentEffect != TargetEffect && Mathf.Abs(effectDiff) > Props.EffectChangeSpeedPerSecond) {
                currentEffect += Mathf.Sign(effectDiff) * Props.EffectChangeSpeedPerSecond;
            }else if (currentEffect != TargetEffect) {
                currentEffect = TargetEffect;
            }
            
            // Update radius
            var radiusDiff = TargetRadius - CurrentRadius;
            if (CurrentRadius != TargetRadius && Mathf.Abs(radiusDiff) > Props.RadiusChangeSpeedPerSecond) {
                CurrentRadius += Mathf.Sign(radiusDiff) * Props.RadiusChangeSpeedPerSecond;
            }else if (CurrentRadius != TargetRadius) {
                CurrentRadius = TargetRadius;
            }
            
            UpdateField();
        }

        public void UpdateField() {
            Manager?.UpdateFieldRadius(this);
            RadiusCached = false;
            UpdatePower();
        }

        public float GetCurrentEffect() {
            return IsOperating ? currentEffect : 0f;
        }

        public float PredictedPowerConsumption() {
            var cells = GenRadial.RadialCellsAround(parent.Position, TargetRadius, true).Count();
            var intensity = Mathf.Abs(TargetEffect / Props.EffectStep);
            return Props.BasePowerConsumption + Props.PowerPerCellEffect * cells * intensity;
        }

        private float TotalPowerConsumption() {
            var cells = CellsInRange().Count();
            var intensity = Mathf.Abs(currentEffect / Props.EffectStep);
            return Props.BasePowerConsumption + Props.PowerPerCellEffect * cells * intensity;
        }

        private void UpdatePower() {
            if (PowerTrader == null) return;
            
            PowerTrader.PowerOutput = -TotalPowerConsumption();
        }

        public IEnumerable<IntVec3> CellsInRange() {
            if (RadiusCached) return cachedCells;
            
            cachedCells = GenRadial.RadialCellsAround(parent.Position, CurrentRadius, true).ToList();
            RadiusCached = true;
            return cachedCells;
        }

        public override string CompInspectStringExtra() {
            if (parent.Map == null) return "";
            
            return CurrentEffectKey.Translate(currentEffect.ToStringPercent()) + "\n" +
                   CurrentRadiusKey.Translate(CurrentRadius.ToString("#.##"));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            if (hasCachedGizmos) return cachedGizmos;

            var gizmos = new List<Gizmo> {
                new Command_Action {
                    defaultLabel = ConfigureSuppressionFieldKey.Translate(),
                    defaultDesc = ConfigureSuppressionFieldDescKey.Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/SuppressionSettingsGizmo"),
                    action = () => {
                        var window = new SuppressionFieldSettingsWindow(this);
                        Find.WindowStack.Add(window);
                    }
                }
            };
            cachedGizmos = gizmos;
            hasCachedGizmos = true;
            return gizmos;
        }

        public override void PostExposeData() {
            Scribe_Values.Look(ref currentEffect, "CurrentEffect");
            Scribe_Values.Look(ref CurrentRadius, "CurrentRadius");
            Scribe_Values.Look(ref TargetEffect, "TargetEffect");
            Scribe_Values.Look(ref TargetRadius, "TargetRadius");
        }
    }
}