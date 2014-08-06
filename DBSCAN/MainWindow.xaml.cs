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
            Width = 800;
            Height = 800;
            NewRandomSession(100, MainGrid);
        }

        private readonly Brush[] brushes = { Brushes.Orange, Brushes.Red, Brushes.Blue, Brushes.Yellow };

        private void NewRandomSession(int count, Panel container)
        {
            var points = GeneratePoints(count);
            session = new DBSCANSession<Ellipse>(50, 3);
            session.SimilarityFuncion = EuclideanDistance;
            foreach (var ellipse in points)
            {
                session.Points.Add(ellipse);
                container.Children.Add(ellipse);
            }
            int colorCounter = 0;
            foreach (var cluster in session.GetClusters())
            {
                foreach (var ellipse in cluster)
                {
                    ellipse.Stroke = brushes[colorCounter % 4];
                    ellipse.Fill = brushes[colorCounter % 4];
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
                result.Add(CreateEllipse(r.Next(0, 800), r.Next(0, 800)));
            }
            return result;
        }

        public static double EuclideanDistance(Ellipse a, Ellipse b)
        {
            var aa = (Point)a.Tag;
            var bb = (Point)b.Tag;
            return aa.EuclideanDistance(bb);
        }

        public Ellipse CreateEllipse(double x, double y)
        {
            var e = new Ellipse
                    {
                        Margin = new Thickness(x - 5, y - 5, 0, 0),
                        Height = 10,
                        Width = 10,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Stroke = Brushes.Black,
                        Fill = Brushes.Black,
                        Cursor = Cursors.Hand,
                        Tag = new Point(x, y)
                    };
            e.MouseEnter += (a, b) =>
                            {
                                tempo = new Ellipse
                                              {
                                                  Height = session.Epsilon * 2,
                                                  Width = session.Epsilon * 2,
                                                  Margin = new Thickness(x - session.Epsilon, y - session.Epsilon, 0, 0),
                                                  VerticalAlignment = VerticalAlignment.Top,
                                                  HorizontalAlignment = HorizontalAlignment.Left,
                                                  Stroke = Brushes.Green,
                                                  Fill = Brushes.Transparent,
                                              };
                                Panel.SetZIndex(tempo, -1000);
                                ((Grid)(e.Parent)).Children.Add(tempo);
                            };
            e.MouseLeave += (a, b) => ((Grid)(e.Parent)).Children.Remove(tempo);
            return e;
        }

        private Ellipse tempo;
    }
}
