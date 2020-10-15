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

using System.Collections.Generic;
using UnityEngine;

namespace uzSurfaceMapper.Utils.Simplification
{
    public interface ISimplifyUtility
    {
        /// <summary>
        ///     Simplifies a list of vertices to a shorter list of vertices.
        /// </summary>
        /// <param name="vertices">Vertices original list of vertices</param>
        /// <param name="tolerance">Tolerance tolerance in the same measurement as the point coordinates</param>
        /// <param name="highestQuality">Enable highest quality for using Douglas-Peucker, set false for Radial-Distance algorithm</param>
        /// <returns>Simplified list of vertices</returns>
        List<Vector2> Simplify(Vector2[] vertices, float tolerance = 0.3f, bool highestQuality = false);

        List<Vector3> Simplify3D(Vector3[] vertices, float tolerance = 0.3f, bool highestQuality = false);
    }
}