using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers.Calls
{
    public static class EnvidoCall
    {
        //TODO: Implement perico and perica count
        public static int GetPoints(List<Card> cards)
        {
            var vira = TrucoRules.GetVira();

            var suitGroups = cards.GroupBy(card => card.Suit);
            int maxPoints = 0;
            foreach (var group in suitGroups)
            {
                int points;
                if (group.Count() == 2)
                {
                    points = 20 + group.Sum(card => card.ViraValue);
                }
                else
                {
                    points = group.Max(card => card.ViraValue);
                }
                if (points > maxPoints)
                {
                    maxPoints = points;
                }
            }
            return maxPoints;
        }

        public static bool HasEnvido(List<Card> cards)
        {
            var points = GetPoints(cards);
            return points >= 20;
        }
    }
}
