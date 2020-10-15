// High-performance polyline simplification library
//
// This is a port of simplify-js by Vladimir Agafonkin, Copyright (c) 2012
// https://github.com/mourner/simplify-js
//
// The code is ported from JavaScript to C#.
// The library is created as portable and
// is targeting multiple Microsoft plattforms.
//
// This library was ported by imshz @ http://www.shz.no
// https://github.com/imshz/simplify-net
//
// This code is provided as is by the author. For complete license please
// read the original license at https://github.com/mourner/simplify-js

using System;
using System.Collections.Generic;
using UnityEngine;

namespace uzSurfaceMapper.Utils.Simplification
{
    /// <summary>
    ///     Simplification of a 2D-polyline.
    /// </summary>
    public class SimplifyUtility : ISimplifyUtility
    {
        /// <summary>
        ///     Simplifies a list of vertices to a shorter list of vertices.
        /// </summary>
        /// <param name="vertices">Vertices original list of vertices</param>
        /// <param name="tolerance">Tolerance tolerance in the same measurement as the point coordinates</param>
        /// <param name="highestQuality">Enable highest quality for using Douglas-Peucker, set false for Radial-Distance algorithm</param>
        /// <returns>Simplified list of vertices</returns>
        public List<Vector2> Simplify(Vector2[] vertices, float tolerance = 0.3f, bool highestQuality = false)
        {
            if (vertices == null || vertices.Length == 0)
                return new List<Vector2>();

            var sqTolerance = tolerance * tolerance;

            if (highestQuality)
                return SimplifyDouglasPeucker(vertices, sqTolerance);

            var points2 = SimplifyRadialDistance(vertices, sqTolerance);
            return SimplifyDouglasPeucker(points2.ToArray(), sqTolerance);
        }

        public List<Vector3> Simplify3D(Vector3[] vertices, float tolerance = 0.3F, bool highestQuality = false)
        {
            throw new NotImplementedException();
        }

        // square distance between 2 vertices
        private float GetSquareDistance(Vector2 p1, Vector2 p2)
        {
            float dx = p1.x - p2.x,
                dy = p1.y - p2.y;

            return dx * dx + dy * dy;
        }

        // square distance from a point to a segment
        private float GetSquareSegmentDistance(Vector2 p, Vector2 p1, Vector2 p2)
        {
            var x = p1.x;
            var y = p1.y;
            var dx = p2.x - x;
            var dy = p2.y - y;

            if (!dx.Equals(0.0) || !dy.Equals(0.0))
            {
                var t = ((p.x - x) * dx + (p.y - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = p2.x;
                    y = p2.y;
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = p.x - x;
            dy = p.y - y;

            return dx * dx + dy * dy;
        }

        // rest of the code doesn't care about point format

        // basic distance-based simplification
        private List<Vector2> SimplifyRadialDistance(Vector2[] vertices, float sqTolerance)
        {
            var prevPoint = vertices[0];
            var newPoints = new List<Vector2> { prevPoint };
            var point = Vector2.zero;

            for (var i = 1; i < vertices.Length; i++)
            {
                point = vertices[i];

                if (GetSquareDistance(point, prevPoint) > sqTolerance)
                {
                    newPoints.Add(point);
                    prevPoint = point;
                }
            }

            if (point != null && !prevPoint.Equals(point))
                newPoints.Add(point);

            return newPoints;
        }

        // simplification using optimized Douglas-Peucker algorithm with recursion elimination
        private List<Vector2> SimplifyDouglasPeucker(Vector2[] vertices, float sqTolerance)
        {
            var len = vertices.Length;
            var markers = new int?[len];
            int? first = 0;
            int? last = len - 1;
            int? index = 0;
            var stack = new List<int?>();
            var newPoints = new List<Vector2>();

            markers[first.Value] = markers[last.Value] = 1;

            while (last != null)
            {
                var maxSqDist = 0.0d;

                for (var i = first + 1; i < last; i++)
                {
                    var sqDist =
                        GetSquareSegmentDistance(vertices[i.Value], vertices[first.Value], vertices[last.Value]);

                    if (sqDist > maxSqDist)
                    {
                        index = i;
                        maxSqDist = sqDist;
                    }
                }

                if (maxSqDist > sqTolerance)
                {
                    markers[index.Value] = 1;
                    stack.AddRange(new[] { first, index, index, last });
                }

                if (stack.Count > 0)
                {
                    last = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    last = null;
                }

                if (stack.Count > 0)
                {
                    first = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    first = null;
                }
            }

            for (var i = 0; i < len; i++)
                if (markers[i] != null)
                    newPoints.Add(vertices[i]);

            return newPoints;
        }

        /// <summary>
        ///     Simplifies a list of vertices to a shorter list of vertices.
        /// </summary>
        /// <param name="vertices">Vertices original list of vertices</param>
        /// <param name="tolerance">Tolerance tolerance in the same measurement as the point coordinates</param>
        /// <param name="highestQuality">Enable highest quality for using Douglas-Peucker, set false for Radial-Distance algorithm</param>
        /// <returns>Simplified list of vertices</returns>
        public static List<Vector2> SimplifyArray(Vector2[] vertices, float tolerance = 0.3f,
            bool highestQuality = false)
        {
            return new SimplifyUtility().Simplify(vertices, tolerance, highestQuality);
        }
    }
}