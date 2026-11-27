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

namespace Sudoku
{
    public partial class NewGame : Window
    {
        public int empty_cells { get; private set; }
        public NewGame()
        {
            InitializeComponent();
        }

        private void Start_Bth_Click(object sender, RoutedEventArgs e)
        {
            empty_cells = (Easy.IsChecked == true) ? 30 :
                (Medium.IsChecked == true) ? 40 :
                (Hard.IsChecked == true) ? 50 :
                (Expert.IsChecked == true) ? 60 : 0;

            if (empty_cells == 0) return;
            DialogResult = true;
        }
    }
}
