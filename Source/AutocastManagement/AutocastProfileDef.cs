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
using PsiTech.Psionics;
using Verse;

namespace PsiTech.AutocastManagement {

    // This whole file is full of poltergeists
    public class AutocastProfileDef : Def {

        public PsiTechAbilityDef Ability;
        public FilterTargetType TargetType;
        public IntRange TargetRange;

        public float MinSuccessChance;
        public AutocastFilterSelectorDef Selector;
        public bool InvertSelector;

        public int MinTargetsInRange;
        
        public List<AdditionalFilterProfile> AdditionalFilterProfiles = new List<AdditionalFilterProfile>();
    }

    // The game can't load data into a struct from a def, hence this ghost
    // I suppose it has something to do with structs being immutable.
    public class AdditionalFilterProfile {
        public AdditionalTargetFilterDef Def;
        public bool Invert;
        public float Threshold;
    }

}