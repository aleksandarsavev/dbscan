using System.Collections.Generic;

namespace DBSCAN.Base
{
    public class PointInfo<T>
    {
        public PointInfo() { }

        public PointInfo(Point coordinates, IEnumerable<T> cluster)
        {
            Coordinates = coordinates;
            Cluster = cluster;
        }
        public Point Coordinates { get; set; }
        public IEnumerable<T> Cluster { get; set; }
    }
}
