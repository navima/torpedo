using System.Windows;
using System.Windows.Input;

namespace NationalInstruments
{
    public partial class MainWindow : Window
    {
        private readonly IDataStore _store;
        private GridPage? _activeGridPage = null;

        public MainWindow()
        {
            InitializeComponent();
            _store = new EFDataStore();
        }

        private void WindowKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C)
            {
                _activeGridPage?.CheatMode();
            }

            if (e.Key == Key.P)
            {
                _activeGridPage?.PlayerViewMode();
            }
        }

        private void GameClicked1(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow();
            if (w.ShowDialog() ?? false)
            {
                _activeGridPage = new GridPage(_store, _store.GetOrCreatePlayerByName(w.PlayerName), _store.AIPlayer);
                Main.Content = _activeGridPage;
            }
        }

        private void GameClicked2(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow2();
            if (w.ShowDialog() ?? false)
            {
                _activeGridPage = new GridPage(_store, _store.GetOrCreatePlayerByName(w.PlayerName1), _store.GetOrCreatePlayerByName(w.PlayerName2));
                Main.Content = _activeGridPage;
            }
        }

        private void ScoreClicked(object sender, RoutedEventArgs e)
        {
            Main.Content = new ScorePage(_store);
        }
        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
