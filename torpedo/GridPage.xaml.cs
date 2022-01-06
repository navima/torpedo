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

namespace torpedo
{
    /// <summary>
    /// Interaction logic for GridPage.xaml
    /// </summary>
    public partial class GridPage : Page
    {
        public GridPage(String player1, String player2)
        {
            InitializeComponent();
            label_player1.Content = player1;
            label_player2.Content = player2;
            for (int i = 1; i < 10; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    TextBlock tb = new TextBlock();

                    tb.Text = "?";
                    tb.FontSize = 23;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Center;

                    Grid.SetColumn(tb, i);
                    Grid.SetRow(tb, j);

                    table.Children.Add(tb);
                }
            }
            Char[] letters = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I'};
            for(int i=1; i<10; i++)
            {
                TextBlock tb = new TextBlock();
                
                tb.Text = i.ToString();
                tb.FontSize = 23;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Center;

                Grid.SetColumn(tb, i);
                Grid.SetRow(tb, 0);

                table.Children.Add(tb);

                tb = new TextBlock();

                tb.Text = letters[i - 1].ToString();
                tb.FontSize = 23;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Center;

                Grid.SetColumn(tb, 0);
                Grid.SetRow(tb, i);

                table.Children.Add(tb);
            }

        }
    }
}
