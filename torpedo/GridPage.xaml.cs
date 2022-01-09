using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using System.Windows.Threading;

namespace NationalInstruments
{
    /// <summary>
    /// Interaction logic for GridPage.xaml
    /// </summary>
    public partial class GridPage : Page
    {
        private char[] letters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' };
        private int time = 0;
        private int turns = 0;
        private bool inCheatMode = false;
        private bool inAIMode = true;
        private bool inPlayerViewMode = false;
        private DataStore dataStore;
        private TorpedoService _torpedoGameInstance;
        private SolidColorBrush cyan = new SolidColorBrush(Colors.Cyan);
        private SolidColorBrush lightgray = new SolidColorBrush(Colors.LightGray);
        private TorpedoButton selectedButton;
        private Player humanPlayer;
        private Player aiPlayer;
        private List<Position> aiCandidates = new List<Position>();

        TorpedoButton[,] buttonArray = new TorpedoButton[9, 9];

        private void UpdatingUI()
        {
            UpdatingInstrucitons();
            UpdatingRounds();
        }

        private void UpdatingInstrucitons()
        {
            if (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                Instruction.Text = "Click on the area where you want to place your ship then an adjecent area where you want your ship to face";
            }
            if (_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Instruction.Text = "Click on the area you want to sink";
            }
        }

        private void UpdatingRounds()
        {
            turnlabel.Text = ($"Turn {turns.ToString()}: {_torpedoGameInstance.CurrentPlayer}'s turn");
        }

        public void CheatMode()
        {
            if (!inCheatMode && _torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Debug.WriteLine("Cheatmode enabled");
                inCheatMode = true;
                ShipPart[,] shiptTable = _torpedoGameInstance.GetBoard(aiPlayer);
                UpdatePlacing(shiptTable);
            }
            else if(inCheatMode)
            {
                Debug.WriteLine("Cheatmode disabled");
                inCheatMode = false;
                EHitResult?[,] hitResults = _torpedoGameInstance.GetHitBoard(humanPlayer);
                UpdatePlaying(hitResults);
            }
        }

        public void PlayerViewMode()
        {
            if (!inPlayerViewMode && _torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Debug.WriteLine("Playervievmode enabled");
                inPlayerViewMode = true;
                EHitResult?[,] board = _torpedoGameInstance.GetHitBoard(aiPlayer);
                UpdatePlaying(board);
            }
            else
            {
                Debug.WriteLine("Playervievmode disabled");
                inPlayerViewMode = false;
                EHitResult?[,] board = _torpedoGameInstance.GetHitBoard(humanPlayer);
                UpdatePlaying(board);
            }
        }

        private void ButtonPress(int x, int y)
        {
            Debug.WriteLine($"Button {x}:{y} pressed");
            if (_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                EvaluatePlacement(buttonArray[x, y]);
            }
            else if(_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Shoot(x, y);
                UpdatePlaying(_torpedoGameInstance.GetHitBoard(_torpedoGameInstance.CurrentPlayer));
            }
        }

