// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#pragma warning disable SA1000 // Keywords should be spaced correctly
#nullable enable

using System;

namespace NationalInstruments
{
    public readonly struct Position
    {
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; init; }
        public int Y { get; init; }

        public static Position operator +(Position left, Position right) => new(left.X + right.X, left.Y + right.Y);
        public static Position operator -(Position left, Position right) => new(left.X - right.X, left.Y - right.Y);

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

        public Position GetRandomPoint()
        {
            var random = new Random();
            var x = random.Next(X, X + Width + 1);
            var y = random.Next(Y, Y + Height + 1);
            return new Position(x, y);
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
}
