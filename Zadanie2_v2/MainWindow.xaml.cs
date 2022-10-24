using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Diagnostics;
using Image = System.Windows.Controls.Image;
using System.Collections;

namespace Zadanie2_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string filePath;
        MemoryStream memoryStream;
        int index, width, height;
        ushort maxValue;
        byte[] bytes;
        BitmapImage bitmapImage;
        BitArray bits;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "PPM Files (*.ppm)|*.ppm|PGM Files (*.pgm)|*.pgm|PBM Files (*.pbm)|*.pbm|All files (*.*)|*.*";
            if (openDialog.ShowDialog() == true)
            {
                index = 0;
                bytes = File.ReadAllBytes(openDialog.FileName);
                // load parameters
                string fileType = getNextString();
                width = Int32.Parse(getNextString());
                height = Int32.Parse(getNextString());
                if ((fileType != "P1") && (fileType != "P4"))
                    maxValue = ushort.Parse(getNextString());

                switch (fileType)
                {
                    case "P3":
                        loadTextFile(width, height, maxValue);
                        break;
                    case "P6":
                        loadBinaryFile(width, height);
                        break;
                    case "P2":
                        loadPgmPbm(width, height, maxValue, "P2");
                        break;
                    case "P5":
                        index = bytes.Length - (width * height);
                        loadPgmPbm(width, height, 0, "P5");
                        break;
                    case "P1":
                        loadPgmPbm(width, height, 0, "P1");
                        break;
                    case "P4":
                        bits = new BitArray(bytes);
                        index = bits.Count - (height * width);
                        loadPgmPbm(width, height, 0, "P4");
                        break;
                }
                BitmapImage ppmImg = new BitmapImage();
                ppmImg.BeginInit();
                ppmImg.StreamSource = memoryStream;
                ppmImg.EndInit();
                bitmapImage = ppmImg;
                Img.Source = bitmapImage;
            }
            filePath = openDialog.FileName;
        }

        private void saveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PNG Files (*.png)| *.png";
            Bitmap bitmap;
            if (saveDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                using (var fileStream = new System.IO.FileStream(saveDialog.FileName, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }

            }
        }

        void loadTextFile(int width, int height, ushort maxValue)
        {
            using (Bitmap bmp = new Bitmap(width, height))
            {
                int pixelIndex = 0, x = 0, y = 0;
                while (width * height > pixelIndex)
                {
                    ushort r, g, b;
                    r = ushort.Parse(getNextString());
                    g = ushort.Parse(getNextString());
                    b = ushort.Parse(getNextString());
                    if (maxValue == 65535)
                    {
                        r = (ushort)(r >> 8);
                        g = (ushort)(g >> 8);
                        b = (ushort)(b >> 8);
                    }
                    bmp.SetPixel(x, y, Color.FromArgb(100, r, g, b));
                    pixelIndex++;
                    x++;
                    if (x >= width)
                    {
                        x = 0;
                        y++;
                    }
                }
                memoryStream = new MemoryStream();
                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;
            }
        }
        void loadBinaryFile(int width, int height)
        {
            using (Bitmap bmp = new Bitmap(width, height))
            {
                int pixelIndex = 0, x = 0, y = 0;
                index++;
                while (width * height > pixelIndex)
                {
                    byte r = bytes[index];
                    byte g = bytes[index + 1];
                    byte b = bytes[index + 2];
                    bmp.SetPixel(x, y, Color.FromArgb(100, r, g, b));
                    index += 3;
                    pixelIndex++;
                    x++;
                    if (x >= width)
                    {
                        x = 0;
                        y++;
                    }
                }
                memoryStream = new MemoryStream();
                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;
            }
        }

        void loadPgmPbm(int width, int height, int maxValue, string fileType)
        {
            using (Bitmap bmp = new Bitmap(width, height))
            {
                int pixelIndex = 0, x = 0, y = 0, pbmPixel;
                ushort r = 0, g = 0, b = 0;
                for (int i = 0; i < width * height; i++)
                {
                    if (fileType == "P2")
                    {
                        r = g = b = ushort.Parse(getNextString());
                        if (maxValue == 65535)
                        {
                            r = g = b = (ushort)(r >> 8);
                        }
                    }
                    else if (fileType == "P5")
                    {
                        r = g = b = bytes[index];
                        index++;
                    }
                    else if (fileType == "P1")
                    {
                        pbmPixel = Int32.Parse(getNextString());
                        if (pbmPixel == 1)
                            r = g = b = 0;
                        else
                            r = g = b = 255;
                    }
                    else if (fileType == "P4")
                    {
                        if (bits[index])
                            r = g = b = 0;
                        else
                            r = g = b = 255;
                        index++;
                    }
                    bmp.SetPixel(x, y, Color.FromArgb(255, r, g, b));
                    x++;
                    if (x >= width)
                    {
                        x = 0;
                        y++;
                    }
                }
                memoryStream = new MemoryStream();
                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;
            }
        }


        private string getNextString()
        {
            while (index < bytes.Length && (char.IsWhiteSpace((char)bytes[index]) || char.IsControl((char)bytes[index])))
                index++;

            if (index == bytes.Length)
                return null;

            if (bytes[index] == '#')
            {
                while (index < bytes.Length && bytes[index] != '\n')
                    index++;

                if (index == bytes.Length)
                    return null;

                index++;
                return getNextString();
            }

            int startIndex = index;

            while (index < bytes.Length && !char.IsWhiteSpace((char)bytes[index]) && !char.IsControl((char)bytes[index]))
                index++;

            if (index == bytes.Length)
                return null;

            return Encoding.ASCII.GetString(bytes, startIndex, index - startIndex);
        }
    }
}
