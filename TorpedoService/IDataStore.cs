// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#nullable enable
#pragma warning disable SA1000 // Keywords should be spaced correctly
#pragma warning disable SA1313

using System.Collections.Generic;

namespace NationalInstruments
{
    public interface IDataStore
    {
        public Player? GetPlayerByName(string name);
        public Player GetOrCreatePlayerByName(string name);
        public Player AIPlayer { get; }
        public Player CreatePlayer(string name);
        public IEnumerable<Player> GetAllPlayers();
        public void AddOutcome(Outcome outcome);
        public IEnumerable<Outcome> GetAllOutcomes();
    }

    public class InMemoryDataStore : IDataStore
    {
        private readonly List<Player> _players = new();
        private readonly List<Outcome> _outcomes = new();
        private readonly Player _aiPlayer = new("AI");

        public Player CreatePlayer(string name)
        {
            var player = new Player(name);
            _players.Add(player);
            return player;
        }

        public void AddOutcome(Outcome outcome) => _outcomes.Add(outcome);

        public IEnumerable<Outcome> GetAllOutcomes() => _outcomes.AsReadOnly();

        public IEnumerable<Player> GetAllPlayers() => _players.AsReadOnly();

        public Player GetOrCreatePlayerByName(string name) => GetPlayerByName(name) ?? CreatePlayer(name);

        public Player? GetPlayerByName(string name) => _players.Find(p => p.Name == name);

        public Player AIPlayer => _aiPlayer;
    }
}