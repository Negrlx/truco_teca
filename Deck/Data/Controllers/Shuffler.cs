namespace truco_teca.Deck.Data.Controllers
{
    internal class Shuffler
    {
        private static readonly Random rng = new();

        public static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
