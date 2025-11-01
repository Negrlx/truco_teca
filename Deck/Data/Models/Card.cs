using System.ComponentModel;

namespace truco_teca.Deck.Data.Models
{
    public class Card(int value, CardSuit suit)
    {
        public int Value { get; } = value;
        public CardSuit Suit { get; } = suit;
        public int ViraValue { get; } = value > 10 ? 0 : value;

        public override string ToString() => $"{Value} de {Suit.GetDescription()}";
        
    }
}
