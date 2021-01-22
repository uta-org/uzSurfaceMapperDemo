using System;
using System.Collections.Generic;
using System.Linq;
using uzSurfaceMapper.Model;

namespace UnityEngine.Extensions
{
    public static class GeometryHelper
    {
        public static float DifferenceBetweenLines(Vector2[] drawn, Vector2[] toMatch)
        {
            float sqrDistAcc = 0f;
            float length = 0f;

            Vector2 prevPoint = toMatch[0];

            foreach (var toMatchPoint in WalkAlongLine(toMatch))
            {
                sqrDistAcc += SqrDistanceToLine(drawn, toMatchPoint);
                length += Vector2.Distance(toMatchPoint, prevPoint);

                prevPoint = toMatchPoint;
            }

            return sqrDistAcc / length;
        }

        /// <summary>
        /// Move a point from the beginning of the line to its end using a maximum step, yielding the point at each step.
        /// </summary>
        private static IEnumerable<Vector2> WalkAlongLine(IEnumerable<Vector2> line, float maxStep = .1f)
        {
            using (var lineEnum = line.GetEnumerator())
            {
                if (!lineEnum.MoveNext())
                    yield break;

                var pos = lineEnum.Current;

                while (lineEnum.MoveNext())
                {
                    //Debug.Log(lineEnum.Current);
                    var target = lineEnum.Current;
                    while (pos != target)
                    {
                        yield return pos = Vector2.MoveTowards(pos, target, maxStep);
                    }
                }
            }
        }

        private static float SqrDistanceToLine(Vector2[] line, Vector2 point)
        {
            return ListSegments(line)
                .Select(seg => SqrDistanceToSegment(seg.a, seg.b, point))
                .Min();
        }

        private static float SqrDistanceToSegment(Vector2 linePoint1, Vector2 linePoint2, Vector2 point)
        {
            var projected = ProjectPointOnLineSegment(linePoint1, linePoint1, point);
            return (projected - point).sqrMagnitude;
        }

        /// <summary>
        /// Outputs each position of the line (but the last) and the consecutive one wrapped in a Segment.
        /// Example: a, b, c, d --> (a, b), (b, c), (c, d)
        /// </summary>
        private static IEnumerable<Segment> ListSegments(IEnumerable<Vector2> line)
        {
            using (var pt1 = line.GetEnumerator())
            using (var pt2 = line.GetEnumerator())
            {
                pt2.MoveNext();

                while (pt2.MoveNext())
                {
                    pt1.MoveNext();

                    yield return new Segment { a = pt1.Current, b = pt2.Current };
                }
            }
        }

        private struct Segment
        {
            public Vector2 a;
            public Vector2 b;
        }

        //This function finds out on which side of a line segment the point is located.
        //The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
        //the line segment, project it on the line using ProjectPointOnLine() first.
        //Returns 0 if point is on the line segment.
        //Returns 1 if point is outside of the line segment and located on the side of linePoint1.
        //Returns 2 if point is outside of the line segment and located on the side of linePoint2.
        private static int PointOnWhichSideOfLineSegment(Vector2 linePoint1, Vector2 linePoint2, Vector2 point)
        {
            Vector2 lineVec = linePoint2 - linePoint1;
            Vector2 pointVec = point - linePoint1;

            if (Vector2.Dot(pointVec, lineVec) > 0)
            {
                return pointVec.magnitude <= lineVec.magnitude ? 0 : 2;
            }

            return 1;
        }

        //This function returns a point which is a projection from a point to a line.
        //The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
        private static Vector2 ProjectPointOnLine(Vector2 linePoint, Vector2 lineVec, Vector2 point)
        {
            //get vector from point on line to point in space
            Vector2 linePointToPoint = point - linePoint;
            float t = Vector2.Dot(linePointToPoint, lineVec);
            return linePoint + lineVec * t;
        }

        //This function returns a point which is a projection from a point to a line segment.
        //If the projected point lies outside of the line segment, the projected point will
        //be clamped to the appropriate line edge.
        //If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
        private static Vector2 ProjectPointOnLineSegment(Vector2 linePoint1, Vector2 linePoint2, Vector2 point)
        {
            Vector2 vector = linePoint2 - linePoint1;
            Vector2 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

            switch (PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint))
            {
                case 0:
                    return projectedPoint;

                case 1:
                    return linePoint1;

                case 2:
                    return linePoint2;

                default:
                    //output is invalid
                    return Vector2.zero;
            }
        }

        public static IEnumerable<Point> DrawLineAsEnumerable(Point p1, Point p2, Func<int, int, bool> predicate = null)
        {
            return DrawLineAsEnumerable(p1.x, p1.y, p2.x, p2.y, predicate);
        }

        public static IEnumerable<Point> DrawLineAsEnumerable(int x0, int y0, int x1, int y1, Func<int, int, bool> predicate = null)
        {
            int sx, sy;

            int dx = Mathf.Abs(x1 - x0),
                dy = Mathf.Abs(y1 - y0);

            if (x0 < x1)
                sx = 1;
            else
                sx = -1;
            if (y0 < y1)
                sy = 1;
            else
                sy = -1;

            int err = dx - dy,
                e2;

            while (true)
            {
                if (predicate == null || !predicate(x0, y0))
                    yield return new Point(x0, y0);

                if (x0 == x1 && y0 == y1)
                    break;

                e2 = 2 * err;

                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }

                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
        }
    }
}