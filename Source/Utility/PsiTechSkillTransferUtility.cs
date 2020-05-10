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
using Verse;

namespace PsiTech.Utility {
    public class PsiTechSkillTransferUtility : GameComponent {

        private static List<TransferPair> _activeTransferPairs = new List<TransferPair>();

        public PsiTechSkillTransferUtility (Game game) { }
        
        public static List<TransferPair> GetActiveTransferPairsForReceiver(Pawn receiver) {
            return _activeTransferPairs.FindAll(pair => pair.Receiver == receiver);
        }

        public static bool TryAddNewTransferPair(TransferPair pair) {
            if (!_activeTransferPairs.Any(existing => existing.Initiator == pair.Initiator)) {
                _activeTransferPairs.Add(pair);
                return true;
            }
            else {
                Log.Warning("PsiTech tried to activate a new transfer pair with initiator " + pair.Initiator.Name +
                            " but they had already initiated a pairing");
                return false;
            }
        }

        public static void RemoveTransferPairWithInitiator(Pawn initiator) {
            _activeTransferPairs.RemoveAll(pair => pair.Initiator == initiator);
        }

        public override void ExposeData() {
            Scribe_Collections.Look(ref _activeTransferPairs, "activeTransferPairs", LookMode.Deep);
        }
    }

    public struct TransferPair : IExposable {
        public Pawn Initiator;
        public Pawn Receiver;
        public float SkillTransferAmount;

        public Pawn GetOtherPawn(Pawn pawn) {
            if (pawn == Initiator) {
                return Receiver;
            }
            if (pawn == Receiver) {
                return Initiator;
            }
            
            Log.Warning("PsiTech tried to get the other pawn in a transfer pair but given pawn wasn't a receiver");
            return null;
        }

        public void ExposeData() {
            Scribe_Deep.Look(ref Initiator, "Initiator");
            Scribe_Deep.Look(ref Receiver, "Receiver");
            Scribe_Values.Look(ref SkillTransferAmount, "SkillTransferAmount");
        }
    }
}