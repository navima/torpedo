// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#nullable enable
#pragma warning disable SA1000 // Keywords should be spaced correctly
#pragma warning disable SA1313

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NationalInstruments
{
    public class Outcome
    {
        public Outcome()
        {
        }

        public Outcome(ICollection<Player> players, ICollection<PlayerStat> playerStats, Player winner, int numberOfRounds)
        {
            Players = players;
            PlayerStats = playerStats;
            Winner = winner;
            NumberOfRounds = numberOfRounds;
        }

        [Key]
        public int Id { get; set; }
        [Required]
        public ICollection<Player> Players { get; set; }
        [Required]
        public ICollection<PlayerStat> PlayerStats { get; set; }
        [Required]
        public Player Winner { get; set; }
        [Required]
        public int NumberOfRounds { get; set; }
    }
}