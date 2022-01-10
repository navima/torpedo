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

namespace NationalInstruments
{
    /// <summary>
    /// Interaction logic for GridPage.xaml
    /// </summary>
    public partial class GridPage : Page
    {
        private char[] _letters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' };
        private int _time = 0;
        private int _turns = 1;

        private bool _inCheatMode = false;
        private bool _inAIMode = false;
        private bool _inPlayerViewMode = false;

        private IDataStore? _dataStore;
        private TorpedoService? _torpedoGameInstance;
        private TorpedoButton? _selectedButton;
        private Player? _humanPlayer;
        private Player? _aiPlayer;
        private readonly List<Position> _aiCandidates = new ();
        private Player? _winner;
        private Dictionary<string, PlayerStats> _playerStats = new ();
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
                TextBlock tb = new TextBlock();

                tb.Text = i.ToString();
                tb.FontSize = 23;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.TextAlignment = TextAlignment.Center;

                Grid.SetColumn(tb, i);
                Grid.SetRow(tb, 0);

                table.Children.Add(tb);

                tb = new TextBlock();

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
            if (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                Instruction.Text = "Click on the area where you want to place your ship then an adjecent area where you want your ship to face";
            }
            if (_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Instruction.Text = "Click on the area you want to sink";
                if (_inAIMode)
                {
                    Instruction.Text += "\nYou can check the enemy ships with button C and your own board with button P";
                }
            }
            if (_torpedoGameInstance.GameState == EGameState.GameOver)
            {
                Instruction.Text = "The game is over, you can check your score in the High Scores tab";
            }
        }

        private void UpdateRounds()
        {
            if (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                turnlabel.Text = ($"Placing ships, {_torpedoGameInstance.CurrentPlayer}'s turn");
            }
            if (_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                turnlabel.Text = ($"Turn {_turns}, {_torpedoGameInstance.CurrentPlayer}'s turn");
            }
            if (_torpedoGameInstance.GameState == EGameState.GameOver)
            {
                turnlabel.Text = ($"Game over, {_winner.Name} won");
            }
        }

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
            int sunken_ships = _playerStats[player.Name].GetSunken_Ships();
            int hits = _playerStats[player.Name].GetHits();
            int misses = _playerStats[player.Name].GetMisses();
            string[] shipStatus = _playerStats[player.Name].GetShipStatus();
            tb.Text = ($"sunken ships: {sunken_ships} \nhits: {hits} \nmisses: {misses} \nship-1: {shipStatus[0]} \nship-2: {shipStatus[1]} \nship-3: {shipStatus[2]} \nship-4: {shipStatus[3]}");
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
                    Debug.WriteLine($"Ship-{x.Value.Size - 1} sunk");
                    _playerStats[player.Name].SetShipStatus(x.Value.Size - 1, "sunk");
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
            _dataStore = new InMemoryDataStore();
            _torpedoGameInstance = new (_dataStore, (9, 9));
            Debug.WriteLine("Initialized service");
            Debug.WriteLine($"Current state is: {_torpedoGameInstance.GameState}");
            if (label_player2.Text == "AI")
            {
                _inAIMode = true;
                Debug.WriteLine("Game is in AI mode");
            }
            else
            {
                _inAIMode = false;
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
            _playerStats.Add(label_player1.Text, new PlayerStats());
            _playerStats.Add(label_player2.Text, new PlayerStats());
            UpdateUI();
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
                _playerStats[player.Name].SetShipStatus(ship.Size - 1, "Placed");
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
            ShipPart?[,] parts = _torpedoGameInstance.GetBoard(_aiPlayer);
            Debug.WriteLine($"Current ship to place is {_torpedoGameInstance.ShipsToPlace(_aiPlayer).First().Size}");
            while (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                Random rand = new Random();
                int x = rand.Next(0, 9);
                int y = rand.Next(0, 9);
                if (_torpedoGameInstance.ShipsToPlace(_aiPlayer).First().Size == 1)
                {
                    EvaluatePlacement(_buttonArray[x, y]);
                }
                else
                {
                    EvaluatePlacement(_buttonArray[x, y]);
                    int side = rand.Next(0, 4);
                    switch (side)
                    {
                        case 0:
                            x += 1;
                            break;
                        case 1:
                            x -= 1;
                            break;
                        case 2:
                            y += 1;
                            break;

                        default:
                            y -= 1;
                            break;
                    }
                    if (x < 0 || y < 0 || x > 8 || y > 8)
                    {
                        continue;
                    }
                    EvaluatePlacement(_buttonArray[x, y]);
                }
            }
        }

