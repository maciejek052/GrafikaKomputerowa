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
using System.Diagnostics;

namespace Zadanie7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double? green;
        BitmapImage bitmapImage;
        Bitmap originalBitmap, bitmap; 
        int wspolmax_x;
        int wspolmax_y;
        ulong maxi=0;
        ulong obecnymaxi;
        System.Collections.Generic.Queue<ulong> q = new System.Collections.Generic.Queue<ulong>();
        public class Graph
        {
            public LinkedList<ulong>[] listaSasiedztwa = new LinkedList<ulong>[99999999];
            public ulong Size;
        }
        public void initializeList()
        {
            // wywalało błąd że wychodzi poza zakres to powiększyłem o jeden rząd pikseli
            for (int i = 0; i < (bitmap.Width * bitmap.Height) + bitmap.Width; i++)
            {
                greens.listaSasiedztwa[i] = new LinkedList<ulong>();
            }
        }
        Graph greens = new Graph();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void LoadBitmap()
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Jpeg);
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
            bitmap = new Bitmap(originalBitmap);
            LoadBitmap();
        }
        public void openFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select file";
            openFileDialog.Filter = "JPG|*.jpg";
            if(openFileDialog.ShowDialog()==true)
            {
                originalBitmap = new Bitmap(System.Drawing.Image.FromFile(openFileDialog.FileName));
                bitmap = new Bitmap(originalBitmap);
                LoadBitmap(); 
                greens.Size=(ulong)bitmap.Width * (ulong)bitmap.Height;
            }
        }
        private double GetGreen()
        {
            var tmp = new Bitmap(bitmap.Width, bitmap.Height);
            ulong pixelscount=(ulong)bitmap.Width*(ulong)bitmap.Height;
            ulong greencount = 0;
            for(int i = 0; i < bitmap.Width; i++)
                for(int j = 0; j < bitmap.Height; j++)
                {
                    var pixelColor=bitmap.GetPixel(i, j);
                    if(pixelColor.G>100 && pixelColor.G>pixelColor.R && pixelColor.G>pixelColor.B )
                    {
                        greencount++;
                        var newColor = System.Drawing.Color.Black;
                        tmp.SetPixel(i,j, newColor);

                    }
                    else
                    {
                        var newColor = System.Drawing.Color.White;
                        tmp.SetPixel(i, j, newColor);
                    }

                }
            bitmap = tmp; 
            LoadBitmap();
            return 100.0 * greencount / pixelscount;
        }
        private void maxGreen()
        {
            var tmp = new Bitmap(bitmap.Width, bitmap.Height);
            bool[] visited=new bool[greens.Size + (ulong)bitmap.Width]; // greens.Size
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var pixelColor = bitmap.GetPixel(i, j);
                    if (pixelColor.G > 100 && pixelColor.G > pixelColor.R && pixelColor.G > pixelColor.B)
                    {
                        //Trace.WriteLine("Wykryto zielone w " + i + "x" + j); 
                        ulong startowy = (ulong)((j * bitmap.Width) + i);
                        q.Enqueue(startowy);
                        visited[startowy] = true;
                        obecnymaxi=1;
                        if(obecnymaxi>maxi)
                        {
                            maxi=obecnymaxi;
                            wspolmax_x=(int)(startowy)%bitmap.Width;
                            wspolmax_y = (int)(startowy)/ bitmap.Width;
                        }
                        while(q.Any())
                        {
                            ulong v=q.Dequeue();
                            foreach(ulong obecny in greens.listaSasiedztwa[v])
                            {
                                if(visited[obecny]==false)
                                {
                                    visited[obecny] = true;
                                    q.Enqueue(obecny);
                                    //var newColor = System.Drawing.Color.Green;
                                    //int w = (int)obecny % bitmap.Width;
                                    //int h = (int)obecny / bitmap.Height;
                                    //Trace.WriteLine("Zmianakoloru wspolrzedna " + w + "x" + h);
                                    //tmp.SetPixel(w, h, newColor);
                                    obecnymaxi++;
                                    if (obecnymaxi > maxi)
                                    {
                                        maxi = obecnymaxi;
                                        wspolmax_x = (int)(obecny) % bitmap.Width;
                                        wspolmax_y = (int)(obecny) / bitmap.Width;
                                    }
                                }
                            }
                        }

                    }
                    
                }
            }
            bitmap = tmp;
            LoadBitmap();
        }
        private void changecolor()
        {
            var tmp = new Bitmap(bitmap.Width, bitmap.Height);
            bool[] visited = new bool[greens.Size]; 
            tmp= new Bitmap(originalBitmap);
            // imo jest błąd albo w tym albo w liście
            ulong startowy = (ulong)(wspolmax_y*bitmap.Width+wspolmax_x);
            Trace.WriteLine("Startowy "+ wspolmax_x+ " "+ wspolmax_y);
            q.Enqueue(startowy);
            visited[startowy] = true;
            while (q.Count > 0)
            {
                ulong v = q.Dequeue();
                foreach (ulong obecny in greens.listaSasiedztwa[v])
                {
                    if (visited[obecny] == false)
                    {
                        visited[obecny] = true;
                        q.Enqueue(obecny);
                        var newColor = System.Drawing.Color.Green;
                        int w = (int)obecny % bitmap.Width;
                        int h = (int)obecny / bitmap.Width;
                        //Trace.WriteLine("Zmianakoloru wspolrzedna " + w + "x" + h);
                        tmp.SetPixel(w, h, newColor);

                    }
                }
            }
            bitmap = tmp;
            LoadBitmap();

        }

        private void detectPixel_Click(object sender, RoutedEventArgs e)
        {
            green = GetGreen();
            MessageBox.Show("Ilość zieleni: " + green);
        }

        private void bfs_Click(object sender, RoutedEventArgs e)
        {
            initializeList(); 
            addingcorners();
            maxGreen();
            changecolor();
        }

        private void addingcorners()
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                greens.listaSasiedztwa[i] = new LinkedList<ulong>(); 
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var pixelColor = bitmap.GetPixel(i, j);
                    if (pixelColor.G > 100 && pixelColor.G > pixelColor.R && pixelColor.G > pixelColor.B)
                    {
                        if(i==0 && j!= bitmap.Height-1)//lewa krawedz
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j+1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j+1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j+1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = (j + 1) * bitmap.Width + i;
                                int x = j * bitmap.Width + i;
                                int z = j + 1;
                                //Trace.WriteLine("dodano do listy w "+ x +" x "+ y);
                                //Trace.WriteLine("dodano do listy w " + i + " "+ j + " x " + i + " " + z);
                            }
                            pixelColor2= bitmap.GetPixel(i+1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i+1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i+1)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = j * bitmap.Width + i+1;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = i + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + z + " " + j);
                            }
                        }
                        else if (i == 0 && j == bitmap.Height - 1)//lewa dolny rog 
                        {
                            var pixelColor2 = bitmap.GetPixel(i + 1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = j * bitmap.Width + i + 1;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = i + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + z + " " + j);
                            }
                        }
                        else if(i== bitmap.Width-1 && j != bitmap.Height - 1)//prawa krawedz
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j + 1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j + 1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j + 1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = (j + 1) * bitmap.Width + i;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = j + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + i + " " + z);
                            }
                        }
                        else if(j==0 && i!= bitmap.Width-1)//gorna krawedz
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j + 1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j + 1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j + 1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = (j + 1) * bitmap.Width + i;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = j + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + i + " " + z);
                            }
                            pixelColor2 = bitmap.GetPixel(i + 1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = j * bitmap.Width + i + 1;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = i + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + z + " " + j);
                            }
                        }
                        else if(j==0 && i==bitmap.Width-1)//prawa góra róg
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j + 1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j + 1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j + 1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = (j + 1) * bitmap.Width + i;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = j + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + i + " " + z);
                            }
                        }
                        else if(j==bitmap.Height-1 && i != bitmap.Width - 1)//dolna krawedz
                        {
                            var pixelColor2 = bitmap.GetPixel(i + 1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = j * bitmap.Width + i + 1;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = i + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + z + " " + j);
                            }
                        }
                        else if(j == bitmap.Height - 1 && i == bitmap.Width - 1)//dol lewa
                        {
                            return; 
                        }
                        else //srodek
                        {
                            var pixelColor2 = bitmap.GetPixel(i, j + 1);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)((j + 1) * bitmap.Width + i));
                                greens.listaSasiedztwa[(ulong)(((j + 1) * bitmap.Width) + i)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = (j + 1) * bitmap.Width + i;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = j + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + i + " " + z);
                            }

                            pixelColor2 = bitmap.GetPixel(i + 1, j);
                            if (pixelColor2.G > 100 && pixelColor2.G > pixelColor2.R && pixelColor2.G > pixelColor2.B)
                            {
                                greens.listaSasiedztwa[j * bitmap.Width + i].AddLast((ulong)(j * bitmap.Width + i + 1));
                                greens.listaSasiedztwa[(ulong)((j * bitmap.Width) + i + 1)].AddLast((ulong)(j * bitmap.Width + i));
                                int y = j * bitmap.Width + i + 1;
                                int x = j * bitmap.Width + i;
                                //Trace.WriteLine("dodano do listy w " + x + " x " + y);
                                int z = i + 1;
                                //Trace.WriteLine("dodano do listy w " + i + " " + j + " x " + z + " " + j);
                            }

                        }
                    }
                }
            }
        }
    }
}
