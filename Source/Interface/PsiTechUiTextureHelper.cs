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

using UnityEngine;
using Verse;

namespace PsiTech.Interface {
    
    [StaticConstructorOnStartup]
    public static class PsiTechUiTextureHelper {
        
        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");
        public static readonly Texture2D ReorderUp = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderUp");
        public static readonly Texture2D ReorderDown = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderDown");
        
        public static readonly Texture2D FocusNodes1 = ContentFinder<Texture2D>.Get("UI/FocusNodes1");
        public static readonly Texture2D FocusNodes2 = ContentFinder<Texture2D>.Get("UI/FocusNodes2");
        public static readonly Texture2D FocusNodes2pre1 = ContentFinder<Texture2D>.Get("UI/FocusNodes2pre1");
        public static readonly Texture2D FocusNodes2pre2 = ContentFinder<Texture2D>.Get("UI/FocusNodes2pre2");
        public static readonly Texture2D FocusNodes3 = ContentFinder<Texture2D>.Get("UI/FocusNodes3");
        public static readonly Texture2D FocusNodes3pre = ContentFinder<Texture2D>.Get("UI/FocusNodes3pre");
        public static readonly Texture2D EnergyNodes1 = ContentFinder<Texture2D>.Get("UI/EnergyNodes1");
        public static readonly Texture2D EnergyNodes2 = ContentFinder<Texture2D>.Get("UI/EnergyNodes2");
        public static readonly Texture2D EnergyNodes2pre1 = ContentFinder<Texture2D>.Get("UI/EnergyNodes2pre1");
        public static readonly Texture2D EnergyNodes2pre2 = ContentFinder<Texture2D>.Get("UI/EnergyNodes2pre2");
        public static readonly Texture2D EnergyNodes3 = ContentFinder<Texture2D>.Get("UI/EnergyNodes3");
        public static readonly Texture2D EnergyNodes3pre = ContentFinder<Texture2D>.Get("UI/EnergyNodes3pre");

        static PsiTechUiTextureHelper() {
            SetFilterModes();
        }
        
        public static void SetFilterModes() {
                FocusNodes1.mipMapBias = -1f;
                FocusNodes2.mipMapBias = -1f;
                FocusNodes2pre1.mipMapBias = -1f;
                FocusNodes2pre2.mipMapBias = -1f;
                FocusNodes3.mipMapBias = -1f;
                FocusNodes3pre.mipMapBias = -1f;
                EnergyNodes1.mipMapBias = -1f;
                FocusNodes1.mipMapBias = -1f;
                EnergyNodes2.mipMapBias = -1f;
                EnergyNodes2pre1.mipMapBias = -1f;
                EnergyNodes2pre2.mipMapBias = -1f;
                EnergyNodes3.mipMapBias = -1f;
                EnergyNodes3pre.mipMapBias = -1f;
        }
        
    }
}