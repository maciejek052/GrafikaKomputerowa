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

namespace Zadanie4
{
    /// <summary>
    /// Logika interakcji dla klasy DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        bool IsNonCloseButtonClicked; 
        public DialogWindow()
        {
            InitializeComponent();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                this.DialogResult = true;
                Close();
            }
        }
        public int transformationValue
        {
            get { return Int32.Parse(InputTextBox.Text); }
        }

    }
}
