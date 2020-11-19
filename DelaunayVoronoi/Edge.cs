using System;

namespace DelaunayVoronoi
{
    public class Edge
    {
        public Point Point1 { get; }
        public Point Point2 { get; }
        public Point Midpoint
        {
            get
            {
                var x = ((Point1.X + Point2.X) / 2);
                var y = ((Point1.Y + Point2.Y) / 2);
                return new Point(x, y);
            }
        }
        public double? Slope
        {
            get
            {
                if ((Point1.X - Point2.X) != 0)
                {
                    return (Point1.Y - Point2.Y) / (Point1.X - Point2.X);
                }
                return null;
            }
        }
        public double? Intercept
        {
            get
            {
                double? slope = Slope;
                if (slope != null)
                {
                    return (Point1.Y - Point1.X * Slope);
                }
                return null;
            }
        }

        public Edge(Point point1, Point point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var edge = obj as Edge;

            var samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
            var samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
            return samePoints || samePointsReversed;
        }

        public override int GetHashCode()
        {
            int hCode = (int)Point1.X ^ (int)Point1.Y ^ (int)Point2.X ^ (int)Point2.Y;
            return hCode.GetHashCode();
        }

        /// <summary>
        /// If two edges (line segments) intersect, returns the point at which they intersect.
        /// Does not count shared vertices
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Point intersection(Edge other)
        {
            double? slope1 = this.Slope;
            double? slope2 = other.Slope;
            double intersectX;
            double intersectY;
            if (slope1 != null)
            {
                if (slope1 == slope2)
                {
                    return null;
                }
                //both lines are non-parallel & non-vertical
                else if (slope2 != null)
                {
                    intersectX = (double)((this.Intercept - other.Intercept) / (other.Slope - this.Slope));
                    if (intersectX < Math.Max(Math.Min(this.Point1.X, this.Point2.X), Math.Min(other.Point1.X, other.Point2.X))
                        || intersectX > Math.Min(Math.Max(this.Point1.X, this.Point2.X), Math.Max(other.Point1.X, other.Point2.X)))
                    {
                        return null;
                    }
                    intersectY = (double)(this.Slope * intersectX + this.Intercept);
                    return new Point(intersectX, intersectY);
                }

                //other line is vertical
                intersectX = other.Point1.X;
                intersectY = (double)(this.Slope * intersectX + this.Intercept);
                
                if (intersectY > Math.Min(other.Point1.Y, other.Point2.Y) && intersectY < Math.Max(other.Point1.Y, other.Point2.Y))
                {
                    return new Point(intersectX, intersectY);
                }
            }
            //this line is vertical
            else if (slope2 != null)
            {
                intersectX = this.Point1.X;
                intersectY = (double)(other.Slope * intersectX + other.Intercept);
                
                if (intersectY > Math.Min(this.Point1.Y, this.Point2.Y) && intersectY < Math.Max(this.Point1.Y, this.Point2.Y))
                {
                    return new Point(intersectX, intersectY);
                }
            }
            return null;
        }


        //public double Length()
        //{
        //    return Math.Sqrt(Math.Pow(Point1.X - Point2.X, 2) + Math.Pow(Point1.Y - Point2.Y, 2));
        //}
    }
}