        private void Shoot(int x, int y)
        {
            Player player = _torpedoGameInstance.CurrentPlayer;
            Debug.Write($"{_torpedoGameInstance.CurrentPlayer} is trying to hit position {x}:{y}. ");
            bool success = _torpedoGameInstance.TryHit(player, new Position(x, y), out var res);
            Debug.Write($"{res} \n");
            switch (res)
            {
                case EHitResult.Sink:
                    _playerStats[player.Name].IncrementSunkenShips();
                    break;
                case EHitResult.Hit:
                    _playerStats[player.Name].IncrementHits();
                    break;
                default:
                    _playerStats[player.Name].IncrementMisses();
                    break;
            }
            if (_torpedoGameInstance.GameState != EGameState.GameOver)
            {
                if (_inAIMode)
                {
                    AI_shoot();
                    _turns++;
                }
                else
                {
                    if (_torpedoGameInstance.CurrentPlayer == _torpedoGameInstance.Players.First())
                    {
                        _turns++;
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
            Random rand = new Random();
            Player player = _torpedoGameInstance.CurrentPlayer;
            EHitResult?[,] table = _torpedoGameInstance.GetHitBoard(_humanPlayer);
            while (_torpedoGameInstance.CurrentPlayer != _humanPlayer)
            {
                int x = rand.Next(0, 9);
                int y = rand.Next(0, 9);
                if (_aiCandidates.Count != 0)
                {
                    x = _aiCandidates.First().X;
                    y = _aiCandidates.First().Y;
                    _aiCandidates.RemoveAt(0);
                }
                if (_torpedoGameInstance.TryHit(player, new Position(x, y), out var res))
                {
                    switch (res)
                    {
                        case EHitResult.Sink:
                            _playerStats[player.Name].IncrementSunkenShips();
                            break;
                        case EHitResult.Hit:
                            _playerStats[player.Name].IncrementHits();
                            break;
                        default:
                            _playerStats[player.Name].IncrementMisses();
                            break;
                    }
                    Debug.Write($"AI is trying to hit position {x}:{y}. {res}");
                    if (res == EHitResult.Hit)
                    {
                        Debug.WriteLine("Adding surrounding tiles to list");
                        _aiCandidates.Add(new Position(x + 1, y));
                        _aiCandidates.Add(new Position(x - 1, y));
                        _aiCandidates.Add(new Position(x, y + 1));
                        _aiCandidates.Add(new Position(x, y - 1));
                    }
                }
                else
                {
                    Debug.Write("Can't hit this position\n");
                }
            }
            UpdateUI();
        }

        public void FinishGame()
        {
            Debug.WriteLine($"Current state is {_torpedoGameInstance.GameState}, current player is {_torpedoGameInstance.CurrentPlayer}");
            Array.ForEach(_torpedoGameInstance.Players.ToArray(), x =>
            {
                if (_torpedoGameInstance.IsPlayerDead(x))
                {
                    Debug.WriteLine($"Player {x.Name} lost");
                    for (int i = 0; i < 4; i++)
                    {
                        _playerStats[x.Name].SetShipStatus(i, "sunk");
                    }
                }
                else
                {
                    Debug.WriteLine($"Playere {x.Name} won");
                    _winner = x;
                    UpdatePlaying(_torpedoGameInstance.GetHitBoard(x));
                }
            });
        }

        private void MoveState()
        {
            if (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                if (_inAIMode)
                {
                    Debug.WriteLine("AI is placing ships");
                    AI_place();
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
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += IncrementTime;
            dispatcherTimer.Start();
        }

        private void IncrementTime(object sender, EventArgs e)
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

        public GridPage(string player1 = "Player1", string player2 = "Player2")
        {
            InitializeComponent();
            InitializeGridPage(player1, player2);
            StartGame();
        }
    }
}
