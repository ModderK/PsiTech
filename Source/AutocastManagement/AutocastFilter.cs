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

using System;
using System.Collections.Generic;
using PsiTech.Psionics;
using RimWorld;
using UnityEngine;
using Verse;

namespace PsiTech.AutocastManagement {
    public abstract class AutocastFilter : IExposable {
        
        public PsiTechAbility Ability;
        public Pawn User;
        public FilterTargetType FilterTargetType;
        public IntRange TargetRange;
        
        private const string FilterTargetTypeEnemiesKey = "PsiTech.AutocastManagement.FilterTargetTypeEnemies";
        private const string FilterTargetTypeHostilesKey = "PsiTech.AutocastManagement.FilterTargetTypeHostiles";
        private const string FilterTargetTypeFriendliesKey = "PsiTech.AutocastManagement.FilterTargetTypeFriendlies";
        private const string FilterTargetTypeAnyKey = "PsiTech.AutocastManagement.FilterTargetTypeAny";
        
        protected const float YSeparation = 5f;
        protected const float XSeparation = 5f;
        
        protected const float OptionHeight = 22f;
        
        public abstract Pawn GetBestTarget(List<Pawn> targets);
        
        public abstract void Draw(Rect inRect);

        protected void FilterForTargetType(ref List<Pawn> targets) {
            switch(FilterTargetType) {
                
                case FilterTargetType.Enemies:
                    targets.RemoveAll(target => !target.Faction.HostileTo(User.Faction));
                    break;
                
                case FilterTargetType.Hostiles:
                    targets.RemoveAll(target =>
                        !target.Faction.HostileTo(User.Faction) && !target.InAggroMentalState);
                    break;
                
                case FilterTargetType.Friendlies:
                    targets.RemoveAll(target =>
                        !target.Faction.AllyOrNeutralTo(User.Faction));
                    break;
                
                case FilterTargetType.Any:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected IEnumerable<Widgets.DropdownMenuElement<string>> GenerateTargetTypeOptions() {

            foreach (FilterTargetType targetType in Enum.GetValues(typeof(FilterTargetType))) {

                yield return new Widgets.DropdownMenuElement<string> {
                    payload = "",
                    option = new FloatMenuOption(LocalizeTargetType(targetType),
                        () => { FilterTargetType = targetType; })
                };

            }

        }
        
        protected static string LocalizeTargetType(FilterTargetType type) {
            
            switch(type) {
                
                case FilterTargetType.Enemies:
                    return FilterTargetTypeEnemiesKey.Translate();
                
                case FilterTargetType.Hostiles:
                    return FilterTargetTypeHostilesKey.Translate();
                
                case FilterTargetType.Friendlies:
                    return FilterTargetTypeFriendliesKey.Translate();
                
                case FilterTargetType.Any:
                    return FilterTargetTypeAnyKey.Translate();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected virtual void PostExpose(){ }
        
        public void ExposeData() {
            Scribe_References.Look(ref Ability, "Ability");
            Scribe_References.Look(ref User, "User");
            Scribe_Values.Look(ref FilterTargetType, "FilterTargetType");
            Scribe_Values.Look(ref TargetRange, "TargetRange");
            PostExpose();
        }
        
    }
}