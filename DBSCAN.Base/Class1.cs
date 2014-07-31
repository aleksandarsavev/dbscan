using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace DBSCAN.Base
{
    public class DBSCANSession<T>
    {
        public DBSCANSession()
        {
            Points = new ObservableCollection<T>();
            Points.CollectionChanged += OnCollectionChanged;
        }

        private IEnumerable<T> coreCache;
        private IEnumerable<T> borderCache;
        private IEnumerable<T> noiseCache;
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            coreCache = null;
            borderCache = null;
            noiseCache = null;
        }

        public DBSCANSession(double epsilon, uint minPoints)
            : this()
        {
            Epsilon = epsilon;
            MinPoints = minPoints;
        }
        public ObservableCollection<T> Points { get; private set; }
        public Similarity<T> SimilarityFuncion { get; set; }
        public IEnumerable<T> CorePoints
        {
            get
            {
                return coreCache ?? (coreCache = Points.Where(x => x.GetNeighbours(Points, Epsilon, SimilarityFuncion).Count() >= MinPoints).ToArray());
            }
        }

        public IEnumerable<T> BorderPoints
        {
            get { return borderCache ?? (borderCache = Points.Except(CorePoints).Where(x => x.GetNeighbours(Points, Epsilon, SimilarityFuncion).Intersect(CorePoints).Any()).ToArray()); }
        }

        public IEnumerable<T> NoisePoints
        {
            get
            {
                return noiseCache ?? (noiseCache = Points.Except(CorePoints.Union(BorderPoints)).ToArray());
            }
        }

        public uint MinPoints { get; set; }

        public double Epsilon { get; set; }
        private List<T> visited;
        public IEnumerable<IEnumerable<T>> GetClusters()
        {
            visited = new List<T>(Points.Count);
            foreach (var corePoint in CorePoints)
            {
                if (!visited.Contains(corePoint))
                {
                    var cluster = new List<T>();
                    ExpandClusterAroundPointIntern(corePoint,cluster);
                    yield return cluster;
                }
            }
        }

        private void ExpandClusterAroundPointIntern(T point, ICollection<T> cluster)
        {
            foreach (var naighbour in point.GetNeighbours(Points, Epsilon, SimilarityFuncion).Where(naighbour => !visited.Contains(naighbour)))
            {
                visited.Add(naighbour);
                cluster.Add(naighbour);
                var naighbourNaighbours = naighbour.GetNeighbours(Points, Epsilon, SimilarityFuncion);
                if (naighbourNaighbours.Count() >= MinPoints)
                {
                    ExpandClusterAroundPointIntern(naighbour, cluster);
                }
            }
        }
    }
    //DBSCAN(D, eps, MinPts)
    //   C = 0
    //   for each unvisited point P in dataset D
    //      mark P as visited
    //      NeighborPts = regionQuery(P, eps)
    //      if sizeof(NeighborPts) < MinPts
    //         mark P as NOISE
    //      else
    //         C = next cluster
    //         expandCluster(P, NeighborPts, C, eps, MinPts)

    //expandCluster(P, NeighborPts, C, eps, MinPts)
    //   add P to cluster C
    //   for each point P' in NeighborPts 
    //      if P' is not visited
    //         mark P' as visited
    //         NeighborPts' = regionQuery(P', eps)
    //         if sizeof(NeighborPts') >= MinPts
    //            NeighborPts = NeighborPts joined with NeighborPts'
    //      if P' is not yet member of any cluster
    //         add P' to cluster C

    //regionQuery(P, eps)
    //   return all points within P's eps-neighborhood (including P)

    public delegate double Similarity<in T>(T a, T b);
    public static class Utilities
    {
        public static IEnumerable<T> GetNeighbours<T>(this T point, IEnumerable<T> domain, double epsilon, Similarity<T> similarityFunction)
        {
            return domain.Where(x => similarityFunction(point, x) <= epsilon);
        }

        public static int counter = 0;
        public static double EuclideanDistance(this Point a, Point b)
        {
            counter++;
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}
