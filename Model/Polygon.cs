using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using uzSurfaceMapper.Model.Enums;
using uzSurfaceMapper.Extensions.Demo;
using uzSurfaceMapper.Utils.Simplification;
using Newtonsoft.Json;
using UnityEngine;
using uzLib.Lite.ExternalCode.Extensions;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    /// </summary>
    /// <seealso cref="System.Collections.Generic.ICollection{Point}" />
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class Polygon : ICollection<Point>
    {
        /// <summary>
        ///     The order by path
        /// </summary>
        private const bool orderByPath = false;

        #region "Fields & Properties"

        /// <summary>
        ///     The vertices
        /// </summary>
#if !UNITY_EDITOR
        [JsonIgnore]
#else

        [JsonProperty("Vertices")]
#endif
        private HashSet<Point> vertices;

        /// <summary>
        ///     Gets the vertices.
        /// </summary>
        /// <value>
        ///     The vertices.
        /// </value>
        [JsonIgnore]
        public HashSet<Point> Vertices => vertices;

        /// <summary>
        ///     The edges
        /// </summary>
        [JsonProperty("Edges")] private HashSet<Point> edges;

        /// <summary>
        ///     Gets the edges.
        /// </summary>
        /// <value>
        ///     The edges.
        /// </value>
        [JsonIgnore]
        public HashSet<Point> Edges
        {
            get
            {
                if (CanBeOptimized() && edges.IsNullOrEmpty())
                    GetCenter();

                return edges;
            }
        }

        /// <summary>
        ///     The segments
        /// </summary>
        [JsonProperty("Segments")] private Segment[] segments;

        /// <summary>
        ///     Gets the segments.
        /// </summary>
        /// <value>
        ///     The segments.
        /// </value>
        [JsonIgnore]
        public Segment[] Segments
        {
            get
            {
                if (CanBeOptimized() && segments.IsNullOrEmpty())
                    GetSegments();

                return segments;
            }
        }

        /// <summary>
        ///     The position of the building
        /// </summary>
        [JsonProperty("Position")] private Point position;

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        /// <value>
        ///     The position.
        /// </value>
        [JsonIgnore]
        public Point Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        ///     The center of the building
        /// </summary>
        [JsonProperty("Center")] private Point center;

        /// <summary>
        ///     Gets the center.
        /// </summary>
        /// <value>
        ///     The center.
        /// </value>
        [JsonIgnore]
        public Point Center
        {
            get
            {
                if (center == default)
                    GetCenter();

                return center;
            }
        }

        /// <summary>
        ///     The longest segment
        /// </summary>
#if !UNITY_EDITOR
        [JsonIgnore]
#else

        [JsonProperty("LongestSegment")]
#endif
        public float? longestSegment;

        /// <summary>
        ///     The edge array
        /// </summary>
        [JsonIgnore] private Point[] _edgeArray;

        /// <summary>
        ///     Gets the edge array.
        /// </summary>
        /// <value>
        ///     The edge array.
        /// </value>
        [JsonIgnore]
        private Point[] edgeArray
        {
            get
            {
                if (_edgeArray == null && edges != null && edges.Count > 0)
                    _edgeArray = edges.ToArray();
                else if (_edgeArray == null && (edges == null || edges != null && edges.Count == 0))
                    return null;

                return _edgeArray;
            }
        }

        /// <summary>
        ///     The edge vec array
        /// </summary>
        [JsonIgnore] private Vector2[] _edgeVecArray;

        /// <summary>
        ///     Gets the edge vec array.
        /// </summary>
        /// <value>
        ///     The edge vec array.
        /// </value>
        [JsonIgnore]
        private Vector2[] edgeVecArray
        {
            get
            {
                if (_edgeVecArray == null)
                    _edgeVecArray = _edgeArray.Select(x => (Vector2)x).ToArray();

                return _edgeVecArray;
            }
        }

        /// <summary>
        ///     The point array
        /// </summary>
        [JsonIgnore] private Point[] _pointArray;

        /// <summary>
        ///     Gets the point array.
        /// </summary>
        /// <value>
        ///     The point array.
        /// </value>
        [JsonIgnore]
        private Point[] pointArray
        {
            get
            {
                if (_pointArray == null && vertices != null && vertices.Count > 0)
                    _pointArray = vertices.ToArray();
                else if (_pointArray == null && (vertices == null || vertices != null && vertices.Count == 0))
                    return null;

                return _pointArray;
            }
        }

        /// <summary>
        ///     The point vec array
        /// </summary>
        [JsonIgnore] private Vector2[] _pointVecArray;

        /// <summary>
        ///     Gets the point vec array.
        /// </summary>
        /// <value>
        ///     The point vec array.
        /// </value>
        [JsonIgnore]
        private Vector2[] pointVecArray
        {
            get
            {
                if (_pointVecArray == null)
                    _pointVecArray = _pointArray.Select(x => (Vector2)x).ToArray();

                return _pointVecArray;
            }
        }

        /// <summary>
        ///     The x minimum
        /// </summary>
        [JsonIgnore] private int _xMin = -1;

        /// <summary>
        ///     The y minimum
        /// </summary>
        [JsonIgnore] private int _yMin = -1;

        /// <summary>
        ///     Gets the x minimum.
        /// </summary>
        /// <value>
        ///     The x minimum.
        /// </value>
        private int xMin
        {
            get
            {
                if (_xMin == -1)
                    _xMin = vertices.Min(e => e.x);

                return _xMin;
            }
        }

        /// <summary>
        ///     Gets the y minimum.
        /// </summary>
        /// <value>
        ///     The y minimum.
        /// </value>
        private int yMin
        {
            get
            {
                if (_yMin == -1)
                    _yMin = vertices.Min(e => e.y);

                return _yMin;
            }
        }

        /// <summary>
        ///     The offset
        /// </summary>
        [JsonIgnore] private Point _offset;

        /// <summary>
        ///     Gets the offset.
        /// </summary>
        /// <value>
        ///     The offset.
        /// </value>
        [JsonIgnore]
        public Point offset
        {
            get
            {
                if (_offset == null)
                    _offset = center - new Point(xMin, yMin);

                return _offset;
            }
        }

#if !UNITY_EDITOR
        [JsonIgnore]
#else

        [JsonProperty("VerticeCount")]
#endif

        /// <summary>
        /// Gets the vertice count.
        /// </summary>
        /// <value>
        /// The vertice count.
        /// </value>
        public int VerticeCount
        {
            get
            {
                if (vertices != null)
                    return vertices.Count;

                return -1;
            }
#if UNITY_EDITOR
            set { }
#endif
        }

#if !UNITY_EDITOR
        [JsonIgnore]
#else

        [JsonProperty("EdgeCount")]
#endif

        /// <summary>
        /// Gets the edge count.
        /// </summary>
        /// <value>
        /// The edge count.
        /// </value>
        public int EdgeCount
        {
            get
            {
                if (edges != null)
                    return Edges.Count;

                throw new NullReferenceException(
                    "Edges are null, first you need to call 'edges = new HashSet<Point>();'");
            }
#if UNITY_EDITOR
            set { }
#endif
        }

#if !UNITY_EDITOR
        [JsonIgnore]
#else

        [JsonProperty("SegmentCount")]
#endif

        /// <summary>
        /// Gets the segment count.
        /// </summary>
        /// <value>
        /// The segment count.
        /// </value>
        public int SegmentCount
        {
            get
            {
                if (segments != null)
                    return segments.Length;

                return -1;
            }
#if UNITY_EDITOR
            set { }
#endif
        }

#if !UNITY_EDITOR
        [JsonIgnore]
#else

        [JsonProperty("EdgeRelation")]
#endif

        /// <summary>
        /// Gets the point relation.
        /// </summary>
        /// <value>
        /// The point relation.
        /// </value>
        public float EdgesRelation
        {
            get
            {
                if (EdgeCount > 0 && VerticeCount > 0)
                    return (float)edges.Count / VerticeCount;

                return 0;
            }
#if UNITY_EDITOR
            set { }
#endif
        }

        /// <summary>
        ///     Gets the minimum bounds.
        /// </summary>
        /// <value>
        ///     The minimum bounds.
        /// </value>
        [JsonIgnore]
        public Vector2 MinBounds
        {
            get
            {
                if (!CanBeOptimized())
                    return Vector2.zero;

                return new Vector2(vertices.Min(v => v.x), vertices.Min(v => v.y));
            }
        }

        /// <summary>
        ///     Gets the maximum bounds.
        /// </summary>
        /// <value>
        ///     The maximum bounds.
        /// </value>
        [JsonIgnore]
        public Vector2 MaxBounds
        {
            get
            {
                if (!CanBeOptimized())
                    return Vector2.zero;

                return new Vector2(vertices.Max(v => v.x), vertices.Max(v => v.y));
            }
        }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        [JsonIgnore]
        public float Width => MaxBounds.x - MinBounds.x;

        /// <summary>
        ///     Gets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        [JsonIgnore]
        public float Height => MaxBounds.y - MinBounds.y;

        #endregion "Fields & Properties"

        #region "Constructors"

        public Polygon()
        {
            vertices = new HashSet<Point>();
            edges = new HashSet<Point>();
        }

        #endregion "Constructors"

        #region "Interface impl"

        /// <summary>
        ///     Gets the count.
        /// </summary>
        /// <value>
        ///     The count.
        /// </value>
        [JsonIgnore]
        public int Count => vertices.Count;

        /// <summary>
        ///     Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        [JsonIgnore]
        public bool IsReadOnly => false;

        /// <summary>
        ///     Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Add(Point item)
        {
            //if (vertices == null) vertices = new HashSet<Point>();

            return vertices.Add(item);
        }

        /// <summary>
        ///     Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        void ICollection<Point>.Add(Point item)
        {
            vertices.Add(item);
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        public void Clear()
        {
            vertices.Clear();
        }

        /// <summary>
        ///     Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///     <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Point item)
        {
            return vertices.Contains(item);
        }

        /// <summary>
        ///     Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <exception cref="ArgumentNullException">array</exception>
        public void CopyTo(Point[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            ((ICollection<Point>)this).CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Point> GetEnumerator()
        {
            return vertices.GetEnumerator();
        }

        /// <summary>
        ///     Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(Point item)
        {
            return vertices.Remove(item);
        }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion "Interface impl"

        #region "Custom methods"

        /// <summary>
        ///     Determines whether this instance [can be optimized].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance [can be optimized]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanBeOptimized()
        {
            return !vertices.IsNullOrEmpty();
        }

        /// <summary>
        ///     Checks if optimized.
        /// </summary>
        /// <returns></returns>
        private bool CheckIfOptimized()
        {
            var isNull = edges.IsNullOrEmpty();

            if (isNull || center == default)
                GetCenter();

            return !isNull;
        }

        /// <summary>
        ///     Adds the edge (checking if the edge list is null).
        /// </summary>
        /// <param name="p">The position of the edge.</param>
        /// <param name="origin">The origin of the building.</param>
        /// <param name="first">if set to <c>true</c> [first] sets the edge in first place on the edge list.</param>
        private void AddEdge(Point p, Point origin = default, bool first = false)
        {
            //edges = !edges.IsNullOrEmpty() ? edges : new HashSet<Point>();
            var pp = p;

            if (origin != default) //Esto no esq tenga mucho sentido... (por ahorrarse una resta)
                pp -= origin;

            if (first)
                edges.Insert(0, pp);
            else
                edges.Add(pp);
        }

        /// <summary>
        ///     Loops the edges.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="offsetCorrection">if set to <c>true</c> [offset correction].</param>
        public void LoopEdges(Action<Point> action, bool offsetCorrection = true)
        {
            if (!CheckIfOptimized())
                return;

            for (var i = 0; i < EdgeCount; ++i)
                action?.Invoke(GetEdge(i, offsetCorrection));
        }

        /// <summary>
        ///     Loops the edges.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="offsetCorrection">if set to <c>true</c> [offset correction].</param>
        public void LoopEdges(Action<Point, int> action, bool offsetCorrection = true)
        {
            if (!CheckIfOptimized())
                return;

            for (var i = 0; i < EdgeCount; ++i)
                action?.Invoke(GetEdge(i, offsetCorrection), i);
        }

        /// <summary>
        ///     Loops the edges.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function.</param>
        /// <param name="offsetCorrection">if set to <c>true</c> [offset correction].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">func</exception>
        public IEnumerable<T> LoopEdges<T>(Func<Point, int, T> func, bool offsetCorrection = true)
        {
            if (func == null)
                throw new ArgumentException("func");

            if (!CheckIfOptimized())
                yield break;

            for (var i = 0; i < EdgeCount; ++i)
                yield return func(GetEdge(i, offsetCorrection), i);
        }

        /// <summary>
        ///     Optimizes this instance. (This calculates the vertices of the building)
        /// </summary>
        /// <returns>
        ///     Returns the optimized building.
        /// </returns>
        private void Optimize()
        {
            // If the vertices are null or empty this can't be executed
            if (!CanBeOptimized())
                return; // In this case exit

            if (!edges.IsNullOrEmpty())
                return;

            var vs = vertices.Select(x => (Vector2)(x - center)).ToArray();

            var lPoints =
                new SimplifyUtility().Simplify(vs, vs.Length < 30000 ? 1f : 2f); // vertices.Count > 30 ? 1.5f : 1f

            if (orderByPath)
                foreach (var v in vs)
                    AddEdge(lPoints.OrderBy(x => Vector2.Distance(v, x)).First());
            foreach (Point p in lPoints)
                AddEdge(p);
        }

        /// <summary>
        ///     Gets the center of the building.
        /// </summary>
        /// <param name="orderClockwise">if set to <c>true</c> [order clockwise] orders the verticles in a clockwise order.</param>
        /// <returns>
        ///     Returns a instance of this building with the center set.
        /// </returns>
        private void GetCenter()
        {
            // Get center & position of the building

            if (!CanBeOptimized())
            {
                Debug.LogWarning("Vertices from this polygon are null!");
                return;
            }

            if (center == default)
                center = new Point(Mathf.RoundToInt((float)vertices.Average(p => p.x)),
                    Mathf.RoundToInt((float)vertices.Average(p => p.y))); // vertices.GetCenter();

            if (position == default)
                position = new Point(vertices.Min(v => v.x), vertices.Min(v => v.y));

            Optimize();
        }

        /// <summary>
        ///     Gets the segments.
        /// </summary>
        private void GetSegments()
        {
            if (!CanBeOptimized())
            {
                Debug.LogWarning("Poly can't be optimized!");
                return;
            }

            if (!CheckIfOptimized())
            {
                Debug.LogWarning("Poly isn't optimized!");
                GetCenter();
            }

            if (edges.IsNullOrEmpty())
            {
                Debug.LogError("Poly optimization failed!");
                return;
            }

            segments = LoopEdges((e, i) =>
            {
                // Previous index
                var pI = i == 0 ? EdgeCount - 1 : i - 1;

                // Previous edge
                var pE = GetEdge(pI, false);

                return new Segment(this, i, pE, e);
            }).ToArray();

            longestSegment = segments.Max(x => x.Distance);
        }

        #region "Serialization methods"

        /// <summary>
        ///     Called when [serializing method].
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (CanBeOptimized() && CheckIfOptimized())
                GetSegments();

            //_Center = Center;
            //_Edges = Edges;
            //_Segments = Segments;
        }

        /// <summary>
        ///     Called when [deserializing method].
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
        }

        /// <summary>
        ///     Called when [deserialized method].
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (CanBeOptimized() && !CheckIfOptimized())
                GetSegments();

            if (longestSegment == null && !segments.IsNullOrEmpty())
                longestSegment = segments.Max(x => x.Distance);
        }

        #endregion "Serialization methods"

        /// <summary>
        ///     Gets a edge (point) by its index.
        /// </summary>
        /// <param name="e">The index.</param>
        /// <returns>Returns the edge.</returns>
        public Point GetEdgeByIndex(int e)
        {
            return e > 0 && e < edges.Count - 1 ? edges.ElementAt(e) : Point.zero;
        }

        /// <summary>
        ///     Check if the vertice (point) instance exists on the building (Building) instance.
        /// </summary>
        /// <param name="p">The point of the vertice to check.</param>
        /// <returns></returns>
        public bool ExistVertice(Point p)
        {
            return !vertices.IsNullOrEmpty() && vertices.Contains(p);
        }

        /// <summary>
        ///     Ases the array.
        /// </summary>
        /// <returns></returns>
        public Point[] GetPointArray()
        {
            return pointArray;
        }

        /// <summary>
        ///     Ases the vec array.
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetPointVecArray()
        {
            return pointVecArray;
        }

        /// <summary>
        ///     Ases the array.
        /// </summary>
        /// <returns></returns>
        public Point[] GetEdgeArray()
        {
            return edgeArray;
        }

        /// <summary>
        ///     Ases the vec array.
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetEdgeVecArray()
        {
            return edgeVecArray;
        }

        /// <summary>
        ///     Gets the edge.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="correction">if set to <c>true</c> [correction].</param>
        /// <returns></returns>
        public Point GetEdge(int index, bool correction = true)
        {
            return edgeArray[index] + (correction ? offset : Point.zero);
        }

        /// <summary>
        ///     Ases the biome polygon.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public BiomePolygon AsBiomePolygon(GroundType? type)
        {
            var bPoly = this.FromBaseClassToDerivedClass<BiomePolygon>();

            bPoly.Type = type;

            return bPoly;
        }

        #endregion "Custom methods"
    }
}