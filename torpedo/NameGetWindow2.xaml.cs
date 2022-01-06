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

namespace torpedo
{
    /// <summary>
    /// Interaction logic for NameGetWindow2.xaml
    /// </summary>
    public partial class NameGetWindow2 : Window
    {

        public string playername1 = "";
        public string playername2 = "";
        public NameGetWindow2()
        {
            InitializeComponent();
        }
        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            playername1 = InputBox.Text.ToString();
            playername2 = InputBox2.Text.ToString();
            this.Close();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
