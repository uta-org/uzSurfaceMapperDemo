using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using RoadArchitect;
using RoadArchitect.Roads;
using UnityEngine;
using uzSurfaceMapper.Core.Generators;
using uzSurfaceMapper.Extensions.Demo;
using uzSurfaceMapper.Model;

//using uzLib.Lite.ExternalCode.Extensions;

// ReSharper disable ConvertToLambdaExpression

namespace uzSurfaceMapper.Utils.Generators
{
    public static class RoadGeneratorUtil
    {
        [CanBeNull]
        private static RoadSystem System { get; }

        public static Dictionary<Chunk, List<Road>> Roads { get; } = new Dictionary<Chunk, List<Road>>();

        static RoadGeneratorUtil()
        {
            try
            {
                System = GameObject.Find("RoadGenerator")?.GetComponent<RoadSystem>();
                if (System == null) return;
                System.isAllowingRoadUpdates = false;
                System.isMultithreaded = false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        //public class RoadComposer : List<Vector3>
        //{
        //}

        public static IEnumerable<Road> CreateRoad(this IEnumerable<Point> points, StringBuilder builder)
        {
            if (points.IsNullOrEmpty())
            {
                builder?.AppendLine($"\tskipped...");
                yield break; // This chunk doesn't contain any road. Exiting.
            }

            //var dict = points.Select(p => new {Index = p.GetKey(), Point = p}).ToDictionary(x => x.Index, x => x.Point);
            //var builder = new StringBuilder();
            var roads = GetRoads(points, builder);
            foreach (var list in roads)
            {
                if (list.IsNullOrEmpty()) continue;
                //var first = road.First();
                //var backIndex = ((Point)CityGenerator.SConv.GetRealPositionOnMap(first)).GetKey();

                var road = CreateIndependantRoad(list);
                if (road == null) continue;
                builder?.AppendLine($"\t... finished road ({road.name}) with {list.Count} nodes.");
                yield return road;
            }
            //Debug.Log(builder?.ToString());
        }

        private static IEnumerable<List<Vector3>> GetRoads(IEnumerable<Point> points, StringBuilder builder)
        {
            var model = RoadGenerator.RoadModel;

            var queue = new Queue<Point>(points);
            int i = 0;

            builder?.AppendLine($"\tcount: {queue.Count}");

            var dictionary = new Dictionary<int, List<Vector3>>();

            while (queue.Count > 0)
            {
                var list = new List<Vector3>();

                var pt = queue.Dequeue();
                var itemIndex = pt.GetKey();
                dictionary.Add(itemIndex, list);

                lock (model.SimplifiedRoadNodes)
                {
                    var node = model.SimplifiedRoadNodes[itemIndex];

                    builder?.AppendLine($"\troad iteration: {i}");

                    //var conn = node.Connections;
                    var nodes = GetRoadNodes(node, ptVal =>
                        {
                            if (ptVal.HasValue) queue = new Queue<Point>(queue.Remove(ptVal.Value));
                            return queue;
                        },
                        parentNodeIndex => { return dictionary[parentNodeIndex]; },
                        builder);

                    foreach (var point in nodes)
                        list.Add(CityGenerator.SConv.GetRealPositionOnMap((Vector2)point).GetHeightForPoint());

                    yield return list;
                    ++i;
                }
            }
        }

        private static IEnumerable<Point> GetRoadNodes(RoadNode node, Func<Point?, Queue<Point>> queueFunc, Func<int, List<Vector3>> parentFunc, StringBuilder builder, int level = -1)
        {
            if (queueFunc == null) throw new ArgumentNullException(nameof(queueFunc));
            if (parentFunc == null) throw new ArgumentNullException(nameof(parentFunc));

            //lock (node.Connections) // TODO?

            var conn = node.Connections;
            if (conn.IsNullOrEmpty())
            {
                yield return node.Position;
                yield break;
            }

            if (queueFunc(null).Count == 0) yield break;

            ++level;
            builder?.AppendLine($"{new string('\t', 2)}level: {level} -> {queueFunc(null).Count} items");

            //if (conn.Count == 1)
            //{
            //    var firstNode = conn.First().GetNode();
            //    ////var firstPoint = conn.First().GetPoint();

            //    var list = parentFunc(firstNode.ParentNodes.First()); // TODO: parent nodes should be one...
            //    list.Add(CityGenerator.SConv.GetRealPositionOnMap((Vector2)conn.First().GetPoint()).GetHeightForPoint());
            //}
            //else
            {
                foreach (var item in conn)
                {
                    var pt = item.GetPoint();
                    if (!queueFunc(null).Contains(pt)) yield break;
                    yield return pt;
                    if (queueFunc(pt).Count == 0) yield break;

                    var subnode = pt.GetKey().GetNode();
                    var pts = GetRoadNodes(subnode, queueFunc, parentFunc, builder, level);
                    foreach (var point in pts)
                        yield return point;
                }
            }
        }

        internal static Point GetPoint(this int index)
        {
            return index.GetNode().Position;
        }

        private static RoadNode GetNode(this int index)
        {
            lock (RoadGenerator.RoadModel.SimplifiedRoadNodes)
                return RoadGenerator.RoadModel.SimplifiedRoadNodes[index];
        }

        public static Road CreateIndependantRoad(this List<Vector3> points)
        {
            if (System == null)
            {
                Debug.LogWarning("Null system.");
                return null;
            }

            // TODO: ref?
            var road = RoadAutomation.CreateRoadProgrammatically(System, ref points);
            System.isAllowingRoadUpdates = true;
            {
                road.UpdateRoad();
            }
            System.isAllowingRoadUpdates = false;
            //Roads.Add(road);
            return road;
        }
    }
}