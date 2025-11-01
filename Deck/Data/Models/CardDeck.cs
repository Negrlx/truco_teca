namespace truco_teca.Deck.Data.Models
{
    public static class CardDeck
    {
        private static readonly int[] ValidCardValues = { 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 };
        public static List<Card> PredefinedCards { get; } = GenerateDeck();
        private static List<Card> GenerateDeck()
        {
            var cards = new List<Card>();
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                foreach (var value in ValidCardValues)
                {
                    cards.Add(new Card(value, suit));
                }
            }
            return cards;
        }
    }
}

