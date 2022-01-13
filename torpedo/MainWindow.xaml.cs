using System.Windows;
using System.Windows.Input;

namespace NationalInstruments
{
    public partial class MainWindow : Window
    {
        private char _openedPage = 'N';
        private GridPage? _page = null;
        private readonly IDataStore _store;

        public MainWindow()
        {
            InitializeComponent();
            _store = new InMemoryDataStore();
        }

        private void WindowKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && _openedPage == 'O')
            {
                _page?.CheatMode();
            }

            if (e.Key == Key.P && _openedPage == 'O')
            {
                _page?.PlayerViewMode();
            }
        }

        private void GameClicked1(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow();
            if (w.ShowDialog() ?? false)
            {
                _page = new GridPage(_store, _store.GetOrCreatePlayerByName(w.PlayerName), _store.AIPlayer);
                Main.Content = _page;
                _openedPage = 'O';
            }
        }

        private void GameClicked2(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow2();
            if (w.ShowDialog() ?? false)
            {
                Main.Content = new GridPage(_store, _store.GetOrCreatePlayerByName(w.PlayerName1), _store.GetOrCreatePlayerByName(w.PlayerName2));
                _openedPage = 'T';
            }
        }

        private void ScoreClicked(object sender, RoutedEventArgs e)
        {
            Main.Content = new ScorePage(_store);
            _openedPage = 'S';
        }
        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
