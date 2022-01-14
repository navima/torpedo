// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#nullable enable
#pragma warning disable SA1000 // Keywords should be spaced correctly
#pragma warning disable SA1313


using System.ComponentModel.DataAnnotations;

namespace NationalInstruments
{
    public class PlayerStat
    {
        public PlayerStat()
        {
        }

        public PlayerStat(int hits, int misses, int survivingShipParts)
        {
            Hits = hits;
            Misses = misses;
            SurvivingShipParts = survivingShipParts;
        }

        [Key]
        public int Id { get; set; }
        [Required]
        public Player Player { get; set; }
        [Required]
        public int Hits { get; set; }
        [Required]
        public int Misses { get; set; }
        [Required]
        public int SurvivingShipParts { get; set; }
    }
}