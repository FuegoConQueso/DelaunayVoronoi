using System;
using System.Linq;
using System.Collections.Generic;

namespace DelaunayVoronoi
{
    public class Voronoi
    {
        public IEnumerable<Edge> GenerateEdgesFromDelaunay(IEnumerable<Triangle> triangulation)
        {
            var voronoiEdges = new HashSet<Edge>();
            foreach (var triangle in triangulation)
            {
                foreach (var neighbor in triangle.TrianglesWithSharedEdge)
                {
                    var edge = new Edge(triangle.Circumcenter, neighbor.Circumcenter);
                    voronoiEdges.Add(edge);
                }
            }
            return voronoiEdges;
            //var refinedEdges = RefineEdges(voronoiEdges, MaxX, MaxY);
            //return refinedEdges;
        }

        public IEnumerable<Polygon> GenerateRegionsFromDelaunay(IEnumerable<Point> points, double MaxX, double MaxY)
        {
            var maxYEdge = new Edge(new Point(-MaxX, MaxY), new Point(2 * MaxX, MaxY));
            var minYEdge = new Edge(new Point(-MaxX, 0), new Point(2 * MaxX, 0));

            //is hashset best option? Maybe not?
            var voronoiRegions = new HashSet<Polygon>();
            foreach (var point in points)
            {
                var triangles = new List<Triangle>(point.AdjacentTriangles);
                for (int i = 0; i < triangles.Count - 1; i++)
                {
                    for (int j = i + 1;  j < triangles.Count; j++)
                    {
                        if (triangles[j].SharesEdgeWith(triangles[i]))
                        {
                            var neighbor = triangles[j];
                            triangles[j] = triangles[i + 1];
                            triangles[i + 1] = neighbor;
                            break;
                        }
                    }
                }
                if (!triangles[0].SharesEdgeWith(triangles[triangles.Count - 1]))
                {
                    continue;
                }

                var region = new Polygon(point);
                var vertices = triangles.Select<Triangle, Point>(o => o.Circumcenter).ToList();
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (vertices[i].Y > MaxY)
                    {
                        var edge1 = new Edge(vertices[i], vertices[mod((i - 1), vertices.Count)]);
                        var intersect1 = edge1.intersection(maxYEdge);

                        var edge2 = new Edge(vertices[i], vertices[(i + 1) % vertices.Count]);
                        var intersect2 = edge2.intersection(maxYEdge);

                        if (intersect1 != null && intersect2 != null)
                        {
                            region.Vertices.Add(intersect1);
                            region.Vertices.Add(intersect2);
                        }
                        else if (intersect1 == null ^ intersect2 == null)
                        {
                            Console.WriteLine("XOR condition called for " + point.ToString());
                            if (intersect1 == null)
                            {
                                region.Vertices.Add(intersect2);
                            }
                            else
                            {
                                region.Vertices.Add(intersect1);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Other condition called for " + point.ToString());
                        }

                    }
                    else if (vertices[i].Y < 0)
                    {
                        var edge1 = new Edge(vertices[i], vertices[mod((i - 1), vertices.Count)]);
                        var intersect1 = edge1.intersection(minYEdge);
                        

                        var edge2 = new Edge(vertices[i], vertices[(i + 1) % vertices.Count]);
                        var intersect2 = edge2.intersection(minYEdge);

                        if (intersect1 != null && intersect2 != null)
                        {
                            region.Vertices.Add(intersect1);
                            region.Vertices.Add(intersect2);
                        }
                        else if (intersect1 == null ^ intersect2 == null)
                        {
                            Console.WriteLine("XOR condition called for " + point.ToString());
                            if (intersect1 == null)
                            {
                                region.Vertices.Add(intersect2);
                            }
                            else
                            {
                                region.Vertices.Add(intersect1);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Other condition called for " + point.ToString());

                        }
                    }
                    else
                    {
                        region.Vertices.Add(vertices[i]);
                    }
                }
                //only add regions that fall within the world's boundaries
                region.UpdateBoundingBox();
                if (region.BoundingBox[0] <= MaxX && region.BoundingBox[1] >= 0
                    && region.BoundingBox[2] <= MaxY && region.BoundingBox[3] >= 0)
                {
                    voronoiRegions.Add(region);
                }
            }
            return voronoiRegions;
        }

        public void ConnectRegions(IEnumerable<Polygon> VoronoiRegions)
        {
            foreach (Polygon region in VoronoiRegions)
            {
                region.UpdateBoundingBox();
            }

            foreach (Polygon center in VoronoiRegions)
            {
                List<Polygon> adjacentRegions = new List<Polygon>();
                for (int i = 0; i < center.Vertices.Count; i++)
                {
                    var borderEdge = new Edge(center.Vertices[i], center.Vertices[(i + 1) % center.Vertices.Count]);
                    var extended = extend(center.DelaunayPoint, borderEdge.Midpoint);
                    Polygon adjacentRegion = null;
                    foreach (Polygon region in VoronoiRegions)
                    {
                        if (region != center && region.IsInBoundingBox(extended) && region.IsInside(extended))
                        {
                            adjacentRegion = region;
                            break;
                        }
                    }
                    adjacentRegions.Add(adjacentRegion);
                }
                center.AdjacentRegions = adjacentRegions;
            }

        }

        public Point extend(Point center, Point border)
        {
            var x = border.X - center.X;
            var y = border.Y - center.Y;
            if (x > 0) x++;
            else if (x < 0) x--;
            if (y > 0) y++;
            else if (y < 0) y--;
            return new Point(center.X + x, center.Y + y);
        }

        public void AddRegionEdgeNoise(Polygon region)
        {
            var center = region.DelaunayPoint;
            HashSet <Point> points = new HashSet<Point>();
        }


        //TODO: find a better place for this
        public static int mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}