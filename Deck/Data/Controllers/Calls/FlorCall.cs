using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers.Calls
{
    public class FlorCall
    {
        // === FLOR ===

        public Dictionary<Player, List<int>> FlorPlayers { get; private set; } = new();
        public List<Player> FlorQuemadas { get; private set; } = new();

        public static bool HasFlor(List<Card> hand)
        {
            var card15 = hand.FirstOrDefault(c => TrucoRules.GetCardRank(c) == 15);
            var card16 = hand.FirstOrDefault(c => TrucoRules.GetCardRank(c) == 16);

            var suitGroups = hand.GroupBy(c => c.Suit)
                                 .Where(g => g.Count() >= 2)
                                 .ToList();

            foreach (var group in suitGroups)
            {
                if (group.Count() == 3)
                    return true;

                if (group.Count() == 2 && (card15 != null || card16 != null))
                    return true;
            }

            return false;
        }

        public static int FlorPoints(List<Card> hand)
        {
            var card15 = hand.FirstOrDefault(c => TrucoRules.GetCardRank(c) == 15);
            var card16 = hand.FirstOrDefault(c => TrucoRules.GetCardRank(c) == 16);

            var suitGroups = hand.GroupBy(c => c.Suit)
                                 .Where(g => g.Count() >= 2)
                                 .ToList();

            foreach (var group in suitGroups)
            {
                var cards = group.ToList();
                if (cards.Count == 3)
                    return cards.Sum(GetFlorValue) + 20;

                if (cards.Count == 2 && (card15 != null || card16 != null))
                {
                    int special = card15 != null ? 29 : 30;
                    return cards.Sum(GetFlorValue) + special;
                }
            }

            return 0;
        }

        private static int GetFlorValue(Card card)
        {
            return (card.Value >= 10 && card.Value <= 12) ? 0 : card.Value;
        }

        public void RegistrarFlor(Player player)
        {
            if (!FlorPlayers.ContainsKey(player))
                FlorPlayers[player] = new List<int>();
        }

        public void CallFlor(Player player, int round)
        {
            if (!FlorPlayers.ContainsKey(player))
            {
                Console.WriteLine($"{player.Name} no tiene flor registrada.");
                return;
            }

            if (round == 1 || round == 2)
                Console.WriteLine($"{player.Name} canta: Flor.");
            else if (round == 3)
                Console.WriteLine($"{player.Name} canta: Presento Flor.");

            FlorPlayers[player].Add(round);
        }

        public void CantarALey(Player player)
        {
            if (!FlorPlayers.ContainsKey(player))
            {
                Console.WriteLine($"{player.Name} no tiene flor registrada.");
                return;
            }

            Console.WriteLine($"{player.Name} canta: A Ley.");
            FlorPlayers[player].Add(0); // Marca intención de reservar flor
        }

        public void ReportarFlorQuemada()
        {
            foreach (var kvp in FlorPlayers)
            {
                var rondas = kvp.Value;
                if (!rondas.Contains(1) && !rondas.Contains(0)) continue; // No cantó ni reservó

                if (!rondas.Contains(1) || !rondas.Contains(2) || !rondas.Contains(3))
                {
                    Console.WriteLine($"Flor quemada: {kvp.Key.Name} no cantó flor en todas las rondas.");
                    FlorQuemadas.Add(kvp.Key);
                }
            }
        }

        public int ResolverFlor(Dictionary<Player, int> florValues, int[] teamScores, int pointsToWin)
        {
            var activas = FlorPlayers.Keys.Except(FlorQuemadas).ToList();
            if (activas.Count == 0)
                return 0;

            var equipos = activas.GroupBy(p => p.Team).ToList();

            if (equipos.Count == 1)
            {
                int equipo = equipos[0].Key;
                Console.WriteLine($"Equipo {equipo} gana 3 puntos por Flor.");
                return 3;
            }

            var jugador1 = equipos[0].OrderByDescending(p => florValues[p]).First();
            var jugador2 = equipos[1].OrderByDescending(p => florValues[p]).First();

            Console.WriteLine($"{jugador1.Name} vs {jugador2.Name} — ¿Equipo {jugador2.Team} quiere envidar la Flor? (s/n)");
            string? input = Console.ReadLine()?.Trim().ToLower();

            int flor1 = florValues[jugador1];
            int flor2 = florValues[jugador2];

            if (input == "s")
            {
                int ganador = flor1 >= flor2 ? jugador1.Team : jugador2.Team;
                Console.WriteLine($"Flor envidada. Equipo {ganador} gana 5 puntos.");
                return 5;
            }
            else
            {
                Console.WriteLine($"Flor no envidada. Equipo {jugador1.Team} gana 4 puntos.");
                return 4;
            }
        }

    }
}
