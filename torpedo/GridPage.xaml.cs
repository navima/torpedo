using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

#pragma warning disable CA1305 // Specify IFormatProvider
#pragma warning disable SA1000 // Keywords should be spaced correctly

namespace NationalInstruments
{
    /// <summary>
    /// Interaction logic for GridPage.xaml
    /// </summary>
    public partial class GridPage : Page
    {
        private const string _letters = "ABCDEFGHI";
        private int _time = 0;

        private bool _inCheatMode = false;
        private bool _inPlayerViewMode = false;
        private readonly bool _inAIMode = false;

        private readonly IDataStore _dataStore;
        private readonly TorpedoService _torpedoGameInstance;
        private TorpedoButton? _firstPosition;
        private Player _humanPlayer;
        private Player _aiPlayer;
        private Player? _winner;
        private readonly Dictionary<Player, PlayerStats> _playerStats = new();
        private readonly TorpedoButton[,] _buttonArray = new TorpedoButton[9, 9];

        private readonly SolidColorBrush _cyan = new(Colors.Cyan);
        private readonly SolidColorBrush _lightGray = new(Colors.LightGray);

        #region UIManipulation
        private void InitializeGridPage(Player player1, Player player2)
        {
            InitializeTable();
            label_player1.Text = player1.Name;
            label_player2.Text = player2.Name;
        }

