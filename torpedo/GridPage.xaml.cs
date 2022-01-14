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
        private bool _inCheatMode = false;
        private bool _inPlayerViewMode = false;
        private readonly bool _inAIMode = false;

        private readonly IDataStore _dataStore;
        private readonly TorpedoService _torpedoGameInstance;
        private TorpedoButton? _firstPosition;
        private readonly Player _humanPlayer;
        private readonly Player _aiPlayer;
        private Player? _winner;
        private readonly Dictionary<Player, PlayerStats> _playerStats = new();
        private readonly TorpedoButton[,] _buttonArray = new TorpedoButton[9, 9];

        private readonly SolidColorBrush _cyan = new(Colors.Cyan);
        private readonly SolidColorBrush _lightGray = new(Colors.LightGray);

        private Player OtherPlayer { get => _torpedoGameInstance.CurrentPlayer == _humanPlayer ? _aiPlayer : _humanPlayer; }

        #region UIManipulation

        private void UpdateUI()
        {
            UpdateInstructionLabel();
            UpdateRoundsLabel();

            switch (_torpedoGameInstance.GameState)
            {
                case EGameState.PlacingShips:
                    RefreshBoardPlacingShips(_torpedoGameInstance.GetBoard(_torpedoGameInstance.CurrentPlayer));
                    break;
                case EGameState.SinkingShips:
                    RefreshBoardSinkingShips(_torpedoGameInstance.GetHitBoard(_torpedoGameInstance.CurrentPlayer));
                    break;
                case EGameState.GameOver:
                    LockBoard();
                    break;
            }
            _torpedoGameInstance.Players.ToList().ForEach(player =>
            {
                RefreshShipStatuses(player);
                UpdateStats(player);
            });
        }

        private void UpdateInstructionLabel()
        {
            Instruction.Text = CreateInstructionLabelText();
        }
        private string CreateInstructionLabelText() => _torpedoGameInstance.GameState switch
        {
            EGameState.PlacingShips => "Click on the area you want to place your ship on, then an adjecent area where you want your ship to face",
            EGameState.SinkingShips => "Click on the area you want to hit" + (_inAIMode ? "\nYou can check the enemy ships with C and your own board with P" : string.Empty),
            EGameState.GameOver => "The game is over. You can check your score in the High Scores tab!",
            _ => string.Empty
        };

        private void UpdateRoundsLabel() => turnlabel.Text = CreateLabelText();
        private string CreateLabelText() => _torpedoGameInstance.GameState switch
        {
            EGameState.PlacingShips => $"Placing ships, {_torpedoGameInstance.CurrentPlayer.Name}'s turn",
            EGameState.SinkingShips => $"Turn {_torpedoGameInstance.Rounds}, {_torpedoGameInstance.CurrentPlayer.Name}'s turn",
            EGameState.GameOver => $"Game over. {_winner?.Name} won!",
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

        private void LockBoard()
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

        public void RefreshShipStatuses(Player player)
        {
            _torpedoGameInstance.PlacedShips(player)
                .Where(kvp => kvp.Value.Dead)
                .ToList()
                .ForEach(x =>
            {
                _playerStats[player].ShipStatuses[x.Value] = EShipStatus.Dead;
            });
        }

        public void CheatMode()
        {
            if (!_inAIMode)
            {
                return;
            }

            if (!_inCheatMode && _torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Debug.WriteLine("Cheatmode enabled");
                _inCheatMode = true;
                ShipPart?[,] shipTable = _torpedoGameInstance.GetBoard(_humanPlayer);
                RefreshBoardPlacingShips(shipTable);
            }
            else if (_inCheatMode)
            {
                Debug.WriteLine("Cheatmode disabled");
                _inCheatMode = false;
                EHitResult?[,] hitResults = _torpedoGameInstance.GetHitBoard(_aiPlayer);
                RefreshBoardSinkingShips(hitResults);
            }
        }

        public void PlayerViewMode()
        {
            if (_torpedoGameInstance.GameState != EGameState.SinkingShips)
            {
                return;
            }

            if (!_inPlayerViewMode)
            {
                Debug.WriteLine("Player view mode enabled");
                _inPlayerViewMode = true;
                EHitResult?[,] board = _torpedoGameInstance.GetHitBoard(_aiPlayer);
                RefreshBoardSinkingShips(board);
            }
            else
            {
                Debug.WriteLine("Player view mode disabled");
                _inPlayerViewMode = false;
                EHitResult?[,] board = _torpedoGameInstance.GetHitBoard(_humanPlayer);
                RefreshBoardSinkingShips(board);
            }
        }

        private void RefreshBoardPlacingShips(ShipPart?[,] ships)
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

        private void RefreshBoardSinkingShips(EHitResult?[,] hitTable)
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

        private void ButtonPressedHandler(int x, int y)
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
                Position position = _firstPosition.GetAsPosition();
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
                    _torpedoGameInstance.FinishPlacingShips();
                    MoveState();
                }
                Debug.WriteLine($"{player} placed ship: {ship}");
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
            _torpedoGameInstance.FinishPlacingShips();
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
                    RefreshBoardSinkingShips(_torpedoGameInstance.GetHitBoard(x));
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
                        while (_torpedoGameInstance.CurrentPlayer != _aiPlayer)
                        {
                            _torpedoGameInstance.FinishPlacingShips();
                        }
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
                    Debug.WriteLine($"Current player is {_torpedoGameInstance.CurrentPlayer}");
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

            static int CountAliveParts(IDictionary<Position, Ship> ships) => ships.Select(kvp => kvp.Value.Parts.Where(part => part.Alive).Count()).Sum();

            int p1AliveParts = CountAliveParts(_torpedoGameInstance.PlacedShips(player1));
            int p2AliveParts = CountAliveParts(_torpedoGameInstance.PlacedShips(player2));

            outStats.Add(new Stat(inStats[player1].Hits + inStats[player1].SunkenShips, inStats[player1].Misses, p1AliveParts, player1));
            outStats.Add(new Stat(inStats[player2].Hits + inStats[player2].SunkenShips, inStats[player2].Misses, p2AliveParts, player2));

            return outStats;
        }

        #region Time

        private DateTime _gameStartTime;
        private DateTime _gameEndTime;
        private DispatcherTimer _dispatcherTimer = new();

        private void StartTimer()
        {
            _gameStartTime = DateTime.Now;
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            _dispatcherTimer.Tick += IncrementTime;
            _dispatcherTimer.Start();
        }
        private void StopTimer()
        {
            _dispatcherTimer.Stop();
            _gameEndTime = DateTime.Now;
        }

        private void IncrementTime(object? sender, EventArgs e)
        {
            timer.Text = (DateTime.Now - _gameStartTime).ToString("mm':'ss");
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
            void InitializeTable()
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
                            ButtonPressedHandler(button.X, button.Y);
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

                    tb.Text = ((char)('A' + i - 1)).ToString();
                    tb.FontSize = 23;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.TextAlignment = TextAlignment.Center;

                    Grid.SetColumn(tb, i);
                    Grid.SetRow(tb, 0);

                    table.Children.Add(tb);

                    tb = new();

                    tb.Text = i.ToString();
                    tb.FontSize = 23;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.TextAlignment = TextAlignment.Center;

                    Grid.SetColumn(tb, 0);
                    Grid.SetRow(tb, i);

                    table.Children.Add(tb);
                }
            }
            InitializeTable();
            label_player1.Text = player1.Name;
            label_player2.Text = player2.Name;
            StartGame();
        }

        private void _torpedoGameInstance_GameStateChanged(object? sender, StateChangedEventArgs e)
        {
            Debug.WriteLine(e);
            switch (e.NewGameState)
            {
                case EGameState.None:
                    break;
                case EGameState.AddingPlayers:
                    break;
                case EGameState.PlacingShips:
                    break;
                case EGameState.SinkingShips:
                    break;
                case EGameState.GameOver:
                    Debug.WriteLine("Game is finished");
                    FinishGame();
                    break;
                default:
                    break;
            }
        }
    }
}
