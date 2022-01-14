#pragma warning disable SA1000 // Keywords should be spaced correctly

namespace NationalInstruments
{
    using System;
    using System.Collections.Generic;

    public record PlayerStats
    {
        public int SunkenShips { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }
        public Dictionary<Ship, EShipStatus> ShipStatuses { get; private set; } = new();

        internal void Deconstruct(out int sunkenShips, out int hits, out int misses, out Dictionary<Ship, EShipStatus> shipStatuses)
        {
            sunkenShips = SunkenShips;
            hits = Hits;
            misses = Misses;
            shipStatuses = ShipStatuses;
        }
    }

    public enum EShipStatus
    {
        None = 0,
        NotPlaced,
        Placed,
        Dead
    }
    public static class EShipStatusExtensions
    {
        public static string ToUserReadableString(this EShipStatus status) => status switch
        {
            EShipStatus.NotPlaced => "Not yet placed",
            EShipStatus.Placed => "Placed",
            EShipStatus.Dead => "Sunken",
            _ => status.ToString(),
        };
    }
}
