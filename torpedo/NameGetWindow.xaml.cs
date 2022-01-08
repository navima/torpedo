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
    /// Interaction logic for NameGetWindow.xaml
    /// </summary>
    /// 
    public partial class NameGetWindow : Window
    {

        public string playername = "";
        public NameGetWindow()
        {
            InitializeComponent();
        }

        public bool checkName(string name)
        {
            var reg = new Regex("^[a-zA-Z0-9]*$");
            if (!(reg.IsMatch(name)))
            {
                System.Windows.MessageBox.Show("Name can only contain letters and numbers");
            }
            else
            {
                if(name.Length < 3 || name.Length > 9)
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

        private void SubmitClicked (object sender, RoutedEventArgs e)
        {
            if (checkName(InputBox.Text))
            {
                playername = InputBox.Text.ToString();
                this.Close();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void closingEvent(object sender, CancelEventArgs e)
        {
            if (playername == "")
            {
                e.Cancel = true;
            }
        }
    }
}
