using truco_teca.Deck.Data.Controllers;
using truco_teca.Deck.Data.Models;

namespace truco_teca
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Truco Venezolano ===\n");
            Console.WriteLine("Seleccione el modo de juego:");
            Console.WriteLine("1. 1 vs 1");
            Console.WriteLine("2. 2 vs 2");
            Console.Write("Opción: ");

            string? modeInput = Console.ReadLine();
            int playersCount = modeInput == "2" ? 4 : 2;

            List<Player> players = new();
            for (int i = 0; i < playersCount; i++)
            {
                Console.Write($"Nombre del jugador {i + 1}: ");
                string? name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name)) name = $"Jugador {i + 1}";
                int team = (i % 2) + 1;
                players.Add(new Player(name, team));
            }

            Console.Write("\n¿A cuántos puntos se juega? ");
            int pointsToWin;
            while (!int.TryParse(Console.ReadLine(), out pointsToWin) || pointsToWin <= 0)
            {
                Console.Write("Valor inválido. Ingrese un número mayor a 0: ");
            }

            Console.WriteLine($"\nIniciando partida a {pointsToWin} puntos...\n");

            Game game = new Game(players, pointsToWin);
            game.Start();

            Console.WriteLine("\nJuego finalizado. Presiona cualquier tecla para salir.");
            Console.ReadKey();
        }
    }
}
