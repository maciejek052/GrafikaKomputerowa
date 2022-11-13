using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zadanie4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        BitmapImage bitmapImage;
        Bitmap originalBitmap;
        Bitmap currentBitmap;
        int transValue;
        public enum Filter
        {
            AVERAGE,
            MEDIAN,
            EDGES,
            HIGHPASS,
            GAUSSIN
        }
        public MainWindow()
        {
            InitializeComponent();
        }
        private void LoadBitmap()
        {
            using(var memoryStream=new MemoryStream())
            {
                currentBitmap.Save(memoryStream, ImageFormat.Jpeg);
                memoryStream.Position = 0;
                var bitmapImage2 = new BitmapImage();
                bitmapImage2.BeginInit();
                bitmapImage2.StreamSource = memoryStream;
                bitmapImage2.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage2.EndInit();
                bitmapImage2.Freeze();
                bitmapImage = bitmapImage2;
                Img.Source = bitmapImage; 
            }
        }
        private void Reset(object sender, RoutedEventArgs e)
        {
            currentBitmap = new Bitmap(originalBitmap);
            LoadBitmap();
        }
        public void openFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select file";
            openFileDialog.Filter = "JPG|*.jpg";
            if (openFileDialog.ShowDialog()==true)
            {
                originalBitmap=new Bitmap(System.Drawing.Image.FromFile(openFileDialog.FileName));
                currentBitmap = new Bitmap(originalBitmap);
                LoadBitmap();
            }
        }
        private byte Filtreaded(int[] mask, List<byte?>values)
        {
            if(mask!=null)
            {
                int sum = 0;
                int divisor = 0;
                for(int i = 0;i<9;i++)
                    if(values[i]!=null)
                    {
                        sum+=mask[i]*(int)values[i];
                        divisor+=mask[i];
                    }
                if(divisor!=0)
                    return (byte)((sum/divisor)%256);
                return (byte)(sum%256);
            }
            byte?[] notNull=values.FindAll(x=>x.HasValue).ToArray();
            Array.Sort(notNull);
            int size=notNull.Length;
            int mid=size/2;
            byte mediana = (size % 2 != 0) ? (byte)notNull[mid] : (byte)((notNull[mid] + notNull[mid - 1]) / 2);
            return mediana;
        }
        private void ApplyTrans(object obj, RoutedEventArgs e)
        {
            var mode = ((MenuItem)obj).Tag.ToString();
            if (currentBitmap != null)
            {
                if (mode != "grayscale1" && mode != "grayscale2")
                {
                    DialogWindow dialog = new DialogWindow();
                    dialog.Owner = this; 
                    if (dialog.ShowDialog() == true)
                        transValue = dialog.transformationValue;
                    else
                        transValue = 0;
                }
                int pixelID = 0;
                int x = 0;
                int y = 0;
                while (pixelID < currentBitmap.Width * currentBitmap.Height)
                {
                    var currentPix = currentBitmap.GetPixel(x, y);
                    byte r = currentPix.R;
                    byte g = currentPix.G;
                    byte b = currentPix.B;
                    switch (mode)
                    {
                        case "add":
                            r = (byte)((r + transValue) % 256);
                            g = (byte)((g + transValue) % 256);
                            b = (byte)((b + transValue) % 256);
                            break;
                        case "subtract":
                            r = (byte)((r - transValue) % 256);
                            g = (byte)((g - transValue) % 256);
                            b = (byte)((b - transValue) % 256);
                            break;
                        case "multiply":
                            r = (byte)((r * transValue) % 256);
                            g = (byte)((g * transValue) % 256);
                            b = (byte)((b * transValue) % 256);
                            break;
                        case "divide":
                            r = (byte)((r / transValue) % 256);
                            g = (byte)((g / transValue) % 256);
                            b = (byte)((b / transValue) % 256);
                            break;
                        case "brightness":
                            int rB = (int)(r * (transValue / 100.0));
                            int gB = (int)(g * (transValue / 100.0));
                            int bB = (int)(b * (transValue / 100.0));
                            if (rB < 0) r = 0;
                            else if (rB > 255) r = 255;
                            else r = (byte)rB;
                            if (gB < 0) g = 0;
                            else if (gB > 255) g = 255;
                            else g = (byte)gB;
                            if (bB < 0) b = 0;
                            else if (bB > 255) b = 255;
                            else b = (byte)bB;
                            break;
                        case "grayscale1":
                            r = (byte)((r + g + b) / 3);
                            g = r;
                            b = r;
                            break;
                        case "grayscale2":
                            r = (byte)(r * 0.2126 + g * 0.7152 + b * 0.0722);
                            g = r;
                            b = r;
                            break;

                    }
                    currentBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(r, g, b));
                    pixelID++;
                    x++;
                    if (currentBitmap.Width <= x)
                    {
                        x = 0;
                        y++;
                    }
                }
                LoadBitmap();
            }
        }
        private void ApplyFilter(object obj, RoutedEventArgs e)
        {
            if (currentBitmap != null)
            {
                var mode = ((MenuItem)obj).Tag.ToString();
                var bitmap = new Bitmap(currentBitmap.Width, currentBitmap.Height);
                int pixelID = 0;
                int x = 0;
                int y = 0;
                while (pixelID < currentBitmap.Width * currentBitmap.Height)
                {
                    List<byte?> maskR = new List<byte?>();
                    List<byte?> maskG = new List<byte?>();
                    List<byte?> maskB = new List<byte?>();
                    for (int masky = -1; masky <= 1; masky++)
                        for (int maskx = -1; maskx <= 1; maskx++)
                            if (x + maskx >= 0 && x + maskx < currentBitmap.Width && y + masky >= 0 && y + masky < currentBitmap.Height)
                            {
                                var pixel = currentBitmap.GetPixel(x + maskx, y + masky);
                                maskR.Add(pixel.R);
                                maskG.Add(pixel.G);
                                maskB.Add(pixel.B);
                            }
                            else
                            {
                                maskR.Add(null);
                                maskG.Add(null);
                                maskB.Add(null);
                            }
                    int[] mask = null;
                    switch (mode)
                    {
                        case "average":
                            mask = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                            break;
                        case "edges":
                            mask = new int[] { -1, -2, -1, 0, 0, 0, 1, 2, 1 };
                            this.Title = "Sobel - bandyta"; // wyborny easter egg
                            break;
                        case "highpass":
                            mask = new int[] { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
                            break;
                        case "gaussin":
                            mask = new int[] { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
                            break;
                    }
                    byte r = Filtreaded(mask, maskR);
                    byte g = Filtreaded(mask, maskG);
                    byte b = Filtreaded(mask, maskB);
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(r, g, b));
                    pixelID++;
                    x++;
                    if (currentBitmap.Width <= x)
                    {
                        x = 0;
                        y++;
                    }
                }
                currentBitmap = bitmap;
                LoadBitmap();
            }
        }
    }
}
