using System;
using Newtonsoft.Json;
using UnityEngine;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     The segments of the building (the segment is the line formed between two edge vertices)
    /// </summary>
    [Serializable]
    public class SegmentF
    {
        /// <summary>
        ///     The end
        /// </summary>
        [JsonProperty("End")] public Vector2 end;

        /// <summary>
        ///     The index
        /// </summary>
        public int Index;

        /// <summary>
        ///     The start
        /// </summary>
        [JsonProperty("Start")] public Vector2 start;

        /// <summary>
        ///     Prevents a default instance of the <see cref="SegmentF" /> class from being created.
        /// </summary>
        private SegmentF()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SegmentF" /> class.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="y2">The y2.</param>
        public SegmentF(float x1, float x2, float y1, float y2)
            : this(new Vector2(x1, y1), new Vector2(x2, y2))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Segment" /> class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public SegmentF(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }

        /// <summary>
        ///     Gets the distance.
        /// </summary>
        /// <value>
        ///     The distance.
        /// </value>
        [JsonIgnore]
        public float Distance => Vector2.Distance(start, end);

        /// <summary>
        ///     Gets the normal.
        /// </summary>
        /// <value>
        ///     The normal.
        /// </value>
        [JsonIgnore]
        public Vector2 Normal => (end - start).normalized;

        /// <summary>
        ///     Performs an implicit conversion from <see cref="Segment" /> to <see cref="SegmentF" />.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static implicit operator SegmentF(Segment segment)
        {
            return new SegmentF(segment.start, segment.end);
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="SegmentF" /> to <see cref="Segment" />.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static implicit operator Segment(SegmentF segment)
        {
            return new Segment(segment.start, segment.end);
        }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public Vector2 GetPosition(float offset)
        {
            return start + Normal * offset;
        }
    }
}