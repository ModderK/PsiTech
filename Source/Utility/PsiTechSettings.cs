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

using UnityEngine;
using Verse;

namespace PsiTech.Utility {
    public class PsiTechSettings : ModSettings {

        //Use PsiTechSettings.Get().setting to refer to settings
        public bool EnablePsychicFactionRaids = true;
        public float EssenceLossPerPart => essenceLossPerPartInternal / 100f;
        public bool PatchAllRaces;

        private int essenceLossPerPartInternal = 10;

        private string essenceLossBuffer;
        
        private const string EnablePsychicFactionRaidsKey = "PsiTech.Utility.EnablePsychicFactionRaids";
        private const string EssenceLossPerPartKey = "PsiTech.Utility.EssenceLossPerPart";
        private const string EssenceLossPerPartDescKey = "PsiTech.Utility.EssenceLossPerPartDesc";
        private const string PatchAllRacesKey = "PsiTech.Utility.PatchAllRaces";
        private const string PatchAllRacesDescKey = "PsiTech.Utility.PatchAllRacesDesc";

        public static PsiTechSettings Get() {
            return LoadedModManager.GetMod<PsiTech>().GetSettings<PsiTechSettings>();
        }

        public void DoWindowContents(Rect rect) {
            var options = new Listing_Standard();
            options.Begin(rect);

            options.CheckboxLabeled(EnablePsychicFactionRaidsKey.Translate(), ref EnablePsychicFactionRaids);
            
            options.GapLine();

            options.Label(EssenceLossPerPartKey.Translate(), -1f, EssenceLossPerPartDescKey.Translate());
            options.TextFieldNumeric(ref essenceLossPerPartInternal, ref essenceLossBuffer, 0, 100);
            
            options.GapLine();
            
            options.CheckboxLabeled(PatchAllRacesKey.Translate(), ref PatchAllRaces, PatchAllRacesDescKey.Translate());
            
            options.End();
        }
		
        public override void ExposeData() {
            Scribe_Values.Look(ref EnablePsychicFactionRaids, "EnablePsychicFactionRaids", true);
            Scribe_Values.Look(ref essenceLossPerPartInternal, "essenceLossPerPartInternal", 10);
            Scribe_Values.Look(ref PatchAllRaces, "PatchAllRaces");
        }
        
    }
}