using System;
using System.Collections.Generic;
using System.Linq;
using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers
{
    public class EnvidoCall
    {
        //Suma las cartas con pintas iguales, suma el perico y la perica, y si no hay nada devuelve la carta mas alta
        public static int EnvidoPoints(List<Card> hand)
        {
            if (hand == null || hand.Count != 3)
                throw new ArgumentException("La mano debe contener exactamente 3 cartas.");

            var card15 = hand.FirstOrDefault(c => TrucoRules.GetCardRank(c) == 15);
            var card16 = hand.FirstOrDefault(c => TrucoRules.GetCardRank(c) == 16);

            if (card15 != null || card16 != null)
            {
                int specialValue = card15 != null ? 29 : 30;
                var otherCards = hand.Where(c => c != card15 && c != card16).ToList();
                int bestValue = otherCards.Select(c => GetEnvidoValue(c)).DefaultIfEmpty(0).Max();
                return specialValue + bestValue;
            }

            var suitGroups = hand.GroupBy(c => c.Suit)
                                 .Where(g => g.Count() > 1)
                                 .Select(g => g.Select(GetEnvidoValue).OrderByDescending(v => v).ToList())
                                 .ToList();

            if (suitGroups.Count > 0)
            {
                var bestGroup = suitGroups.Max(g => g.Take(2).Sum());
                return bestGroup + 20;
            }

            return hand.Select(GetEnvidoValue).Max();
        }

        private static int GetEnvidoValue(Card card)
        {
            return (card.Value >= 10 && card.Value <= 12) ? 0 : card.Value;
        }

    }
}
