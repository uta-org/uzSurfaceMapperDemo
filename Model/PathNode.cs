using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using UnityEngine;
using VoronoiLib.Structures;

namespace uzSurfaceMapper.Model
{
    [Serializable]
    public class PathNode
    {
        public Point Position { get; set; }

        // This brokes JSON and the graph law
        //public PathNode ParentNode { get; set; }
        public HashSet<PathNode> Neighbors { get; set; }

        public int X
        {
            get => Position.x;
            set
            {
                var position = Position;
                position.x = value;
            }
        }

        public int Z
        {
            get => Position.y;
            set
            {
                var position = Position;
                position.y = value;
            }
        }

        public float GScore { get; set; }
        public float FScore { get; set; }
        public int HScore { get; set; }

        //public bool IsBlocked { get; set; }
        //public bool IsStart { get; set; }
        //public bool IsFinish { get; set; }

        //[JsonIgnore] public bool Delete { get; set; }

        /* Contructors */

        public PathNode()
        {
        }

        public PathNode(FortuneSite site)
        {
            Position = new Point((int)site.X, (int)site.Y);
        }

        public PathNode(int x, int z)
        {
            GScore = 1;
            X = x;
            Z = z;
        }

        /* Methods */

        public Vector3 CurrentPosition()
        {
            return new Vector3(X, 0, Z);
        }

        public float GetDistance(PathNode otherNode)
        {
            return Vector3.Distance(otherNode.CurrentPosition(), CurrentPosition());
        }

        public void FindNeighbors(PathNode[,] grid, bool force = false)
        {
            //Neighbors = new List<PathNode>();
            if (!Neighbors.IsNullOrEmpty() && !force)
            {
                Debug.LogWarning("This node has already neighbors, use force mode instead.");
                return;
            }

            // Top Middle

            try
            {
                Neighbors.Add(grid[X - 1, Z]);
            }
            catch { }

            // Bottom Middle

            try
            {
                Neighbors.Add(grid[X + 1, Z]);
            }
            catch { }

            // Middle Left

            try
            {
                Neighbors.Add(grid[X, Z + 1]);
            }
            catch { }

            // Middle Right

            try
            {
                Neighbors.Add(grid[X, Z - 1]);
            }
            catch { }
        }

        //public static bool operator ==(PathNode p1, PathNode p2)
        //{
        //    //if(p1?.Equals(null) && p2?.Equals(null))
        //    return p1.Position == p2.Position;

        //    //try
        //    //{
        //    //    return p1.Position == p2.Position;
        //    //}
        //    //catch
        //    //{
        //    //    return false;
        //    //}
        //}

        //public static bool operator !=(PathNode p1, PathNode p2)
        //{
        //    try
        //    {
        //        return p1.Position != p2.Position;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public override string ToString()
        {
            return $"PathNode{{Position={Position},Neighbors={Neighbors.Count}}}";
        }
    }
}