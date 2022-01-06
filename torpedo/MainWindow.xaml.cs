using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using torpedo;

namespace NationalInstruments
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GameClicked1(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow();
            if(w.ShowDialog() == false)
            {
                Main.Content = new GridPage(w.playername,"AI");
            }
        }

        private void GameClicked2(object sender, RoutedEventArgs e)
        {
            var w = new NameGetWindow2();
            if (w.ShowDialog() == false)
            {
                Main.Content = new GridPage(w.playername1,w.playername2);
            }
        }

        private void ScoreClicked(object sender, RoutedEventArgs e)
        {
// Main.Content = new scorepage();
        }
    }
}
