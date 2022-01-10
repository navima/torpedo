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
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.ComponentModel;


namespace NationalInstruments
{
    /// <summary>
    /// Interaction logic for NameGetWindow2.xaml
    /// </summary>
    public partial class NameGetWindow2 : Window
    {
        private string _playername1 = string.Empty;
        public string GetPlayername1()
        {
            return _playername1;
        }

        private string _playername2 = string.Empty;
        public string GetPlayername2()
        {
            return _playername2;
        }
        public NameGetWindow2()
        {
            InitializeComponent();
        }
        public bool CheckName(string name)
        {
            var reg = new Regex("^[a-zA-Z0-9]*$");
            if (!(reg.IsMatch(name)))
            {
                System.Windows.MessageBox.Show("Name can only contain letters and numbers");
            }
            else
            {
                if (name.Length < 3 || name.Length > 9)
                {
                    System.Windows.MessageBox.Show("Name must be between 3 and 9 characters");
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            if (CheckName(InputBox.Text) && CheckName(InputBox2.Text))
            {
                _playername1 = InputBox.Text;
                _playername2 = InputBox2.Text;
                this.Close();
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void ClosingEvent(object sender, CancelEventArgs e)
        {
            if (_playername1 == string.Empty || _playername2 == string.Empty)
            {
                e.Cancel = true;
            }
        }
    }
}
