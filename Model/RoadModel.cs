using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Internal;
using Newtonsoft.Json;
using VoronoiLib.Structures;
using UColor = UnityEngine.Color;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace uzSurfaceMapper.Model
{
    [Serializable]
    public partial class RoadModel : IProgress<float> // Singleton<RoadModel>,
    {
        public static List<Tuple<Point, Point>> Lines { get; } = new List<Tuple<Point, Point>>();

        //public bool UseBuilderForTesting { get; internal set; }
        private StringBuilder Builder { get; set; }

        [JsonIgnore] public static UColor[] Colors { get; set; }
        public HashSet<Point> OuterRoadNodes { get; } = new HashSet<Point>();
        public List<Point> OuterSimplifiedRoadNodes { get; set; }

        public HashSet<RoadNode> InnerRoadNodes { get; } = new HashSet<RoadNode>();
        public List<PathNode> InnerSimplifiedRoadNodes { get; set; }

        //[JsonIgnore]
        //[Obsolete("Must use PathNodes, if not, you must uncomment the assignation of `LinkedNodes = FortunesAlgorithm.Run(...)` in the Link method of this class.")]
        //public LinkedList<VEdge> LinkedNodes { get; set; }

        [JsonIgnore] // This is a midway to create innersimplifiedroadnodes
        public HashSet<PathNode> PathNodes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [keep inner optimization] (this value can be changed manually in the JSON file to force optimization).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [keep inner optimization]; otherwise, <c>false</c>.
        /// </value>
        public bool KeepInnerOptimization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [keep outer optimization] (this value can be changed manually in the JSON file to force optimization).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [keep outer optimization]; otherwise, <c>false</c>.
        /// </value>
        public bool KeepOuterOptimization { get; set; }

        [JsonIgnore]
        public bool OuterOptimized
        {
            get
            {
                // Remove this line if you want Optimize call at every ConnectAtStart
                //if (!KeepOuterOptimization)
                //    return false;

                try
                {
                    lock (OuterSimplifiedRoadNodes)
                        return OuterSimplifiedRoadNodes != null;
                }
                catch
                {
                    // If SimplifiedRoadNodes is null this will be triggered
                    return false;
                }
            }
        }

        [JsonIgnore]
        public bool InnerOptimized
        {
            get
            {
                // Same here
                if (!KeepInnerOptimization)
                    return false;

                try
                {
                    lock (InnerSimplifiedRoadNodes)
                        return InnerSimplifiedRoadNodes != null;
                }
                catch
                {
                    // If SimplifiedRoadNodes is null this will be triggered
                    return false;
                }
            }
        }

        [JsonIgnore]
        public bool Linked
        {
            get
            {
                try
                {
                    lock (PathNodes)
                        return !PathNodes.IsNullOrEmpty(); // != null
                }
                catch
                {
                    // If SimplifiedRoadNodes is null this will be triggered
                    return false;
                }
            }
        }

        public bool AreNodesConnected { get; set; }
        public bool AreNodesRemoved { get; set; }

        public static int DistanceSkippedHigh { get; private set; }
        public static int DistanceSkippedLow { get; private set; }
        public static int SamePoint { get; private set; }
        public static int TotalSteps { get; private set; }
        public static int Finish { get; private set; }
        public static int ValidConnections { get; private set; }
        public static int NotValidConnections { get; private set; }
        public static int OutOfIndexErrors { get; private set; }
        public static int OnSameLine { get; private set; }
        public static int OnSimilarLine { get; private set; }
        public static int ValidatedLines { get; private set; }
        public static int SkippedLines { get; private set; }
        public static int TotalValidations { get; private set; }

        public static bool PrintOutOfIndexErrors { get; set; }
        public static bool TreatOutOfIndexErrorsAsValid { get; set; } = true;

        private int totalConnectionsCheck = -1;

        //[JsonIgnore]
        //public int TotalConnectionsCheck
        //{
        //    get
        //    {
        //        if (totalConnectionsCheck == -1)
        //            totalConnectionsCheck = SimplifiedRoadNodes.Sum(x => (x.Value?.Connections?.Count ?? 0));

        //        return totalConnectionsCheck;
        //    }
        //}

        //private int totalConnectionsSqrCheck = -1;

        //[JsonIgnore]
        //public int TotalConnectionsSqrCheck
        //{
        //    get
        //    {
        //        if (totalConnectionsSqrCheck == -1)
        //            totalConnectionsSqrCheck = SimplifiedRoadNodes.Sum(x => (x.Value?.Connections?.Count ?? 0) * (x.Value?.Connections?.Count ?? 0));

        //        return totalConnectionsSqrCheck;
        //    }
        //}

        public void Report(float value)
        {
            Progress = value;
        }

        [JsonIgnore] public float Progress { get; private set; }
        [JsonIgnore] public string Status { get; private set; } = "Connecting road nodes";
        private int CurrentStep { get; set; }

        [JsonIgnore]
        public bool IsStopped { get; private set; }

        public void Stop()
        {
            IsStopped = true;
        }
    }
}