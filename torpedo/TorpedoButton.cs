using System.Windows.Controls;

namespace NationalInstruments
{
    internal class TorpedoButton : Button
    {
        public Position GetAsPosition()
        {
            return new Position(X, Y);
        }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
