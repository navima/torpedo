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

namespace NationalInstruments
{

    public partial class MainWindow : Window
    {

        private char OpenedPage = 'N';
        private GridPage page = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && OpenedPage == 'O')
            {
                page.CheatMode();
            }

            if (e.Key == Key.P && OpenedPage == 'O')
            {
                page.PlayerViewMode();
            }
        }

        private void GameClicked1(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow();
            if(w.ShowDialog() == false)
            {
                page = new GridPage(w.playername, "AI");
                Main.Content = page;
                OpenedPage = 'O';
            }
        }

        private void GameClicked2(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow2();
            if (w.ShowDialog() == false)
            {
                Main.Content = new GridPage(w.playername1,w.playername2);
                OpenedPage = 'T';
            }
        }

        private void ScoreClicked(object sender, RoutedEventArgs e)
        {
            // Main.Content = new scorepage();
            OpenedPage = 'S';
        }
        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
