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

        private readonly string[] _shipStatus = new string[] { "Not placed yet", "Not placed yet", "Not placed yet", "Not placed yet" };
        public string[] GetShipStatus() => _shipStatus;
        public string GetShipStatusItem(int n) => _shipStatus[n];
        public void SetShipStatus(int n, string s) => _shipStatus[n] = s;
    }
}
