using System;
using Newtonsoft.Json;
using UnityEngine;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     The segments of the building (the segment is the line formed between two edge vertices)
    /// </summary>
    [Serializable]
    public class Segment
    {
        /// <summary>
        ///     The end
        /// </summary>
        [JsonProperty("End")] public Point end;

        /// <summary>
        ///     The index
        /// </summary>
        public int Index;

        /// <summary>
        ///     The assoc building
        /// </summary>
        private readonly Polygon pol;

        /// <summary>
        ///     The start
        /// </summary>
        [JsonProperty("Start")] public Point start;

        /// <summary>
        ///     Prevents a default instance of the <see cref="Segment" /> class from being created.
        /// </summary>
        private Segment()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Segment" /> class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public Segment(Point start, Point end)
        {
            this.start = start;
            this.end = end;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Segment" /> class.
        /// </summary>
        /// <param name="bd">The bd.</param>
        /// <param name="i">The i.</param>
        /// <param name="s">The s.</param>
        /// <param name="e">The e.</param>
        public Segment(Polygon pol, int i, Point s, Point e)
        {
            this.pol = pol;

            Index = i;

            start = s;
            end = e;
        }

        /// <summary>
        ///     Gets the distance.
        /// </summary>
        /// <value>
        ///     The distance.
        /// </value>
        [JsonIgnore]
        public int Distance => (int) Vector2.Distance(start, end);

        /// <summary>
        ///     Gets the proportion.
        /// </summary>
        /// <value>
        ///     The proportion.
        /// </value>
        [JsonIgnore]
        public float Proportion => Distance / pol.longestSegment.Value;

        /// <summary>
        ///     Gets the normal.
        /// </summary>
        /// <value>
        ///     The normal.
        /// </value>
        [JsonIgnore]
        public Vector2 Normal => ((Vector2) end - (Vector2) start).normalized;

        /// <summary>
        ///     Gets the position.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public Vector2 GetPosition(float offset)
        {
            return (Vector2) start + Normal * offset;
        }
    }
}