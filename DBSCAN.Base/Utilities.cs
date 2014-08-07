using System;
using System.Collections.Generic;
using System.Linq;

namespace DBSCAN.Base
{
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
