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
using System.Linq;
using Verse;

namespace PsiTech.TargetValidators {
    public abstract class TargetValidator {

        public virtual bool IsValidTarget(Pawn user, Pawn target) {
            return target.needs.mood != null && PostIsValidTarget(user, target);
        }

        protected abstract bool PostIsValidTarget(Pawn user, Pawn target);

        public virtual Pawn RandomTargetFromLists(Pawn user, List<Pawn> targets) {
            var possible = targets.Where(target => IsValidTarget(user, target)).ToList();

            return possible.Any() ? possible.RandomElement() : null;
        }

        public virtual Pawn SelectBestTargetFromLists(Pawn user, List<Pawn> targets) {
            return RandomTargetFromLists(user, targets);
        }

    }
}