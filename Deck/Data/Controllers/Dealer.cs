using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers
{
    public static class Dealer
    {
        public static Stack<Card> PrepareDeck()
        {
            List<Card> shuffled = new List<Card>(CardDeck.PredefinedCards);
            Shuffler.Shuffle(shuffled);
            return new Stack<Card>(shuffled);
        }

        public static Card Deal(Stack<Card> deck)
        {
            if (deck.Count == 0)
                throw new InvalidOperationException("No hay cartas suficientes en el mazo.");
            return deck.Pop();
        }

        public static Card RevealVira(Stack<Card> deck)
        {
            if (deck.Count == 0)
                throw new InvalidOperationException("No hay cartas suficientes en el mazo.");
            return deck.Peek(); // devuelve la carta superior sin sacarla
        }

        public static List<List<Card>> DealHands(Stack<Card> deck, int players)
        {
            if (players <= 0)
                throw new ArgumentException("Debe haber al menos un jugador.");

            List<List<Card>> hands = new();
            for (int i = 0; i < players; i++)
            {
                hands.Add(new List<Card>());
            }
            for (int round = 0; round < 3; round++)
            {
                for (int i = 0; i < players; i++)
                {
                    if (deck.Count > 0)
                        hands[i].Add(deck.Pop());
                    else
                        throw new InvalidOperationException("No hay cartas suficientes en el mazo.");
                }
            }
            return hands;
        }
    }
}
