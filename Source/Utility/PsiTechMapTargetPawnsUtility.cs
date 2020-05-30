using System.Collections.Generic;
using Verse;

namespace PsiTech.Utility {
    [StaticConstructorOnStartup]
    public class PsiTechMapTargetPawnsUtility : MapComponent {
        
        public static readonly Dictionary<Map, PsiTechMapTargetPawnsUtility> TargetPawnUtilities = new Dictionary<Map, PsiTechMapTargetPawnsUtility>();
        public readonly HashSet<Pawn> PotentialTargetPawns = new HashSet<Pawn>();

        public PsiTechMapTargetPawnsUtility(Map map) : base(map) {
            if (TargetPawnUtilities.ContainsKey(map)) return;
            
            TargetPawnUtilities.Add(map, this);
        }

        public void Notify_PawnSpawned(Pawn pawn) {
            if (PotentialTargetPawns.Contains(pawn) || pawn.needs.mood == null) return;

            PotentialTargetPawns.Add(pawn);
        }

        public void Notify_PawnDespawned(Pawn pawn) {
            if (!PotentialTargetPawns.Contains(pawn)) return;

            PotentialTargetPawns.Remove(pawn);
        }
        
    }
}