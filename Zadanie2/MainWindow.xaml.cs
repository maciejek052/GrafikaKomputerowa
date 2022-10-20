using Microsoft.Win32;
using System;
using System.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Zadanie2
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        string nazwaPliku;
        string typ = "";
        int wysokosc = 0;
        int szerokosc = 0;
        int maxwartosc = 0;
        int licznikbinarny = 0;
        List<int> kolory = new List<int>();
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PPM Files (*.ppm;)|*.ppm;|PGM Files (*.pgm;)|*.pgm;|PBM Files (*.pbm;)|*.pbm;|All files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                nazwaPliku = ofd.FileName;
                using (StreamReader plik = new StreamReader(ofd.FileName))
                {
                    string line;
                    int licznik = 0;
                    licznikbinarny = 0;
                    while ((line = plik.ReadLine()) != null)
                    {
                        var podziellinie = Regex.Split(line, @"\s+").Where(s => s != String.Empty).ToArray();
                        licznikbinarny += podziellinie.Length;
                        foreach (var pod in podziellinie)
                        {
                            var wartosc = pod.Trim();
                            if (wartosc.Contains('#'))
                            {
                                break;
                            }
                            if (String.IsNullOrEmpty(wartosc))
                            {
                                continue;
                            }
                            if (licznik == 0)
                            {
                                typ = wartosc;
                                licznik++;
                            }
                            else if (licznik == 1)
                            {
                                szerokosc = int.Parse(wartosc);
                                licznik++;
                            }
                            else if (licznik == 2)
                            {
                                wysokosc = int.Parse(wartosc);
                                licznik++;
                            }
                            else if (licznik == 3)
                            {
                                maxwartosc = int.Parse(wartosc);
                                licznik++;
                            }
                        }
                    }
                    plik.Close();

                }

                if (int.Parse(typ[typ.Count()-1].ToString()) > 3)
                {
                    PlikBinarny(nazwaPliku);
                }
                else
                {
                    PlikNieBinarny(nazwaPliku);
                }
                var format = typ switch
                {
                    "P1" => System.Drawing.Imaging.PixelFormat.Format1bppIndexed,
                    "P2" => System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                    "P3" => System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                    "P4" => System.Drawing.Imaging.PixelFormat.Format1bppIndexed,
                    "P5" => System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                    "P6" => System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                };

            }
            Console.WriteLine(szerokosc);
            Console.WriteLine(wysokosc);
            Console.WriteLine(typ);
        }

        public void PlikBinarny(string nazwaPliku)
        {
        }
        public void PlikNieBinarny(string nazwaPliku)
        {

        }
        

    }
}
