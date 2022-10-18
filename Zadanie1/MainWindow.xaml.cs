using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zadanie1
{
    public partial class MainWindow : Window
    {
        Brush brush = Brushes.Black, fill = Brushes.White; 
        Shape lastShape = null;
        Shape newShape = null;
        Line line = null;
        Point p = new Point();
        TextBlock text;
        Point currentPoint = new Point();
        string mode = "rectangle";
        public MainWindow()
        {
            InitializeComponent();
            canvas.Focus();
        }

        private void select_triangle(object sender, RoutedEventArgs e)
        {
            mode = "triangle";
        }

        private void select_rectangle(object sender, RoutedEventArgs e)
        {
            mode = "rectangle";
        }

        private void select_elipse(object sender, RoutedEventArgs e)
        {
            mode = "ellipse";
        }

        private void select_line(object sender, RoutedEventArgs e)
        {
            mode = "line";
        }

        private void select_draw(object sender, RoutedEventArgs e)
        {
            mode = "pencil";
        }

        private void select_text(object sender, RoutedEventArgs e)
        {
            mode = "text";
        }

        private void select_save(object sender, RoutedEventArgs e)
        {
            save();
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            p = new Point(); 
            p = e.GetPosition(this);
            currentPoint = p; 
            newShape = null;
            line = null; 
            Mouse.Capture(canvas);
            switch (mode)
            {
                case "rectangle":
                    newShape = new Rectangle();
                    break;
                case "ellipse":
                    newShape = new Ellipse();
                    break;
                case "pencil":
                    newShape = new Line();
                    break;
                case "line":
                    newShape = new Line();
                    break;
                case "triangle":
                    newShape = new Line();
                    break; 
            }
            if (mode == "text")
            {
                p = new Point();
                p = e.GetPosition(this);
                currentPoint = p;
                text = new TextBlock();
                Canvas.SetLeft(text, p.X);
                Canvas.SetTop(text, p.Y);
                canvas.Children.Add(text);
            }
            else if (mode == "line")
            {
                p = new Point();
                p = e.GetPosition(this);
                currentPoint = p;
                line = new Line();
                line.Stroke = brush;
                line.X1 = line.X2 = p.X;
                line.Y1 = line.Y2 = p.Y;
                canvas.Children.Add(line);

            }
            else
            {
                Mouse.Capture(canvas);
                lastShape = newShape;
                newShape.Stroke = brush;
                newShape.Fill = fill; 
                p = new Point();
                p = e.GetPosition(this);
                currentPoint = p;
                newShape.SetValue(Canvas.TopProperty, p.Y);
                newShape.SetValue(Canvas.LeftProperty, p.X);
                newShape.Height = 0;
                newShape.Width = 0;
                canvas.Children.Add(newShape);
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (mode == "pencil")
                {
                    line = new Line();
                    line.Stroke = brush;
                    line.X1 = currentPoint.X;
                    line.Y1 = currentPoint.Y;
                    line.X2 = e.GetPosition(this).X;
                    line.Y2 = e.GetPosition(this).Y;
                    currentPoint = e.GetPosition(this);
                    canvas.Children.Add(line);
                }
                else if (mode == "line" /*&& line != null*/)
                {
                    if (line is null)
                        line = new Line(); 
                    p = e.GetPosition(this);
                    line.X2 = p.X;
                    line.Y2 = p.Y;
                }
                else
                {
                    p = e.GetPosition(this);
                    var top = (double)lastShape.GetValue(Canvas.TopProperty);
                    var left = (double)lastShape.GetValue(Canvas.LeftProperty);
                    var height = p.Y - top;
                    var width = p.X - left;
                    if (height > 0)
                    {
                        if (width < top + lastShape.Height - p.Y)
                        {
                            lastShape.SetValue(Canvas.TopProperty, p.Y);
                            lastShape.Height += Math.Abs(height);
                        }
                        else
                            lastShape.Height = height;
                    }
                    else
                    {
                        lastShape.SetValue(Canvas.TopProperty, p.Y);
                        lastShape.Height += Math.Abs(height);
                    }
                    if (width > 0)
                    {
                        if (width < left + lastShape.Width - p.X)
                        {
                            lastShape.SetValue(Canvas.LeftProperty, p.X);
                            lastShape.Width += Math.Abs(width);
                        }
                        else
                            lastShape.Width = width;
                    }
                    else
                    {
                        lastShape.SetValue(Canvas.LeftProperty, p.X);
                        lastShape.Width += Math.Abs(width);
                    }
                }
            }
        }

        private void canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (lastShape == null)
                return;
            else
            {
                var top = (double)lastShape.GetValue(Canvas.TopProperty);
                var left = (double)lastShape.GetValue(Canvas.LeftProperty);
                switch (e.Key)
                {
                    case Key.Up:
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            lastShape.SetValue(Canvas.TopProperty, top - 1);
                            lastShape.Height++;
                        }
                        else
                            lastShape.SetValue(Canvas.TopProperty, top - 1);
                        break;
                    case Key.Down:
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                            lastShape.Height++;
                        else
                            lastShape.SetValue(Canvas.TopProperty, top + 1);
                        break;
                    case Key.Left:
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            lastShape.SetValue(Canvas.LeftProperty, left - 1);
                            lastShape.Width++;
                        }
                        else
                            lastShape.SetValue(Canvas.LeftProperty, left - 1);
                        break;
                    case Key.Right:
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                            lastShape.Width++;
                        else
                            lastShape.SetValue(Canvas.LeftProperty, left + 1);
                        break;
                }
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void canvas_KeyUp(object sender, KeyEventArgs e)
        {
            if (mode == "text" && text != null)
            {
                this.text.Text += e.Key.ToString().ToLower();
            }
        }
        private Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }

        private void select_stroke(object sender, RoutedEventArgs e)
        {
            brush = PickBrush(); 
        }
        private void select_fill(object sender, RoutedEventArgs e)
        {
            fill = PickBrush(); 
        }

        void save()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Image|*.png";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                pngEncoder.Save(fs);
                fs.Close(); 
            }
        }
    }
}
