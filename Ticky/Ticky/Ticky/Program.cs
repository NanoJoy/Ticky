using System;
using System.Linq;

namespace Ticky
{
    class Program
    {
        static void Main(string[] args)
        {
            var height = int.Parse(args[0]);
            var width = int.Parse(args[1]);
            var winCount = int.Parse(args[2]);
            var firstPlayer = args[3][0];

            var again = true;
            var game = new Game(width, height, winCount);

            while (again)
            {
                game = new Game(width, height, winCount, game.Cache);

                Console.WriteLine($"{winCount} in a row to win.");

                if (args[4] == "R")
                {
                    Console.WriteLine("First move of each player chosen randomly to make it interesting.");
                    game.DoRandomMoves();
                    game.DoRandomMoves();
                    game.DoRandomMoves();
                    Console.WriteLine(game.GetDisplay());
                }

                if (firstPlayer != 'X')
                {
                    Console.WriteLine("Computer moves first as O.");
                    if (game.DoComputerMove())
                    {
                        Console.WriteLine("Computer moves perfectly.");
                    }
                    else
                    {
                        Console.WriteLine("Computer moves randomly.");
                    }
                    Console.WriteLine(game.GetDisplay());
                }
                else
                {
                    Console.WriteLine("You move first as X.");
                }
                while (!game.IsOver)
                {
                    var validMove = false;

                    while (!validMove)
                    {
                        try
                        {
                            Console.Write("Enter move: ");
                            game.AcceptMove(Console.ReadLine());
                            Console.WriteLine(game.GetDisplay());
                            validMove = true;
                        }
                        catch (InvalidMoveException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                if (game.Winner != null)
                {
                    Console.WriteLine($"Winner: {game.Winner}");
                }
                else
                {
                    Console.WriteLine($"Draw");
                    Console.WriteLine();
                }

                Console.WriteLine("Again (y/n):");
                again = Console.ReadLine().Trim().Equals("Y", StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