        private void UpdatePlacing(ShipPart[,] ships)
        {
            if (inCheatMode)
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (ships[i, j] == null)
                        {
                            buttonArray[i, j].Content = " ";
                            buttonArray[i, j].IsEnabled= false;
                        }
                        else
                        {
                            buttonArray[i, j].Content = "X";
                            buttonArray[i, j].IsEnabled = false;
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
                            buttonArray[i, j].Background = lightgray;
                            buttonArray[i, j].IsEnabled = true;
                            buttonArray[i, j].Content = "O";
                        }
                        else
                        {
                            buttonArray[i, j].IsEnabled = false;
                            buttonArray[i, j].Content = "X";
                        }
                    }
                }
            }
        }

        private void UpdatePlaying(EHitResult?[,] hitTable)
        {
            for(int i=0; i<9; i++)
            {
                for(int j=0; j<9; j++)
                {
                    switch (hitTable[i, j])
                    {
                        case EHitResult.Miss:
                            buttonArray[i, j].Background = cyan;
                            buttonArray[i, j].Content = " ";
                            buttonArray[i, j].IsEnabled = false;
                            break;
                        case EHitResult.Sink:
                            buttonArray[i, j].Background = cyan;
                            buttonArray[i, j].Content = "X";
                            buttonArray[i, j].IsEnabled = false;
                            break;
                        case EHitResult.Hit:
                            buttonArray[i, j].Background = cyan;
                            buttonArray[i, j].Content = "X";
                            buttonArray[i, j].IsEnabled = false;
                            break;
                        default:
                            buttonArray[i,j].Background = lightgray;
                            buttonArray[i, j].Content = "?";
                            buttonArray[i,j].IsEnabled = true;
                            break;
                    }
                }
            }
        }

        private void EvaluatePlacement(TorpedoButton button)
        {
            if(_torpedoGameInstance.GameState != EGameState.PlacingShips)
            {
                return;
            }
            Player player = _torpedoGameInstance.CurrentPlayer;
            Ship ship = _torpedoGameInstance.ShipsToPlace(player).First();
            Position position;
            if (ship.Size == 1)
            {
                position = new Position(button.GetX_coord(), button.GetY_coord());
                ship.Orientation = EOrientation.Up;
                PlaceShip(player, ship, position);
            }
            else if (button == selectedButton)
            {
                Debug.WriteLine("Already tried placing ship here, releasing");
                button.Background = lightgray;
                selectedButton = null;
            }
            else if (selectedButton == null)
            {
                selectedButton = button;
                Debug.WriteLine($"First location selected as {button.GetX_coord()}:{button.GetY_coord()}");
                selectedButton.Background = cyan;
            }
            else
            {
                Debug.WriteLine($"Second location selected as {button.GetX_coord()}:{button.GetY_coord()}");
                if (selectedButton.GetX_coord() + 1 == button.GetX_coord() && selectedButton.GetY_coord() == button.GetY_coord())
                {
                    Debug.WriteLine("Second location is right to the first location");
                    ship.Orientation = EOrientation.Right;
                    position = new Position(selectedButton.GetX_coord(), selectedButton.GetY_coord());
                    PlaceShip(player, ship, position);
                }
                else if (selectedButton.GetX_coord() - 1 == button.GetX_coord() && selectedButton.GetY_coord() == button.GetY_coord())
                {
                    Debug.WriteLine("Second location is left to the first location");
                    ship.Orientation = EOrientation.Left;
                    position = new Position(selectedButton.GetX_coord(), selectedButton.GetY_coord());
                    PlaceShip(player, ship, position);
                }
                else if (selectedButton.GetY_coord() - 1 == button.GetY_coord() && selectedButton.GetX_coord() == button.GetX_coord())
                {
                    Debug.WriteLine("Second location is above the first location");
                    ship.Orientation = EOrientation.Up;
                    position = new Position(selectedButton.GetX_coord(), selectedButton.GetY_coord());
                    PlaceShip(player, ship, position);
                }
                else if (selectedButton.GetY_coord() + 1 == button.GetY_coord() && selectedButton.GetX_coord() == button.GetX_coord())
                {
                    Debug.WriteLine("Second location is below the first location");
                    ship.Orientation = EOrientation.Down;
                    position = new Position(selectedButton.GetX_coord(), selectedButton.GetY_coord());
                    PlaceShip(player, ship, position);
                }
                else
                {
                    Debug.WriteLine("Positions are not adjecent, clearing selected button.");
                    selectedButton.Background = lightgray;
                    selectedButton = null;
                }
            }
        }

        private void PlaceShip(Player player, Ship ship, Position position)
        {
            if(_torpedoGameInstance.TryPlaceShip(player, ship, position))
            {
                if(ship.Size == 4)
                {
                    Debug.WriteLine("Last ship placed");
                    _torpedoGameInstance.FinishPlacingShips(player);
                    MoveState();
                }
                else
                {
                    Debug.WriteLine($"Ships successfully placed, next ship is: {_torpedoGameInstance.ShipsToPlace(_torpedoGameInstance.CurrentPlayer).First().Size}");
                    if(!(inAIMode && _torpedoGameInstance.CurrentPlayer == aiPlayer))
                    {
                        UpdatePlacing(_torpedoGameInstance.GetBoard(player));
                    }
                }
                if (selectedButton != null)
                {
                    selectedButton = null;
                }
            }
            else
            {
                Debug.WriteLine("Can't place ship here");
                selectedButton.Background = lightgray;
                selectedButton = null;
            }
        }

        private void MoveState()
        {
            if(_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                if (inAIMode)
                {
                    Debug.WriteLine("AI is placing ships");
                    UpdatePlacing(_torpedoGameInstance.GetBoard(_torpedoGameInstance.CurrentPlayer));
                    AI_place();
                }
            }
            else if(_torpedoGameInstance.GameState == EGameState.SinkingShips)
            {
                Debug.WriteLine($"Current state is {_torpedoGameInstance.GameState}, current player is {_torpedoGameInstance.CurrentPlayer}");
                UpdatePlaying(_torpedoGameInstance.GetHitBoard(aiPlayer));
            }
            UpdatingUI();
        }

        private void AI_place()
        {
            ShipPart[,] parts = _torpedoGameInstance.GetBoard(aiPlayer);
            Debug.WriteLine($"Current ship to place is {_torpedoGameInstance.ShipsToPlace(aiPlayer).First().Size}");
            while(_torpedoGameInstance.GameState == EGameState.PlacingShips)
            {
                if(_torpedoGameInstance.GameState != EGameState.PlacingShips)
                {
                    continue;
                }
                Random rand = new Random();
                int x = rand.Next(0, 9);
                int y = rand.Next(0, 9);
                if(_torpedoGameInstance.ShipsToPlace(_torpedoGameInstance.CurrentPlayer).First().Size == 1)
                {
                    EvaluatePlacement(buttonArray[x, y]);
                }
                else
                {
                    EvaluatePlacement(buttonArray[x, y]);
                    int side = rand.Next(0, 4);
                    switch (side)
                    {
                        case 0:
                            {
                                x += 1;
                                break;
                            }
                        case 1:
                            {
                                x -= 1;
                                break;
                            }
                        case 2:
                            {
                                y += 1;
                                break;
                            }
                        default:
                            {
                                y -= 1;
                                break;
                            }
                    }
                    if(x<0 || y<0 || x>8 || y > 8)
                    {
                        continue;
                    }
                    EvaluatePlacement(buttonArray[x, y]);
                }

            }
        }

        private void Shoot(int x, int y)
        {
            Debug.Write($"{_torpedoGameInstance.CurrentPlayer} is trying to hit position {x}:{y}. ");
            bool success = _torpedoGameInstance.TryHit(_torpedoGameInstance.CurrentPlayer, new Position(x, y), out var res);
            Debug.Write($"{res} \n");
            AI_shoot();
            turns++;
            UpdatingUI();
        }

        private void AI_shoot()
        {
            Random rand = new Random();
            EHitResult?[,] table = _torpedoGameInstance.GetHitBoard(humanPlayer);
            while(_torpedoGameInstance.CurrentPlayer != humanPlayer)
            {
                int x = rand.Next(0, 9);
                int y = rand.Next(0, 9);
                if (aiCandidates.Count != 0)
                {
                    x = aiCandidates.First().X;
                    y = aiCandidates.First().Y;
                    aiCandidates.RemoveAt(0);
                }
                if(_torpedoGameInstance.TryHit(_torpedoGameInstance.CurrentPlayer, new Position(x,y), out var res))
                {
                    Debug.Write($"AI is trying to hit position {x}:{y}. {res}");
                    if (res == EHitResult.Hit)
                    {
                        Debug.WriteLine("Adding surrounding tiles to list");
                        aiCandidates.Add(new Position(x + 1, y));
                        aiCandidates.Add(new Position(x - 1, y));
                        aiCandidates.Add(new Position(x, y + 1));
                        aiCandidates.Add(new Position(x, y - 1));
                    }
                }
                else
                {
                    Debug.Write("Can't hit this position\n");
                }
            }
        }
        private void StartTimer()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += IncrementTime;
            dt.Start();
        }

        private void IncrementTime(object sender, EventArgs e)
        {
            time++;
            if (time % 60 < 10)
            {
                timer.Text= ($"0{time / 60}:0{time % 60}");
            }
            else
            {
                timer.Text = ($"0{time / 60}:{time % 60}");
            }
        }

        private void InitGridPage(string player1, string player2)
        {
            InitTable();
            label_player1.Text = player1;
            label_player2.Text = player2;
            if (player2 == "AI")
            {
                inAIMode = true;
                Debug.WriteLine("Game is in AI mode");
            }
            turnlabel.Text = $"Turn {turns}\n{player1}'s turn to place ships";
            StartTimer();
        }

        private void InitTable()
        {
            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    TorpedoButton btn = new TorpedoButton();

                    btn.Content = "O";

                    btn.SetX_coord(i-1);
                    btn.SetY_coord(j-1);

                    btn.Background = lightgray;

                    btn.Click += (sender, e) =>
                    {
                        ButtonPress(btn.GetX_coord(), btn.GetY_coord());
                    };

                    Grid.SetColumn(btn, i);
                    Grid.SetRow(btn, j);

                    table.Children.Add(btn);
                    buttonArray[i - 1, j - 1] = btn;
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

                tb.Text = letters[i - 1].ToString();
                tb.FontSize = 23;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.TextAlignment = TextAlignment.Center;

                Grid.SetColumn(tb, 0);
                Grid.SetRow(tb, i);

                table.Children.Add(tb);
            }
        }

        private void Run()
        {
            dataStore = new();
            _torpedoGameInstance = new(dataStore, (9, 9));
            Debug.WriteLine("Initialized service");
            Debug.WriteLine($"Current state is: {_torpedoGameInstance.GameState}");
            if (inAIMode)
            {
                humanPlayer = new Player(label_player1.Text);
                aiPlayer = new Player(label_player2.Text);
                _torpedoGameInstance.AddPlayer(humanPlayer);
                _torpedoGameInstance.AddPlayer(aiPlayer);
                Debug.WriteLine($"Added player {label_player1.Text}");
                Debug.WriteLine($"Added player {label_player2.Text}");
            }
            else
            {
                _torpedoGameInstance.AddPlayer(new Player(label_player1.Text));
                _torpedoGameInstance.AddPlayer(new Player(label_player2.Text));
                Debug.WriteLine($"Added player {label_player1.Text}");
                Debug.WriteLine($"Added player {label_player2.Text}");
            }
            _torpedoGameInstance.FinishAddingPlayers();
            Debug.WriteLine($"Current state is: {_torpedoGameInstance.GameState}");
            Debug.WriteLine($"Current ship to palce is: {_torpedoGameInstance.ShipsToPlace(_torpedoGameInstance.CurrentPlayer).First().Size}");
            UpdatingUI();
        }

        public GridPage(string player1 = "Player1", string player2 = "Player2")
        {
            InitializeComponent();
            InitGridPage(player1, player2);
            Run();
        }
    }
}
