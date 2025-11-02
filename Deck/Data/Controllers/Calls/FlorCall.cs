using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers.Calls
{
    internal class FlorCall
    {
        public static bool HasFlor(List<Card> cards)
        {
            if (cards == null || cards.Count != 3) return false;

            // Tres de la misma pinta
            if (cards.All(c => c.Suit == cards[0].Suit)) return true;

            var vira = TrucoRules.GetVira();
            if (vira == null) return false;

            // Detectar perico (11 de la vira o 12 si la vira es 11)
            var pericoCard = cards.FirstOrDefault(c => (c.Value == 11 && c.Suit == vira.Suit) || (vira.Value == 11 && c.Value == 12 && c.Suit == vira.Suit));
            if (pericoCard != null)
            {
                var others = cards.Where(c => !ReferenceEquals(c, pericoCard)).ToList();
                if (others.Count == 2 && others[0].Suit == others[1].Suit) return true;
            }

            // Detectar perica (10 de la vira o 12 si la vira es 10)
            var pericaCard = cards.FirstOrDefault(c => (c.Value == 10 && c.Suit == vira.Suit) || (vira.Value == 10 && c.Value == 12 && c.Suit == vira.Suit));
            if (pericaCard != null)
            {
                var others = cards.Where(c => !ReferenceEquals(c, pericaCard)).ToList();
                if (others.Count == 2 && others[0].Suit == others[1].Suit) return true;
            }

            return false;
        }
    }
}
