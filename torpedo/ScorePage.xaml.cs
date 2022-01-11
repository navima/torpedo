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
    /// <summary>
    /// Interaction logic for ScorePage.xaml
    /// </summary>

    public partial class ScorePage : Page
    {
        private readonly IDataStore _dataStore;
        public ScorePage(IDataStore dataStore)
        {
            InitializeComponent();
            _dataStore = dataStore;
            List<Outcome> outcomes = dataStore.GetAllOutcomes().ToList();
            Debug.WriteLine($"Number of games in dataStore: {outcomes.Count()}");

            outcomes.ForEach(outcome =>
            {
                ScoreStats stats = new ScoreStats();
                stats._score_player1 = outcome.Players.ToArray()[0].Name;
                stats._score_player2 = outcome.Players.ToArray()[1].Name;
                Debug.WriteLine($"Received game with players {outcome.Players.ToArray()[0]}, {outcome.Players.ToArray()[1]}");
                stats._score_rounds = outcome.NumberOfRounds;
                stats._score_winner = outcome.Winner.Name;
                stats._score_p1Hits = outcome.PlayerStats[outcome.Players.ToArray()[0]].Hits;
                stats._score_p2Hits = outcome.PlayerStats[outcome.Players.ToArray()[1]].Hits;
                Datagrid.Items.Add(stats);
            });
        }

        public class ScoreStats
        {
            public string _score_player1 { get; set; }
            public string _score_player2 { get; set; }
            public int _score_rounds { get; set; }
            public int _score_p1Hits { get; set; }
            public int _score_p2Hits { get; set; }
            public string _score_winner { get; set; }
            /*
            public ScoreStats(string player1, string player2, int scoreRounds, int p1Hits, int p2Hits, string winner)
            {
                _score_player1 = player1;
                _score_player2 = player2;
                _score_rounds = scoreRounds;
                _score_p1Hits = p1Hits;
                _score_p2Hits = p2Hits;
                _score_winner = winner;
            }*/
        }
    }
}
