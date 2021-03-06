// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#pragma warning disable SA1000 // Keywords should be spaced correctly
#pragma warning disable NI1704 // Identifiers should be spelled correctly
#pragma warning disable CA2201 // Do not raise reserved exception types
#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace NationalInstruments
{
    public class TorpedoService : IDataStore
    {
        private readonly List<Player> _players = new();
        private EGameState _gameState = EGameState.AddingPlayers;
        private readonly Dictionary<Ship, int> _startingShips = new()
        {
            { new Ship(1), 1 },
            { new Ship(2), 1 },
            { new Ship(3), 1 },
            { new Ship(4), 1 },
        };
        private readonly Dictionary<Player, List<Ship>> _unPlacedShips = new();
        private readonly Dictionary<Player, Dictionary<Position, Ship>> _placedShips = new();
        private readonly Dictionary<Player, Dictionary<Position, EHitResult>> _hitResults = new();
        private readonly IDataStore dataStore;
        private readonly (int, int) _tableSize;
        public event EventHandler<StateChangedEventArgs>? GameStateChanged;

        #region Constructors
        public TorpedoService(IDataStore dataStore, (int, int) tableSize)
        {
            this.dataStore = dataStore;
            this._tableSize = tableSize;
            CurrentPlayer = dataStore.AIPlayer;
        }
        #endregion

        #region Properties
        public IEnumerable<Player> Players => _players;
        public EGameState GameState
        {
            get => _gameState;
            private set
            {
                var oldGameState = _gameState;
                _gameState = value;
                GameStateChanged?.Invoke(this, new StateChangedEventArgs(oldGameState, value));
            }
        }
        public IDictionary<Ship, int> StartingShips => _startingShips;
        public Player CurrentPlayer { get; private set; }
        public (int, int) TableSize => _tableSize;
        public Bounds Bounds { get => new(0, 0, _tableSize.Item1 - 1, _tableSize.Item2 - 1); }
        public int Rounds { get; private set; }
        public IDictionary<Player, Dictionary<Position, EHitResult>> HitResults => _hitResults;

        public Player AIPlayer => dataStore.AIPlayer;
        #endregion

        private void EnsureState(EGameState state)
        {
            if (_gameState != state)
            {
                throw new IllegalStateException($"State should be {state}");
            }
        }

        public void AddPlayer(Player player)
        {
            EnsureState(EGameState.AddingPlayers);
            _players.Add(player);
        }
        public void FinishAddingPlayers()
        {
            EnsureState(EGameState.AddingPlayers);
            foreach (var player in _players)
            {
                // TODO
                _unPlacedShips.Add(player, (new List<Ship>(_startingShips.Keys)).ConvertAll(x => new Ship(x)));
                _placedShips.Add(player, new Dictionary<Position, Ship>());
                _hitResults.Add(player, new Dictionary<Position, EHitResult>());
            }
            IncrementPlayer();
            GameState = EGameState.PlacingShips;
        }
        [Pure]
        public IEnumerable<Ship> ShipsToPlace(Player player) => _unPlacedShips[player];
        [Pure]
        public IDictionary<Position, Ship> PlacedShips(Player player) => _placedShips[player];
        [Pure]
        public bool CanPlaceShip(Player player, Ship ship, Position position)
        {
            EnsureState(EGameState.PlacingShips);
            var expanded = ship.ExpandParts(position);

            //bool isCurrentPlayer = CurrentPlayer == player;
            bool hasShip = ShipsToPlace(player).Contains(ship);
            bool hasCollision = PlacedShips(player)
                        .SelectMany(x => x.Value.ExpandParts(x.Key).Keys)
                        .Any(x => expanded.Keys.Contains(x));
            bool isInsideBounds = expanded.All(part => Bounds.Contains(part.Key));

            return true // isCurrentPlayer
                && hasShip
                && !hasCollision
                && isInsideBounds;
        }
        public bool TryPlaceShip(Player player, Ship ship, Position position)
        {
            EnsureState(EGameState.PlacingShips);
            if (CanPlaceShip(player, ship, position))
            {
                _unPlacedShips[player].Remove(ship);
                PlacedShips(player).Add(position, ship);
                if (_unPlacedShips.All(kvp => !kvp.Value.Any()))
                {
                    _players.Shuffle();
                    Rounds = 1;
                    GameState = EGameState.SinkingShips;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public void PlaceShipRandom(Player player, Ship ship)
        {
            var random = new Random();
            Array values = Enum.GetValues(typeof(EOrientation));
            while (!TryPlaceShip(player, ship, Bounds.GetRandomPoint()))
            {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                ship.Orientation = (EOrientation)values.GetValue(random.Next(values.Length)); // This can never be null, thank you very much.
#pragma warning restore CS8605 // Unboxing a possibly null value.
            }
        }
        public void FinishPlacingShips()
        {
            IncrementPlayer();
        }
        public bool CanHit(Player player, Position position)
        {
            EnsureState(EGameState.SinkingShips);
            if (CurrentPlayer != player)
            {
                throw new InvalidOperationException();
            }
            if (!Bounds.Contains(position))
            {
                return false;
            }
            if (_hitResults[player].ContainsKey(position))
            {
                return false;
            }
            return true;
        }
        public bool TryHit(Player player, Position position, out EHitResult result)
        {
            result = EHitResult.None;
            EnsureState(EGameState.SinkingShips);
            if (!CanHit(player, position))
            {
                return false;
            }

            ShipPart? FindHitPart(IDictionary<Position, Ship> ships, Position position)
            {
                foreach (var (shipPos, ship) in ships)
                {
                    var shipParts = ship.ExpandParts(shipPos);
                    var hitPositionShipPart = shipParts.FirstOrDefault(x => x.Key == position);

                    if (hitPositionShipPart.Value != null)
                    {
                        return hitPositionShipPart.Value;
                    }
                }
                return null;
            }

            EHitResult resultLocal = EHitResult.Miss;
            var enemies = Players.Where(x => x != player);
            foreach (var enemy in enemies)
            {
                var ships = PlacedShips(enemy);
                var hitPart = FindHitPart(ships, position);

                if (hitPart != null)
                {
                    hitPart.Hit();
                    if (hitPart.Parent.Dead)
                    {
                        resultLocal = resultLocal.Escalate(EHitResult.Sink);
                    }
                    else
                    {
                        resultLocal = resultLocal.Escalate(EHitResult.Hit);
                    }
                }
            }
            _hitResults[player].TryAdd(position, resultLocal);
            if (IsGameOver())
            {
                GameState = EGameState.GameOver;
            }
            IncrementPlayer();
            result = resultLocal;
            return true;
        }
        public EHitResult HitSuggested(Player player)
        {
            var previousTries = _hitResults[player];
            var previousHits = previousTries.Where(x => x.Value > EHitResult.Miss);
            var adjacentPositions = previousHits
                    .Select(x => x.Key)
                    .SelectMany(x => new[]
                        {
                            x + new Position(1, 0),
                            x + new Position(-1, 0),
                            x + new Position(0, 1),
                            x + new Position(0, -1)
                        })
                    .Where(x => CanHit(player, x))
                    .ToArray();
            if (adjacentPositions.Any())
            {
                var random = new Random();
                var position = adjacentPositions[random.Next(adjacentPositions.Length)];
                var success = TryHit(player, position, out var result);
                if (success)
                {
                    return result;
                }
                else
                {
                    throw new Exception("This should never happen");
                }
            }
            else
            {
                while (true)
                {
                    if (TryHit(player, Bounds.GetRandomPoint(), out var result))
                    {
                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Ends the current player's turn.
        /// </summary>
        /// <returns>bool indicating if we've wrapped around to the first player</returns>
        private bool IncrementPlayer()
        {
            if (_players.Count == 0)
            {
                return false;
            }
            var index = CurrentPlayer is not null ? _players.IndexOf(CurrentPlayer) : -1;
            if (index == _players.Count - 1 || index == -1)
            {
                CurrentPlayer = _players[0];
                Rounds++;
                return true;
            }
            else
            {
                CurrentPlayer = _players[index + 1];
                return false;
            }
        }
        [Pure]
        public bool IsPlayerDead(Player player) => PlacedShips(player).Values.All(x => x.Dead);
        [Pure]
        private bool IsGameOver() => _players.Where(IsPlayerDead).Count() == _players.Count - 1;
        [Pure]
        public ShipPart?[,] GetBoard(Player player)
        {
            var board = GetBlankBoard<ShipPart?>();
            var ships = PlacedShips(player);
            foreach (var (shipPosition, ship) in ships)
            {
                foreach (var (shipPartPosition, shipPart) in ship.ExpandParts(shipPosition))
                {
                    board[shipPartPosition.X, shipPartPosition.Y] = shipPart;
                }
            }
            return board;
        }
        [Pure]
        public EHitResult?[,] GetHitBoard(Player player)
        {
            EHitResult?[,] board = GetBlankBoard<EHitResult?>();
            var hits = _hitResults[player];
            foreach (var (position, result) in hits)
            {
                board[position.X, position.Y] = result;
            }
            return board;
        }
        [Pure]
        public T[,] GetBlankBoard<T>() => new T[TableSize.Item1, TableSize.Item2];

        public Player? GetPlayerByName(string name)
        {
            return dataStore.GetPlayerByName(name);
        }

        public Player GetOrCreatePlayerByName(string name)
        {
            return dataStore.GetOrCreatePlayerByName(name);
        }

        public Player CreatePlayer(string name)
        {
            return dataStore.CreatePlayer(name);
        }

        public IEnumerable<Player> GetAllPlayers()
        {
            return dataStore.GetAllPlayers();
        }

        public void AddOutcome(Outcome outcome)
        {
            dataStore.AddOutcome(outcome);
        }

        public IEnumerable<Outcome> GetAllOutcomes()
        {
            return dataStore.GetAllOutcomes();
        }
    }

    [Serializable]
    public class IllegalStateException : Exception
    {
        public IllegalStateException() { }
        public IllegalStateException(string message) : base(message) { }
        public IllegalStateException(string message, Exception inner) : base(message, inner) { }
        protected IllegalStateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class StateChangedEventArgs : EventArgs
    {
        public EGameState OldGameState { get; init; }
        public EGameState NewGameState { get; init; }

        public StateChangedEventArgs(EGameState oldGameState, EGameState newGameState) => (this.OldGameState, this.NewGameState) = (oldGameState, newGameState);

        public override string? ToString() => $"{OldGameState} => {NewGameState}";
    }
    public enum EHitResult
    {
        None = 0,
        Miss = 1,
        Hit = 2,
        Sink = 3,
    }
    public static class EHitResultExtensions
    {
        public static EHitResult Escalate(this EHitResult self, EHitResult target) => target > self ? target : self;
    }
}
