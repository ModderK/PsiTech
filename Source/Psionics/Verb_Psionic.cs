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

using Verse;

namespace PsiTech.Psionics {
    public class Verb_Psionic : Verb {
        
        public PsiTechAbility Ability;
        private Pawn target;

        public Verb_Psionic() {
            verbProps = new VerbProperties();
        }

        public void DoCast(Pawn t) {
            target = t;
            TryCastShot();
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ) {
            var pawn = (Pawn)targ;
            return pawn != null && root.InHorDistOf(targ.Cell, Ability.Def.Range) && Ability.CanHitTarget((Pawn) targ);
        }

        protected override bool TryCastShot() {
            
            if (target != null && target?.Map != caster.Map) return false;

            if (target != null) {
                Ability.DoAbilityOnTarget(target);
            }
            else {
                Ability.DoAbility();
            }

            return true;

        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref Ability, "Ability");
            Scribe_References.Look(ref caster, "caster"); // Game doesn't save this for some reason
        }
    }
}