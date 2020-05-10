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

using PsiTech.Interface;
using RimWorld;
using Verse;

namespace PsiTech.Utility {
    [StaticConstructorOnStartup]
    public static class PsiTechAlienRacesPatcherUtility {
        
        static PsiTechAlienRacesPatcherUtility() {

            if (!PsiTechSettings.Get().PatchAllRaces) return;
            
            // This approach has some serious start-up time implications but at least it doesn't impact
            // runtime performance. I'd choose seconds on a load over constant overhead any day.
            var allThings = DefDatabase<ThingDef>.AllDefs;

            foreach (var def in allThings) {
                if (!(def.inspectorTabsResolved?.Any(tab => tab is ITab_Pawn_Needs) ?? false) ||
                    def.inspectorTabsResolved.Any(tab => tab is ITab_Pawn_Psi)) continue;
                
                def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psi)));
            }

        }
        
    }
}