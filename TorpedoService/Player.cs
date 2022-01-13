// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#nullable enable

namespace NationalInstruments
{
    public class Player
    {
        public string Name { get; init; }

        public Player(string name)
        {
            Name = name;
        }

        public override string? ToString()
        {
            return $"Player{{Name: {Name}}}";
        }
    }
}