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

using PsiTech.Training;
using PsiTech.Utility;
using RimWorld;
using UnityEngine;

namespace PsiTech.Interface {
    public class ITab_Pawn_Psi : ITab {

        public override bool IsVisible => (SelPawn?.PsiTracker()?.Activated ?? false) ||
                                          (Trainer?.InnerPawn?.PsiTracker()?.Activated ?? false);
        
        private BuildingPsiTechTrainer Trainer => SelThing as BuildingPsiTechTrainer;
        
        public ITab_Pawn_Psi() {
            size = PawnPsiCard.PsiCardSize + new Vector2(17f, 17f) * 2f;
            labelKey = "PsiTech.Interface.TabPsi";
        }

        protected override void FillTab() {
            size = PawnPsiCard.PsiCardSize + new Vector2(17f, 17f) * 2f;
            PawnPsiCard.DrawPsiCard(new Rect(17f, 17f, PawnPsiCard.PsiCardSize.x, PawnPsiCard.PsiCardSize.y), SelPawn ?? Trainer.InnerPawn, SelPawn == null);
        }
        
    }
}