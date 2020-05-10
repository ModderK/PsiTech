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

using PsiTech.Misc;
using PsiTech.Utility;
using RimWorld;
using Verse;

namespace PsiTech.AbilityEffects {
    public class AbilityEffectThought : AbilityEffect {

        public ThoughtDef Thought;

        public override bool TryDoEffectOnPawn(Pawn user, Pawn target) {
            var existingThought = target.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(Thought);
            if (existingThought != null && existingThought is PsiTechThoughtMemory thought) {
                thought.Renew();
                thought.Multiplier = GetModifier(user, target);
            }

            var newThought = (PsiTechThoughtMemory) ThoughtMaker.MakeThought(Thought);
            newThought.Multiplier = user.PsiTracker().AbilityModifier;
            newThought.otherPawn = user;
            target.needs?.mood?.thoughts?.memories?.TryGainMemory(newThought);

            return true; // There's no easy way to tell whether the thought actually took
        }
    }
}