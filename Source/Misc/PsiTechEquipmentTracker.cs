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

using System.Collections.Generic;
using PsiTech.Utility;
using RimWorld;
using Verse;

namespace PsiTech.Misc {
    public class PsiTechEquipmentTracker : IExposable {

        public bool IsPsychic;

        private Thing thing;
        private bool IsWeapon => thing.def.IsWeapon;
        private bool IsRanged => thing.def.IsRangedWeapon;
        private int randomId;
        
        public PsiTechEquipmentTracker() {} // for scribe
        
        public PsiTechEquipmentTracker(Thing thing) {
            this.thing = thing;
            randomId = Rand.Int;
        }

        public float GetTotalFactorOfStat(StatDef stat, Pawn user) {
            if (!IsWeapon) return 1f;
            
            var sync = user.GetStatValue(PsiTechDefOf.PTPsiWeaponSynchronicity);

            float mod;
            if (IsRanged) {
                if (!EquipmentEnhancementDef.RangedModDict.TryGetValue(stat, out mod)) return 1f;
            }
            else {
                if (!EquipmentEnhancementDef.MeleeModDict.TryGetValue(stat, out mod)) return 1f;
            }
            
            return 1 + mod * sync;
        }
        
        public IEnumerable<Gizmo> GetGizmos() {
            if (!IsWeapon) return null;
            
            IEnumerable<Gizmo> gizmos = null;

            if (PsiTechManager.PsiTechDebug && Prefs.DevMode) {
                gizmos = DebugGizmos();
            }

            return gizmos;
        }
        
        private IEnumerable<Gizmo> DebugGizmos() {

            if(!IsPsychic) {
                yield return new Command_Action {
                    defaultLabel = "DEBUG Make psychic",
                    defaultDesc = "",
                    action = () => IsPsychic = true
                };
            }

        }
        
        public void ExposeData() {
            
            Scribe_References.Look(ref thing, "Thing");
            Scribe_Values.Look(ref randomId, "randomID");
            Scribe_Values.Look(ref IsPsychic, "IsPsychic");
            
        }

    }
}