// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#nullable enable

using System.ComponentModel.DataAnnotations;

namespace NationalInstruments
{
    public class Player
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; init; }

        public Player(string name)
        {
            Name = name;
        }

        public override string? ToString()
        {
            return $"Player{{Name: {Name}}}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Player player &&
                   Id == player.Id &&
                   Name == player.Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}