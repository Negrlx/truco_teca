using System;

namespace truco_teca.Deck.Data.Controllers
{
    public class TrucoCall
    {
        private int currentLevel = 0; 
        private int currentValue = 1; 
        private int callingTeam;
        private int respondingTeam;
        private bool isActive = false;
        private bool isAccepted = false;

        public bool IsAccepted => isAccepted;
        public bool WasRejected { get; private set; } = false;

        public int CallingTeam => callingTeam;

        public void CallTruco(int team)
        {
            if (isActive)
            {
                Console.WriteLine("Ya hay un canto activo.");
                return;
            }

            callingTeam = team;
            respondingTeam = team == 1 ? 2 : 1;
            currentLevel = 1;
            currentValue = 3;
            isActive = true;

            Console.WriteLine($"\nEquipo {callingTeam} canta Truco.");
            AskResponse();
        }

        public void AskResponse()
        {
            Console.WriteLine($"Equipo {respondingTeam}, ¿Quieren? (Quiero / No quiero / Quiero y Retruco / Quiero y Vale 9 / Quiero y Vale Partido)");
            string? input = Console.ReadLine()?.Trim().ToLower();

            switch (input)
            {
                case "quiero":
                    isAccepted = true;
                    Console.WriteLine($"Equipo {respondingTeam} acepta el {GetCallName(currentLevel)}. La mano ahora vale {currentValue} puntos.");
                    SwapVoice();
                    break;

                case "no quiero":
                    // Marcar rechazo y no hacer Reset aquí: Game se encargará de procesarlo
                    WasRejected = true;
                    isActive = false;
                    isAccepted = false;
                    Console.WriteLine($"Equipo {respondingTeam} no acepta. Equipo {callingTeam} ganará {GetRejectionPoints()} punto(s).");
                    break;

                case "quiero y retruco":
                    Escalate(2, 6);
                    break;

                case "quiero y vale 9":
                    Escalate(3, 9);
                    break;

                case "quiero y vale partido":
                    Escalate(4, -1);
                    break;

                default:
                    Console.WriteLine("Respuesta inválida. Intenta de nuevo.");
                    AskResponse();
                    break;
            }
        }

        private void Escalate(int level, int value)
        {
            if (level != currentLevel + 1)
            {
                Console.WriteLine($"No puedes cantar {GetCallName(level)} directamente. Debes seguir el orden de escalamiento.");
                AskResponse();
                return;
            }

            currentLevel = level;
            currentValue = value;
            Console.WriteLine($"Equipo {respondingTeam} acepta y canta {GetCallName(level)}. ¿Equipo {callingTeam}, quieren?");
            SwapVoice();
            AskResponse();
        }

        private void SwapVoice()
        {
            int temp = callingTeam;
            callingTeam = respondingTeam;
            respondingTeam = temp;
        }

        public int ResolveWinner(int winningTeam, int pointsToWin, int[] teamScore)
        {
            if (WasRejected)
            {
                int puntos = GetRejectionPoints();
                Reset();
                return puntos;
            }

            // Si no hubo canto activo o no fue aceptado, mano vale 1 por defecto
            if (!isActive || !isAccepted)
            {
                Reset();
                return 1;
            }

            if (currentLevel == 4)
            {
                Reset();
                return -1;
            }

            int puntosFinales = currentValue;
            Reset();
            return puntosFinales;
        }

        private int GetRejectionPoints()
        {
            return currentLevel switch
            {
                1 => 1,
                2 => 3,
                3 => 6,
                4 => 9,
                _ => 1
            };
        }

        private void Reset()
        {
            currentLevel = 0;
            currentValue = 1;
            isActive = false;
            isAccepted = false;
            WasRejected = false;
        }

        private string GetCallName(int level)
        {
            return level switch
            {
                1 => "Truco",
                2 => "Retruco",
                3 => "Vale 9",
                4 => "Vale Partido",
                _ => "Canto desconocido"
            };
        }
    }
}