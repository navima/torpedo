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
        private int _x_coord;
        public int GetX_coord()
        {
            return _x_coord;
        }

        public void SetX_coord(int x_coord)
        {
             _x_coord = x_coord;
        }

        private int _y_coord;

        public int GetY_coord()
        {
            return _y_coord;
        }

        public void SetY_coord(int y_coord)
        {
            _y_coord = y_coord;
        }
    }
}
