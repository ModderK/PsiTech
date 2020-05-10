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

namespace PsiTech.Misc {
    public class MoteLink : MoteDualAttached {
        private Material beam;
        private Vector3 start;
        private Vector3 target;

        public void SetupLink(Texture2D texture, Vector3 startPoint, Vector3 targetPoint) {
            beam = MaterialPool.MatFrom(texture, ShaderDatabase.MoteGlow, Color.white);
            start = startPoint;
            target = targetPoint;
            
            start.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            target.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        }

        public override void Draw() {
            if (beam == null || start == target) return;
            
            var alpha = Alpha;
            if (alpha <= 0.0) return;
            
            var color = instanceColor;
            color.a *= alpha;

            if (color != beam.color)
                beam = MaterialPool.MatFrom((Texture2D) beam.mainTexture, ShaderDatabase.MoteGlow,
                    color);
            if (Mathf.Abs(start.x - target.x) < 0.00999999977648258 &&
                Mathf.Abs(start.z - target.z) < 0.00999999977648258)
                return;
            
            var pos = (start + target) / 2f;
            var z = (start - target).MagnitudeHorizontal();
            
            var q = Quaternion.LookRotation(start - target);
            var s = new Vector3(1f, 1f, z);
            var matrix = new Matrix4x4();
            matrix.SetTRS(pos, q, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, beam, 0);
        }
    }
}