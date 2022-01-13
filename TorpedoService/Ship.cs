// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace NationalInstruments
{
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
            this.Orientation = other.Orientation;
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
