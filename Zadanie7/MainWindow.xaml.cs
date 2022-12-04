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

namespace Zadanie7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double? green;
        BitmapImage bitmapImage;
        Bitmap bitmap;
        int wspolmax_x;
        int wspolmax_y;
        ulong maxi=0;
        ulong obecnymaxi;
        System.Collections.Generic.Queue<ulong> q = new System.Collections.Generic.Queue<ulong>();
        public class Graph
        {
            public LinkedList<ulong>[]? listaSasiedztwa;
            public ulong Size;
        }
        Graph greens;
        public MainWindow()
        {
            InitializeComponent();
        }
        public void openFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select file";
            openFileDialog.Filter = "JPG|*.jpg";
            if(openFileDialog.ShowDialog()==true)
            {
                bitmap=new Bitmap(System.Drawing.Image.FromFile(openFileDialog.FileName));
                greens.Size=(ulong)bitmap.Width * (ulong)bitmap.Height;
                green = GetGreen();
                addingcorners();
                maxGreen();
                BitmapImage jpg = new BitmapImage();
                changecolor();
                jpg.BeginInit();
                jpg.UriSource = new Uri(openFileDialog.FileName);
                jpg.CacheOption = BitmapCacheOption.OnLoad;
                jpg.EndInit();
                bitmapImage = jpg;

            }
        }
        private double GetGreen()
        {
            ulong pixelscount=(ulong)bitmap.Width*(ulong)bitmap.Height;
            ulong greencount = 0;
            for(int i = 0; i < bitmap.Width; i++)
                for(int j = 0; j < bitmap.Height; j++)
                {
                    var pixelColor=bitmap.GetPixel(i, j);
                    if(pixelColor.G>100 && pixelColor.G>pixelColor.R && pixelColor.G>pixelColor.B )
                    {
                        greencount++;
                    }
                }
            return 100.0 * greencount / pixelscount;
        }
        private void maxGreen()
        {
            bool[] visited=new bool[greens.Size];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var pixelColor = bitmap.GetPixel(i, j);
                    if (pixelColor.G > 100 && pixelColor.G > pixelColor.R && pixelColor.G > pixelColor.B)
                    {
                        ulong startowy = (ulong)((j * bitmap.Width) + i);
                        q.Enqueue(startowy);
                        visited[startowy] = true;
                        obecnymaxi=1;
                        if(obecnymaxi>maxi)
                        {
                            maxi=obecnymaxi;
                            wspolmax_x=(int)(startowy)%bitmap.Width;
                            wspolmax_y = (int)(startowy)/ bitmap.Height;
                        }
                        while(q.Count > 0)
                        {
                            ulong v=q.Dequeue();
                            foreach(ulong obecny in greens.listaSasiedztwa[v])
                            {
                                if(visited[obecny])
                                {
                                    visited[obecny] = true;
                                    q.Enqueue(obecny);
                                    obecnymaxi++;
                                    if (obecnymaxi > maxi)
                                    {
                                        maxi = obecnymaxi;
                                        wspolmax_x = (int)(obecny) % bitmap.Width;
                                        wspolmax_y = (int)(obecny) / bitmap.Height;
                                    }
                                }
                            }
                        }

                    }
                    
                }
            }
        }
        private void changecolor()
        {
            bool[] visited = new bool[greens.Size];
            ulong startowy = (ulong)(wspolmax_y*bitmap.Width+wspolmax_x);
            q.Enqueue(startowy);
            visited[startowy] = true;
            while (q.Count > 0)
            {
                ulong v = q.Dequeue();
                foreach (ulong obecny in greens.listaSasiedztwa[v])
                {
                    if (visited[obecny])
                    {
                        visited[obecny] = true;
                        q.Enqueue(obecny);
                        var newColor = System.Drawing.Color.Black;
                        bitmap.SetPixel((int)obecny%bitmap.Width,(int)obecny/bitmap.Height , newColor);
                    }
                }
            }
        }
        private void addingcorners()
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var pixelColor = bitmap.GetPixel(i, j);
                    if (pixelColor.G > 100 && pixelColor.G > pixelColor.R && pixelColor.G > pixelColor.B)
                    {
                        if(i==0 && j!= bitmap.Height-1)
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j+1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                            pixelColor2= bitmap.GetPixel(i+1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j+1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j+1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                        }
                        else if (i == 0 && j == bitmap.Height - 1)
                        {
                            var pixelColor2 = bitmap.GetPixel(i + 1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j + 1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j + 1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                        }
                        else if(i== bitmap.Width-1 && j == bitmap.Height - 1)
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j + 1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                        }
                        else if(j==0 && i!= bitmap.Width-1)
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j + 1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                            pixelColor2 = bitmap.GetPixel(i + 1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j + 1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j + 1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                        }
                        else if(j==0 && i==bitmap.Width-1)
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j + 1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                        }
                        else if(j==bitmap.Height-1 && i != bitmap.Width - 1)
                        {
                            var pixelColor2 = bitmap.GetPixel(i + 1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j + 1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j + 1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                        }
                        else
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j + 1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                            pixelColor2 = bitmap.GetPixel(i + 1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j + 1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j + 1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                            }
                        }
                    }
                }
            }
        }
    }
}
