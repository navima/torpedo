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
        private char[] letters = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I'};
        private int time = 0;
        private int turns = 0;
        private bool InCheatMode = false;

        TorpedoButton[,] buttonArray = new TorpedoButton[9, 9];

        private void updateTable(int x, int y)
        {
            buttonArray[x, y].Content = 'X';
            buttonArray[x, y].IsEnabled = false;
        }

        public void cheatMode()
        {
            if (!InCheatMode)
            {
                Debug.WriteLine("Cheatmode enabled");
                InCheatMode = true;
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        buttonArray[i, j].Content = "C";
                    }
                }
            }
            else
            {
                Debug.WriteLine("Cheatmode disabled");
                InCheatMode =false;
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        buttonArray[i, j].Content = "O";
                    }
                }
            }
        }

        private void butonPress(int x, int y)
        {
            Debug.WriteLine($"Button {x}:{y} pressed");
            updateTable(x-1, y-1);
        }

        private void startTimer()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += incrementTime;
            dt.Start();
        }

        private void incrementTime(object sender, EventArgs e)
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

        public GridPage(string player1, string player2)
        {
            InitializeComponent();
            table.Focus();
            label_player1.Text = player1;
            label_player2.Text = player2;
            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    TorpedoButton btn = new TorpedoButton();

                    btn.Content = "O";

                    btn.SetX_coord(i);
                    btn.SetY_coord(j);

                    btn.Click += (sender, e) =>
                    {
                        butonPress(btn.GetX_coord(), btn.GetY_coord());
                    };

                    Grid.SetColumn(btn, i);
                    Grid.SetRow(btn, j);

                    table.Children.Add(btn);
                    buttonArray[i-1, j-1] = btn;
                }
            }
            for(int i=1; i<10; i++)
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

                tb.Text = letters[i- 1].ToString();
                tb.FontSize = 23;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.TextAlignment = TextAlignment.Center;

                Grid.SetColumn(tb, 0);
                Grid.SetRow(tb, i);

                table.Children.Add(tb);
            }
            turnlabel.Text = $"Turn {turns}\n{player1}'s turn to place ships";
            startTimer();
        }
    }
}
