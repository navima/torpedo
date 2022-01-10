using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NationalInstruments
{
    internal class PlayerStats
    {
        private int _sunken_ships = 0;
        public int GetSunken_Ships()
        {
            return _sunken_ships;
        }
        public void IncrementSunkenShips()
        {
            _sunken_ships++;
        }

        private int _hits = 0;
        public int GetHits()
        {
            return _hits;
        }
        public void IncrementHits()
        {
            _hits++;
        }

        private int _misses = 0;
        public int GetMisses()
        {
            return _misses;
        }
        public void IncrementMisses()
        {
            _misses++;
        }

        private readonly string[] _shipStatus = new string[] { "Not placed yet", "Not placed yet", "Not placed yet", "Not placed yet" };

        public string[] GetShipStatus()
        {
            return _shipStatus;
        }

        public string GetShipStatusItem(int n)
        {
            return _shipStatus[n];
        }
        public void SetShipStatus(int n, string s)
        {
            _shipStatus[n] = s;
        }
    }
}
