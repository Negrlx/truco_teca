using System.ComponentModel;
using System.Reflection;

namespace truco_teca.Deck.Data.Models
{

    public enum CardSuit
    {
        [Description("Basto")]
        Stick,

        [Description("Espada")]
        Sword,

        [Description("Oro")]
        Gold,

        [Description("Copas")]
        Cup
    }

    public static class EnumExtensions
    {
        public static String GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}
