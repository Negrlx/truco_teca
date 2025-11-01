using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers
{
    public class Player
    {
        public string Name { get; private set; }
        public int Team { get; private set; } // 1 o 2
        private List<Card> hand;

        public Player(string name, int team)
        {
            Name = name;
            Team = team;
            hand = new List<Card>();
        }

        public void SetHand(List<Card> cards)
        {
            hand = new List<Card>(cards);
        }

        public int HandCount => hand.Count;

        public void ShowHand()
        {
            Console.WriteLine($"{Name} tiene:");
            for (int i = 0; i < hand.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {hand[i]}");
            }
        }

        public Card GetCardAt(int index)
        {
            if (index < 0 || index >= hand.Count)
                throw new ArgumentOutOfRangeException();
            return hand[index];
        }

        public Card PlayCardAt(int index)
        {
            if (index < 0 || index >= hand.Count)
                throw new ArgumentOutOfRangeException();
            Card c = hand[index];
            hand.RemoveAt(index);
            return c;
        }
    }
}
