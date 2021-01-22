using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Extensions;
using uzSurfaceMapper.Core;
using uzSurfaceMapper.Model.Enums;
using uzSurfaceMapper.Core.Generators;
using uzSurfaceMapper.Extensions;
using uzSurfaceMapper.Utils.Generators;
using uzSurfaceMapper.Utils.Simplification;
using static uzSurfaceMapper.Core.Generators.MapGenerator;
using F = uzSurfaceMapper.Extensions.F;
using UColor = UnityEngine.Color;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace uzSurfaceMapper.Model
{
    [Serializable]
    public class RoadModel : IProgress<float>
    {
        public static List<Tuple<Point, Point>> Lines { get; } = new List<Tuple<Point, Point>>();

        //public bool UseBuilderForTesting { get; internal set; }
        private StringBuilder Builder { get; set; }

        [JsonIgnore] public static UColor[] Colors { get; set; }
        public HashSet<RoadNode> RoadNodes { get; } = new HashSet<RoadNode>();
        public Dictionary<int, RoadNode> SimplifiedRoadNodes { get; set; }
        public HashSet<Point> IntersectionNodes { get; } = new HashSet<Point>();
        public HashSet<Point> SimplifiedIntersectionNodes { get; private set; }

        [JsonIgnore]
        public bool Optimized
        {
            get
            {
                try
                {
                    lock (SimplifiedRoadNodes)
                        return SimplifiedRoadNodes != null;
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

            //lock (SimplifiedRoadNodes) // lock doesn't make sense here
            {
                SimplifiedRoadNodes = new Dictionary<int, RoadNode>(nodes
                    .Select(p => new { Index = p.GetKey(), Value = dictionary[p.GetKey()] })
                    .ToDictionary(x => x.Index, x => x.Value));

                // nodes.Select(p => new { Index =  dictionary[F.P(p.x, p.y, mapWidth, mapHeight)])};
                //SimplifiedRoadNodes = new HashSet<RoadNode>(nodes.Select(p => dictionary[F.P(p.x, p.y, mapWidth, mapHeight)]));

                Debug.Log($"Optimized roads nodes resulting in {SimplifiedRoadNodes.Count} items.");
            }
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
            ConnectNodes(false);
        }

        public void ConnectNodes(bool useBuilderForTesting)
        {
            if (!Optimized) throw new Exception("Can't connect nodes without optimizing them first.");
            if (AreNodesConnected) return;

            lock (SimplifiedRoadNodes)
            {
                // ReSharper disable once RedundantCast
                var roadNodes = SimplifiedRoadNodes;
                //var colors = RoadGenerator.StaticSource;
                var distance = (CityGenerator.SConv.SinglePlaneSize * 15f) * 20f; // 1.5 * 10

                // Mathf.Pow(CityGenerator.SConv.SinglePlaneSize * 15f, 2); // To compare valid nodes, we must set a limit to operate
                float count = roadNodes.Count;
                TotalSteps = (int)count;
                var length = Colors.Length;

                Debug.Log(
                    $"Connecting nodes ({count:F0}) with distance = {distance} ({CityGenerator.SConv.SinglePlaneSize})");

                foreach (var pair in SimplifiedRoadNodes)
                {
                    //var returnNode = new RoadNode(node.Position, node.Thickness);

                    var node = pair.Value;
                    var p1 = node.Position;

                    // Compute distance to all nodes...
                    if (IsStopped) return;
                    var distances = roadNodes
                        .Where(n => n.Value != node)
                        .Select(n => new
                        {
                            Dictionary = n,
                            Distance = Vector2.Distance(
                                CityGenerator.SConv.GetRealPositionOnMap((Vector2)n.Value.Position),
                                CityGenerator.SConv.GetRealPositionOnMap((Vector2)node.Position)),
                            PointDistance = Vector2.Distance(n.Value.Position, node.Position)
                        })
                        .OrderBy(n => n.Distance);

                    foreach (var n in distances)
                    {
                        if (n.Distance > distance)
                        {
                            // skipped due to distance
                            DistanceSkippedHigh++;
                            continue;
                        }

                        if (n.PointDistance < 5)
                        {
                            DistanceSkippedLow++;
                            continue;
                        }

                        var p2 = n.Dictionary.Value.Position;
                        if (p1 == p2)
                        {
                            ++SamePoint;
                            continue; // skip if same point
                        }

                        // TODO ?
                        //lock (node.Connections)
                        //    lock (node.ParentNodes)
                        {
                            if (useBuilderForTesting)
                            {
                                Builder = new StringBuilder($"Creating line from {p1} to {p2}");
                                Builder.AppendLine();

                                Lines.Add(new Tuple<Point, Point>(p1, p2));
                            }

                            var isValid = Colors.DrawLine(p1, p2, (x, y) =>
                            {
                                var index = F.P(x, y, mapWidth, mapHeight);

                                if (useBuilderForTesting)
                                    Builder.AppendLine($"{index} ({x}, {y}) = {Colors[index]}");

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

                                var color = Colors[index].AsComponentColor();
                                return color == GroundType.Asphalt.GetColor()
                                       || color == GroundType.Asphalt2.GetColor()
                                       || color == GroundType.Asphalt3.GetColor();
                            });

                            if (useBuilderForTesting)
                                Debug.Log(Builder.ToString());

                            if (isValid)
                            {
                                if (node.Connections == null) node.Connections = new HashSet<int>();

                                var val = n.Dictionary.Value;
                                var ind = val.GetKey();

                                node.Connections.Add(ind);

                                if (SimplifiedRoadNodes[ind].ParentNodes == null)
                                    SimplifiedRoadNodes[ind].ParentNodes = new List<int>();
                                SimplifiedRoadNodes[ind].ParentNodes
                                    .Add(node.GetKey()); // Add parents back to this node

                                ValidConnections++;
                            }
                            else
                                NotValidConnections++;
                        }
                    }

                    //yield return returnNode;

                    Finish++;
                    Report(++CurrentStep / count);
                }
            }

            AreNodesConnected = true;
        }

        public void RemoveSimilarConnections()
        {
            if (!Optimized) throw new Exception("Can't connect nodes without optimizing them first.");
            if (!AreNodesConnected) throw new Exception("Can't optimize node connection without connecting them first.");
            if (AreNodesRemoved) return;

            TotalSteps = SimplifiedRoadNodes.Count;
            CurrentStep = 0;
            Status = "Deleting unneeded connections";

            Debug.Log("Started similarity check");

            try
            {
                TotalValidations =
                    SimplifiedRoadNodes.Sum(x => (x.Value?.Connections?.Count ?? 0) * (x.Value?.Connections?.Count ?? 0));

                foreach (var nodeKV in SimplifiedRoadNodes)
                {
                    var node = nodeKV.Value;
                    var i = 0;

                    if (nodeKV.Value?.Connections == null)
                    {
                        Report(++CurrentStep / (float)TotalSteps);
                        continue;
                    }

                    var similarity = node.Connections.Select(x => new
                    {
                        Line = new Line(GeometryHelper
                            .DrawLineAsEnumerable(x.GetPoint(), node.Position)
                            .Select(p => (Vector2)p)
                            .ToArray(), x.GetPoint(), node.Position),
                        Key = x
                    });

                    node.Connections = new HashSet<int>();

                    //SimplifiedRoadNodes[ind].ParentNodes
                    //    .Add(node.GetKey());

                    //var ind = nodeKV.Key.GetKey();
                    //SimplifiedRoadNodes[ind].ParentNodes = new List<int>(); // TODO

                    bool isReset = false;
                    bool skip = false;
                    foreach (var item in similarity)
                    {
                        var distances = similarity.Select(x => new
                        {
                            SimilarNode = x,
                            Distance = GeometryHelper.DifferenceBetweenLines(item.Line.Points, x.Line.Points)
                        })
                            .OrderBy(x => x.Distance);

                        foreach (var subitem in distances)
                        {
                            if (item.Line == subitem.SimilarNode.Line)
                            {
                                ++SkippedLines;
                                continue;
                            }

                            // TODO: Order by distance

                            var ind = item.Line.Origin.GetKey();

                            //builder.AppendLine($"A: {item.Line} | B: {subitem.Line} || Similarity = {GeometryHelper.DifferenceBetweenLines(item.Line.Points, subitem.Line.Points):F2}");
                            if (subitem.Distance >= 10)
                            {
                                //if (!isReset)
                                //{
                                //    SimplifiedRoadNodes[ind].ParentNodes = new List<int>();
                                //    isReset = true;
                                //}

                                //node.Connections.Add(ind);
                                //SimplifiedRoadNodes[ind].ParentNodes.Add(node.GetKey());

                                ++ValidatedLines;
                            }
                            else
                            {
                                var key = subitem.SimilarNode.Key;
                                node.Connections.Remove(key);
                                SimplifiedRoadNodes[ind].ParentNodes.Remove(node.GetKey());

                                ++OnSimilarLine;
                            }
                        }
                    }

                    Report(++CurrentStep / (float)TotalSteps);
                    ++Finish;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            Debug.Log("Finished similarity checks");

            AreNodesRemoved = true;
        }

        public IEnumerator ConnectNodesAsEnumerator(bool reset, bool force = false, int stepsToSkip = 0)
        {
            return ConnectNodesAsEnumerator(reset, false, force, stepsToSkip);
        }

        public IEnumerator ConnectNodesAsEnumerator(bool reset, bool useBuilderForTesting, bool force = false, int stepsToSkip = 0)
        {
            if (!Optimized) throw new Exception("Can't connect nodes without optimizing them first.");
            if (AreNodesConnected && !force) yield break;

            // ReSharper disable once RedundantCast
            var roadNodes = SimplifiedRoadNodes;
            var roadNodesAsEnumerable = SimplifiedRoadNodes.AsEnumerable();
            //var colors = RoadGenerator.StaticSource;
            var distance = CityGenerator.SConv.SinglePlaneSize * 15f; // 1.5 * 10

            // Mathf.Pow(CityGenerator.SConv.SinglePlaneSize * 15f, 2); // To compare valid nodes, we must set a limit to operate
            float count = roadNodes.Count;
            TotalSteps = (int)count;
            var length = Colors.Length;

            Debug.Log(
                $"Connecting nodes ({count:F0}) with distance = {distance} ({CityGenerator.SConv.SinglePlaneSize})");

            if (stepsToSkip > 0)
            {
                roadNodesAsEnumerable = roadNodesAsEnumerable?.Skip(stepsToSkip);

                if (roadNodesAsEnumerable?.IsNullOrEmpty() == true)
                {
                    Debug.LogWarning("Skipped more steps than allowed...");
                    yield break;
                }
            }

            foreach (var pair in roadNodesAsEnumerable)
            {
                //var returnNode = new RoadNode(node.Position, node.Thickness);

                var node = pair.Value;
                var p1 = node.Position;

                // Compute distance to all nodes...
                if (IsStopped) yield break;
                var distances = roadNodes
                    .Where(n => n.Value != node)
                    .Select(n => new
                    {
                        Dictionary = n,
                        Distance = Vector2.Distance(
                            CityGenerator.SConv.GetRealPositionOnMap((Vector2)n.Value.Position),
                            CityGenerator.SConv.GetRealPositionOnMap((Vector2)node.Position)),
                        PointDistance = Vector2.Distance(n.Value.Position, node.Position)
                    })
                    .OrderBy(n => n.Distance);

                foreach (var n in distances)
                {
                    if (n.Distance > distance)
                    {
                        // skipped due to distance
                        DistanceSkippedHigh++;
                        continue;
                    }

                    if (n.PointDistance < 5)
                    {
                        DistanceSkippedLow++;
                        continue;
                    }

                    var p2 = n.Dictionary.Value.Position;
                    if (p1 == p2)
                    {
                        ++SamePoint;
                        continue; // skip if same point
                    }

                    if (useBuilderForTesting)
                    {
                        Builder = new StringBuilder($"Creating line from {p1} to {p2}");
                        Builder.AppendLine();
                        Builder.AppendLine("Lambda information:");
                        Builder.AppendLine($"Dictionary: ({n.Dictionary.ToDetailedString()})");
                        Builder.AppendLine(
                            $"Distance: {n.Distance} (from: {CityGenerator.SConv.GetRealPositionOnMap((Vector2)n.Dictionary.Value.Position)} to: {CityGenerator.SConv.GetRealPositionOnMap((Vector2)node.Position)})");
                        Builder.AppendLine(
                            $"Point Distance: {n.PointDistance} (from: {n.Dictionary.Value.Position} to: {node.Position})");
                        Builder.AppendLine();
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

                        if (useBuilderForTesting)
                            Builder.AppendLine(
                                $"{index} ({x}, {y}) = {Colors[index]} ({((Color)Colors[index]).GetGroundTypeData()})");

                        return Colors[index].AsComponentColor() == GroundType.Asphalt.GetColor();
                    });

                    if (useBuilderForTesting && Builder.GetLineCount() > 10 && isValid) //  && node.Connections.Count > 30)
                    {
                        var builder = Builder.ToString();
                        Debug.Log(builder);
                        Debug.Break();
                    }

                    if (isValid)
                    {
                        if (node.Connections == null) node.Connections = new HashSet<int>();

                        var val = n.Dictionary.Value;
                        var ind = val.GetKey();

                        node.Connections.Add(ind);

                        if (SimplifiedRoadNodes[ind].ParentNodes == null)
                            SimplifiedRoadNodes[ind].ParentNodes = new List<int>();
                        SimplifiedRoadNodes[ind].ParentNodes
                            .Add(node.GetKey()); // Add parents back to this node

                        ValidConnections++;
                    }
                    else
                        NotValidConnections++;
                }

                yield return null;

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