using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using APIScripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using uzSurfaceMapper.Model.Enums;
using uzSurfaceMapper.Core.Generators;
using uzSurfaceMapper.Extensions;
using uzSurfaceMapper.Utils.Simplification;
using static uzSurfaceMapper.Core.Generators.MapGenerator;
using Color = uzSurfaceMapper.Model.Color;
using UColor = UnityEngine.Color;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace uzSurfaceMapper.Model
{
    public class RoadModel : IProgress<float>
    {
        [JsonIgnore] public static UColor[] Colors { get; set; }
        public ConcurrentHashSet<RoadNode> RoadNodes { get; } = new ConcurrentHashSet<RoadNode>();
        public ConcurrentDictionary<int, RoadNode> SimplifiedRoadNodes { get; set; }
        public HashSet<Point> IntersectionNodes { get; } = new HashSet<Point>();
        public HashSet<Point> SimplifiedIntersectionNodes { get; private set; }

        [JsonIgnore] public bool Optimized => SimplifiedRoadNodes != null;

        public bool AreNodesConnected { get; set; }

        public static int DistanceSkipped { get; private set; }
        public static int SamePoint { get; private set; }
        public static int Finish { get; private set; }
        public static int ValidConnections { get; private set; }
        public static int NotValidConnections { get; private set; }
        public static int OutOfIndexErrors { get; private set; }
        public static int OnSameLine { get; private set; }

        public static bool PrintOutOfIndexErrors { get; set; }
        public static bool TreatOutOfIndexErrorsAsValid { get; set; } = true;

        public void Optimize()
        {
            if (Optimized) return;

            Debug.Log($"Optimizing nodes ({RoadNodes.Count}) for roads.");

            var vertices = RoadNodes.Select(x => (Vector2)x.Position).ToArray();

            var nodes =
                new SimplifyUtility().Simplify(vertices, 1.5f).Select(x => (Point)x);

            //Debug.Log(nodes.Count());

            // Map for road nodes
            var dictionary = RoadNodes
                .Select(node => new
                { Index = node.Position.GetKey(), Node = node })
                .SafeToDictionary(x => x.Index, x => x.Node);

            SimplifiedRoadNodes = new ConcurrentDictionary<int, RoadNode>(nodes
                .Select(p => new { Index = p.GetKey(), Value = dictionary[p.GetKey()] })
                .ToDictionary(x => x.Index, x => x.Value));

            // nodes.Select(p => new { Index =  dictionary[F.P(p.x, p.y, mapWidth, mapHeight)])};
            //SimplifiedRoadNodes = new HashSet<RoadNode>(nodes.Select(p => dictionary[F.P(p.x, p.y, mapWidth, mapHeight)]));

            Debug.Log($"Optimized roads nodes resulting in {SimplifiedRoadNodes.Count} items.");
            // TODO: Remove near nodes?

            //SimplifiedRoadNodes = new HashSet<RoadNode>(nodes.Select(p => new RoadNode(p, -1)));
            // dictionary[F.P(p.x, p.y, mapWidth, mapHeight)]

            // TODO: this doesn't work
            //Debug.Log($"Optimizing nodes ({IntersectionNodes.Count}) for intersections.");

            //SimplifiedIntersectionNodes = new HashSet<Point>(IntersectionNodes);
            //var list = SimplifiedIntersectionNodes.ToList();

            //foreach (var intersectionNode in list)
            //{
            //    foreach (var point in list)
            //    {
            //        var distance = point.Distance2(intersectionNode);
            //        if (distance > 25) continue;
            //        SimplifiedIntersectionNodes.Remove(point); // TODO: Is this enough optimized?
            //    }
            //}
        }

        public void ConnectNodes()
        {
            if (!Optimized) throw new Exception("Can't connect nodes without optimizing them first.");
            if (AreNodesConnected) return;

            // ReSharper disable once RedundantCast
            var roadNodes = SimplifiedRoadNodes;
            //var colors = RoadGenerator.StaticSource;
            var distance = CityGenerator.SConv.SinglePlaneSize * 15f; // 1.5 * 10

            // Mathf.Pow(CityGenerator.SConv.SinglePlaneSize * 15f, 2); // To compare valid nodes, we must set a limit to operate
            float count = roadNodes.Count;
            var length = Colors.Length;

            Debug.Log($"Connecting nodes ({count:F0}) with distance = {distance} ({CityGenerator.SConv.SinglePlaneSize})");

            foreach (var pair in SimplifiedRoadNodes)
            {
                //var returnNode = new RoadNode(node.Position, node.Thickness);

                var node = pair.Value;
                var p1 = node.Position;

                // Compute distance to all nodes...
                if (IsStopped) return;
                var distances = roadNodes
                    .Where(n => n.Value != node)
                    .Select(n => new { Dictionary = n, Distance = Vector2.Distance(CityGenerator.SConv.GetRealPositionOnMap((Vector2)n.Value.Position), CityGenerator.SConv.GetRealPositionOnMap((Vector2)node.Position)) })
                    .OrderBy(n => n.Distance);

                foreach (var n in distances)
                {
                    if (n.Distance > distance)
                    {
                        // skipped due to distance
                        DistanceSkipped++;
                        continue;
                    }

                    var p2 = n.Dictionary.Value.Position;
                    if (p1 == p2)
                    {
                        ++SamePoint;
                        continue; // skip if same point
                    }

                    var isValid = Colors.DrawLine(p1, p2, (x, y) =>
                    {
                        var index = F.P(x, y, mapWidth, mapHeight);

                        if (SimplifiedRoadNodes.ContainsKey(index) && node.Connections?.Contains(index) == true)
                        {
                            ++OnSameLine;
                            return false;
                        }

                        if (index < 0 || index >= length)
                        {
                            if (PrintOutOfIndexErrors) Debug.LogError($"Out of index in ({x}, {y}) -> {index}");
                            ++OutOfIndexErrors;
                            return TreatOutOfIndexErrorsAsValid;
                        }

                        return Colors[index].AsComponentColor() == GroundType.Asphalt.GetColor();
                    });

                    if (isValid)
                    {
                        if (node.Connections == null) node.Connections = new ConcurrentBag<int>();

                        var val = n.Dictionary.Value;
                        var ind = val.GetKey();

                        node.Connections.Add(ind);

                        if (SimplifiedRoadNodes[ind].ParentNodes == null) SimplifiedRoadNodes[ind].ParentNodes = new ConcurrentBag<int>();
                        SimplifiedRoadNodes[ind].ParentNodes.Add(node.GetKey()); // Add parents back to this node

                        ValidConnections++;
                    }
                    else
                        NotValidConnections++;
                }

                //yield return returnNode;

                Finish++;
                Report(++CurrentStep / count);
            }

            AreNodesConnected = true;
        }

        public void Report(float value)
        {
            Progress = value;
        }

        [JsonIgnore] public float Progress { get; private set; }
        private int CurrentStep { get; set; }

        [JsonIgnore]
        public bool IsStopped { get; private set; }

        public void Stop()
        {
            IsStopped = true;
        }
    }
}