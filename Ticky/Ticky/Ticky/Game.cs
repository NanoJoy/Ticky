using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ticky
{
    public class Game
    {

        private int Width { get; }

        private int Height { get; }

        private int WinCount { get; }

        private char[,] Board { get; }

        public IDictionary<(string board, char player), (bool exists, int y, int x)> Cache { get; }

        public bool IsOver => IsDraw() || CheckWinner() != null;

        public char? Winner => CheckWinner();

        public Game(int width, int height, int winCount, IDictionary<(string board, char player), (bool exists, int y, int x)> cache)
        {
            Board = new char[height, width];
            Width = width;
            Height = height;
            WinCount = winCount;
            Cache = cache;

            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    Board[i, j] = '-';
                }
            }
        }

        public Game(char[,] board, int width, int height, int winCount, IDictionary<(string board, char player), (bool exists, int y, int x)> cache)
        {
            Board = board;
            Width = width;
            Height = height;
            WinCount = winCount;
            Cache = cache;
        }

        public Game(int width, int height, int winCount)
        {
            Width = width;
            Height = height;
            WinCount = winCount;
            Board = new char[height, width];
            Cache = new Dictionary<(string board, char player), (bool exists, int y, int x)>();

            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    Board[i, j] = '-';
                }
            }
        }

        public void AcceptMove(string move)
        {
            if (move.Length != 2)
            {
                throw new InvalidMoveException("Move should consist of Letter followed by Number with no space.");
            }

            if (move[0] > 'Z' || move[0] < 'A')
            {
                throw new InvalidMoveException("First character of move is not a capital letter.");
            }

            if (move[1] > '9' || move[1] < '1')
            {
                throw new InvalidMoveException("Second character of move is not a digit.");
            }

            AcceptMove(move[0] - 'A', move[1] - '1');
        }

        public void AcceptMove(int x, int y)
        {
            if (x < 0 || x > Width - 1)
            {
                throw new InvalidMoveException("Horizontal coordinate out of bounds.");
            }
            if (y < 0 || y > Height - 1)
            {
                throw new InvalidMoveException("Vertical coordinate out of bounds.");
            }

            if (Board[y, x] != '-')
            {
                throw new InvalidMoveException("Spot already taken.");
            }

            Board[y, x] = 'X';

            if (IsOver)
            {
                return;
            }

            DoComputerMove();
        }

        public void DoRandomMoves()
        {
            DoRandomMove('X');
            DoRandomMove('O');
        }

        public void DoRandomMove(char player)
        {
            var openSpots = GetOpenSpots();
            var rando = new Random();
            var pos = rando.Next(0, openSpots.Count - 1);
            var (b, a) = openSpots[pos];

            Board[b, a] = player;
        }

        public bool DoComputerMove()
        {
            var (exists, b, a) = GetOptimalMove('O');

            if (exists)
            {
                Board[b, a] = 'O';
                return true;
            }
            else
            {
                DoRandomMove('O');
                return false;
            }
        }

        public (bool exists, int y, int x) GetOptimalMove(char player, int depth = 1)
        {
            if (Cache.ContainsKey((GetStr(), player)))
            {
                return Cache[(GetStr(), player)];
            }

            var otherPlayer = player == 'X' ? 'O' : 'X';

            var possibleMoves = GetOpenSpots();
            var subGames = new List<(int y, int x, Game game)>();

            foreach (var (y, x) in possibleMoves)
            {
                var board = new char[Height, Width];

                for (var i = 0; i < Height; i++)
                {
                    for (var j = 0; j < Width; j++)
                    {
                        board[i, j] = Board[i, j];
                    }
                }

                board[y, x] = player;
                subGames.Add((y, x, new Game(board, Width, Height, WinCount, Cache)));
            }

            var winningGame = subGames.FirstOrDefault(g => g.game.Winner == player);

            if (winningGame != default)
            {
                Cache.Add((GetStr(), player), (true, winningGame.y, winningGame.x));
                return (true, winningGame.y, winningGame.x);
            }

            if (depth == WinCount + 1)
            {
                return (false, -1, -1);
            }

            subGames = subGames.Where(g => !g.game.IsDraw()).ToList();

            if (!subGames.Any())
            {
                Cache.Add((GetStr(), player), (false, -1, -1));
                return (false, -1, -1);
            }

            var gg = subGames.Where(g => !g.game.GetOptimalMove(otherPlayer, depth).exists)
                .FirstOrDefault(g => g.game.GetOptimalMove(player, depth + 1).exists);

            if (gg != default)
            {
                Cache.Add((GetStr(), player), (true, gg.y, gg.x));
                return (true, gg.y, gg.x);
            }

            return (false, -1, -1);
        }

        private IList<(int y, int x)> GetOpenSpots()
        {
            var openSpots = new List<(int y, int x)>();

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (Board[i, j] == '-')
                    {
                        openSpots.Add((i, j));
                    }
                }
            }

            return openSpots;
        }

        private char? CheckWinner()
        {
            if (CheckWinner('X'))
            {
                return 'X';
            }
            if (CheckWinner('O'))
            {
                return 'O';
            }
            return null;
        }

        private bool CheckWinner(char player)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (Board[i, j] == player)
                    {
                        if (j > WinCount - 2)
                        {
                            var win = true;
                            for (var k = 0; k < WinCount; k++)
                            {
                                if (Board[i, j - k] != player)
                                {
                                    win = false;
                                    break;
                                }
                            }
                            if (win) return true;
                        }
                        if (i > WinCount - 2)
                        {
                            var win = true;
                            for (var k = 0; k < WinCount; k++)
                            {
                                if (Board[i - k, j] != player)
                                {
                                    win = false;
                                    break;
                                }
                            }
                            if (win) return true;
                        }
                        if (i > WinCount - 2 && j > WinCount - 2)
                        {
                            var win = true;
                            for (var k = 0; k < WinCount; k++)
                            {
                                if (Board[i - k, j - k] != player)
                                {
                                    win = false;
                                    break;
                                }
                            }
                            if (win) return true;
                        }
                        if (i > WinCount - 2 && j < Width - (WinCount - 1))
                        {
                            var win = true;
                            for (var k = 0; k < WinCount; k++)
                            {
                                if (Board[i - k, j + k] != player)
                                {
                                    win = false;
                                    break;
                                }
                            }
                            if (win) return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsDraw()
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (Board[i, j] == '-')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string GetDisplay()
        {
            var display = new StringBuilder(" ");
            for (int i = 0; i < Width; i++)
            {
                display.Append((char)('A' + i));
            }
            display.Append("\n");

            for (int i = 0; i < Height; i++)
            {
                display.Append(i + 1);
                for (int j = 0; j < Width; j++)
                {
                    display.Append(Board[i, j]);
                }
                display.Append("\n");
            }

            return display.ToString();
        }

        private string GetStr()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    builder.Append(Board[i, j]);
                }
            }
            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Game otherGame))
            {
                return false;
            }
            if (otherGame.Width != Width || otherGame.Height != Height || otherGame.WinCount != WinCount)
            {
                return false;
            }
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    if (otherGame.Board[i, j] != Board[j, i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