        private void InitializeTable()
        {
            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    TorpedoButton button = new();

                    button.Content = "O";

                    button.X = i - 1;
                    button.Y = j - 1;

                    button.Background = _lightGray;

                    button.Click += (sender, e) =>
                    {
                        ButtonPress(button.X, button.Y);
                    };

                    Grid.SetColumn(button, i);
                    Grid.SetRow(button, j);

                    table.Children.Add(button);
                    _buttonArray[i - 1, j - 1] = button;
                }
            }
            for (int i = 1; i < 10; i++)
            {
                TextBlock tb = new();

                tb.Text = i.ToString();
                tb.FontSize = 23;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.TextAlignment = TextAlignment.Center;

                Grid.SetColumn(tb, i);
                Grid.SetRow(tb, 0);

                table.Children.Add(tb);

                tb = new();

                tb.Text = _letters[i - 1].ToString();
                tb.FontSize = 23;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.TextAlignment = TextAlignment.Center;

                Grid.SetColumn(tb, 0);
                Grid.SetRow(tb, i);

                table.Children.Add(tb);
            }
        }

        private void UpdateUI()
        {
            UpdateInstructions();
            UpdateRounds();

            switch (_torpedoGameInstance.GameState)
            {
                case EGameState.PlacingShips:
                    UpdatePlacing(_torpedoGameInstance.GetBoard(_torpedoGameInstance.CurrentPlayer));
                    break;
                case EGameState.SinkingShips:
                    UpdatePlaying(_torpedoGameInstance.GetHitBoard(_torpedoGameInstance.CurrentPlayer));
                    break;
                case EGameState.GameOver:
                    LockTable();
                    break;
            }
            _torpedoGameInstance.Players.ToList().ForEach(player =>
            {
                CheckSunkenShips(player);
                UpdateStats(player);
            });
        }

        private void UpdateInstructions()
        {
            Instruction.Text = CreateInstructionLabelText();
        }
        private string CreateInstructionLabelText() => _torpedoGameInstance.GameState switch
        {
            EGameState.PlacingShips => "Click on the area where you want to place your ship then an adjecent area where you want your ship to face",
            EGameState.SinkingShips => "Click on the area you want to sink" + (_inAIMode ? "\nYou can check the enemy ships with button C and your own board with button P" : string.Empty),
            EGameState.GameOver => "The game is over, you can check your score in the High Scores tab",
            _ => string.Empty
        };

        private void UpdateRounds() => turnlabel.Text = CreateLabelText();
        private string CreateLabelText() => _torpedoGameInstance.GameState switch
        {
            EGameState.PlacingShips => $"Placing ships, {_torpedoGameInstance.CurrentPlayer.Name}'s turn",
            EGameState.SinkingShips => $"Turn {_torpedoGameInstance.Rounds}, {_torpedoGameInstance.CurrentPlayer.Name}'s turn",
            EGameState.GameOver => $"Game over, {_winner?.Name} won",
            _ => string.Empty
        };

        private void UpdateStats(Player player)
        {
            TextBlock tb;
            if (player.Name == label_player1.Text)
            {
                tb = Player1Status;
            }
            else
            {
                tb = Player2Status;
            }

            var (sunkenShips, hits, misses, shipStatuses) = _playerStats[player];

            StringBuilder sb = new();
            sb.AppendLine($"Sunken ships: {sunkenShips}");
            sb.AppendLine($"Hits: {hits}");
            sb.AppendLine($"Misses: {misses}");
            foreach (var (ship, status) in shipStatuses)
            {
                sb.AppendLine($"ship {ship.Size}: {status.ToUserReadableString()}");
            }
            tb.Text = sb.ToString();
        }

        private void LockTable()
        {
            foreach (var button in _buttonArray)
            {
                button.IsEnabled = false;
                if (button.Content.ToString() != "X")
                {
                    button.Content = " ";
                    button.Background = _cyan;
                }
            }
        }

        public void CheckSunkenShips(Player player)
        {
            _torpedoGameInstance.PlacedShips(player).ToList().ForEach(x =>
            {
                if (x.Value.Dead)
                {
                    _playerStats[player].ShipStatuses[x.Value] = EShipStatus.Dead;
                }
            });
        }

        public void CheatMode()
        {
            if (!_inCheatMode && _torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Debug.WriteLine("Cheatmode enabled");
                _inCheatMode = true;
                ShipPart?[,] shipTable = _torpedoGameInstance.GetBoard(_aiPlayer);
                UpdatePlacing(shipTable);
            }
            else if (_inCheatMode)
            {
                Debug.WriteLine("Cheatmode disabled");
                _inCheatMode = false;
                EHitResult?[,] hitResults = _torpedoGameInstance.GetHitBoard(_humanPlayer);
                UpdatePlaying(hitResults);
            }
        }

        public void PlayerViewMode()
        {
            if (!_inPlayerViewMode && _torpedoGameInstance.GameState == EGameState.SinkingShips || !_inPlayerViewMode && _torpedoGameInstance.GameState == EGameState.GameOver)
            {
                Debug.WriteLine("Playervievmode enabled");
                _inPlayerViewMode = true;
                EHitResult?[,] board = _torpedoGameInstance.GetHitBoard(_aiPlayer);
                UpdatePlaying(board);
            }
            else if (_inPlayerViewMode)
            {
                Debug.WriteLine("Playervievmode disabled");
                _inPlayerViewMode = false;
                EHitResult?[,] board = _torpedoGameInstance.GetHitBoard(_humanPlayer);
                UpdatePlaying(board);
            }
        }

        private void UpdatePlacing(ShipPart?[,] ships)
        {
            IterateOver2DArray(ships, (i, j, elem) =>
            {
                if (elem is null)
                {
                    if (_inCheatMode)
                    {
                        _buttonArray[i, j].Content = " ";
                        _buttonArray[i, j].IsEnabled = false;
                    }
                    else
                    {
                        _buttonArray[i, j].Background = _lightGray;
                        _buttonArray[i, j].IsEnabled = true;
                        _buttonArray[i, j].Content = "O";
                    }
                }
                else
                {
                    _buttonArray[i, j].Content = "X";
                    _buttonArray[i, j].IsEnabled = false;
                }
            });
        }

        private void UpdatePlaying(EHitResult?[,] hitTable)
        {
            IterateOver2DArray(hitTable, (i, j, elem) =>
            {
                var button = _buttonArray[i, j];
                button.IsEnabled = false;
                switch (elem)
                {
                    case EHitResult.Miss:
                        button.Background = _cyan;
                        button.Content = " ";
                        if (_inPlayerViewMode)
                        {
                            button.Content = "O";
                        }
                        break;
                    case EHitResult.Sink:
                        button.Background = _cyan;
                        button.Content = "X";
                        break;
                    case EHitResult.Hit:
                        button.Background = _cyan;
                        button.Content = "X";
                        break;
                    default:
                        button.Background = _lightGray;
                        button.Content = " ";
                        button.IsEnabled = true;
                        break;
                }
                if (_inPlayerViewMode)
                {
                    button.IsEnabled = false;
                }
            });
        }

        public void IterateOver2DArray<T>(T[,] arr, Action<int, int, T> action)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    action(i, j, arr[i, j]);
                }
            }
        }
        #endregion

        #region GameLogic

        private void StartGame()
        {
            _torpedoGameInstance.AddPlayer(_humanPlayer);
            _torpedoGameInstance.AddPlayer(_aiPlayer);
            Debug.WriteLine($"Added player {label_player1.Text}");
            Debug.WriteLine($"Added player {label_player2.Text}");

            _torpedoGameInstance.FinishAddingPlayers();
            Debug.WriteLine($"Current ship to palce is: {_torpedoGameInstance.ShipsToPlace(_torpedoGameInstance.CurrentPlayer).First()}");
            _torpedoGameInstance.Players.ToList().ForEach(player =>
            {
                _playerStats.Add(player, new PlayerStats());
            });
            UpdateUI();
            MoveState();
        }

        private void ButtonPress(int x, int y)
        {
            Debug.WriteLine($"Button {x}:{y} pressed");
            if (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                ProcessButtonPress(_buttonArray[x, y]);
            }
            else if (_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Shoot(x, y);
            }
        }

        private void ProcessButtonPress(TorpedoButton? button)
        {
            if (button is null)
            {
                throw new ArgumentNullException(nameof(button));
            }
            if (_torpedoGameInstance.CurrentPlayer == null)
            {
                return;
            }
            Player player = _torpedoGameInstance.CurrentPlayer;

            Ship ship = _torpedoGameInstance.ShipsToPlace(player).First();

            if (button == _firstPosition)
            {
                Debug.WriteLine("Already tried placing ship here, releasing");
                button.Background = _lightGray;
                _firstPosition = null;
                return;
            }

            if (_firstPosition is null)
            {
                _firstPosition = button;
                Debug.WriteLine($"First location selected as {button.X}:{button.Y}");
                _firstPosition.Background = _cyan;
                button = null;
            }

            if (TryCalculateOrientation(button, ship, out var orientation))
            {
                Position position = new(_firstPosition.X, _firstPosition.Y);
                Debug.WriteLine($"Orientation is: {orientation}");
                ship.Orientation = orientation;
                PlaceShip(player, ship, position);
                _firstPosition = null;
            }
            else
            {
                if (button is not null)
                {
                    Debug.WriteLine("Positions are not adjecent, clearing selected button.");
                    _firstPosition.Background = _lightGray;
                    _firstPosition = null;
                }
            }
        }

        private bool TryCalculateOrientation(TorpedoButton? button, Ship ship, out EOrientation orientation)
        {
            if (ship.Size == 1)
            {
                orientation = EOrientation.Down;
                return true;
            }
            if (button is null)
            {
                orientation = default;
                return false;
            }
            if (_firstPosition is null)
            {
                orientation = default;
                return false;
            }
            if (_firstPosition.X + 1 == button.X && _firstPosition.Y == button.Y)
            {
                orientation = EOrientation.Right;
            }
            else if (_firstPosition.X - 1 == button.X && _firstPosition.Y == button.Y)
            {
                orientation = EOrientation.Left;
            }
            else if (_firstPosition.Y - 1 == button.Y && _firstPosition.X == button.X)
            {
                orientation = EOrientation.Up;
            }
            else if (_firstPosition.Y + 1 == button.Y && _firstPosition.X == button.X)
            {
                orientation = EOrientation.Down;
            }
            else
            {
                orientation = default;
                return false;
            }
            return true;
        }

        private void PlaceShip(Player player, Ship ship, Position position)
        {
            if (_torpedoGameInstance.TryPlaceShip(player, ship, position))
            {
                _playerStats[player].ShipStatuses[ship] = EShipStatus.Placed;
                if (!_torpedoGameInstance.ShipsToPlace(player).Any())
                {
                    Debug.WriteLine("Last ship placed");
                    _torpedoGameInstance.FinishPlacingShips(player);
                    MoveState();
                }
                else
                {
                    Debug.WriteLine($"Ship successfully placed, next ship is: {_torpedoGameInstance.ShipsToPlace(_torpedoGameInstance.CurrentPlayer).First().Size}");
                    _firstPosition = null;
                }
            }
            else
            {
                Debug.WriteLine("Can't place ship here");
                _firstPosition = null;
            }
            UpdateUI();
        }

        private void AI_place()
        {
            while (_torpedoGameInstance.ShipsToPlace(_aiPlayer).Any())
            {
                var ship = _torpedoGameInstance.ShipsToPlace(_aiPlayer).First();
                _torpedoGameInstance.PlaceShipRandom(_aiPlayer, ship);
                _playerStats[_aiPlayer].ShipStatuses[ship] = EShipStatus.Placed;
                Debug.WriteLine($"AI placed ship: {ship}");
            }
            _torpedoGameInstance.FinishPlacingShips(_aiPlayer);
        }

        private void Shoot(int x, int y)
        {
            Player player = _torpedoGameInstance.CurrentPlayer;
            Debug.Write($"{_torpedoGameInstance.CurrentPlayer} is trying to hit position {x}:{y}. ");
            _torpedoGameInstance.TryHit(player, new Position(x, y), out var res);
            Debug.WriteLine(res);
            switch (res)
            {
                case EHitResult.Sink:
                    _playerStats[player].SunkenShips++;
                    break;
                case EHitResult.Hit:
                    _playerStats[player].Hits++;
                    break;
                default:
                    _playerStats[player].Misses++;
                    break;
            }
            if (_torpedoGameInstance.GameState != EGameState.GameOver)
            {
                if (_inAIMode)
                {
                    AI_shoot();
                    if (_torpedoGameInstance.GameState == EGameState.GameOver)
                    {
                        Debug.WriteLine("Game is finished");
                        FinishGame();
                    }
                }
            }
            else
            {
                MoveState();
            }
            UpdateUI();
        }

        private void AI_shoot()
        {
            EHitResult res = _torpedoGameInstance.HitSuggested(_aiPlayer);
            Debug.WriteLine($"AI result: {res}");
            switch (res)
            {
                case EHitResult.Sink:
                    _playerStats[_aiPlayer].SunkenShips++;
                    break;
                case EHitResult.Hit:
                    _playerStats[_aiPlayer].Hits++;
                    break;
                default:
                    _playerStats[_aiPlayer].Misses++;
                    break;
            }
        }

        public void FinishGame()
        {
            Array.ForEach(_torpedoGameInstance.Players.ToArray(), x =>
            {
                if (_torpedoGameInstance.IsPlayerDead(x))
                {
                    Debug.WriteLine($"Player {x.Name} lost");
                    foreach (var (ship, _) in _playerStats[x].ShipStatuses)
                    {
                        _playerStats[x].ShipStatuses[ship] = EShipStatus.Dead;
                    }
                }
                else
                {
                    Debug.WriteLine($"Playere {x.Name} won");
                    _winner = x;
                    UpdatePlaying(_torpedoGameInstance.GetHitBoard(x));
                }
            });
            Debug.WriteLine("Finishing game");
            if (_winner is null)
            {
                Debug.WriteLine("Winner is null");
                return;
            }

            Debug.WriteLine($"Current state is {_torpedoGameInstance.GameState}, current player is {_torpedoGameInstance.CurrentPlayer}");
            _dataStore.AddOutcome(new Outcome(_torpedoGameInstance.Players.First(), _torpedoGameInstance.Players.Skip(1).First(), ConvertStats(_playerStats), _winner, _torpedoGameInstance.Rounds));
        }

        private void MoveState()
        {
            switch (_torpedoGameInstance.GameState)
            {
                case EGameState.PlacingShips:
                    if (_inAIMode && _torpedoGameInstance.ShipsToPlace(_aiPlayer).Any())
                    {
                        Debug.WriteLine("AI is placing ships");
                        AI_place();
                    }
                    else
                    {
                        Debug.WriteLine($"{_torpedoGameInstance.CurrentPlayer} is placing ships");
                    }
                    UpdateUI();
                    break;
                case EGameState.SinkingShips:
                    Debug.WriteLine($"Current state is {_torpedoGameInstance.GameState}, current player is {_torpedoGameInstance.CurrentPlayer}");
                    StartTimer();
                    if (_torpedoGameInstance.CurrentPlayer == _aiPlayer)
                    {
                        AI_shoot();
                    }
                    break;
                case EGameState.GameOver:
                    FinishGame();
                    UpdateUI();
                    break;
            }
        }

        #endregion

        private IList<Stat> ConvertStats(Dictionary<Player, PlayerStats> inStats)
        {
            List<Stat> outStats = new();
            Player player1 = inStats.Keys.ToArray()[0];
            Player player2 = inStats.Keys.ToArray()[1];

            int player1Survive = 10 - (inStats[player2].Hits + inStats[player2].SunkenShips);
            int player2Survive = 10 - (inStats[player1].Hits + inStats[player1].SunkenShips);

            outStats.Add(new Stat(inStats[player1].Hits + inStats[player1].SunkenShips, inStats[player1].Misses, player1Survive, player1));
            outStats.Add(new Stat(inStats[player2].Hits + inStats[player2].SunkenShips, inStats[player2].Misses, player2Survive, player2));

            return outStats;
        }

        #region _timer
        private void StartTimer()
        {
            DispatcherTimer dispatcherTimer = new();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += IncrementTime;
            dispatcherTimer.Start();
        }

        private void IncrementTime(object? sender, EventArgs e)
        {
            if (_torpedoGameInstance.GameState != EGameState.GameOver)
            {
                _time++;
            }
            if (_time % 60 < 10)
            {
                timer.Text = ($"0{_time / 60}:0{_time % 60}");
            }
            else
            {
                timer.Text = ($"0{_time / 60}:{_time % 60}");
            }
        }

        #endregion

        public GridPage(IDataStore dataStore, Player player1, Player player2)
        {
            InitializeComponent();
            _dataStore = dataStore;
            _torpedoGameInstance = new TorpedoService(_dataStore, (9, 9));
            _humanPlayer = player1;
            _aiPlayer = player2;
            _inAIMode = _aiPlayer.Equals(_dataStore.AIPlayer);
            _torpedoGameInstance.GameStateChanged += _torpedoGameInstance_GameStateChanged;
            if (_inAIMode)
            {
                Debug.WriteLine("Game is in AI mode");
            }
            InitializeGridPage(player1, player2);
            StartGame();
        }

        private void _torpedoGameInstance_GameStateChanged(object? sender, StateChangedEventArgs e)
        {
            Debug.WriteLine(e);
        }
    }
}
