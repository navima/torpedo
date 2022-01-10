using System.Collections.Generic;

namespace NationalInstruments
{
    internal class PlayerStats
    {
        public int SunkenShips { get; private set; }
        public void IncrementSunkenShips() => SunkenShips++;
        public int Hits { get; private set; }
        public void IncrementHits() => Hits++;
        public int Misses { get; private set; }
        public void IncrementMisses() => Misses++;

        private readonly Dictionary<Ship, EShipStatus> _shipStatus = new();
        public Dictionary<Ship, EShipStatus> GetShipStatuses() => _shipStatus;
        public EShipStatus GetShipStatus(Ship ship) => _shipStatus[ship];
        public void SetShipStatus(Ship ship, EShipStatus status) => _shipStatus[ship] = status;
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
