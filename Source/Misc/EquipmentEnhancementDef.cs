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

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PsiTech.Misc {
    public class EquipmentEnhancementDef : Def {

        public List<StatModifier> RangedMods = new List<StatModifier>();
        public List<StatModifier> MeleeMods = new List<StatModifier>();
        public List<StatModifier> ShellMods = new List<StatModifier>();
        public List<StatModifier> OverheadMods = new List<StatModifier>();

        public static Dictionary<StatDef, float> RangedModDict = new Dictionary<StatDef, float>();
        public static Dictionary<StatDef, float> MeleeModDict = new Dictionary<StatDef, float>();
        public static Dictionary<StatDef, float> ShellModDict = new Dictionary<StatDef, float>();
        public static Dictionary<StatDef, float> OverheadModDict = new Dictionary<StatDef, float>();

        public override void ResolveReferences() {
            RangedMods.ForEach(mod => RangedModDict.Add(mod.stat, mod.value));
            MeleeMods.ForEach(mod => MeleeModDict.Add(mod.stat, mod.value));
            ShellMods.ForEach(mod => ShellModDict.Add(mod.stat, mod.value));
            OverheadMods.ForEach(mod => OverheadModDict.Add(mod.stat, mod.value));
        }
    }
}