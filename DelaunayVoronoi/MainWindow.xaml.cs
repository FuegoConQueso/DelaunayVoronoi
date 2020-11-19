using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using Delaunay.Annotations;
using System.Windows.Media;

namespace DelaunayVoronoi
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DelaunayTriangulator delaunay = new DelaunayTriangulator();
        private Voronoi voronoi = new Voronoi();
        private bool isGenerated = false;
        public int PointCount { get; set; } = 2000;
        public double DiagramWidth => (int)Canvas.ActualWidth;
        public double DiagramHeight => (int)Canvas.ActualHeight;

        public IEnumerable<Polygon> VoronoiRegions;

        public ICommand DrawCommand { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            DrawCommand = new Command(param => GenerateAndDraw());

            Canvas.SizeChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(DiagramHeight));
                OnPropertyChanged(nameof(DiagramWidth));
            };

            Canvas.MouseLeftButtonUp += (sender, e) =>
            {
                Debug.WriteLine("called");
                System.Windows.Point mousePos = e.GetPosition((IInputElement)sender);
                FindRegion(new Point(mousePos.X, mousePos.Y));
            };

        }

        private void GenerateAndDraw()
        {
            Canvas.Children.Clear();
            Canvas.ClipToBounds = true;
            //var points = delaunay.GeneratePoints(PointCount, DiagramWidth, DiagramHeight);
            var points = delaunay.GenerateFixedPoints(DiagramWidth, DiagramHeight);

            var delaunayTimer = Stopwatch.StartNew();
            var triangulation = delaunay.BowyerWatson(points);
            delaunayTimer.Stop();
            //DrawTriangulation(triangulation);

            var voronoiTimer = Stopwatch.StartNew();
            //var voronoiEdges = voronoi.GenerateEdgesFromDelaunay(triangulation);
            VoronoiRegions = voronoi.GenerateRegionsFromDelaunay(points, DiagramWidth, DiagramHeight);
            voronoi.ConnectRegions(VoronoiRegions);
            //update bounding boxes
            foreach (Polygon region in VoronoiRegions)
            {
                region.UpdateBoundingBox();
            }
                voronoiTimer.Stop();
            //DrawVoronoi(voronoiEdges);
            DrawVoronoiRegions(VoronoiRegions);
            DrawVoronoiConnections(VoronoiRegions);

            DrawPoints(points);
            isGenerated = true;
        }

        private void FindRegion(Point point)
        {
            if (isGenerated)
            {
                foreach (Polygon region in VoronoiRegions)
                {
                    if (region.IsInBoundingBox(point) && region.IsInside(point))
                    {
                        Console.WriteLine("Region Center: " + region.DelaunayPoint.ToString());
                        return;
                    }
                }
                Console.WriteLine("Not Found");
                return;
            }
            Console.WriteLine("Not Generated");
        }

        private void DrawPoints(IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                var myEllipse = new Ellipse();
                myEllipse.Fill = System.Windows.Media.Brushes.Red;
                myEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                myEllipse.VerticalAlignment = VerticalAlignment.Top;
                myEllipse.Width = 1;
                myEllipse.Height = 1;
                var ellipseX = point.X - 0.5 * myEllipse.Height;
                var ellipseY = point.Y - 0.5 * myEllipse.Width;
                myEllipse.Margin = new Thickness(ellipseX, ellipseY, 0, 0);

                Canvas.Children.Add(myEllipse);
            }
        }

        private void DrawTriangulation(IEnumerable<Triangle> triangulation)
        {
            var edges = new List<Edge>();
            foreach (var triangle in triangulation)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }

            foreach (var edge in edges)
            {
                var line = new Line();
                line.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                line.StrokeThickness = 0.5;

                line.X1 = edge.Point1.X;
                line.X2 = edge.Point2.X;
                line.Y1 = edge.Point1.Y;
                line.Y2 = edge.Point2.Y;

                Canvas.Children.Add(line);
            }
        }

        private void DrawVoronoiConnections(IEnumerable<Polygon> polygons)
        {
            foreach (var polygon in polygons)
            {
                foreach(var neighbor in polygon.AdjacentRegions)
                {
                    if (neighbor != null)
                    {
                        var line = new Line();
                        line.Stroke = System.Windows.Media.Brushes.DarkViolet;
                        line.StrokeThickness = 1;

                        line.X1 = polygon.DelaunayPoint.X;
                        line.X2 = neighbor.DelaunayPoint.X;
                        line.Y1 = polygon.DelaunayPoint.Y;
                        line.Y2 = neighbor.DelaunayPoint.Y;
                        Console.WriteLine("from " + polygon.DelaunayPoint.ToString() + " to " + neighbor.DelaunayPoint.ToString());
                        Canvas.Children.Add(line);
                    }
                }
            }
        }

        private void DrawVoronoiRegions(IEnumerable<Polygon> polygons)
        {

            Random rand = new Random(57346);
            foreach(var polygon in polygons)
            {
                var drawnShape = new System.Windows.Shapes.Polygon();
                drawnShape.Stroke = Brushes.DarkGreen;
                byte RedAndGreen = (byte)rand.Next(0, 255);
                byte Red = (byte)rand.Next(0, RedAndGreen);
                drawnShape.Fill = new SolidColorBrush(Color.FromArgb(120, Red, (byte)(RedAndGreen - Red), (byte)(255 - RedAndGreen)));
                for (int i = 0; i < polygon.Vertices.Count; i++)
                {
                    IEnumerable<System.Windows.Point> points = polygon.Vertices.Select<Point, System.Windows.Point>(
                        p => new System.Windows.Point(p.X, p.Y)
                        );
                    drawnShape.Points = new PointCollection(points);
                }
                Canvas.Children.Add(drawnShape);
            }
        }

        private void DrawVoronoi(IEnumerable<Edge> voronoiEdges)
        {
            foreach (var edge in voronoiEdges)
            {
                var line = new Line();
                line.Stroke = System.Windows.Media.Brushes.DarkViolet;
                line.StrokeThickness = 1;

                line.X1 = edge.Point1.X;
                line.X2 = edge.Point2.X;
                line.Y1 = edge.Point1.Y;
                line.Y2 = edge.Point2.Y;

                Canvas.Children.Add(line);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Command : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public Command(Action<object> execute)
            : this(execute, param => true)
        {
        }

        public Command(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (_canExecute == null) || _canExecute(parameter);
        }
    }
}