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
using PsiTech.Misc;
using PsiTech.Psionics;
using Verse;

namespace PsiTech.Utility {
    public static class ExtensionMethods {

        public static PsiTechManager Manager;

        public static PsiTechTracker PsiTracker(this Pawn pawn) {
            return Manager[pawn];
        }

        public static PsiTechEquipmentTracker PsiEquipmentTracker(this Thing thing) {
            return Manager[thing];
        }

        public static HashSet<Pawn> PotentialPsiTargets(this Map map) {
            return PsiTechMapTargetPawnsUtility.TargetPawnUtilities.TryGetValue(map, out var utility) ? utility.PotentialTargetPawns : null;
        }

        public static float TicksToHours(this int numTicks) {
            return numTicks / 2500f;
        }

        public static bool Contains(this IntRange range, float value) {
            return value >= range.min && value <= range.max;
        }

    }
}