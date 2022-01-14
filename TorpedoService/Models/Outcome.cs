// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#nullable enable
#pragma warning disable SA1000 // Keywords should be spaced correctly
#pragma warning disable SA1313

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NationalInstruments
{
    public class Outcome
    {
        public Outcome()
        {
        }

        public Outcome(Player player1, Player player2, ICollection<Stat> playerStats, Player winner, int numberOfRounds)
        {
            Player1 = player1;
            Player2 = player2;
            PlayerStats = playerStats;
            Winner = winner;
            NumberOfRounds = numberOfRounds;
        }

        [Key]
        public int Id { get; set; }
        [Required]
        public Player Player1 { get; set; }
        [Required]
        public Player Player2 { get; set; }
        [Required]
        public ICollection<Stat> PlayerStats { get; set; }
        [Required]
        public Player Winner { get; set; }
        public int WinnerId { get; set; }
        [Required]
        public int NumberOfRounds { get; set; }
    }
}