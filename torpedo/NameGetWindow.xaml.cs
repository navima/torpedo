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

        public string PlayerName { get; set; } = "Player1";

        public NameGetWindow()
        {
            InitializeComponent();
        }

        public bool CheckName(string name) => Validation.ValidateName(name);

        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            if (CheckName(InputBox.Text))
            {
                DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Name can only contain letters and numbers, and must be between 3 and 9 characters");
            }
        }
    }
}
