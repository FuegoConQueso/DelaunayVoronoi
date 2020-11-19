using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// utilizes segment-intersection method found at
// https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/

namespace DelaunayVoronoi
{
    public class Polygon
    {
        public Point DelaunayPoint;

        public List<Point> Vertices { get; set; } = new List<Point>();

        public List<Polygon> AdjacentRegions { get; set; }

        //{minX, maxX, minY, maxY}
        public double[] BoundingBox { get; private set; }

        public Polygon(Point delaunayPoint)
        {
            DelaunayPoint = delaunayPoint;
        }


        public void UpdateBoundingBox() {
            BoundingBox = new double[] { Vertices.First().X, Vertices.First().X, Vertices.First().Y, Vertices.First().Y };

            foreach (var vertex in Vertices)
            {
                if (vertex.X < BoundingBox[0])
                {
                    BoundingBox[0] = vertex.X;
                }
                else if (vertex.X > BoundingBox[1])
                {
                    BoundingBox[1] = vertex.X;
                }
                if (vertex.Y < BoundingBox[2])
                {
                    BoundingBox[2] = vertex.Y;
                }
                else if (vertex.Y > BoundingBox[3])
                {
                    BoundingBox[3] = vertex.Y;
                }
            }
        }

        public bool IsInBoundingBox(Point point)
        {
            if (point.X > BoundingBox[0] && point.X < BoundingBox[1] && point.Y > BoundingBox[2] && point.Y < BoundingBox[3])
            {
                return true;
            }
            return false;
        }

        public bool IsInside(Point point)
        {
            int count = 0;
            Point extreme = new Point(BoundingBox[1] + 1, point.Y);
            for (int i = 0; i < Vertices.Count; i++)
            {
                var next = (i + 1) % Vertices.Count;
                if (DoIntersect(point, extreme, Vertices[i], Vertices[next]))
                {
                    if (Orientation(Vertices[i], point, Vertices[next]) == 0)
                    {
                        return OnSegment(Vertices[i], point, Vertices[next]);
                    }
                    count++;
                }
            }
            return (count % 2 == 1);
        }



        // Given three colinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        private static bool OnSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        private static bool DoIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        /// <summary>
        /// returns the orientation of the triangle (point1, point2, point3)
        /// 1 = clockwise, -1 = counterclockwise, 0 = colinear
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <returns></returns>
        private static int Orientation(Point point1, Point point2, Point point3)
        {
            var result = (point2.X - point1.X) * (point3.Y - point1.Y) -
                (point3.X - point1.X) * (point2.Y - point1.Y);
            if (result > 0)
            {
                return -1;
            }
            if (result < 0)
            {
                return 1;
            }
            return 0;
        }
    }
}
