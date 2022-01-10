using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NationalInstruments
{
    /// <summary>
    /// Interaction logic for NameGetWindow.xaml
    /// </summary>
    ///
    public partial class NameGetWindow : Window
    {

        public string? PlayerName { get; private set; }

        public NameGetWindow()
        {
            InitializeComponent();
            DialogResult = false;
        }

        public bool CheckName(string name)
        {
            var reg = new Regex("^[a-zA-Z0-9]*$");
            if (!(reg.IsMatch(name)))
            {
                MessageBox.Show("Name can only contain letters and numbers");
                return false;
            }
            if (name.Length < 3 || name.Length > 9)
            {
                MessageBox.Show("Name must be between 3 and 9 characters");
                return false;
            }
            return true;
        }

        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            if (CheckName(InputBox.Text))
            {
                PlayerName = InputBox.Text;
                DialogResult = true;
                this.Close();
            }
        }
    }
}
