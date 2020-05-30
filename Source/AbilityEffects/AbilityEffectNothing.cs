using Verse;

namespace PsiTech.AbilityEffects {
    public class AbilityEffectNothing : AbilityEffect {
        
        public override bool TryDoEffectOnPawn(Pawn user, Pawn target) {
            return true;
        }
        
    }
}