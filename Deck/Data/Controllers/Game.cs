using System;
using System.Collections.Generic;
using System.Linq;
using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers
{
    public class Game
    {
        private List<Player> players;
        private int pointsToWin;
        private int[] teamScore;
        private int dealerIndex;

        public Game(List<Player> players, int pointsToWin)
        {
            if (players.Count != 2 && players.Count != 4)
                throw new ArgumentException("Solo se permite 1vs1 o 2vs2.");
            this.players = players;
            this.pointsToWin = pointsToWin;
            this.teamScore = new int[2];
            this.dealerIndex = 0;
        }

        public void Start()
        {
            while (teamScore[0] < pointsToWin && teamScore[1] < pointsToWin)
            {
                Console.WriteLine($"\n=== Nueva mano (Dealer: {players[dealerIndex].Name}) ===");
                Stack<Card> deck = Dealer.PrepareDeck();
                Card vira = Dealer.Deal(deck);
                TrucoRules.SetVira(vira);

                Console.WriteLine($"\nVira: {vira}\n");

                DealHands(deck);
                TrucoCall truco = new TrucoCall();
                int winningTeam = PlayHand(truco);

                if (truco.WasRejected)
                {
                    // winningTeam ya debe ser el equipo que cantó (CallingTeam)
                    int puntos = truco.ResolveWinner(winningTeam, pointsToWin, teamScore);
                    if (puntos == -1) break;
                    teamScore[winningTeam - 1] += puntos;
                    Console.WriteLine($"\nEquipo {winningTeam} gana la mano por rechazo. Marcador: Equipo1 {teamScore[0]} - Equipo2 {teamScore[1]}");
                    dealerIndex = (dealerIndex + 1) % players.Count;
                    continue;
                }

                int puntosGanados = truco.ResolveWinner(winningTeam, pointsToWin, teamScore);
                if (puntosGanados == -1) break;

                teamScore[winningTeam - 1] += puntosGanados;
                Console.WriteLine($"\nEquipo {winningTeam} gana la mano. Marcador: Equipo1 {teamScore[0]} - Equipo2 {teamScore[1]}");

                dealerIndex = (dealerIndex + 1) % players.Count;
            }

            int ganador = teamScore[0] > teamScore[1] ? 1 : 2;
            Console.WriteLine($"\n=== ¡Equipo {ganador} gana el juego! ===");
        }

        private void DealHands(Stack<Card> deck)
        {
            var hands = Dealer.DealHands(deck, players.Count);
            for (int i = 0; i < players.Count; i++)
                players[i].SetHand(hands[i]);
        }

        private List<Player> GetTurnOrder()
        {
            var equipo1 = players.Where(p => p.Team == 1).ToList();
            var equipo2 = players.Where(p => p.Team == 2).ToList();
            List<Player> orden = new();
            int max = Math.Max(equipo1.Count, equipo2.Count);
            for (int i = 0; i < max; i++)
            {
                if (i < equipo1.Count) orden.Add(equipo1[i]);
                if (i < equipo2.Count) orden.Add(equipo2[i]);
            }
            return orden;
        }

        private int PlayHand(TrucoCall truco)
        {
            int[] roundsWon = new int[2];
            List<Player> turnOrder = GetTurnOrder();
            int firstTurnIndex = dealerIndex;

            for (int round = 1; round <= 3; round++)
            {
                Console.WriteLine($"\n--- Ronda {round} ---");
                Dictionary<Player, Card> playedCards = new();

                for (int i = 0; i < turnOrder.Count; i++)
                {
                    Player current = turnOrder[(firstTurnIndex + i) % turnOrder.Count];
                    Console.WriteLine($"\nTurno de {current.Name}:");
                    current.ShowHand();

                    Console.WriteLine("¿Deseas cantar Truco? (s/n)");
                    string? canto = Console.ReadLine()?.Trim().ToLower();
                    if (canto == "s")
                        truco.CallTruco(current.Team);

                    if (truco.WasRejected)
                    {
                        return truco.CallingTeam;
                    }

                    Card played = SelectCard(current);
                    playedCards[current] = played;
                    Console.WriteLine($"{current.Name} juega {played}");

                    // (la comprobación de antes ya no es necesaria, pero voy a quemar esta mierda si sigue sin servir)
                    if (truco.WasRejected)
                    {
                        return truco.CallingTeam;
                    }
                }

                int winnerTeam = TrucoRules.DetermineRoundWinner(playedCards);
                if (winnerTeam == 0)
                {
                    Console.WriteLine("Parda.");
                    if (round == 2 && roundsWon.Contains(1))
                        return roundsWon[0] > roundsWon[1] ? 1 : 2;
                }
                else
                {
                    Console.WriteLine($"Equipo {winnerTeam} gana la ronda");
                    roundsWon[winnerTeam - 1]++;
                    if (roundsWon[winnerTeam - 1] == 2)
                        return winnerTeam;

                    Player firstInTeam = players.First(p => p.Team == winnerTeam);
                    firstTurnIndex = turnOrder.IndexOf(firstInTeam);
                }
            }

            return roundsWon[0] >= roundsWon[1] ? 1 : 2;
        }

        private Card SelectCard(Player player)
        {
            while (true)
            {
                Console.WriteLine("Selecciona la carta:");
                for (int i = 0; i < player.HandCount; i++)
                    Console.WriteLine($"{i + 1}: {player.GetCardAt(i)}");
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int choice) &&
                    choice >= 1 && choice <= player.HandCount)
                    return player.PlayCardAt(choice - 1);
                Console.WriteLine("Entrada inválida.");
            }
        }
    }
}
