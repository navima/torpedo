// <copyright file="Program.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#pragma warning disable SA1000 // Keywords should be spaced correctly
#pragma warning disable NI1004 // Do not use string literals in code
#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1305 // Specify IFormatProvider

// This one conflicts with the "static should come first" one.
#pragma warning disable SA1202 // Elements should be ordered by access

namespace NationalInstruments
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class Program
    {
        public static readonly Regex RxPositionOrientation = new("([A-Z]+)([0-9]+)(UP|DOWN|LEFT|RIGHT)");
        public static readonly Regex RxPosition = new("([A-Z]+)([0-9]+)");

        public static void Main()
        {
            new Program().Run();
        }

#nullable enable
        private static string[] BoardToString(ShipPart?[,] board)
        {
            string[] rows = new string[board.GetLength(0)];
            for (int i = 0; i < board.GetLength(0); i++)
            {
                StringBuilder sb = new();
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    var elem = board[j, i];
                    sb.Append(elem switch
                    {
                        { Alive: true } => "██",
                        { Alive: false } => "><",
                        _ => (i + j) % 2 == 1 ? "░░" : "▒▒",
                    });
                }

                rows[i] = sb.ToString();
            }

            return rows;
        }

        private static string[] BoardToString(EHitResult?[,] board)
        {
            string[] rows = new string[board.GetLength(0)];
            for (int i = 0; i < board.GetLength(0); i++)
            {
                StringBuilder sb = new();
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    var elem = board[j, i];
                    sb.Append(elem switch
                    {
                        EHitResult.Miss => "__",
                        EHitResult.Hit => "Hi",
                        EHitResult.Sink => "Si",
                        _ => (i + j) % 2 == 1 ? "░░" : "▒▒",
                    });
                }

                rows[i] = sb.ToString();
            }

            return rows;
        }

#nullable restore
        private static void PrintBoard(string[] board)
        {
            Console.Write(" ");
            Array.ForEach(Enumerable.Range('A', board.Length).Select(x => ((char)x).ToString().PadRight(2)).ToArray(), Console.Write);
            Console.WriteLine();
            Array.ForEach(board.Select((x, i) => (i + 1).ToString() + x).ToArray(), Console.WriteLine);
        }

        private static bool TryParsePosition(string input, out Position output)
        {
            var matches = RxPosition.Matches(input.ToUpperInvariant());
            if (matches.Count < 1)
            {
                output = default;
                return false;
            }

            var groups = matches[0].Groups;
            var inCol = AlphaStringToInt(groups[1].Value);
            var inRow = int.Parse(groups[2].Value) - 1;

            output = new Position(inCol, inRow);
            return true;
        }

        private static bool TryParsePositionOrientation(string input, out Position position, out EOrientation orientation)
        {
            var matches = RxPositionOrientation.Matches(input.ToUpperInvariant());
            if (matches.Count < 1)
            {
                position = default;
                orientation = default;
                return false;
            }

            var groups = matches[0].Groups;
            var inCol = AlphaStringToInt(groups[1].Value);
            var inRow = int.Parse(groups[2].Value) - 1;
            var inOrientation = (EOrientation)Enum.Parse(typeof(EOrientation), groups[3].Value.CapitalizeInvariant());

            orientation = inOrientation;
            position = new Position(inCol, inRow);
            return true;
        }

        private static int IntPow(int x, uint pow)
        {
            int result = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                {
                    result *= x;
                }

                x *= x;
                pow >>= 1;
            }

            return result;
        }

        private static int AlphaStringToInt(string value)
        {
            value = value.Trim().ToUpperInvariant();
            int res = 0;
            for (uint i = (uint)(value.Length - 1); i >= 0; i--)
            {
                res += (value[(int)i] - 'A') * IntPow('A' - 'Z', i);
            }

            return res;
        }

        public void Run()
        {
            IDataStore dataStore = new InMemoryDataStore();
            TorpedoService torpedoGameInstance = new(dataStore, (9, 9));

            torpedoGameInstance.GameStateChanged += this.TorpedoService_GameStateChanged;

            Console.WriteLine("Initialized service");
            void RequestPlayerPlaceShips(Player player)
            {
                while (torpedoGameInstance.ShipsToPlace(player).Any())
                {
                    Console.WriteLine("Placing Ships.");
                    Console.WriteLine("Available ships:");
                    torpedoGameInstance.ShipsToPlace(player).ToList().ForEach(ship => Console.WriteLine(ship));

                    Console.WriteLine("Your board:");
                    PrintBoard(BoardToString(torpedoGameInstance.GetBoard(player)));

                    var ship = torpedoGameInstance.ShipsToPlace(player).First();
                    Console.WriteLine($"Please place your {ship.Size} length ship ([A-Z][0-9][Up|Down|Left|Right])");
                    Console.WriteLine(new string(' ', Console.WindowWidth));

                    if (TryParsePositionOrientation(Console.ReadLine(), out var pos, out var ori))
                    {
                        ship.Orientation = ori;
                        torpedoGameInstance.TryPlaceShip(player, ship, pos);
                    }
                    else
                    {
                        Console.WriteLine("Bad format!");
                    }
                }
            }

            void RequestPlayerSinkShip(Player player)
            {
                Console.WriteLine("Your board:");
                PrintBoard(BoardToString(torpedoGameInstance.GetBoard(player)));
                Console.WriteLine("Enemy Board:");
                PrintBoard(BoardToString(torpedoGameInstance.GetHitBoard(player)));

                bool success = false;
                while (!success)
                {
                    Console.WriteLine("Please select where to hit ([A-Z][0-9])");
                    var input = Console.ReadLine();
                    if (TryParsePosition(input, out var position))
                    {
                        success = torpedoGameInstance.TryHit(player, position, out var res);
                        if (!success)
                        {
                            Console.WriteLine($"Invalid position! {position}");
                        }
                        else
                        {
                            Console.WriteLine(res);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Bad format!");
                    }
                }
            }

            while (torpedoGameInstance.GameState != EGameState.None)
            {
                switch (torpedoGameInstance.GameState)
                {
                    case EGameState.AddingPlayers:
                        {
                            Console.WriteLine("Adding alice");
                            torpedoGameInstance.AddPlayer(new Player("alice"));
                            Console.WriteLine("Ading bob");
                            torpedoGameInstance.AddPlayer(new Player("bob"));
                            Console.WriteLine("Calling finish adding players");
                            torpedoGameInstance.FinishAddingPlayers();
                        }

                        break;
                    case EGameState.PlacingShips:
                        {
                            var player = torpedoGameInstance.CurrentPlayer;
                            Console.WriteLine($"Current Player: {player}");
                            RequestPlayerPlaceShips(player);
                            torpedoGameInstance.FinishPlacingShips(player);
                        }

                        break;
                    case EGameState.SinkingShips:
                        {
                            var player = torpedoGameInstance.CurrentPlayer;
                            Console.WriteLine($"Current Player: {player}");
                            RequestPlayerSinkShip(player);
                        }

                        break;
                    case EGameState.GameOver:
                        {
                            Console.WriteLine("Game Over!");
                            Console.WriteLine("Results:");
                            Array.ForEach(torpedoGameInstance.Players.ToArray(), x => Console.WriteLine($"{x}: " + (torpedoGameInstance.IsPlayerDead(x) ? "Lose" : "Win")));
                            goto Rest;
                        }

                    default:
                        break;
                }
            }

        Rest:
            Console.WriteLine("Bye!");
        }

        private void TorpedoService_GameStateChanged(object sender, StateChangedEventArgs e)
        {
            Console.WriteLine("STATE CHANGED: " + e);
        }
    }
}
