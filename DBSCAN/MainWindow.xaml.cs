using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DBSCAN.Base;
using Point = DBSCAN.Base.Point;

namespace DBSCAN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DBSCANSession<Ellipse> session;

        public MainWindow()
        {
            InitializeComponent();
            Width = 1100;
            Height = 600;
            NewRandomSession(350, 50, 3, MainGrid);
        }

        private readonly Brush[] brushes = { Brushes.Orange, Brushes.Red, Brushes.Blue, Brushes.Yellow, Brushes.Brown, Brushes.Violet };

        private void NewRandomSession(int count, double epsilon, uint minPoints, Panel container)
        {
            var points = GeneratePoints(count);
            session = new DBSCANSession<Ellipse>(epsilon, minPoints) { SimilarityFuncion = EuclideanDistance };
            foreach (var ellipse in points)
            {
                session.Points.Add(ellipse);
                container.Children.Add(ellipse);
            }
            int colorCounter = 0;
            foreach (var cluster in session.GetClusters())
            {
                // ReSharper disable PossibleMultipleEnumeration
                foreach (var ellipse in cluster)
                {
                    ellipse.Stroke = ellipse.Fill = brushes[colorCounter % 6];
                    ((PointInfo<Ellipse>)ellipse.Tag).Cluster = cluster;
                    // ReSharper restore PossibleMultipleEnumeration
                    ellipse.ToolTip = "Cluster " + (colorCounter + 1);
                }
                colorCounter++;
            }
        }

        private IEnumerable<Ellipse> GeneratePoints(int count)
        {
            var r = new Random();
            var result = new List<Ellipse>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(CreateEllipse(r.Next(0, 1100), r.Next(0, 600), 5.0));
            }
            return result;
        }

        public static double EuclideanDistance(Ellipse a, Ellipse b)
        {
            var aa = ((PointInfo<Ellipse>)a.Tag).Coordinates;
            var bb = ((PointInfo<Ellipse>)b.Tag).Coordinates;
            return aa.EuclideanDistance(bb);
        }

        public Ellipse CreateEllipse(double x, double y, double radius)
        {
            var e = new Ellipse
                    {
                        Margin = new Thickness(x - radius, y - radius, 0, 0),
                        Height = radius * 2,
                        Width = radius * 2,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Stroke = Brushes.Black,
                        Fill = Brushes.Black,
                        Cursor = Cursors.Hand,
                        Tag = new PointInfo<Ellipse>(new Point(x, y), null)
                    };
            e.MouseEnter += (a, b) => DrawAllEpsilonCircles((Ellipse)a, session.Epsilon);
            e.MouseLeave += (a, b) => RemoveAllEpsilonCircles();
            return e;
        }

        private Ellipse DrawEpsilonEllipse(Point center, double epsilon)
        {
            var ellipse = new Ellipse
                          {
                              VerticalAlignment = VerticalAlignment.Top,
                              HorizontalAlignment = HorizontalAlignment.Left,
                              Height = epsilon * 2,
                              Width = epsilon * 2,
                              Margin = new Thickness(center.X - epsilon, center.Y - epsilon, 0, 0),
                              Stroke = Brushes.Green,
                              Fill = Brushes.Transparent
                          };
            Panel.SetZIndex(ellipse, -1000);
            return ellipse;
        }

        private IEnumerable<Ellipse> lastEllipses;

        private void DrawAllEpsilonCircles(Ellipse ellipse, double epsilon)
        {
            DrawAllEpsilonCircles(((PointInfo<Ellipse>)ellipse.Tag).Cluster, epsilon);
        }
        private void DrawAllEpsilonCircles(IEnumerable<Ellipse> cluster, double epsilon)
        {
            if (cluster == null) return;//noise point
            lastEllipses = cluster.Select(x => ((PointInfo<Ellipse>)x.Tag).Coordinates).Select(x => DrawEpsilonEllipse(x, epsilon)).ToArray();
            foreach (var epsilonEllipse in lastEllipses)
            {
                MainGrid.Children.Add(epsilonEllipse);
            }
        }

        private void RemoveAllEpsilonCircles()
        {
            if (lastEllipses != null)
            {
                foreach (var lastEllipse in lastEllipses)
                {
                    MainGrid.Children.Remove(lastEllipse);
                }
                lastEllipses = null;
            }
        }
    }
}
