using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    public enum PathfindResult
    {
        SUCCESS,
        NOT_FOUND
    }

    public struct Pathfinder2DResult
    {
        public PathfindResult result;
        public float weightedDistance;
        public int distance;
    }

    /// <summary>
    /// Represents a node in the pathfinding system, containing position, cost values, and parent references.
    /// </summary>
    public class Node : ClassObject, IHeapItem<Node>, IArgs<Vector2Int, float, float>
    {
        int _index;

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public Vector2Int Coord;
        public float FactDistanceFromStart;
        public float EmpiricalDistanceToEnd;
        public Node Parent;

        public override void resetProperty()
        {
            base.resetProperty();
            _index = 0;
            Coord = Vector2Int.zero;
            FactDistanceFromStart = 0;
            EmpiricalDistanceToEnd = 0;
            Parent = null;
        }

        public float GetTotalDistance()
        {
            return FactDistanceFromStart + EmpiricalDistanceToEnd;
        }

        public int CompareTo(Node other)
        {
            return -(GetTotalDistance().CompareTo(other.GetTotalDistance()));
        }

        public void onCreate(Vector2Int coord, float factDistanceFromStart, float empiricalDistanceToEnd)
        {
            Coord = coord;
            FactDistanceFromStart = factDistanceFromStart;
            EmpiricalDistanceToEnd = empiricalDistanceToEnd;
        }
    }


    /// <summary>
    /// A pathfinding system for a 2D grid-based environment using a weighted graph approach.
    /// </summary>
    public class Pathfinder2D
    {
        protected Dictionary<Vector2Int, float> _weightedMap;
        protected List<Vector2Int> _movementBlockers;
        protected Heap<Node> opened;

        /// <summary>
        /// Initializes a new instance of the Pathfinder2D class with a weighted map, movement blockers, and a connection type.
        /// </summary>
        /// <param name="weightedMap">Dictionary mapping grid positions to movement costs.</param>
        /// <param name="movementBlockers">List of grid positions that block movement.</param>
        /// <param name="nodeConnectionType">Defines how nodes are connected (e.g., with or without diagonals).</param>
        public Pathfinder2D(Dictionary<Vector2Int, float> weightedMap, List<Vector2Int> movementBlockers)
        {
            _weightedMap = weightedMap;
            _movementBlockers = movementBlockers;
            opened = new(_weightedMap.Count);
        }

        /// <summary>
        /// Initializes a new instance of the Pathfinder2D class with a weighted map and connection type, without movement blockers.
        /// </summary>
        /// <param name="weightedMap">Dictionary mapping grid positions to movement costs.</param>
        /// <param name="nodeConnectionType">Defines how nodes are connected (e.g., with or without diagonals).</param>
        public Pathfinder2D(Dictionary<Vector2Int, float> weightedMap)
        {
            _weightedMap = weightedMap;
            _movementBlockers = new();
            opened = new(_weightedMap.Count);
        }

        /// <summary>
        /// Finds the shortest path from the start position to the end position using a weighted pathfinding algorithm.
        /// </summary>
        /// <param name="start">The starting grid position.</param>
        /// <param name="end">The target grid position.</param>
        /// <returns>
        /// A list of grid positions representing the path from start to end. Returns an empty list if no valid path exists.
        /// </returns>
        public bool FindPath(Vector2Int start, Vector2Int end, ref List<Vector2Int> path, out Pathfinder2DResult result)
        {
            path.Clear();
            result = new Pathfinder2DResult();
            if (start == end)
            {
                result.result = PathfindResult.SUCCESS;
                result.weightedDistance = 0;
                result.distance = 0;
                return true;
            }

            if (!(IsEligibleNode(start) & IsEligibleNode(end)))
            {
                result.result = PathfindResult.NOT_FOUND;
                result.weightedDistance = 0;
                result.distance = 0;
                return false;
            }

            opened.Clear();
            using var a = new HashSetScope<Node>(out var closed);
            using var b = new DicScope<Vector2Int, Node>(out var nodeMap);
            CLASS<Node>(out var endNode).with(end, float.MaxValue, 0.0f);
            CLASS<Node>(out var startNode).with(start, 0.0f, GetEmpiricalDistanceToEndPoint(start, end));
            nodeMap[start] = startNode;
            nodeMap[end] = endNode;
            opened.Add(startNode);

            while (opened.Count > 0)
            {
                var curNode = opened.Pop();
                closed.Add(curNode);
                if (curNode == endNode)
                    break;

                foreach (var coord in GetNeighbours(curNode.Coord))
                {
                    if (!nodeMap.ContainsKey(coord))
                    {
                        CLASS<Node>(out var newNode).with(coord, float.MaxValue, GetEmpiricalDistanceToEndPoint(coord, end));
                        nodeMap.Add(coord, newNode);
                    }

                    var connectedNode = nodeMap[coord];
                    if (!closed.Contains(connectedNode))
                    {
                        if (!opened.Contains(connectedNode))
                        {
                            opened.Add(connectedNode);
                        }

                        float newDistance = curNode.FactDistanceFromStart + _weightedMap[coord];
                        if (newDistance < connectedNode.FactDistanceFromStart)
                        {
                            connectedNode.FactDistanceFromStart = newDistance;
                            connectedNode.Parent = curNode;
                            opened.Update(connectedNode);
                        }
                    }
                }
            }

            var curPathNode = endNode;
            result.result = PathfindResult.SUCCESS;
            result.weightedDistance = endNode.GetTotalDistance();
            result.distance = 0;
            while (curPathNode.Coord != start)
            {
                path.Add(curPathNode.Coord);
                result.distance += 1;
                if (curPathNode.Parent)
                {
                    curPathNode = curPathNode.Parent;
                }
                else
                {
                    result.result = PathfindResult.NOT_FOUND;
                    return false;
                }
            }

            if (result.result == PathfindResult.NOT_FOUND)
            {
                path.Clear();
                return false;
            }

            return true;
        }

        float GetEmpiricalDistanceToEndPoint(Vector2Int node, Vector2Int end)
        {
            return Mathf.Pow(Mathf.Pow(node.x - end.x, 2) + Mathf.Pow(node.y - end.y, 2), 0.5f);
        }

        bool IsEligibleNode(Vector2Int node)
        {
            return (_weightedMap.ContainsKey(node)) & (!_movementBlockers.Contains(node));
        }

        List<Vector2Int> GetNeighbours(Vector2Int c)
        {
            List<Vector2Int> nodes = new();
            nodes.Add(new(c.x + 1, c.y));
            nodes.Add(new(c.x - 1, c.y));
            nodes.Add(new(c.x, c.y + 1));
            nodes.Add(new(c.x, c.y - 1));

            List<Vector2Int> result = new();
            foreach (Vector2Int vec in nodes)
            {
                if (IsEligibleNode(vec))
                {
                    result.Add(vec);
                }
            }

            return result;
        }
    }
}