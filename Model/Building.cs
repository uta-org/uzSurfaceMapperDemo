using System;
using System.Collections.Generic;
using uzSurfaceMapper.Core.Attrs.CodeAnalysis;
using uzSurfaceMapper.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using uzLib.Lite.ExternalCode.Extensions;
using uzSurfaceMapper.Extensions.Demo;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Building class (this is the building)
    /// </summary>
    [Serializable]
    public class Building
    {
        #region "Constructors"

        /// <summary>
        ///     Prevents a default instance of the <see cref="Building" /> class from being created.
        /// </summary>
        private Building()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Building" /> class.
        /// </summary>
        /// <param name="p">Starting position.</param>
        public Building(Point p)
        {
            pol = new Polygon
            {
                Position = p
            };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Building" /> class.
        /// </summary>
        /// <param name="x">X value of starting position.</param>
        /// <param name="y">Y value of starting position.</param>
        public Building(int x, int y)
        {
            pol = new Polygon
            {
                Position = new Point(x, y)
            };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Building" /> class.
        /// </summary>
        /// <param name="i">The index of the building.</param>
        /// <param name="p">Starting position.</param>
        public Building(int i, Point p)
        {
            index = i;

            pol = new Polygon
            {
                Position = p
            };
        }

        #endregion "Constructors"

        #region "Public fields"

        /// <summary>
        ///     The index of the building
        /// </summary>
        [JsonProperty("Index")] public int index = -1;

        /// <summary>
        ///     The lines props
        /// </summary>
        [OnlyDebug] [JsonIgnore] public float[] linesProps;

        #endregion "Public fields"

        #region "Private fields"

        /// <summary>
        ///     The edges of the building
        /// </summary>
        private Polygon pol;

        /// <summary>
        ///     The entrances
        /// </summary>
        private HashSet<Entrance> entrances;

        #endregion "Private fields"

        #region "Properties"

        /// <summary>
        ///     Gets the entrances.
        /// </summary>
        /// <value>
        ///     The entrances.
        /// </value>
        [JsonIgnore]
        [WIP]
        public HashSet<Entrance> Entrances => entrances;

        /// <summary>
        ///     Gets or sets the edges.
        /// </summary>
        /// <value>
        ///     The edges.
        /// </value>
        [JsonProperty("Polygon")]
        public Polygon Pol
        {
            get => pol;
            set => pol = value;
        }

        #endregion "Properties"

        #region "Methods"

        /// <summary>
        ///     Gets the weight of the current building. (The weight is used to get the height of the building later)
        /// </summary>
        /// <returns>Returns the weight (long).</returns>
        public long GetWeight()
        {
            return (Pol.VerticeCount > 0 ? Pol.VerticeCount : 0) +
                   (!Pol.Vertices.IsNullOrEmpty() ? Pol.EdgeCount : 0); //position.x + position.y +
        }

        /// <summary>
        ///     Loads the entrance data.
        /// </summary>
        /// <param name="doorEveryMeters">The door every meters.</param>
        /// <param name="minimumProp">The minimum property.</param>
        [WIP]
        [Testing]
        public void LoadEntranceData(float doorEveryMeters = 20f, float minimumProp = -1)
        {
            var entrances = new HashSet<Entrance>();

            Pol.Segments.ForEach(segment =>
            {
                if (minimumProp == -1)
                    minimumProp = .35f; // .5f // WIP: Rule of three

                if (segment.Proportion >= minimumProp)
                {
                    var numDoors = Mathf.CeilToInt(segment.Distance / doorEveryMeters);

                    var delta = segment.Distance % doorEveryMeters / 2;

                    for (var i = 0; i < numDoors; ++i)
                        entrances.Add(new Entrance(segment.Index, segment.GetPosition(delta + i * doorEveryMeters)));
                }
            });

            this.entrances = entrances;
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"ID: #{index} | Location: {pol.Center} | Offset: {pol.offset}";
        }

        #endregion "Methods"
    }
}