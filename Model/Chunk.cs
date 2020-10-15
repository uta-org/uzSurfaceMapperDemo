using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using uzSurfaceMapper.Core.Func;
using uzSurfaceMapper.Extensions;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     The chunk class that will have the info of the current portion of loaded land
    /// </summary>
    [Serializable]
    public class Chunk
    {
        /// <summary>
        ///     The list of buildings
        /// </summary>
        public HashSet<int> listOfIndexBuildings; // JsonIgnore?

        /// <summary>
        ///     The rectangle that defined the current chunk
        /// </summary>
        public Rect r;

        [JsonIgnore] public Vector2 Position => r.position + Vector2.one * SceneConversion.Instance.SinglePlaneSize / 2;

        [JsonIgnore] public HashSet<Point> roadPoints;

        /// <summary>
        ///     Prevents a default instance of the <see cref="Chunk" /> class from being created.
        /// </summary>
        private Chunk()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Chunk" /> class.
        /// </summary>
        /// <param name="r">The r.</param>
        public Chunk(Rect r)
        {
            this.r = r;
        }

        public override string ToString()
        {
            return $"Count of buildings: {listOfIndexBuildings.Count}\n" +
                   $"Rect: {r}\n" +
                   $"Position: {Position.SimplifiedToString()}";
        }
    }
}