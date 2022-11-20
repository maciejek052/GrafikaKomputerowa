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
using Color = System.Drawing.Color;

namespace Zadanie5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapImage bitmapImage;
        Bitmap originalBitmap;
        Bitmap currentBitmap;
        private int[] rHistogram;
        private int[] gHistogram;
        private int[] bHistogram;
        int transValue;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void LoadBitmap()
        {
            using (var memoryStream = new MemoryStream())
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
            if (openFileDialog.ShowDialog() == true)
            {
                originalBitmap = new Bitmap(System.Drawing.Image.FromFile(openFileDialog.FileName));
                currentBitmap = new Bitmap(originalBitmap);
                LoadBitmap();
            }
        }
        private void ApplyHistogram(object obj, RoutedEventArgs e)
        {
            var mode = ((MenuItem)obj).Tag.ToString();
            if (currentBitmap != null)
            {
                GetHistograms();
                int[] rLut = null;
                int[] gLut = null;
                int[] bLut = null;
                switch (mode)
                {
                    case "wyr":
                        rLut = GetEqualizationLut(rHistogram);
                        gLut = GetEqualizationLut(gHistogram);
                        bLut = GetEqualizationLut(bHistogram);
                        break;
                    case "roz":
                        rLut = GetExpansionLut(rHistogram);
                        gLut= GetExpansionLut(gHistogram);
                        bLut = GetExpansionLut(bHistogram);
                        break;
                }
                for (int x=0; x < currentBitmap.Width; x++)           
                    for (int y = 0; y < currentBitmap.Height; y++)
                    {
                        var pixelColor = currentBitmap.GetPixel(x, y);
                        var newColor= Color.FromArgb(rLut[pixelColor.R], gLut[pixelColor.G], bLut[pixelColor.B]);
                        currentBitmap.SetPixel(x, y, newColor);
                    }

                LoadBitmap();

            }
        }
        private void ApplyBinarization(object obj, RoutedEventArgs e)
        {
            var mode = ((MenuItem)obj).Tag.ToString();
            if (currentBitmap != null)
            {
                if(mode != "entropii")
                {
                    Window1 dialog = new Window1();
                    dialog.Owner = this;
                    if (dialog.ShowDialog() == true)
                        transValue = dialog.transformationValue;
                    else
                        transValue = 0;
                }
                switch(mode)
                {
                    case "reczny":
                        for(int x=0; x < currentBitmap.Width;x++)
                            for (int y=0; y < currentBitmap.Height;y++)
                            {
                                var pixelColor =currentBitmap.GetPixel(x, y);
                                double grayScale = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0;
                                var newColor = grayScale < transValue ? Color.Black : Color.White;
                                currentBitmap.SetPixel(x, y, newColor);
                            }
                        break;
                    case "czarny":
                        if (transValue > 100)
                            transValue = 100;
                        var bitmap = new Bitmap(currentBitmap.Width, currentBitmap.Height);
                        var grayHistogram = new int[256];
                        for(int x=0;x<currentBitmap.Width;x++)
                            for(int y=0;y<currentBitmap.Height; y++)
                            {
                                var pixelColor = currentBitmap.GetPixel(x, y);
                                byte grayScale = (byte)((pixelColor.R + pixelColor.G + pixelColor.B) / 3.0);
                                bitmap.SetPixel(x, y,Color.FromArgb(grayScale,grayScale,grayScale));
                                grayHistogram[grayScale]++;
                            }
                        double maxBlackPixels =transValue*0.01*currentBitmap.Width*currentBitmap.Height;
                        double blackPixelSum = 0;
                        byte treshold = 0;
                        for(int i=0;i<grayHistogram.Length;i++)
                        {
                            treshold = (byte)i;
                            blackPixelSum+=grayHistogram[i];
                            if(blackPixelSum >= maxBlackPixels)
                            {
                                break;
                            }
                        }
                        for(int x=0;x < currentBitmap.Width;x++)
                            for(int y=0;y< currentBitmap.Height;y++)
                            {
                                var pixelColor = bitmap.GetPixel(x, y);
                                var newColor=pixelColor.R<treshold?Color.Black:Color.White;
                                currentBitmap.SetPixel(x, y,newColor);
                            }
                        break;
                    case "entropii":
                        bitmap = new Bitmap(currentBitmap.Width, currentBitmap.Height);
                        grayHistogram = new int[256];
                        for(int x=0;x<currentBitmap.Width;x++)
                            for(int y=0;y < currentBitmap.Height;y++)
                            {
                                var pixelColor=currentBitmap.GetPixel(x, y);
                                byte grayScale=(byte)((pixelColor.R+pixelColor.G+pixelColor.B)/3.0);
                                bitmap.SetPixel(x, y,Color.FromArgb(grayScale,grayScale,grayScale));
                                grayHistogram[grayScale]++;
                            }
                        double entropyTreshold = 0;
                        double pixelCount =currentBitmap.Width*currentBitmap.Height;
                        for (int i = 0; i < grayHistogram.Length; i++)
                        {
                            double probability = grayHistogram[i] / pixelCount;
                            if (probability != 0)
                                entropyTreshold += probability * Math.Log(probability);
                        }
                        entropyTreshold = -entropyTreshold;
                        for(int x=0;x< currentBitmap.Width;x++)
                        {
                            for(int y=0;y<currentBitmap.Height;y++)
                            {
                                var pixelColor = bitmap.GetPixel(x, y);
                                var newColor=pixelColor.R<entropyTreshold?Color.Black:Color.White;
                                currentBitmap.SetPixel(x, y,newColor);
                            }
                        }
                        break;
                }
                LoadBitmap();
            }

        }
        private int[] GetEqualizationLut(int[] values)
        {
            double minValue = 0;
            for(int i=0;i<256;i++)
            {
                if(values[i]!=0)
                {
                    minValue = values[i];
                    break;
                }
            }
            int[] result = new int[256];
            double sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += values[i];
                result[i] = (int)((sum-minValue)/(currentBitmap.Width*currentBitmap.Height-minValue)*255.0);
            }

            return result;
        }
        private int[] GetExpansionLut(int[] values)
        {
            int minValue = 0;
            for (int i = 0; i < 256; i++)
            {
                if (values[i] != 0)
                {
                    minValue = i;
                    break;
                }
            }

            int maxValue = 255;
            for (int i = 255; i >= 0; i--)
            {
                if (values[i] != 0)
                {
                    maxValue = i;
                    break;
                }
            }

            int[] result = new int[256];
            double a = 255.0 / (maxValue - minValue);
            for (int i = 0; i < 256; i++)
            {
                result[i] = (int)(a * (i - minValue));
            }

            return result;
        }
        private void GetHistograms()
        {
            rHistogram = new int[256];
            gHistogram = new int[256];
            bHistogram = new int[256];
            for(int x=0;x<currentBitmap.Width;x++)
                for(int y=0;y<currentBitmap.Height;y++)
                {
                    var pixelColor=currentBitmap.GetPixel(x,y);
                    rHistogram[pixelColor.R]++;
                    gHistogram[pixelColor.G]++;
                    bHistogram[pixelColor.B]++;
                }
        }
    }
}
