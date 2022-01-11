using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private bool _inAIMode = false;
        private bool _inPlayerViewMode = false;

        private IDataStore _dataStore;
        private TorpedoService _torpedoGameInstance;
        private TorpedoButton? _selectedButton;
        private Player? _humanPlayer;
        private Player _aiPlayer;
        private Player? _winner;
        private readonly Dictionary<Player, PlayerStats> _playerStats = new ();
        private readonly TorpedoButton[,] _buttonArray = new TorpedoButton[9, 9];

        private readonly SolidColorBrush _cyan = new (Colors.Cyan);
        private readonly SolidColorBrush _lightGray = new (Colors.LightGray);

        #region UIManipulation
        private void InitializeGridPage(string player1, string player2)
        {
            InitializeTable();
            label_player1.Text = player1;
            label_player2.Text = player2;
        }

        private void InitializeTable()
        {
            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    TorpedoButton button = new ();

                    button.Content = "O";

                    button.SetX_coord(i - 1);
                    button.SetY_coord(j - 1);

                    button.Background = _lightGray;

                    button.Click += (sender, e) =>
                    {
                        ButtonPress(button.GetX_coord(), button.GetY_coord());
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

            if (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                UpdatePlacing(_torpedoGameInstance.GetBoard(_torpedoGameInstance.CurrentPlayer));
            }

            if (_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                UpdatePlaying(_torpedoGameInstance.GetHitBoard(_torpedoGameInstance.CurrentPlayer));
            }

            if (_torpedoGameInstance.GameState == EGameState.GameOver)
            {
                LockTable();
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
            EGameState.PlacingShips => $"Placing ships, {_torpedoGameInstance.CurrentPlayer}'s turn",
            EGameState.SinkingShips => $"Turn {_torpedoGameInstance.Rounds}, {_torpedoGameInstance.CurrentPlayer}'s turn",
            EGameState.GameOver => $"Game over, {_winner.Name} won",
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
            int sunken_ships = _playerStats[player].SunkenShips;
            int hits = _playerStats[player].Hits;
            int misses = _playerStats[player].Misses;
            var shipStatus = _playerStats[player].GetShipStatuses();
            StringBuilder sb = new();
            sb.AppendLine($"Sunken ships: {sunken_ships}");
            sb.AppendLine($"Hits: {hits}");
            sb.AppendLine($"Misses: {misses}");
            foreach (var (ship, status) in shipStatus)
            {
                sb.AppendLine($"ship {ship.Size}: {status.ToUserReadableString()}");
            }
            tb.Text = sb.ToString();
        }

        private void LockTable()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    _buttonArray[i, j].IsEnabled = false;
                    if (_buttonArray[i, j].Content != "X")
                    {
                        _buttonArray[i, j].Content = " ";
                        _buttonArray[i, j].Background = _cyan;
                    }
                }
            }
        }

        public void CheckSunkenShips(Player player)
        {
            _torpedoGameInstance.PlacedShips(player).ToList().ForEach(x =>
            {
                if (x.Value.Dead)
                {
                    _playerStats[player].SetShipStatus(x.Value, EShipStatus.Dead);
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
            if (_inCheatMode)
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (ships[i, j] == null)
                        {
                            _buttonArray[i, j].Content = " ";
                            _buttonArray[i, j].IsEnabled = false;
                        }
                        else
                        {
                            _buttonArray[i, j].Content = "X";
                            _buttonArray[i, j].IsEnabled = false;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (ships[i, j] == null)
                        {
                            _buttonArray[i, j].Background = _lightGray;
                            _buttonArray[i, j].IsEnabled = true;
                            _buttonArray[i, j].Content = "O";
                        }
                        else
                        {
                            _buttonArray[i, j].IsEnabled = false;
                            _buttonArray[i, j].Content = "X";
                        }
                    }
                }
            }
        }

        private void UpdatePlaying(EHitResult?[,] hitTable)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    switch (hitTable[i, j])
                    {
                        case EHitResult.Miss:
                            _buttonArray[i, j].Background = _cyan;
                            _buttonArray[i, j].Background = _cyan;
                            _buttonArray[i, j].Content = " ";
                            _buttonArray[i, j].IsEnabled = false;
                            if (_inPlayerViewMode)
                            {
                                _buttonArray[i, j].Content = "O";
                            }
                            break;
                        case EHitResult.Sink:
                            _buttonArray[i, j].Background = _cyan;
                            _buttonArray[i, j].Content = "X";
                            _buttonArray[i, j].IsEnabled = false;
                            break;
                        case EHitResult.Hit:
                            _buttonArray[i, j].Background = _cyan;
                            _buttonArray[i, j].Content = "X";
                            _buttonArray[i, j].IsEnabled = false;
                            break;
                        default:
                            _buttonArray[i, j].Background = _lightGray;
                            _buttonArray[i, j].Content = " ";
                            _buttonArray[i, j].IsEnabled = true;
                            break;
                    }
                    if (_inPlayerViewMode)
                    {
                        _buttonArray[i, j].IsEnabled = false;
                    }
                }
            }
        }

        #endregion

        #region GameLogic

        private void StartGame()
        {
            Debug.WriteLine($"Current state is: {_torpedoGameInstance.GameState}");
            if (_inAIMode)
            {
                Debug.WriteLine("Game is in AI mode");
            }
            else
            {
                Debug.WriteLine("Game is not in AI mode");
            }

            if (_inAIMode)
            {
                _humanPlayer = _torpedoGameInstance.GetOrCreatePlayerByName(label_player1.Text);
                _aiPlayer = _torpedoGameInstance.GetOrCreatePlayerByName(label_player2.Text);
                _torpedoGameInstance.AddPlayer(_humanPlayer);
                _torpedoGameInstance.AddPlayer(_aiPlayer);
                Debug.WriteLine($"Added player {label_player1.Text}");
                Debug.WriteLine($"Added player {label_player2.Text}");
            }
            else
            {
                _torpedoGameInstance.AddPlayer(_torpedoGameInstance.GetOrCreatePlayerByName(label_player1.Text));
                _torpedoGameInstance.AddPlayer(_torpedoGameInstance.GetOrCreatePlayerByName(label_player2.Text));
                Debug.WriteLine($"Added player {label_player1.Text}");
                Debug.WriteLine($"Added player {label_player2.Text}");
            }
            _torpedoGameInstance.FinishAddingPlayers();
            Debug.WriteLine($"Current state is: {_torpedoGameInstance.GameState}");
            Debug.WriteLine($"Current ship to palce is: {_torpedoGameInstance.ShipsToPlace(_torpedoGameInstance.CurrentPlayer).First().Size}");
            _torpedoGameInstance.GetAllPlayers().ToList().ForEach(player =>
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
                EvaluatePlacement(_buttonArray[x, y]);
            }
            else if (_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Shoot(x, y);
            }
        }

        private void EvaluatePlacement(TorpedoButton button)
        {
            Player player = _torpedoGameInstance.CurrentPlayer;
            Ship ship = _torpedoGameInstance.ShipsToPlace(player).First();
            Position position;
            if (ship.Size == 1)
            {
                position = new Position(button.GetX_coord(), button.GetY_coord());
                ship.Orientation = EOrientation.Up;
                PlaceShip(player, ship, position);
            }
            else if (button == _selectedButton)
            {
                Debug.WriteLine("Already tried placing ship here, releasing");
                button.Background = _lightGray;
                _selectedButton = null;
            }
            else if (_selectedButton == null)
            {
                _selectedButton = button;
                Debug.WriteLine($"First location selected as {button.GetX_coord()}:{button.GetY_coord()}");
                _selectedButton.Background = _cyan;
            }
            else
            {
                Debug.WriteLine($"Second location selected as {button.GetX_coord()}:{button.GetY_coord()}");
                if (_selectedButton.GetX_coord() + 1 == button.GetX_coord() && _selectedButton.GetY_coord() == button.GetY_coord())
                {
                    Debug.WriteLine("Second location is right to the first location");
                    ship.Orientation = EOrientation.Right;
                    position = new Position(_selectedButton.GetX_coord(), _selectedButton.GetY_coord());
                    PlaceShip(player, ship, position);
                }
                else if (_selectedButton.GetX_coord() - 1 == button.GetX_coord() && _selectedButton.GetY_coord() == button.GetY_coord())
                {
                    Debug.WriteLine("Second location is left to the first location");
                    ship.Orientation = EOrientation.Left;
                    position = new Position(_selectedButton.GetX_coord(), _selectedButton.GetY_coord());
                    PlaceShip(player, ship, position);
                }
                else if (_selectedButton.GetY_coord() - 1 == button.GetY_coord() && _selectedButton.GetX_coord() == button.GetX_coord())
                {
                    Debug.WriteLine("Second location is above the first location");
                    ship.Orientation = EOrientation.Up;
                    position = new Position(_selectedButton.GetX_coord(), _selectedButton.GetY_coord());
                    PlaceShip(player, ship, position);
                }
                else if (_selectedButton.GetY_coord() + 1 == button.GetY_coord() && _selectedButton.GetX_coord() == button.GetX_coord())
                {
                    Debug.WriteLine("Second location is below the first location");
                    ship.Orientation = EOrientation.Down;
                    position = new Position(_selectedButton.GetX_coord(), _selectedButton.GetY_coord());
                    PlaceShip(player, ship, position);
                }
                else
                {
                    Debug.WriteLine("Positions are not adjecent, clearing selected button.");
                    _selectedButton.Background = _lightGray;
                    _selectedButton = null;
                }
            }
        }

        private void PlaceShip(Player player, Ship ship, Position position)
        {
            if (_torpedoGameInstance.TryPlaceShip(player, ship, position))
            {
                _playerStats[player].SetShipStatus(ship, EShipStatus.Placed);
                if (ship.Size == 4)
                {
                    Debug.WriteLine("Last ship placed");
                    _torpedoGameInstance.FinishPlacingShips(player);
                    MoveState();
                }
                else
                {
                    Debug.WriteLine($"Ships successfully placed, next ship is: {_torpedoGameInstance.ShipsToPlace(_torpedoGameInstance.CurrentPlayer).First().Size}");
                    _selectedButton = null;
                }
            }
            else
            {
                Debug.WriteLine("Can't place ship here");
                _selectedButton = null;
            }
            UpdateUI();
        }

        private void AI_place()
        {
            while (_torpedoGameInstance.ShipsToPlace(_aiPlayer).Any())
            {
                var ship = _torpedoGameInstance.ShipsToPlace(_aiPlayer).First();
                Debug.WriteLine($"Current ship to place is {ship.Size}");
                _torpedoGameInstance.PlaceShipRandom(_aiPlayer, ship);
                _playerStats[_aiPlayer].SetShipStatus(ship, EShipStatus.Placed);
            }
            _torpedoGameInstance.FinishPlacingShips(_aiPlayer);
        }

        private void Shoot(int x, int y)
        {
            Player player = _torpedoGameInstance.CurrentPlayer;
            Debug.Write($"{_torpedoGameInstance.CurrentPlayer} is trying to hit position {x}:{y}. ");
            _torpedoGameInstance.TryHit(player, new Position(x, y), out var res);
            Debug.Write($"{res} \n");
            switch (res)
            {
                case EHitResult.Sink:
                    _playerStats[player].IncrementSunkenShips();
                    break;
                case EHitResult.Hit:
                    _playerStats[player].IncrementHits();
                    break;
                default:
                    _playerStats[player].IncrementMisses();
                    break;
            }
            if (_torpedoGameInstance.GameState != EGameState.GameOver)
            {
                if (_inAIMode)
                {
                    AI_shoot();
                    if(_torpedoGameInstance.GameState == EGameState.GameOver)
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
                    _playerStats[_aiPlayer].IncrementSunkenShips();
                    break;
                case EHitResult.Hit:
                    _playerStats[_aiPlayer].IncrementHits();
                    break;
                default:
                    _playerStats[_aiPlayer].IncrementMisses();
                    break;
            }
        }

        public void FinishGame()
        {
            Debug.WriteLine($"Current state is {_torpedoGameInstance.GameState}, current player is {_torpedoGameInstance.CurrentPlayer}");
            Array.ForEach(_torpedoGameInstance.Players.ToArray(), x =>
            {
                if (_torpedoGameInstance.IsPlayerDead(x))
                {
                    Debug.WriteLine($"Player {x.Name} lost");
                    foreach (var (ship, _) in _playerStats[x].GetShipStatuses())
                    {
                        _playerStats[x].SetShipStatus(ship, EShipStatus.Dead);
                    }
                }
                else
                {
                    Debug.WriteLine($"Playere {x.Name} won");
                    _winner = x;
                    UpdatePlaying(_torpedoGameInstance.GetHitBoard(x));
                }
            });
            //_dataStore.AddOutcome(new Outcome(_torpedoGameInstance.GetAllPlayers(), _playerStats, _winner, _torpedoGameInstance.Rounds));
        }

        private void MoveState()
        {
            if (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                if (_inAIMode && _torpedoGameInstance.CurrentPlayer == _aiPlayer)
                {
                    Debug.WriteLine("AI is placing ships");
                    AI_place();
                    UpdateUI();
                }
                else
                {
                    Debug.WriteLine($"{_torpedoGameInstance.CurrentPlayer} is placing ships");
                    UpdateUI();
                }
            }
            else if (_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Debug.WriteLine($"Current state is {_torpedoGameInstance.GameState}, current player is {_torpedoGameInstance.CurrentPlayer}");
                StartTimer();
                if (_torpedoGameInstance.CurrentPlayer == _aiPlayer)
                {
                    AI_shoot();
                }
            }
            if (_torpedoGameInstance.GameState == EGameState.GameOver)
            {
                FinishGame();
                UpdateUI();
            }
        }

        #endregion

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

        public GridPage(IDataStore dataStore, string player1 = "Player1", string player2 = "Player2")
        {
            InitializeComponent();
            _dataStore = dataStore;
            _torpedoGameInstance = new TorpedoService(_dataStore, (9, 9));
            _aiPlayer = _dataStore.GetOrCreatePlayerByName("AI");
            _humanPlayer = _dataStore.GetOrCreatePlayerByName(player1);
            _inAIMode = player2 == "AI";
            InitializeGridPage(player1, player2);
            StartGame();
        }
    }
}
