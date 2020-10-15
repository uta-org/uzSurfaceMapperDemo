using System;
using UnityEngine;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Aka Door
    /// </summary>
    [Serializable]
    public class Entrance
    {
        /// <summary>
        ///     The position
        /// </summary>
        public Vector3 position;

        /// <summary>
        ///     The segment index
        /// </summary>
        public int segmentIndex;

        /// <summary>
        ///     Prevents a default instance of the <see cref="Entrance" /> class from being created.
        /// </summary>
        private Entrance()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Entrance" /> class.
        /// </summary>
        /// <param name="segmentIndex">Index of the segment.</param>
        /// <param name="pos">The position.</param>
        public Entrance(int segmentIndex, Vector3 pos)
        {
            this.segmentIndex = segmentIndex;
            position = pos;
        }
    }
}