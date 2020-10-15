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
    ///     Simplification of a 3D-polyline.
    ///     Use only the 3D version if your point contains altitude information, if no altitude information is provided the 2D
    ///     library gives a 20% performance gain.
    /// </summary>
    public class SimplifyUtility3D : ISimplifyUtility
    {
        /// <summary>
        ///     Simplifies a list of vertices to a shorter list of vertices.
        /// </summary>
        /// <param name="vertices">Vertices original list of vertices</param>
        /// <param name="tolerance">Tolerance tolerance in the same measurement as the point coordinates</param>
        /// <param name="highestQuality">Enable highest quality for using Douglas-Peucker, set false for Radial-Distance algorithm</param>
        /// <returns>Simplified list of vertices</returns>
        public List<Vector3> Simplify3D(Vector3[] vertices, float tolerance = 0.3f, bool highestQuality = false)
        {
            if (vertices == null || vertices.Length == 0)
                return new List<Vector3>();

            var sqTolerance = tolerance * tolerance;

            if (!highestQuality)
            {
                var points2 = SimplifyRadialDistance(vertices, sqTolerance);
                return SimplifyDouglasPeucker(points2.ToArray(), sqTolerance);
            }

            return SimplifyDouglasPeucker(vertices, sqTolerance);
        }

        public List<Vector2> Simplify(Vector2[] vertices, float tolerance = 0.3F, bool highestQuality = false)
        {
            throw new NotImplementedException();
        }

        // square distance between 2 vertices
        private float GetSquareDistance(Vector3 p1, Vector3 p2)
        {
            float dx = p1.x - p2.x,
                dy = p1.y - p2.y,
                dz = p1.z - p2.z;

            return dx * dx + dy * dy + dz * dz;
        }

        // square distance from a point to a segment
        private float GetSquareSegmentDistance(Vector3 p, Vector3 p1, Vector3 p2)
        {
            var x = p1.x;
            var y = p1.y;
            var z = p1.z;
            var dx = p2.x - x;
            var dy = p2.y - y;
            var dz = p2.z - z;

            if (!dx.Equals(0.0) || !dy.Equals(0.0) || !dz.Equals(0.0))
            {
                var t = ((p.x - x) * dx + (p.y - y) * dy + (p.z - z) * dz) / (dx * dx + dy * dy + dz * dz);

                if (t > 1)
                {
                    x = p2.x;
                    y = p2.y;
                    z = p2.z;
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                    z += dz * t;
                }
            }

            dx = p.x - x;
            dy = p.y - y;
            dz = p.z - z;

            return dx * dx + dy * dy + dz * dz;
        }

        // rest of the code doesn't care about point format

        // basic distance-based simplification
        private List<Vector3> SimplifyRadialDistance(Vector3[] vertices, float sqTolerance)
        {
            var prevPoint = vertices[0];
            var newPoints = new List<Vector3> {prevPoint};
            var point = Vector3.zero;

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
        private List<Vector3> SimplifyDouglasPeucker(Vector3[] vertices, float sqTolerance)
        {
            var len = vertices.Length;
            var markers = new int?[len];
            int? first = 0;
            int? last = len - 1;
            int? index = 0;
            var stack = new List<int?>();
            var newPoints = new List<Vector3>();

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
                    stack.AddRange(new[] {first, index, index, last});
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
        public static List<Vector3> SimplifyArray(Vector3[] vertices, float tolerance = 0.3f,
            bool highestQuality = false)
        {
            return new SimplifyUtility3D().Simplify3D(vertices, tolerance, highestQuality);
        }
    }
}