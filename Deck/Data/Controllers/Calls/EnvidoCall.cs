using System;
using System.Collections.Generic;
using System.Linq;
using truco_teca.Deck.Data.Models;
using truco_teca.Deck.Data.Controllers.Calls;

namespace truco_teca.Deck.Data.Controllers
{
    public class EnvidoCall
    {
        // Estado del canto
        public bool IsActive { get; private set; } = false;
        public bool IsAccepted { get; private set; } = false;
        public bool WasRejected { get; private set; } = false;

        // Equipos involucrados
        public int CallingTeam { get; private set; }
        public int RespondingTeam { get; private set; }

        // Valores del canto
        public int CurrentValue { get; private set; } = 2;
        public int EscalationLevel { get; private set; } = 1;

        // Configuración del juego
        private readonly int pointsToWin;

        // Flor y jugador que responde
        private Player respondingPlayer;
        private FlorCall flor;
        
        public EnvidoCall(int pointsToWin)
        {
            this.pointsToWin = pointsToWin;
        }

        public void CallEnvido(int team, Player respondingPlayer, FlorCall flor)
        {
            if (IsActive)
            {
                Console.WriteLine("Ya hay un canto de Envido activo.");
                return;
            }

            CallingTeam = team;
            RespondingTeam = team == 1 ? 2 : 1;
            IsActive = true;
            EscalationLevel = 1;
            CurrentValue = 2;

            this.respondingPlayer = respondingPlayer;
            this.flor = flor;

            Console.WriteLine($"\nEquipo {CallingTeam} canta Envido.");
            AskResponse();
        }

        private void AskResponse()
        {
            bool tieneFlor = flor.FlorPlayers.ContainsKey(respondingPlayer);

            Console.WriteLine($"Equipo {RespondingTeam}, ¿Quieren? (Quiero / No quiero / Quiero y Envido / Quiero y Envido la Falta{(tieneFlor ? " / A las flores no" : "")})");
            string? input = Console.ReadLine()?.Trim().ToLower();

            switch (input)
            {
                case "quiero":
                    IsAccepted = true;
                    Console.WriteLine($"Equipo {RespondingTeam} acepta el Envido. El ganador se llevará {CurrentValue} puntos.");

                    if (tieneFlor)
                    {
                        flor.FlorQuemadas.Add(respondingPlayer);
                        Console.WriteLine($"Flor quemada: {respondingPlayer.Name} aceptó el Envido.");
                    }
                    break;

                case "no quiero":
                    if (tieneFlor)
                    {
                        flor.FlorQuemadas.Add(respondingPlayer);
                        Console.WriteLine($"Flor quemada: {respondingPlayer.Name} No dijo 'A las flores no mi vida'.");
                    }

                    WasRejected = true;
                    IsActive = false;
                    IsAccepted = false;
                    int rejectionPoints = EscalationLevel switch
                    {
                        1 => 1,
                        2 => 2,
                        3 => 4,
                        _ => 1
                    };
                    Console.WriteLine($"Equipo {RespondingTeam} no acepta. Equipo {CallingTeam} gana {rejectionPoints} punto(s).");
                    CurrentValue = rejectionPoints;
                    break;

                case "quiero y envido":
                    Escalate(2, 4);
                    break;

                case "quiero y envido la falta":
                    Escalate(3, -1);
                    break;

                case "a las flores no":
                    if (!tieneFlor)
                    {
                        Console.WriteLine("Solo puedes decir 'A las flores no' si tienes Flor.");
                        AskResponse();
                        return;
                    }

                    WasRejected = true;
                    IsActive = false;
                    IsAccepted = false;
                    CurrentValue = 1;
                    Console.WriteLine($"Equipo {RespondingTeam} rechaza el Envido con 'A las flores no'. Se protege la Flor.");
                    break;

                default:
                    Console.WriteLine("Respuesta inválida. Intenta de nuevo.");
                    AskResponse();
                    break;
            }
        }

        private void Escalate(int level, int value)
        {
            EscalationLevel = level;
            CallingTeam = RespondingTeam;
            RespondingTeam = CallingTeam == 1 ? 2 : 1;

            if (value == -1)
            {
                Console.WriteLine($"\nEquipo {CallingTeam} canta Envido la Falta.");
            }
            else
            {
                CurrentValue = value;
                Console.WriteLine($"\nEquipo {CallingTeam} sube el canto a Quiero y Envido.");
            }

            AskResponse();
        }

        public int ResolveWinner(int winningTeam, int[] teamScores)
        {
            if (WasRejected)
                return CurrentValue;

            if (EscalationLevel == 3)
            {
                int losingTeam = winningTeam == 1 ? 2 : 1;
                int falta = pointsToWin - teamScores[losingTeam - 1];
                Console.WriteLine($"Envido la Falta: Equipo {winningTeam} gana {falta} puntos.");
                return falta;
            }

            Console.WriteLine($"Equipo {winningTeam} gana el Envido y suma {CurrentValue} puntos.");
            return CurrentValue;
        }

        public static int EnvidoPoints(List<Card> hand)
        {
            if (hand == null || hand.Count != 3)
                throw new ArgumentException("La mano debe contener exactamente 3 cartas.");

            var perico = hand.FirstOrDefault(c => TrucoRules.GetCardRank(c) == 15);
            var perica = hand.FirstOrDefault(c => TrucoRules.GetCardRank(c) == 16);

            if (perico != null || perica != null)
            {
                int specialValue = perico != null ? 29 : 30;
                var otherCards = hand.Where(c => c != perico && c != perica).ToList();
                int bestValue = otherCards.Select(c => GetEnvidoValue(c)).DefaultIfEmpty(0).Max();
                return specialValue + bestValue;
            }

            var suitGroups = hand.GroupBy(c => c.Suit)
                                 .Where(g => g.Count() > 1)
                                 .Select(g => g.Select(GetEnvidoValue).OrderByDescending(v => v).ToList())
                                 .ToList();

            if (suitGroups.Count > 0)
            {
                var bestGroup = suitGroups.Max(g => g.Take(2).Sum());
                return bestGroup + 20;
            }

            return hand.Select(GetEnvidoValue).Max();
        }

        private static int GetEnvidoValue(Card card)
        {
            return (card.Value >= 1 && card.Value <= 7) ? card.Value : 0;
        }
    }
}
