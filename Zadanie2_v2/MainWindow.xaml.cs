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
                maxValue = ushort.Parse(getNextString());
                Trace.WriteLine(fileType + " " + width + " " + height + " " + maxValue); 
                if (fileType == "P3")
                    loadTextFile(width, height, maxValue);
                else if (fileType == "P6")
                    loadBinaryFile(width, height); 
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
        void loadTextFile (int width, int height, ushort maxValue)
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
                    bmp.SetPixel(x, y, Color.FromArgb(100, r, g, b)); // 100 r g b
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
