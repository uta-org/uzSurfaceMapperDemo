using System;
using System.Collections.Generic;
using uzSurfaceMapper.Core.Attrs.CodeAnalysis;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     This class is very similar to City class
    /// </summary>
    [Serializable]
    [WIP]
    public class Map
    {
        /// <summary>
        ///     The chunks
        /// </summary>
        public List<Chunk> Chunks = new List<Chunk>();

        // WIP: This will be deleted on the future
        /// <summary>
        ///     The polygon of the map
        /// </summary>
        public List<BiomePolygon> Pols = new List<BiomePolygon>();
    }
}