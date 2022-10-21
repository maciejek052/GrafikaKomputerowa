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
using System.Diagnostics;
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
        int? wysokosc = null;
        int? szerokosc = null;
        int? maxwartosc = null;
        int licznikbinarny = 0;
        List<int> kolory = new List<int>();
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PPM Files (*.ppm;)|*.ppm;|PGM Files (*.pgm;)|*.pgm;|PBM Files (*.pbm;)|*.pbm;|All files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                nazwaPliku = ofd.FileName;
                using (StreamReader plik = new StreamReader(nazwaPliku))
                {
                    string line;
                    int licznik = 0;
                    licznikbinarny = 0;
                    kolory = new();
                    while ((line = plik.ReadLine()) != null)
                    {
                        var podziellinie = Regex.Split(line, @"\s+").Where(s => s != string.Empty).ToArray();
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
                var mapa=new Bitmap((int)szerokosc, (int)wysokosc,format);
                var bytes = Array.ConvertAll<int, byte>(kolory.ToArray(), Convert.ToByte);
                Trace.WriteLine(bytes.Count());

                mapa =Podaj(mapa,bytes);
                Img.Source = obrazbitmapa(mapa);

            }
            Console.WriteLine((int)szerokosc);
            Console.WriteLine((int)wysokosc);
            Console.WriteLine(typ);
        }
        public int IntNormalizacja(string tekst)
        {
            var wartosc=int.Parse(tekst);
            if (maxwartosc != 255)
                return wartosc * 255 / (int)maxwartosc;
            return wartosc;
        }
        public int BinarnaNormalizacja(string tekst)
        {
            var wartosc = int.Parse(tekst, System.Globalization.NumberStyles.HexNumber);
            if (maxwartosc != 255)
                return wartosc * 255 / (int)maxwartosc;
            return wartosc;
        }
        public void PlikBinarny(string nazwaPliku)
        {
            FileStream fs = new FileStream(nazwaPliku, FileMode.Open);
            int toInt;
            string to;
            int pom=0;
            Trace.WriteLine(licznikbinarny);
            for (int i = 0;(toInt = fs.ReadByte()) != -1; i++)
            {
                to = string.Format("{0:X2}", toInt);
                pom++;
                var wartosc = this.BinarnaNormalizacja(to);
                kolory.Add(wartosc);
            }
            Trace.WriteLine("Bitch ass nigga");
            Trace.WriteLine(kolory.Count());
            Trace.WriteLine(kolory.Count());

            fs.Close();
        }
        public void PlikNieBinarny(string nazwaPliku)
        {
            using (StreamReader plik = new StreamReader(nazwaPliku))
            {
                int licznik = 0;
                string dlugosc;
                kolory = new();
                int pom = 0;
                while ((dlugosc=plik.ReadLine())!=null)
                {
                    pom++;
                    var xd = Regex.Split(dlugosc, @"\s+").Where(s => s != string.Empty).ToArray();
                    foreach(var tekst in xd)
                    {
                        var lokalny = tekst.Trim();
                        if (lokalny.Contains('#')) break;
                        if (String.IsNullOrEmpty(lokalny)) continue;
                        if (licznik<4)
                        {
                            licznik++;
                            continue;
                        }
                        kolory.Add(this.IntNormalizacja(lokalny));
                    }
                }
                Trace.WriteLine("Bitch ass nigga p3");
                Trace.WriteLine(kolory.Count());
                plik.Close();
            }
        }
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource obrazbitmapa(Bitmap map)
        {
            var handle = map.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle,IntPtr.Zero,Int32Rect.Empty,BitmapSizeOptions.FromWidthAndHeight(map.Width,map.Height));
            }
            finally { DeleteObject(handle); }
        }
        public static Bitmap Podaj(Bitmap map, byte[] wartosc)
        {
            BitmapData mapdata = map.LockBits(new System.Drawing.Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadWrite, map.PixelFormat);
            byte[] ne=new byte[mapdata.Stride*mapdata.Height];
            Marshal.Copy(mapdata.Scan0,ne,0, ne.Length);
            Trace.WriteLine(mapdata.Height*mapdata.Width*3);
            for (int i=0;i<mapdata.Height;i++)
            {
                for(int j=0;j<mapdata.Width*3;j++)
                {
                    //Trace.WriteLine((byte)(wartosc[i * map.Width*3 + j]));
                    ne[i*map.Width*3+j] = (byte)(wartosc[i * map.Width*3 + j]);
                }
            }
            Marshal.Copy(ne,0,mapdata.Scan0,ne.Length);
            map.UnlockBits(mapdata);
            return map;

        }
        

    }
}
