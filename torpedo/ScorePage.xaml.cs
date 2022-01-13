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
            Debug.WriteLine($"Number of games in dataStore: {outcomes.Count}");

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
                Sort(stats);
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
        }

        private void Sort(ScoreStats stats)
        {
            if(stats._score_player2 == "Player1" || stats._score_player1 == "AI")
            {
                (stats._score_player1, stats._score_player2) = (stats._score_player2, stats._score_player1);
                (stats._score_p1Hits, stats._score_p2Hits) = (stats._score_p2Hits, stats._score_p1Hits);
            }
        }
    }
}
