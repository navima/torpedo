using System.Windows;

namespace NationalInstruments
{
    /// <summary>
    /// Interaction logic for NameGetWindow2.xaml
    /// </summary>
    public partial class NameGetWindow2 : Window
    {
        public string PlayerName1 { get; set; } = "Player1";
        public string PlayerName2 { get; set; } = "Player2";

        public NameGetWindow2()
        {
            InitializeComponent();
        }
        public bool CheckName(string name) => Validation.ValidateName(name);
        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            if (CheckName(InputBox.Text) && CheckName(InputBox2.Text))
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
