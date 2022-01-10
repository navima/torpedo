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

namespace NationalInstruments
{
    public partial class MainWindow : Window
    {
        private char _openedPage = 'N';
        private GridPage? _page = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && _openedPage == 'O')
            {
                _page.CheatMode();
            }

            if (e.Key == Key.P && _openedPage == 'O')
            {
                _page.PlayerViewMode();
            }
        }

        private void GameClicked1(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow();
            if (w.ShowDialog() == false)
            {
                _page = new GridPage(w.GetPlayername(), "AI");
                Main.Content = _page;
                _openedPage = 'O';
            }
        }

        private void GameClicked2(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow2();
            if (w.ShowDialog() == false)
            {
                Main.Content = new GridPage(w.GetPlayername1(), w.GetPlayername2());
                _openedPage = 'T';
            }
        }

        private void ScoreClicked(object sender, RoutedEventArgs e)
        {
            Main.Content = new ScorePage();
            _openedPage = 'S';
        }
        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
