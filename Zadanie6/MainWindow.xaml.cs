using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Zadanie6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Point> controlPoints = new List<Point>();
        List<Ellipse> Ellipses = new List<Ellipse>();
        public MainWindow()
        {
            InitializeComponent();
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
            Ellipse point = new Ellipse();
            point.Fill = System.Windows.Media.Brushes.LightGray; // ZMIEN
            point.StrokeThickness = 2;
            point.Stroke = System.Windows.Media.Brushes.Gray; // ZMIEN
            point.Width = 16;
            point.Height = 16;
            point.PreviewMouseMove += ControlPoint_MouseMove;
            double x = e.GetPosition(this).X;
            double y = e.GetPosition(this).Y;
            Canvas.SetLeft(point, x - point.Width / 2);
            Canvas.SetTop(point, y - point.Width / 2);
            canvas.Children.Add(point);
            Canvas.SetZIndex(point, 9999); // nwm o chuj chodzi
            controlPoints.Add(new Point(x, y));
            Ellipses.Add(point);
            indexes.ItemsSource = IndexesList();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            controlPoints.Clear();
            Ellipses.Clear();
            indexes.ItemsSource = IndexesList();
        }
        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
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
                point.Width = 16;
                point.Height = 16;
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
                point.Width = 16;
                point.Height = 16;
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
    }
}
