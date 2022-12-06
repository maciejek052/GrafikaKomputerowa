using System;
using System.Collections.Generic;
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

namespace Zadanie7
{
    /// <summary>
    /// Logika interakcji dla klasy ParameterDialog.xaml
    /// </summary>
    public partial class ParameterDialog : Window
    {
        public ParameterDialog(int value)
        {
            InitializeComponent();
            InputTextBox.Text = value.ToString();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.Parse(InputTextBox.Text) > 255)
            {
                MessageBox.Show("Wartość musi być mniejsza lub równa 255", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
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
