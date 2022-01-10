#nullable enable
#pragma warning disable SA1000 // Keywords should be spaced correctly

using System.Collections.Generic;

namespace NationalInstruments
{
    public interface IDataStore
    {
        Player? GetPlayerByName(string name);
        Player GetOrCreatePlayerByName(string name);
        Player CreatePlayer(string name);
        IEnumerable<Player> GetAllPlayers();
    }

    public class InMemoryDataStore : IDataStore
    {

        private readonly List<Player> _players = new();

        public Player CreatePlayer(string name)
        {
            var player = new Player(name);
            _players.Add(player);
            return player;
        }

        public IEnumerable<Player> GetAllPlayers() => _players.AsReadOnly();

        public Player GetOrCreatePlayerByName(string name) => GetPlayerByName(name) ?? CreatePlayer(name);

        public Player? GetPlayerByName(string name) => _players.Find(p => p.Name == name);
    }
}