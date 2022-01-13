using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NationalInstruments
{
    internal class TorpedoButton : Button
    {
        private int _xCoordinate;
        public int GetXCoordinate()
        {
            return _xCoordinate;
        }

        public void SetXCoordinate(int xCoordinate)
        {
             _xCoordinate = xCoordinate;
        }

        private int _yCoordinate;

        public int GetYCoordinate()
        {
            return _yCoordinate;
        }

        public void SetYCoordinate(int yCoordinate)
        {
            _yCoordinate = yCoordinate;
        }
    }
}
