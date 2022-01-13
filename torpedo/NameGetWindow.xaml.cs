using System.Windows;

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
