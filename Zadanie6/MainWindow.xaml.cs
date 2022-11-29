using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Path = System.Windows.Shapes.Path;

namespace Zadanie6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool bezier = true, shapes = false, loadingFromFile = false;
        List<Point> controlPoints = new List<Point>();
        List<Ellipse> Ellipses = new List<Ellipse>();

        List<List<Line>> Shapes;
        List<Point> Points;
        List<Line> SelectShape;

        public MainWindow()
        {
            InitializeComponent();
            ShapesBox.Visibility = Visibility.Collapsed;
            Points = new List<Point>();
            Shapes = new List<List<Line>>();
            Shapes.Add(new List<Line>());
            SelectShape = null;
        }
        private bool IsNumeric(string text)
        {
            Regex reg = new Regex("[^0-9]");
            return reg.IsMatch(text);
        }
        private void PrevTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox TB = (TextBox)sender;
            if (!IsNumeric(TB.Text + e.Text)) e.Handled = false;
            else { e.Handled = true; TB.Text = "0"; }
        }
        private void DrawPoints(List<Point> points)
        {
            Path path = MakeCurve(Bezier.GetPoints(points).ToArray(), 0.01);
            path.Stroke = Brushes.Black;
            path.StrokeThickness = 2;
            canvas.Children.Add(path);
        }
        private Path MakeBezierPath(Point[] points)
        {
            Path path = new Path();
            PathGeometry pathGeometry = new PathGeometry();
            path.Data = pathGeometry;
            PathFigure pathFigure = new PathFigure();
            pathGeometry.Figures.Add(pathFigure);
            pathFigure.StartPoint = points[0];
            PathSegmentCollection segments = new PathSegmentCollection();
            pathFigure.Segments = segments;
            PointCollection pointsCollection = new PointCollection(points.Length - 1);
            for (int i = 1; i < points.Length; i++)
            {
                pointsCollection.Add(points[i]);
            }
            PolyBezierSegment polyBezierSegment = new PolyBezierSegment();
            polyBezierSegment.Points = pointsCollection;
            segments.Add(polyBezierSegment);
            return path;
        }
        private Point[] MakeCurvePoints(Point[] points, double tension)
        {
            if (points.Length < 2) 
                return null;
            double control_scale = tension / 0.5 * 0.175;
            List<Point> result_points = new List<Point>();
            result_points.Add(points[0]);
            for (int i = 0; i < points.Length - 1; i++)
            {
                Point pt_before = points[Math.Max(i - 1, 0)];
                Point pt = points[i];
                Point pt_after = points[i + 1];
                Point pt_after2 = points[Math.Min(i + 2, points.Length - 1)];
                double dx1 = pt_after.X - pt_before.X;
                double dy1 = pt_after.Y - pt_before.Y;
                Point p1 = points[i];
                Point p4 = pt_after;
                double dx = pt_after.X - pt_before.X;
                double dy = pt_after.Y - pt_before.Y;
                Point p2 = new Point(
                    pt.X + control_scale * dx,
                    pt.Y + control_scale * dy);
                dx = pt_after2.X - pt.X;
                dy = pt_after2.Y - pt.Y;
                Point p3 = new Point(
                    pt_after.X - control_scale * dx,
                    pt_after.Y - control_scale * dy);
                result_points.Add(p2);
                result_points.Add(p3);
                result_points.Add(p4);
            }
            return result_points.ToArray();
        }
        private Path MakeCurve(Point[] points, double tension)
        {
            if (points.Length < 2) 
                return null;
            Point[] result_points = MakeCurvePoints(points, tension);
            return MakeBezierPath(result_points.ToArray());
        }


        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (bezier)
            {
                Ellipse point = new Ellipse();
                point.Fill = System.Windows.Media.Brushes.LightGray;
                point.StrokeThickness = 2;
                point.Stroke = System.Windows.Media.Brushes.Gray;
                point.Width = 20;
                point.Height = 20;
                point.PreviewMouseMove += ControlPoint_MouseMove;
                double x = e.GetPosition(this).X;
                double y = e.GetPosition(this).Y;
                Canvas.SetLeft(point, x - point.Width / 2);
                Canvas.SetTop(point, y - point.Width / 2);
                canvas.Children.Add(point);
                Canvas.SetZIndex(point, 9999);
                controlPoints.Add(new Point(x, y));
                Ellipses.Add(point);
                indexes.ItemsSource = IndexesList();
            }
            else
            {
                if (SelectShape != null)
                    FindShape();
                LastLine();
            }
        }
        private void LastLine()
        {
            if (Points.Count > 2)
            {
                Line line = new Line
                {
                    X1 = Points[^1].X,
                    Y1 = Points[^1].Y,
                    X2 = Points[0].X,
                    Y2 = Points[0].Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 5
                };
                Shapes[^1].Add(line);
                canvas.Children.Add(line);
                Points.Clear();
                Shapes.Add(new List<Line>()); 
            }
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            controlPoints.Clear();
            Ellipses.Clear();
            Points.Clear();
            indexes.ItemsSource = IndexesList();
        }
        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            if (!loadingFromFile)
            {
                canvas.Children.Clear();
            }
            else
            {
                loadingFromFile = true;
            }
            DrawPoints(controlPoints);
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                if (canvas.Children[i].GetType() == typeof(Ellipse))
                    canvas.Children.Remove(canvas.Children[i]);
            }
            Ellipses.Clear();
            foreach (var item in controlPoints)
            {
                Ellipse point = new Ellipse();
                point.Fill = System.Windows.Media.Brushes.LightGray;
                point.StrokeThickness = 2;
                point.Stroke = System.Windows.Media.Brushes.Gray;
                point.Width = 20;
                point.Height = 20;
                point.MouseMove += ControlPoint_MouseMove;
                double x = item.X, y = item.Y;
                Canvas.SetLeft(point, x - point.Width / 2);
                Canvas.SetTop(point, y - point.Width / 2);
                canvas.Children.Add(point);
                Ellipses.Add(point);
            }
        }
        private void ControlPoint_MouseMove(object sender, MouseEventArgs e)
        {
            var ellipse = (Ellipse)sender;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                int index = 0;
                index = Ellipses.IndexOf(ellipse);
                Canvas.SetLeft(ellipse, e.GetPosition(this).X - ellipse.ActualWidth / 2);
                Canvas.SetTop(ellipse, e.GetPosition(this).Y - ellipse.ActualHeight / 2);
                Point point = new Point();
                point.X = e.GetPosition(this).X;
                point.Y = e.GetPosition(this).Y;
                controlPoints[index] = new Point();
                controlPoints[index] = point;
                DeletePath();
                DrawPoints(controlPoints);
                indexes.ItemsSource = IndexesList();
            }
        }
        private void DeletePath()
        {
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                if (canvas.Children[i].GetType() == typeof(Path))
                    canvas.Children.Remove(canvas.Children[i]);
            }
        }
        private void AddPoint_Click(object sender, RoutedEventArgs e)
        {
            double xVal, yVal;
            bool failed = false;
            if (!Double.TryParse(X.Text, out xVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość X", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;

            }
            if (!Double.TryParse(Y.Text, out yVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość Y", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true; 
            }
            if (!failed)
            {
                Point p = new Point(Convert.ToDouble(X.Text), Convert.ToDouble(Y.Text));
                controlPoints.Add(p);
                Ellipse point = new Ellipse();
                point.Fill = System.Windows.Media.Brushes.LightGray;
                point.StrokeThickness = 2;
                point.Stroke = System.Windows.Media.Brushes.Gray;
                point.Width = 20;
                point.Height = 20;
                point.MouseMove += ControlPoint_MouseMove;
                double x = p.X, y = p.Y;
                Canvas.SetLeft(point, x - point.Width / 2);
                Canvas.SetTop(point, y - point.Width / 2);
                canvas.Children.Add(point);
                Ellipses.Add(point);
                indexes.ItemsSource = IndexesList();
            }
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            double xVal, yVal;
            int index;
            bool failed = false;
            if (!Double.TryParse(X.Text, out xVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość X", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;

            }
            if (!Double.TryParse(Y.Text, out yVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość Y", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;
            }
            if (!Int32.TryParse(indexes.Text, out index))
            {
                MessageBox.Show("Wybrano nieprawidłowy indeks punktu", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;
            }
            if (!failed)
            {
                controlPoints[Convert.ToInt32(indexes.Text)] = new Point(Convert.ToDouble(X.Text), Convert.ToDouble(Y.Text));
                Ellipse el = Ellipses.ElementAt(Convert.ToInt32(indexes.Text));
                List<Ellipse> ell = new List<Ellipse>();
                foreach (var item in canvas.Children)
                {
                    if (item.GetType() == typeof(Ellipse) && item == el)
                    {
                        Ellipse tmp = (Ellipse)item;
                        Canvas.SetLeft(tmp, controlPoints[Convert.ToInt32(indexes.Text)].X - tmp.Width / 2);
                        Canvas.SetTop(tmp, controlPoints[Convert.ToInt32(indexes.Text)].Y - tmp.Width / 2);
                    }

                }
                DeletePath();
                DrawPoints(controlPoints);
            }
        }
        private List<int> IndexesList()
        {
            List<int> list = new List<int>();
            for (int i = 0; i < controlPoints.Count; i++)
            {
                list.Add(i);
            }
            return list; 
        }

        private void BezierSelected_Click(object sender, RoutedEventArgs e)
        {
            bezier = true;
            shapes = false;
            BezierBox.Visibility = Visibility.Visible;
            ShapesBox.Visibility = Visibility.Collapsed;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (shapes)
            {
                if (Shapes[^1].Count == 0)
                    FindShape();
                if (SelectShape == null)
                    AddLine(e.GetPosition(canvas)); 
            }
        }

        private void AddPointBtn_Click(object sender, RoutedEventArgs e)
        {
            int xVal, yVal;
            bool failed = false; 
            if (!Int32.TryParse(X.Text, out xVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość X", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;

            }
            if (!Int32.TryParse(Y.Text, out yVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość Y", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;
            }
            if (!failed)
            {
                AddLine(new Point(xVal, yVal)); 
            }
        }
        private void AddLine(Point p)
        {
            Points.Add(p);
            if (Points.Count > 1)
            {
                Line line = new Line
                {
                    X1 = Points[^2].X,
                    Y1 = Points[^2].Y,
                    X2 = Points[^1].X,
                    Y2 = Points[^1].Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 5
                };
                Shapes[^1].Add(line);
                canvas.Children.Add(line); 
            }
        }
        private void FindShape()
        {
            foreach (Line Line in canvas.Children.OfType<Line>())
                if (Line.IsMouseOver)
                    foreach (var shape in Shapes)
                        if (shape.Contains(Line))
                        {
                            SelectShape = shape;
                            foreach (var line in SelectShape)
                                line.Stroke = Brushes.Green;
                            Line L = SelectShape[0];
                            X.Text = Convert.ToInt32(L.X1).ToString();
                            Y.Text = Convert.ToInt32(L.Y1).ToString();
                            return; 
                        }
            SelectShape = null;
            foreach (Line child in canvas.Children.OfType<Line>()) 
                child.Stroke = Brushes.Black;
        }

        private void finishShapeBtn_Click(object sender, RoutedEventArgs e)
        {
            LastLine();
        }

        private void moveBtn_Click(object sender, RoutedEventArgs e)
        {
            int xVal, yVal;
            bool failed = false;
            if (!Int32.TryParse(X.Text, out xVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość X", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;

            }
            if (!Int32.TryParse(Y.Text, out yVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość Y", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;
            }
            if (!failed)
            {
                Point Vector = new Point(xVal, yVal);
                Move(Vector); 
            }
        }

        private void rotateBtn_Click(object sender, RoutedEventArgs e)
        {
            int xVal, yVal;
            double alpha; 
            bool failed = false;
            if (!Int32.TryParse(X.Text, out xVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość X", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;

            }
            if (!Int32.TryParse(Y.Text, out yVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość Y", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;
            }
            if (!Double.TryParse(rotateTextbox.Text, out alpha))
            {
                MessageBox.Show("Podano nieprawidłową wartość alpha", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;
            } 
            if (!failed)
            {
                alpha = alpha * Math.PI / 180;
                Point vector = new Point(xVal, yVal);
                Rotate(vector, alpha); 
            }
        }

        private void scaleBtn_Click(object sender, RoutedEventArgs e)
        {
            int xVal, yVal, alpha;
            bool failed = false;
            if (!Int32.TryParse(X.Text, out xVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość X", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;

            }
            if (!Int32.TryParse(Y.Text, out yVal))
            {
                MessageBox.Show("Podano nieprawidłową wartość Y", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;
            }
            if (!Int32.TryParse(rotateTextbox.Text, out alpha))
            {
                MessageBox.Show("Podano nieprawidłową wartość alpha", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                failed = true;
            }
            if (!failed)
            {
                Point vector = new Point(xVal, yVal);
                Scale(vector, alpha);
            }
        }

        private void ShapesSelected_Click(object sender, RoutedEventArgs e)
        {
            bezier = false;
            shapes = true; 
            BezierBox.Visibility = Visibility.Collapsed;
            ShapesBox.Visibility = Visibility.Visible;
        }
        private void EditShape(object sender, MouseEventArgs e)
        {
            if (shapes)
            {
                if (SelectShape != null)
                {
                    Point Vector = new Point();
                    foreach (var line in SelectShape)
                    {
                        Vector.X += line.X1;
                        Vector.Y += line.Y1;
                    }
                    Vector.X /= SelectShape.Count;
                    Vector.Y /= SelectShape.Count;

                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Vector.X = (e.GetPosition(canvas).X - Vector.X) / 20;
                        Vector.Y = (e.GetPosition(canvas).Y - Vector.Y) / 20;
                        Move(Vector);
                    }
                    else if (e.RightButton == MouseButtonState.Pressed)
                    {
                        double Alpha = (e.GetPosition(canvas).X - Vector.X) / 10000 * Math.PI;
                        Rotate(Vector, Alpha);
                    }
                    else if (e.MiddleButton == MouseButtonState.Pressed)
                    {
                        double scale = (10000 + e.GetPosition(canvas).X) / (10000 + Vector.X);
                        Scale(Vector, scale);
                    }
                }
            }
        }

        private void Move(Point Vector)
        {
            if (SelectShape != null)
            {
                foreach (var line in SelectShape)
                {
                    line.X1 += Vector.X;
                    line.X2 += Vector.X;
                    line.Y1 += Vector.Y;
                    line.Y2 += Vector.Y;
                }
            }

        }
        private void Rotate(Point Vector, double Alpha)
        {
            if (SelectShape != null)
            {
                foreach (var line in SelectShape)
                {
                    double X1 = line.X1, X2 = line.X2, Y1 = line.Y1, Y2 = line.Y2;
                    line.X1 = Vector.X + (X1 - Vector.X) * Math.Cos(Alpha) - (Y1 - Vector.Y) * Math.Sin(Alpha);
                    line.Y1 = Vector.Y + (X1 - Vector.X) * Math.Sin(Alpha) + (Y1 - Vector.Y) * Math.Cos(Alpha);

                    line.X2 = Vector.X + (X2 - Vector.X) * Math.Cos(Alpha) - (Y2 - Vector.Y) * Math.Sin(Alpha);
                    line.Y2 = Vector.Y + (X2 - Vector.X) * Math.Sin(Alpha) + (Y2 - Vector.Y) * Math.Cos(Alpha);
                }
            }
        }
        private void Scale(Point Vector, double Scale)
        {
            if (SelectShape != null)
            {
                foreach (var line in SelectShape)
                {
                    double X1 = line.X1, X2 = line.X2, Y1 = line.Y1, Y2 = line.Y2;
                    line.X1 = Vector.X + (X1 - Vector.X) * Scale;
                    line.X2 = Vector.X + (X2 - Vector.X) * Scale;

                    line.Y1 = Vector.Y + (Y1 - Vector.Y) * Scale;
                    line.Y2 = Vector.Y + (Y2 - Vector.Y) * Scale;
                }
            }
        }
        public void openFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Zapisane obrazy|*.sav"; 
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
                Canvas saved = XamlReader.Load(fs) as Canvas;
                fs.Close();
                while (saved.Children.Count > 0)
                {
                    UIElement uie = saved.Children[0];
                    saved.Children.Remove(uie);
                    canvas.Children.Add(uie);
                }
                XmlSerializer ctrlPoints = new XmlSerializer(typeof(List<Point>));
                XmlSerializer points = new XmlSerializer(typeof(List<Point>));
                fs = File.Open(filePath + "0", FileMode.Open, FileAccess.Read);
                controlPoints = (List<Point>)ctrlPoints.Deserialize(fs);
                fs.Close();
                fs = File.Open(filePath + "1", FileMode.Open, FileAccess.Read);
                Points = (List<Point>)points.Deserialize(fs);
                fs.Close();
                loadingFromFile = true;
                Draw_Click(null, null);
            }
        }
        public void saveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Zapisane obrazy|*.sav";
            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;
                FileStream fs = File.Open(filePath, FileMode.Create);
                XamlWriter.Save(canvas, fs);
                fs.Close();
                XmlSerializer ctrlPoints = new XmlSerializer(typeof(List<Point>));
                XmlSerializer points = new XmlSerializer(typeof(List<Point>));
                fs = File.Open(filePath + "0", FileMode.Create);
                ctrlPoints.Serialize(fs, controlPoints);
                fs.Close();
                fs = File.Open(filePath + "1", FileMode.Create);
                points.Serialize(fs, Points); 
                fs.Close();
            }
        }
    }
}
