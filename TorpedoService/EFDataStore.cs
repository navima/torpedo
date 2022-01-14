#nullable enable
#pragma warning disable SA1000 // Keywords should be spaced correctly

namespace NationalInstruments
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;

    public class EFDataStore : IDataStore
    {
        private readonly TorpedoContext db;
        public EFDataStore()
        {
            db = new();
        }

        public Player AIPlayer => GetOrCreatePlayerByName("AI");

        public void AddOutcome(Outcome outcome)
        {
            using (var ts = new TransactionScope())
            {
                foreach (var stat in outcome.PlayerStats)
                {
                    db.Add(stat);
                }
                db.SaveChanges();
                ts.Complete();
            }
            db.Outcomes.Add(outcome);
            db.SaveChanges();
        }

        public Player CreatePlayer(string name)
        {
            var player = new Player(name);
            db.Players.Add(player);
            db.SaveChanges();
            return player;
        }

        public IEnumerable<Outcome> GetAllOutcomes()
        {
            return db.Outcomes
                .Include(outcome => outcome.Player2)
                .Include(outcome => outcome.Player1)
                .Include(outcome => outcome.PlayerStats);/*
                    .ThenInclude(playerstats => playerstats.Hits)
                .Include(outcome => outcome.PlayerStats)
                    .ThenInclude(playerstats => playerstats.Misses)
                .Include(outcome => outcome.PlayerStats)
                    .ThenInclude(playerstats => playerstats.SurvivingShipParts);*/
        }

        public IEnumerable<Player> GetAllPlayers()
        {
            return db.Players;
        }

        public Player GetOrCreatePlayerByName(string name)
        {
            Player? p = GetPlayerByName(name);
            if (p is null)
            {
                p = CreatePlayer(name);
            }
            return p;
        }

        public Player? GetPlayerByName(string name)
        {
            return db.Players.SingleOrDefault(x => x.Name == name);
        }
    }
}
