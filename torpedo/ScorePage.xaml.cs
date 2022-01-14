using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

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

            List<Outcome> outcomes = _dataStore.GetAllOutcomes().ToList();
            Debug.WriteLine($"Number of games in dataStore: {outcomes.Count}");

            outcomes.ForEach(outcome => Datagrid.Items.Add(new OutcomeDataRowAdapter(outcome)));
        }
    }

    public struct OutcomeDataRowAdapter
    {
        private readonly Outcome _outcome;
        private readonly Player _player1;
        private readonly Player _player2;

        public OutcomeDataRowAdapter(Outcome outcome)
        {
            _outcome = outcome;
            // These could be expensive to get, so we cache them
            _player1 = _outcome.Players.First();
            _player2 = _outcome.Players.Skip(1).First();
        }

        public Player Player1 => _player1;
        public string Player1Name { get => Player1.Name; }
        public Player Player2 => _player2;
        public string Player2Name { get => Player2.Name; }
        public int Rounds { get => _outcome.NumberOfRounds; }
        public int Player1Hits
        {
            get
            {
                var p1 = Player1;
                return _outcome.PlayerStats.FirstOrDefault(x => x.Player == p1).Hits;
            }
        }
        public int Player2Hits
        {
            get
            {
                var p2 = Player2;
                return _outcome.PlayerStats.FirstOrDefault(x => x.Player == p2).Hits;
            }
        }
        public string Winner { get => _outcome.Winner.Name; }

        #region Junk

        public override bool Equals(object? obj)
        {
            return obj is OutcomeDataRowAdapter adapter
                && EqualityComparer<Outcome>.Default.Equals(_outcome, adapter._outcome);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_outcome);
        }

        public static bool operator ==(OutcomeDataRowAdapter left, OutcomeDataRowAdapter right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OutcomeDataRowAdapter left, OutcomeDataRowAdapter right)
        {
            return !(left == right);
        }

        #endregion
    }
}
