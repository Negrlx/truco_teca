using System;
using System.Collections.Generic;
using System.Linq;
using truco_teca.Deck.Data.Controllers.Calls;
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

                RecetaCall receta = new RecetaCall();
                receta.DetectarReceta(players);

                Dictionary<Player, int> envidoValues = players.ToDictionary(
                    p => p,
                    p => EnvidoCall.EnvidoPoints(p.GetHand())
                );

                Console.WriteLine("\nValores de Envido:");
                foreach (var kvp in envidoValues.OrderBy(p => p.Key.Team))
                {
                    Console.WriteLine($"Equipo {kvp.Key.Team} - {kvp.Key.Name}: {kvp.Value} puntos");
                }

                // === FLOR ===
                FlorCall flor = new FlorCall();
                Dictionary<Player, int> florValues = players.ToDictionary(
                    p => p,
                    p => FlorCall.HasFlor(p.GetHand()) ? FlorCall.FlorPoints(p.GetHand()) : 0
                );

                foreach (var player in players)
                {
                    if (FlorCall.HasFlor(player.GetHand()))
                        flor.RegistrarFlor(player);
                }

                Console.WriteLine("\nValores de Flor:");
                foreach (var kvp in florValues.Where(f => f.Value > 0).OrderBy(p => p.Key.Team))
                {
                    Console.WriteLine($"Equipo {kvp.Key.Team} - {kvp.Key.Name}: {kvp.Value} puntos");
                }

                // === CANTOS ===
                TrucoCall truco = new TrucoCall();
                EnvidoCall envido = new EnvidoCall(pointsToWin);

                int winningTeam = PlayHand(truco, envido, envidoValues, flor, florValues);

                // === RESOLVER ENVIDO ===
                if (envido.IsAccepted)
                {
                    int team1Max = players.Where(p => p.Team == 1).Select(p => envidoValues[p]).Max();
                    int team2Max = players.Where(p => p.Team == 2).Select(p => envidoValues[p]).Max();
                    int envidoWinner = team1Max >= team2Max ? 1 : 2;
                    teamScore[envidoWinner - 1] += envido.ResolveWinner(envidoWinner, teamScore);
                }
                else if (envido.WasRejected)
                {
                    teamScore[envido.CallingTeam - 1] += envido.ResolveWinner(envido.CallingTeam, teamScore);
                }

                // === RESOLVER FLOR ===
                flor.ReportarFlorQuemada();
                int florPuntos = flor.ResolverFlor(florValues, teamScore, pointsToWin);
                if (florPuntos > 0)
                {
                    int equipoGanador = flor.FlorPlayers
                        .Where(kvp => !flor.FlorQuemadas.Contains(kvp.Key))
                        .GroupBy(kvp => kvp.Key.Team)
                        .OrderByDescending(g => g.Max(p => florValues[p.Key]))
                        .First().Key;

                    teamScore[equipoGanador - 1] += florPuntos;
                    Console.WriteLine($"Equipo {equipoGanador} gana {florPuntos} punto(s) por Flor.");
                }

                // === RESOLVER TRUCO ===
                if (truco.WasRejected)
                {
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
                receta.ResolverReceta(teamScore);
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

        private int PlayHand(TrucoCall truco, EnvidoCall envido, Dictionary<Player, int> envidoValues, FlorCall flor, Dictionary<Player, int> florValues)
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

                    Console.WriteLine("¿Deseas hacer un canto? (s/n)");
                    string? canto = Console.ReadLine()?.Trim().ToLower();
                    if (canto == "s")
                    {
                        Console.WriteLine("¿Qué quieres cantar? (envido / truco / flor / a ley / reportar flor quemada)");
                        string? tipoCanto = Console.ReadLine()?.Trim().ToLower();

                        if (tipoCanto == "envido" && round == 1 && !envido.IsActive)
                            envido.CallEnvido(current.Team, current, flor);
                        else if (tipoCanto == "envido" && round != 1)
                            Console.WriteLine("El Envido solo puede cantarse en la primera ronda.");
                        else if (tipoCanto == "truco")
                            truco.CallTruco(current.Team);
                        else if (tipoCanto == "flor")
                            flor.CallFlor(current, round);
                        else if (tipoCanto == "a ley" && round == 1)
                            flor.CantarALey(current);
                        else if (tipoCanto == "reportar flor quemada")
                            flor.ReportarFlorQuemada();
                        else
                            Console.WriteLine("Canto inválido.");

                    }

                    if (truco.WasRejected)
                        return truco.CallingTeam;

                    Card played = SelectCard(current);
                    playedCards[current] = played;
                    Console.WriteLine($"{current.Name} juega {played}");

                    if (truco.WasRejected)
                        return truco.CallingTeam;
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

        public void RunSimulation()
        {
            Console.WriteLine("\n=== MODO SIMULACRO ===\n");

            // === Vira manual ===
            Console.Write("Ingresa la Vira (ej: 7 Gold, 1 Sword): ");
            Card vira = ParseCardInput(Console.ReadLine());
            TrucoRules.SetVira(vira);
            Console.WriteLine($"Vira seleccionada -> {vira}\n");

            // === Cartas manuales por jugador ===
            foreach (var p in players)
            {
                Console.WriteLine($"Ingresa las 3 cartas de {p.Name} separadas por coma (ej: 1 Gold, 3 Sword, 7 Cup)");
                Console.Write("> ");
                string input = Console.ReadLine();
                var cards = input.Split(',')
                                 .Select(x => ParseCardInput(x.Trim()))
                                 .ToList();
                p.SetHand(cards);
            }

            // === Lógica inicial igual que Start() ===
            RecetaCall receta = new RecetaCall();
            receta.DetectarReceta(players);

            Dictionary<Player, int> envidoValues = players.ToDictionary(
                p => p,
                p => EnvidoCall.EnvidoPoints(p.GetHand())
            );

            FlorCall flor = new FlorCall();
            Dictionary<Player, int> florValues = players.ToDictionary(
                p => p,
                p => FlorCall.HasFlor(p.GetHand()) ? FlorCall.FlorPoints(p.GetHand()) : 0
            );

            // IMPORTANTE: registrar Flor igual que en Start()
            foreach (var p in players)
            {
                if (FlorCall.HasFlor(p.GetHand()))
                    flor.RegistrarFlor(p);
            }

            TrucoCall truco = new TrucoCall();
            EnvidoCall envido = new EnvidoCall(pointsToWin);

            // === Mostrar manos para verificación (útil en simulación) ===
            Console.WriteLine("\n=== Manos cargadas ===");
            foreach (var p in players)
            {
                Console.WriteLine($"{p.Name}: {string.Join(", ", p.GetHand())}");
            }

            Console.WriteLine("\n=== Comienza la simulación de la mano ===\n");

            // === Jugar mano manual ===
            int ganador = PlayHand(truco, envido, envidoValues, flor, florValues);

            Console.WriteLine($"\n=== Fin de simulación: ganó Equipo {ganador} ===");
        }


        private Card ParseCardInput(string raw)
        {
            string[] parts = raw.Trim().Split(' ');
            int value = int.Parse(parts[0]);
            CardSuit suit = Enum.Parse<CardSuit>(parts[1], true);
            return new Card(value, suit);
        }


    }
}
