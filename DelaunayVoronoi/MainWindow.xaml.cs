using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using BenchmarkDotNet.Attributes;
using HappyUnity.Spawners.ObjectPools;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Arction.Wpf.BindableCharting;
using Arction.Wpf.BindableCharting.Series3D;
using Arction.Wpf.BindableCharting.SeriesXY;
using Arction.Wpf.BindableCharting.Views.ViewXY;
using Color = System.Drawing.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Shape = Arction.Wpf.BindableCharting.Shape;

namespace DelaunayVoronoi
{
    public enum GenerationType
    {
        Guassian,
        Random,
        Circle
    }
    public partial class MainWindow : Window
    {
        private GenerationType getn_type;
        private DelaunayTriangulator delaunay = new DelaunayTriangulator();
        private Voronoi              voronoi  = new Voronoi();

        private ObjectPool<Ellipse> _ellipsePool;
        private ObjectPool<Line>    _linePool;

        private int pointsCount = 100;
        
        public MainWindow()
        {
            var delaunayTimer = Stopwatch.StartNew();

            InitializeComponent();

            BuildVoronoiDiagram();
            delaunayTimer.Stop();
        }

        public void BuildVoronoiDiagram()
        {
            var timerGenerate = Stopwatch.StartNew();
            var points        = delaunay.GeneratePoints(pointsCount, 10000, 10000, getn_type);
            timerGenerate.Stop();

            var delaunayTimer = Stopwatch.StartNew();
            var triangulation = delaunay.BowyerWatson(in points);
            delaunayTimer.Stop();

            var voronoiTimer = Stopwatch.StartNew();
            var vornoiEdges = voronoi.GenerateEdgesFromDelaunay(in triangulation);
            voronoiTimer.Stop();

            DrawPointsLightning2D(in points);
            DrawVorornoiLightning2D(in vornoiEdges);
            DrawTriangulationDelouneLitghtning2D(in triangulation);
        }

        public void DrawVorornoiLightning2D(in HashSet<Edge> voronoiEdges)
        {
            var chart = FieldCanvas.Children[0] as LightningChartUltimate;
            chart?.BeginUpdate();
            LineCollection lines = new LineCollection();

            foreach (var edge in voronoiEdges)
            {
                lines.Lines.Add(new SegmentLine()
                {
                        AX = edge.Point1.X,
                        BX = edge.Point2.X,
                        AY = edge.Point1.Y,
                        BY = edge.Point2.Y
                });
            }

            if (chart == null) return;
            lines.LineStyle.Color = Color.Red.ToPublicColor();
            chart.ViewXY.LineCollections.Add(lines);

            chart.EndUpdate();
        }

        private void DrawPointsLightning2D(in List<Point> points)
        {
            Stopwatch s = Stopwatch.StartNew();

            var chart = FieldCanvas.Children[0] as LightningChartUltimate;
            chart?.BeginUpdate();

            var seriesPoint = new List<SeriesPoint>();

            Arction.Wpf.BindableCharting.SeriesXY.PointDouble2DCollection points2D = new PointDouble2DCollection();

            foreach (var point in points)
            {
                seriesPoint.Add(new SeriesPoint()
                {
                        PointColor = Colors.White,
                        X          = point.X,
                        Y          = point.Y
                });
            }

            if (chart == null) return;
            
            var axisX = chart.ViewXY.XAxes[0];
            var axisY = chart.ViewXY.YAxes[0];
            var series = new PointLineSeries(chart.ViewXY, axisX, axisY);
            series.LineVisible = false;
            series.PointsVisible = true;
            series.PointStyle = new PointShapeStyle()
            {
                    Shape = Shape.Circle,
                    Color1 = Colors.White,
                    Color2 = Colors.White,
                    Color3 = Colors.White,
                    Width  = 5,
                    Height = 5,
                    BorderColor = Colors.White,
            };
            series.Points.AddRange(seriesPoint);
            chart.ViewXY.PointLineSeries.Add(series);

            chart.EndUpdate();
        }

        public void DrawTriangulationDelouneLitghtning2D(in HashSet<Triangle> triangulation)
        {
            Stopwatch s = Stopwatch.StartNew();

            var chart = FieldCanvas.Children[0] as LightningChartUltimate;
            chart?.BeginUpdate();
            LineCollection lines = new LineCollection();

            var edges = new List<Edge>(triangulation.Count * 3) { };
            foreach (var triangle in triangulation)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }

            foreach (var edge in edges)
            {
                lines.Lines.Add(new SegmentLine()
                {
                        AX = edge.Point1.X,
                        BX = edge.Point2.X,
                        AY = edge.Point1.Y,
                        BY = edge.Point2.Y
                });
            }

            if (chart == null) return;

            //lines.LineStyle.Color = Color.Bisque.ToPublicColor();
            lines.LineStyle.Color = System.Windows.Media.Color.FromArgb(50, 0, 100, 0);
            chart.ViewXY.LineCollections.Add(lines);

            chart.EndUpdate();

            s.Stop();
        }

        private void Rebuild__Click(object sender, RoutedEventArgs e)
        {
            if (FieldCanvas.Children[0] is LightningChartUltimate chart)
            {
                chart.ViewXY.LineCollections = new LineCollectionCollection();
                chart.ViewXY.PointLineSeries = new PointLineSeriesCollection();
            }

            BuildVoronoiDiagram();
        }


        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (FieldCanvas.Children[0] is LightningChartUltimate chart)
            {
                chart.ViewXY.LineCollections = new LineCollectionCollection();
                chart.ViewXY.PointLineSeries = new PointLineSeriesCollection();
            }
        }

        private void PointsApply_Click(object sender, RoutedEventArgs e)
        {
            pointsCount = int.Parse(PointsCountTextBox.Text);
        }

        private void ComboBox_Selected(object sender, SelectionChangedEventArgs e)
        {
            var s = e.AddedItems[0] as TextBlock;
            getn_type = (GenerationType) Enum.Parse(typeof(GenerationType), s.Text);
        }
    }
}