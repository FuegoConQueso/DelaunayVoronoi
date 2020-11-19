using System;
using System.Collections.Generic;
using System.Linq;

namespace DelaunayVoronoi
{
    public class DelaunayTriangulator
    {
        private double MaxX { get; set; }
        private double MaxY { get; set; }
        private IEnumerable<Triangle> border;

        /// <summary>
        /// generates a fixed set of points
        /// </summary>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        /// <returns></returns>
        public IEnumerable<Point> GenerateFixedPoints(double maxX, double maxY)
        {
            MaxX = maxX;
            MaxY = maxY;

            //set up initial points/ tris
            var point0 = new Point(-MaxX, 0);
            var point1 = new Point(-MaxX, MaxY);
            var point2 = new Point(2 * MaxX, MaxY);
            var point3 = new Point(2 * MaxX, 0);
            var points = new List<Point>() { point0, point1, point2, point3 };
            var tri1 = new Triangle(point0, point1, point2);
            var tri2 = new Triangle(point0, point2, point3);
            border = new List<Triangle>() { tri1, tri2 };


            //add fixed points
            var fixedPoints = new List<Point>();
            fixedPoints.Add(new Point(MaxX / 2, MaxY / 2));
            fixedPoints.Add(new Point(10, MaxY / 3));
            fixedPoints.Add(new Point(MaxX / 2 + 15, MaxY * 2 / 3));
            fixedPoints.Add(new Point(20, MaxY * 3/4));

            //duplicate points to left and right
            var duplicatePoints = new List<Point>();
            foreach(Point point in fixedPoints)
            {
                duplicatePoints.Add(new Point(point.X - MaxX, point.Y));
                duplicatePoints.Add(new Point(point.X + MaxX, point.Y));
            }


            points.AddRange(fixedPoints);
            points.AddRange(duplicatePoints);

            return points;
        }

        public IEnumerable<Point> GeneratePoints(int amount, double maxX, double maxY)
        {
            MaxX = maxX;
            MaxY = maxY;

            // TODO make more beautiful
            var point0 = new Point(0, 0);
            var point1 = new Point(0, MaxY);
            var point2 = new Point(MaxX, MaxY);
            var point3 = new Point(MaxX, 0);
            var points = new List<Point>() { point0, point1, point2, point3 };
            var tri1 = new Triangle(point0, point1, point2);
            var tri2 = new Triangle(point0, point2, point3);
            border = new List<Triangle>() { tri1, tri2 };

            var random = new Random();
            for (int i = 0; i < amount - 4; i++)
            {
                var pointX = random.NextDouble() * MaxX;
                var pointY = random.NextDouble() * MaxY;
                points.Add(new Point(pointX, pointY));
            }

            return points;
        }

        public IEnumerable<Triangle> BowyerWatson(IEnumerable<Point> points)
        {
            //var supraTriangle = GenerateSupraTriangle();
            var triangulation = new HashSet<Triangle>(border);

            foreach (var point in points)
            {
                //identifies triangles that contain the given point
                var badTriangles = FindBadTriangles(point, triangulation);
                //identifies the edges of badtriangles that do not share edges with other bad triangles
                var polygon = FindHoleBoundaries(badTriangles);

                foreach (var triangle in badTriangles)
                {
                    foreach (var vertex in triangle.Vertices)
                    {
                        //for each point in a bad triangle, remove that triangle from its associated triangles
                        vertex.AdjacentTriangles.Remove(triangle);
                    }
                }
                //remove the bad triangles from triangulation
                triangulation.RemoveWhere(o => badTriangles.Contains(o));

                //foreach edge of the badtriangles that was removed that was not shared between multiple triangles, and which does not have the current point as a point
                foreach (var edge in polygon.Where(possibleEdge => possibleEdge.Point1 != point && possibleEdge.Point2 != point))
                {
                    var triangle = new Triangle(point, edge.Point1, edge.Point2);
                    triangulation.Add(triangle);
                }
            }

            //triangulation.RemoveWhere(o => o.Vertices.Any(v => supraTriangle.Vertices.Contains(v)));
            return triangulation;
        }

        private List<Edge> FindHoleBoundaries(ISet<Triangle> badTriangles)
        {
            var edges = new List<Edge>();
            foreach (var triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            //GroupBy groups the edges that are identical, then Where filters groups where only 1 edge exists, then Select transforms it into a list of each first edge in those groups.
            //So: we're getting the IEnumerable of all edges which aren't shared between bad triangles
            var boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
            //transforms the IEnumerable to a List<>
            return boundaryEdges.ToList();
        }

        private Triangle GenerateSupraTriangle()
        {
            //   1  -> maxX
            //  / \
            // 2---3
            // |
            // v maxY
            var margin = 500;
            var point1 = new Point(0.5 * MaxX, -2 * MaxX - margin);
            var point2 = new Point(-2 * MaxY - margin, 2 * MaxY + margin);
            var point3 = new Point(2 * MaxX + MaxY + margin, 2 * MaxY + margin);
            return new Triangle(point1, point2, point3);
        }

        private ISet<Triangle> FindBadTriangles(Point point, HashSet<Triangle> triangles)
        {
            var badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
            return new HashSet<Triangle>(badTriangles);
        }
    }
}