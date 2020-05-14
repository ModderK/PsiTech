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
using Verse;

namespace PsiTech.SuppressionField {
    public class SuppressionFieldManager : MapComponent {
        
        private Dictionary<IntVec3, SuppressionFieldEntry> suppressionField = new Dictionary<IntVec3, SuppressionFieldEntry>();

        public SuppressionFieldManager(Map map) : base(map) { }

        public void RegisterField(CompPsychicSuppressionField comp) {
            foreach (var cell in comp.CellsInRange()) {
                if (suppressionField.TryGetValue(cell, out var existing)) {
                    existing.Comps.Add(comp);
                }
                else {
                    var entry = new SuppressionFieldEntry();
                    entry.Comps.Add(comp);
                    suppressionField.Add(cell, entry);
                }
            }
        }

        public void UnregisterField(CompPsychicSuppressionField comp) {
            var entriesToRemove = new List<IntVec3>();
            foreach (var entry in suppressionField) {
                if(!entry.Value.Comps.Contains(comp)) continue;

                entry.Value.Comps.Remove(comp);
                
                if(entry.Value.Comps.Any()) continue;

                entriesToRemove.Add(entry.Key);
            }
            
            entriesToRemove.ForEach(entry => suppressionField.Remove(entry));
        }

        public void UpdateFieldRadius(CompPsychicSuppressionField comp) {
            UnregisterField(comp);
            RegisterField(comp);
        }

        public float GetEffectOnCell(IntVec3 cell) {
            return suppressionField.TryGetValue(cell, out var entry) ? entry.Effect : 0f;
        }

    }

    public class SuppressionFieldEntry {

        public List<CompPsychicSuppressionField> Comps = new List<CompPsychicSuppressionField>();
        public float Effect => Comps.Min(comp => comp.GetCurrentEffect());
    }
}