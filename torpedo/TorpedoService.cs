#pragma warning disable SA1000 // Keywords should be spaced correctly

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NationalInstruments
{
    internal class TorpedoService
    {
        private readonly List<Player> _players = new();
        private EGameState _gameState = EGameState.None;
        private readonly Dictionary<Player, List<Ship>> _unPlacedShips = new();
        private readonly Dictionary<Player, List<Ship>> _placedShips = new();
        private readonly DataStore dataStore;

        public TorpedoService(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public List<Player> Players { get { return _players; } }
        public void AddPlayer(Player player) => _players.Add(player);
        public EGameState GameState { get => _gameState; private set => _gameState = value; }
        public List<Ship> ShipsToPlace(Player player) => _unPlacedShips[player];
        public List<Ship> Ships(Player player) => _placedShips[player];
        public bool CanPlaceShip(Player player, Ship ship, Position position)
        {
            var expanded = ship.ExpandParts(position);
            return ShipsToPlace(player).Contains(ship) && !Ships(player).SelectMany(x => x.ExpandParts(position)).Where(x => expanded.Contains(x)).Any();
        }
        public bool TryPlaceShip(Player player, Ship ship, Position position)
        {
            if (CanPlaceShip(player, ship, position))
            {
                ShipsToPlace(player).Remove(ship);
                Ships(player).Add(ship);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public struct Position
    {
        public int X { get; init; }
        public int Y { get; init; }

        public override bool Equals(object obj)
        {
            return obj is Position other && X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }
    }

    public class Ship
    {
        private List<bool> _parts = new();

        public int Size { get; init; }
        public EOrientation Orientation { get; init; }
        public bool this[int index]
        {
            get => _parts[index];
            set => _parts[index] = value;
        }
        public List<Position> ExpandParts(Position position)
        {
            List<Position> positions = new();
            for (int i = 0; i < Size; i++)
            {
                positions.Add(new Position
                {
                    X = position.X + i * (Orientation == EOrientation.Right ? 1 : Orientation == EOrientation.Left ? -1 : 0),
                    Y = position.Y + i * (Orientation == EOrientation.Up ? 1 : Orientation == EOrientation.Down ? -1 : 0)
                });
            }
            return positions;
        }
    }

    public enum EOrientation
    {
        Up, Right, Down, Left
    }

    public enum EGameState
    {
        None = 0,
        PlacingShips
    }
}
