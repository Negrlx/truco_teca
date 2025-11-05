using System;
using System.Collections.Generic;
using System.Linq;
using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers.Calls
{
    internal class RecetaCall
    {
        public Dictionary<Player, List<Card>> RecetaPlayers { get; private set; } = new();

        public void DetectarReceta(List<Player> players)
        {
            foreach (var player in players)
            {
                var hand = player.GetHand();
                bool tiene15 = hand.Any(c => TrucoRules.GetCardRank(c) == 15);
                bool tiene16 = hand.Any(c => TrucoRules.GetCardRank(c) == 16);

                if (tiene15 && tiene16)
                {
                    RecetaPlayers[player] = new List<Card>(hand);
                    Console.WriteLine($"Jugador {player.Name} tiene receta.");
                }
            }
        }

        public int ResolverReceta(int[] teamScores)
        {
            if (RecetaPlayers.Count == 0)
                return 0;

            foreach (var kvp in RecetaPlayers)
            {
                var player = kvp.Key;
                var cartas = kvp.Value;
                Console.WriteLine($"\nEquipo {player.Team} tiene receta, estas son sus cartas:");
                foreach (var carta in cartas)
                    Console.WriteLine($"- {carta}");

                Console.WriteLine($"Por la receta se lleva 5pts.");
                teamScores[player.Team - 1] += 5;
            }

            return 5 * RecetaPlayers.Count;
        }
    }
}
