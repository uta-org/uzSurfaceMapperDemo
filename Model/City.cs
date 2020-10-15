using System;
using System.Collections.Generic;
using System.Linq;
using uzSurfaceMapper.Core.Generators;
using uzSurfaceMapper.Extensions;
using Newtonsoft.Json;
using ProceduralToolkit.Buildings;
using UnityEngine;
using SConvert = uzSurfaceMapper.Core.Func.SceneConversion;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     This is the class that contains all the buildings. (aka Shpaes)
    /// </summary>
    [Serializable]
    public class City
    {
        // TODO: I dont like the use of Instance, in the future maybe I will create a class called regions, and depending at which zone you are, it will load the info of the City instance

        static City()
        {
            if (DefaultBuildConfig == null) DefaultBuildConfig = new BuildingGenerator.Config(); // TODO: Random?
        }

        /// <summary>
        ///     The instance
        /// </summary>
        public static City Instance { get; private set; }

        // => MapGenerator.Instance.city; // TODO: Use this or use ctor?

        public static BuildingGenerator.Config DefaultBuildConfig { get; private set; }

        /// <summary>
        ///     The buildings (buildings)
        /// </summary>
        [JsonProperty("Buildings")] public HashSet<Building> buildings = new HashSet<Building>();

        /// <summary>
        ///     The chunks
        /// </summary>
        [JsonIgnore] public HashSet<Chunk> chunks = new HashSet<Chunk>();

        /// <summary>
        ///     The bridges
        /// </summary>
        [JsonIgnore] public List<Bridge> bridges = new List<Bridge>();

        [JsonIgnore] public List<BuildingGenerator> buildingGenerators = new List<BuildingGenerator>();

        /// <summary>
        ///     The is map plane size set
        /// </summary>
        public static bool IsMapPlaneSizeSet { get; private set; }

        /// <summary>
        ///     The map plane size
        /// </summary>
        private static Vector2 _mapPlaneSize;

        /// <summary>
        ///     The map plane size
        /// </summary>
        public static Vector2 mapPlaneSize
        {
            get => _mapPlaneSize;
            set
            {
                if (_mapPlaneSize == default)
                    IsMapPlaneSizeSet = true;

                _mapPlaneSize = value;
            }
        }

        /// <summary>
        ///     The map plane half size
        /// </summary>
        private static Vector2 _mapPlaneHalfSize;

        /// <summary>
        ///     Gets or sets the size of the map plane half.
        /// </summary>
        /// <value>
        ///     The size of the map plane half.
        /// </value>
        public static Vector2 mapPlaneHalfSize
        {
            get
            {
                if (mapPlaneSize == default)
                    throw new Exception(
                        "You must set mapPlaneSize before trying to get the value from mapPlaneHalfSize!");

                if (_mapPlaneHalfSize == default)
                    _mapPlaneHalfSize = mapPlaneSize / 2;

                return _mapPlaneHalfSize;
            }
        }

        /// <summary>
        ///     Gets or sets the size of the single size.
        /// </summary>
        /// <value>
        ///     The size of the single size.
        /// </value>
        //public static float singlePlaneSize { get; set; }

        private static bool debugOnceFlag { get; set; }

#if !UNITY_EDITOR
        [JsonIgnore]
#endif

        /// <summary>
        ///     Gets the shape count.
        /// </summary>
        /// <value>
        ///     The shape count.
        /// </value>
        public int BuildingCount
        {
            get
            {
                if (buildings != null)
                    return buildings.Count;

                return 0;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="City" /> class.
        /// </summary>
        public City()
        {
            Instance = this;
        }

        /// <summary>
        ///     Adds the building (checking if the building list is null).
        /// </summary>
        /// <param name="bd">The building.</param>
        public void AddBuilding(Building bd)
        {
            buildings = !buildings.IsNullOrEmpty() ? buildings : new HashSet<Building>(); //Aki ai 1 buj

            //bd.Pol.CheckIfOptimized();

            buildings.Add(bd);
        }

        /// <summary>
        ///     Gets the chunk.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="debugStr">The debug string.</param>
        /// <returns></returns>
        public static Chunk GetChunk(int x, int y, float px, float py)
        {
            return GetChunk(x, y, px, py, out _);
        }

        /// <summary>
        ///     Gets the chunk.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="debugStr">The debug string.</param>
        /// <returns></returns>
        public static Chunk GetChunk(int x, int y, float px, float py, out string debugStr)
        {
#if UNITY_EDITOR
            //if (Instance.chunks.Count > 0 && !new Rect(Instance.chunks.Min(c => c.xMin), Instance.chunks.Min(c => c.yMin), Instance.chunks.Max(c => c.xMax), Instance.chunks.Max(c => c.yMax)).Contains(new Vector2(x, y)))
            //    throw new Exception($"The bounding box of the plane doesn't contains ({x}, {y})"); // You have reached the limit...
#endif

            var vector = new Vector2(px, py);
            debugStr = $"Getting chunk from {Instance.chunks.Count} loaded chunks at {vector} ({x}, {y})!\n\nSConv Instance\n{new string('=', 10)}\n{CityGenerator.SConv}\n\n";

            //if (!debugOnceFlag)
            //{
            //    Debug.Log($"Total building count: {Instance.BuildingCount}");
            //    debugOnceFlag = true;
            //}

            if (Instance.chunks.Count == 0)
                // In this case, generate the complete vecindary
                for (var i = -1; i <= 1; ++i)
                    for (var j = -1; j <= 1; ++j)
                    {
                        int _x = x + i,
                            _y = y + j;

                        AddChunk(_x, _y);
                    }

            var chunk = Instance.chunks
                .FirstOrDefault(c => c.r
                    .Contains(vector)) ?? AddChunk(px, py); // , c.xMax <= 0 || c.yMax <= 0

            return chunk;
        }

        private static Chunk AddChunk(float px, float py)
        {
            //var scaledSPZ = GetScaledVector(x, y);

            var sps = CityGenerator.SConv.SinglePlaneSize * 10; // Plane scale is x10 bigger
            float xSingleMinPos = px,
                ySingleMinPos = py,
                xSingleMaxPos = sps,
                ySingleMaxPos = sps;

            var instance = SConvert.Instance;

            // And convert back with the exact precission the position of each chunk
            // Use * if you are using floor, and / if you are using ceil
            Rect rect = new Rect(xSingleMinPos, ySingleMinPos, xSingleMaxPos, ySingleMaxPos),
                // We will get the converted position (if the map is 4,200 units this will obtain the total mapWidth of the texture that is 7,000)
                rectOnMap = instance.ConvertRect(xSingleMinPos, ySingleMinPos, xSingleMaxPos, ySingleMaxPos);

            var chunk = new Chunk(rect);

            if (chunk.listOfIndexBuildings == null)
            {
                var set = SetChunkBuilds(rectOnMap);
                chunk.listOfIndexBuildings = set;

                //Debug.Log($"Set: {set.Count} buildings. (Loaded: {Instance.buildings.Count}) || Rect: {rectOnMap}");
            }

            if (chunk.roadPoints == null)
                chunk.roadPoints = SetChunkRoadPoints(rectOnMap);
            //Debug.Log($"Count of buildings at chunk ({rect.x}, {rect.y}): {_c.listOfBuildings.Count}");

            // We add the chunk
            Instance.chunks.Add(chunk);

            return chunk;
        }

        private static HashSet<int> SetChunkBuilds(Rect rect)
        {
            return new HashSet<int>(Instance.buildings.Where(building => rect.Contains((Vector2)building.Pol.Center))
                .Select(b => b.index));
        }

        private static HashSet<Point> SetChunkRoadPoints(Rect rect)
        {
            //var roadModel = MapGenerator.GetInstance<RoadGenerator>()?.Model;
            var roadModel = RoadGenerator.RoadModel;
            // ((RoadGenerator)RoadGenerator.Instance)?.Model;
            if (roadModel == null) throw new Exception("Can't load Road Model!");
            return new HashSet<Point>(roadModel.SimplifiedRoadNodes.Where(node => rect.Contains((Vector2)node.Value.Position)).Select(n => n.Value.Position));
        }

        /// <summary>
        ///     Gets the center (insegment of the plane).
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetBuildingCenter(Building b)
        {
            return new Vector3(
                CityGenerator.SConv.GetScaleMult(b.Pol.Center.x - MapGenerator.mapWidth / 2, false),
                0,
                CityGenerator.SConv.GetScaleMult(b.Pol.Center.y - MapGenerator.mapHeight / 2, false));
        }
    }
}