

namespace NationalInstruments
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class Program
    {
        public static void Main()
        {
            new Program().Run();
        }

        public readonly static Regex rxPositionOrientation = new Regex(@"([a-z]+)([0-9]+)(up|down|left|right)");
        public readonly static Regex rxPosition = new Regex(@"([a-z]+)([0-9]+)");

        public void Run()
        {
            DataStore dataStore = new();
            TorpedoService _torpedoGameInstance = new(dataStore, (9, 9));

            _torpedoGameInstance.GameStateChanged += TorpedoService_GameStateChanged;

            Console.WriteLine("Initialized service");
            Action<Player> RequestPlayerPlaceShips = (player) =>
            {
                while (_torpedoGameInstance.ShipsToPlace(player).Count() > 0)
                {
                    Console.WriteLine("Placing Ships.");
                    Console.WriteLine("Available ships:");
                    _torpedoGameInstance.ShipsToPlace(player).ToList().ForEach(ship => Console.WriteLine(ship));

                    Console.WriteLine("Your board:");
                    printBoard(boardToString(_torpedoGameInstance.GetBoard(player)));

                    var ship = _torpedoGameInstance.ShipsToPlace(player).First();
                    Console.WriteLine($"Please place your {ship.Size} length ship ([A-Z][0-9][Up|Down|Left|Right])");
                    Console.WriteLine(new string(' ', Console.WindowWidth));
                    
                    if(TryParsePositionOrientation(Console.ReadLine(), out var pos, out var ori))
                    {
                        ship.Orientation = ori;
                        _torpedoGameInstance.TryPlaceShip(player, ship, pos);
                    }
                    else
                    {
                        Console.WriteLine("Bad format!");
                    }
                }
            };
            Action<Player> RequestPlayerSinkShip = (player) =>
            {
                Console.WriteLine("Your board:");
                printBoard(boardToString(_torpedoGameInstance.GetBoard(player)));
                Console.WriteLine("Enemy Board:");
                printBoard(boardToString(_torpedoGameInstance.GetHitBoard(player)));

                bool success = false;
                while (!success)
                {
                    Console.WriteLine("Please select where to hit ([A-Z][0-9])");
                    var input = Console.ReadLine();
                    if (TryParsePosition(input, out var position))
                    {
                        success = _torpedoGameInstance.TryHit(player, position, out var res);
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
            };
            while (_torpedoGameInstance.GameState != EGameState.None)
            {
                switch (_torpedoGameInstance.GameState)
                {
                    case EGameState.AddingPlayers:
                        {
                            Console.WriteLine("Adding alice");
                            _torpedoGameInstance.AddPlayer(new Player("alice"));
                            Console.WriteLine("Ading bob");
                            _torpedoGameInstance.AddPlayer(new Player("bob"));
                            Console.WriteLine("Calling finish adding players");
                            _torpedoGameInstance.FinishAddingPlayers();
                        }
                        break;
                    case EGameState.PlacingShips:
                        {
                            var player = _torpedoGameInstance.CurrentPlayer;
                            Console.WriteLine($"Current Player: {player}");
                            RequestPlayerPlaceShips(player);
                            _torpedoGameInstance.FinishPlacingShips(player);
                        }
                        break;
                    case EGameState.SinkingShips:
                        {
                            var player = _torpedoGameInstance.CurrentPlayer;
                            Console.WriteLine($"Current Player: {player}");
                            RequestPlayerSinkShip(player);
                        }
                        break;
                    case EGameState.GameOver:
                        {
                            Console.WriteLine("Game Over!");
                            Console.WriteLine("Results:");
                            Array.ForEach(_torpedoGameInstance.Players.ToArray(), x => Console.WriteLine($"{x.ToString()}: " + (_torpedoGameInstance.IsPlayerDead(x) ? "Lose" : "Win")));
                            goto Rest;
                        }
                        break;
                    default:
                        break;
                }
            }
        Rest:
            Console.WriteLine("Bye!");
        }

#nullable enable
        private void TorpedoService_GameStateChanged(object? sender, StateChangedEventArgs e)
        {
            Console.WriteLine("STATE CHANGED: " + e);
        }
        private static string[] boardToString(ShipPart?[,] board)
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
        private static string[] boardToString(EHitResult?[,] board)
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
        private static void printBoard(string[] board)
        {
            Console.Write(" ");
            Array.ForEach(Enumerable.Range('A', board.Length).Select(x => ((char)x).ToString().PadRight(2)).ToArray(), Console.Write);
            Console.WriteLine();
            Array.ForEach(board.Select((x, i) => (i + 1).ToString() + x).ToArray(), Console.WriteLine);
        }
        private static bool TryParsePosition(string input, out Position output)
        {
            var matches = rxPosition.Matches(input.ToLower());
            if (matches.Count < 1)
            {
                output = new Position();
                return false;
            }
            var groups = matches[0].Groups;
            var inCol = ((short)groups[1].Value[0]) - (short)'a';
            var inRow = int.Parse(groups[2].Value) - 1;

            output = new Position(inCol, inRow); ;
            return true;
        }
        private static bool TryParsePositionOrientation(string input, out Position position, out EOrientation orientation)
        {
            var matches = rxPositionOrientation.Matches(input.ToLower());
            if (matches.Count < 1)
            {
                position = new Position();
                orientation = EOrientation.Up;
                return false;
            }
            var groups = matches[0].Groups;
            var inCol = ((short)groups[1].Value[0]) - (short)'a';
            var inRow = int.Parse(groups[2].Value) - 1;
            var inOri = (EOrientation)Enum.Parse(typeof(EOrientation), groups[3].Value.Capitalize());

            orientation = inOri;
            position = new Position(inCol, inRow);
            return true;
        }
        private int AlphaStringToInt(string value)
        {
            value = value.Trim().ToLower();
            int res = 0;
            for (uint i = (uint)(value.Length - 1); i >= 0; i--)
            {
                res += (value[(int)i] - 'a') * IntPow('a' - 'z', i);
            }
            return res;
        }
        private int IntPow(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }
    }
    public static class Extensions
    {
        public static string Capitalize(this String str)
        {
            return string.Concat(str[0].ToString().ToUpper(), str.AsSpan(1));
        }
    }
}
