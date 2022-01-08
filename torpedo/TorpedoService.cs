#pragma warning disable SA1000 // Keywords should be spaced correctly
#pragma warning disable NI1704 // Identifiers should be spelled correctly

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NationalInstruments
{
    public class TorpedoService
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
        private readonly DataStore dataStore;
        private readonly (int, int) _tableSize;
        public event EventHandler<StateChangedEventArgs>? GameStateChanged;

        #region Constructors
        public TorpedoService(DataStore dataStore, (int, int) tableSize)
        {
            this.dataStore = dataStore;
            this._tableSize = tableSize;
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
        public Player? CurrentPlayer { get; private set; }
        public (int, int) TableSize => _tableSize;
        public Bounds Bounds { get => new Bounds(0, 0, _tableSize.Item1 - 1, _tableSize.Item2 - 1); }

        #endregion

        private void EnsureState(EGameState state)
        {
            if (_gameState != state)
            {
                throw new IllegalStateException($"State should be {state.ToString()}");
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

            bool isCurrentPlayer = CurrentPlayer == player;
            bool hasShip = ShipsToPlace(player).Contains(ship);
            bool hasCollision = PlacedShips(player)
                        .SelectMany(x => x.Value.ExpandParts(x.Key).Keys)
                        .Any(x => expanded.Keys.Contains(x));
            bool isInsideBounds = expanded.All(part => Bounds.Contains(part.Key));

            return isCurrentPlayer
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
                return true;
            }
            else
            {
                return false;
            }
        }
        public void FinishPlacingShips(Player player)
        {
            EnsureState(EGameState.PlacingShips);
            if (CurrentPlayer == player)
            {
                var wraparound = IncrementPlayer();
                if (wraparound)
                {
                    GameState = EGameState.SinkingShips;
                }
            }
        }
        public bool TryHit(Player player, Position position, out EHitResult result)
        {
            result = EHitResult.None;
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

            Func<IDictionary<Position, Ship>, Position, ShipPart?> findHitPart = (ships, position) =>
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
            };

            EHitResult resultLocal = EHitResult.Miss;
            var enemies = Players.Where(x => x != player);
            foreach (var enemy in enemies)
            {
                var ships = PlacedShips(enemy);
                var hitPart = findHitPart(ships, position);

                if (hitPart != null)
                {
                    hitPart.Hit();
                    if (hitPart.Parent.Dead)
                    {
                        resultLocal.Escalate(EHitResult.Sink);
                    }
                    else
                    {
                        resultLocal.Escalate(EHitResult.Hit);
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
        public static void Escalate(this EHitResult self, EHitResult other)
        {
            if ((short)other > (short)self)
            {
                self = other;
            }
        }
    }

    public readonly struct Position
    {
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; init; }
        public int Y { get; init; }

        #region Junk
        public override bool Equals(object? obj)
        {
            return obj is Position position
                && X == position.X
                && Y == position.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string? ToString() => $"x: {X}, y: {Y}";

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }
        #endregion
    }

    public readonly struct Bounds
    {
        public Bounds(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }

        public bool Contains(Position position)
        {
            return position.X >= X
                && position.Y >= Y
                && position.X <= Width
                && position.Y <= Height;
        }

        #region Junk

        public override bool Equals(object? obj)
        {
            return obj is Bounds bounds
                && X == bounds.X
                && Y == bounds.Y
                && Width == bounds.Width
                && Height == bounds.Height;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public override string? ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(Bounds left, Bounds right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Bounds left, Bounds right)
        {
            return !(left == right);
        }

        #endregion
    }

    public class Ship
    {
        // TODO Change to Dictionary to support arbitrary shapes
        private readonly List<ShipPart> _parts;
        public int Size { get; init; }
        public EOrientation Orientation { get; set; }
        public bool Dead { get => !Parts.Where(x => x.Alive).Any(); }
        public ShipPart this[int index]
        {
            get => _parts[index];
            set => _parts[index] = value;
        }

        public Ship(int size)
        {
            Size = size;
            _parts = new List<ShipPart>(Size);
            for (int i = 0; i < Size; i++)
            {
                _parts.Add(new ShipPart(this));
            }
        }
        // Copy constructor
        public Ship(Ship other) : this(other.Size)
        {
        }

        public IEnumerable<ShipPart> Parts => _parts;

        [Pure]
        public IDictionary<Position, ShipPart> ExpandParts(Position position) =>
            new Dictionary<Position, ShipPart>(Enumerable.Range(0, Size).Select(i =>
               new KeyValuePair<Position, ShipPart>(
                   new Position(
                       position.X + i * (Orientation == EOrientation.Right ? 1 : Orientation == EOrientation.Left ? -1 : 0),
                       position.Y + i * (Orientation == EOrientation.Up ? -1 : Orientation == EOrientation.Down ? 1 : 0)),
                   _parts[i])));
        public override string ToString() => $"Ship[{Parts.Select(x => x.Alive ? "O" : "X").Aggregate((x, y) => x + y)}]";
    }

    public class ShipPart
    {
        public bool Alive { get; private set; } = true;
        public Ship Parent { get; init; }
        public ShipPart(Ship parent) => Parent = parent;
        public void Hit() => Alive = false;
    }

    public enum EOrientation
    {
        Up, Right, Down, Left
    }

    public enum EGameState
    {
        None = 0,
        AddingPlayers,
        PlacingShips,
        SinkingShips,
        GameOver
    }
}
