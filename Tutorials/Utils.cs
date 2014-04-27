using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PH.Common;
using PH.Map;
namespace Tutorials
{
    class Utils
    {
        public static int Distance(Point pa, Point pb)
        {
            return (pa.X - pb.X) * (pa.X - pb.X) + (pa.Y - pb.Y) * (pa.Y - pb.Y);
        }

        public static Point getNearestPoint(Point currentlocation, List<Entity> entities)
        {
            Point pReturn = Point.Empty;
            int dist = 200 * 200;
            foreach(Entity ent in entities)
            {
                Point entPoint = new Point(ent.X, ent.Y);
                int entDistance = Distance(entPoint, currentlocation);
                if (entDistance < dist)
                {
                    dist = entDistance;
                    pReturn = entPoint;
                }
            }

            return pReturn;
        }

        public static Point getMiddlePoint(Point[] points)
        {
            if (points == null || points.Length == 0) return Point.Empty;

            int sumX = 0;
            int sumY = 0;
            foreach (Point p in points)
            {
                sumX += p.X;
                sumY += p.Y;
            }

            int x = (int)Math.Round(1f * sumX / points.Length);
            int y = (int)Math.Round(1f * sumY / points.Length);

            return new Point(x, y);
        }

        public static bool isPointOK(Tissue tissue, int X, int Y)
        {
            if (!tissue.IsInMap(X, Y)) return false;
            return tissue[X, Y].AreaType == AreaEnum.HighDensity |
                tissue[X, Y].AreaType == AreaEnum.MediumDensity |
                tissue[X, Y].AreaType == AreaEnum.LowDensity;
        }

        public static Point getValidPoint(Tissue tissue, Point p)
        {
            if (isPointOK(tissue, p.X, p.Y))
                return p;
            int dist = 1;
            while (true)
            {
                //up
                for (int iX = -dist; iX < dist + 1; iX++)
                    if (isPointOK(tissue, p.X + iX, p.Y + dist))
                        return new Point(p.X + iX, p.Y + dist);
                //down
                for (int iX = -dist; iX < dist + 1; iX++)
                    if (isPointOK(tissue, p.X + iX, p.Y - dist))
                        return new Point(p.X + iX, p.Y - dist);
                //left
                for (int iY = -dist; iY < dist + 1; iY++)
                    if (isPointOK(tissue, p.X - dist, p.Y + iY))
                        return new Point(p.X - dist, p.Y + iY);
                //right
                for (int iY = -dist; iY < dist + 1; iY++)
                    if (isPointOK(tissue, p.X + dist, p.Y + iY))
                        return new Point(p.X + dist, p.Y + iY);
                dist++;
            }
        }
    }
}
