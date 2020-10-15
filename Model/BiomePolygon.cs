using System;
using uzSurfaceMapper.Model.Enums;
using Newtonsoft.Json;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Biome Polygon
    /// </summary>
    /// <seealso cref="Polygon" />
    [Serializable]
    [JsonObject]
    public sealed class BiomePolygon : Polygon
    {
        /// <summary>
        ///     Gets the data.
        /// </summary>
        /// <value>
        ///     The associated polygon data.
        /// </value>
        [JsonProperty("Type")]
        public GroundType? Type { get; set; }
    }
}