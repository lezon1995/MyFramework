using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MoreMountains
{
    public class MapGenerator
    {
        public static List<List<MapRoomNode>> generateDungeon(int height, int width, int pathDensity, Rand rng)
        {
            var map = createNodes(height, width);
            if (ModHelper.isModEnabled("Uncertain Future"))
                map = createPaths(map, 1, rng);
            else
                map = createPaths(map, pathDensity, rng);

            map = filterRedundantEdgesFromRow(map);
            return map;
        }

        static List<List<MapRoomNode>> filterRedundantEdgesFromRow(List<List<MapRoomNode>> map)
        {
            List<MapEdge> existingEdges = new();
            List<MapEdge> deleteList = new();
            foreach (var node in map[0])
            {
                if (node.hasEdges())
                {
                    foreach (var edge in node.edges)
                    {
                        foreach (var prevEdge in existingEdges)
                        {
                            if (edge.dstX == prevEdge.dstX && edge.dstY == prevEdge.dstY)
                                deleteList.Add(edge);
                        }

                        existingEdges.Add(edge);
                    }

                    foreach (var edge in deleteList)
                        node.delEdge(edge);

                    deleteList.Clear();
                }
            }

            return map;
        }

        static List<List<MapRoomNode>> createNodes(int height, int width)
        {
            List<List<MapRoomNode>> nodes = new();
            for (int y = 0; y < height; y++)
            {
                List<MapRoomNode> row = new();
                for (int x = 0; x < width; x++)
                    row.Add(new(x, y));

                nodes.Add(row);
            }

            return nodes;
        }

        static List<List<MapRoomNode>> createPaths(List<List<MapRoomNode>> nodes, int pathDensity, Rand rng)
        {
            int firstRow = 0;
            int rowSize = nodes[firstRow].Count - 1;
            int firstStartingNode = -1;
            for (int i = 0; i < pathDensity; i++)
            {
                int startingNode = randRange(rng, 0, rowSize);
                if (i == 0)
                    firstStartingNode = startingNode;

                while (startingNode == firstStartingNode && i == 1)
                {
                    startingNode = randRange(rng, 0, rowSize);
                }

                _createPaths(nodes, new MapEdge(startingNode, -1, startingNode, 0), rng);
            }

            return nodes;
        }

        static MapEdge getMaxEdge(List<MapEdge> edges)
        {
            edges.Sort();
            // assert !edges.isEmpty() : "Somehow the edges are empty. This shouldn't happen.";
            return edges[^1];
        }

        static MapEdge getMinEdge(List<MapEdge> edges)
        {
            edges.Sort();
            // assert !edges.isEmpty() : "Somehow the edges are empty. This shouldn't happen.";
            return edges[0];
        }

        static MapRoomNode getNodeWithMaxX(List<MapRoomNode> nodes)
        {
            // assert !nodes.isEmpty() : "The nodes are empty, this shouldn't happen.";
            MapRoomNode max = nodes[0];
            foreach (MapRoomNode node in nodes)
            {
                if (node.x > max.x)
                    max = node;
            }

            return max;
        }

        static MapRoomNode getNodeWithMinX(List<MapRoomNode> nodes)
        {
            // assert !nodes.isEmpty() : "The nodes are empty, this shouldn't happen.";
            MapRoomNode min = nodes[0];
            foreach (MapRoomNode node in nodes)
            {
                if (node.x < min.x)
                    min = node;
            }

            return min;
        }

        static MapRoomNode getCommonAncestor(MapRoomNode node1, MapRoomNode node2, int max_depth)
        {
            MapRoomNode left, right;
            // assert node1.y == node2.y;
            // assert node1 != node2;
            if (node1.x < node2.y)
            {
                left = node1;
                right = node2;
            }
            else
            {
                left = node2;
                right = node1;
            }

            int curY = node1.y;
            while (curY >= 0 && curY >= node1.y - max_depth)
            {
                if (left.parents.Count == 0 || right.parents.Count == 0)
                    return null;
                left = getNodeWithMaxX(left.parents);
                right = getNodeWithMinX(right.parents);
                if (left == right)
                    return left;
                curY--;
            }

            return null;
        }

        static List<List<MapRoomNode>> _createPaths(List<List<MapRoomNode>> nodes, MapEdge edge, Rand rng)
        {
            int min, max;
            var curNode = getNode(edge.dstX, edge.dstY, nodes);
            if (edge.dstY + 1 >= nodes.Count)
            {
                var mapEdge = new MapEdge(
                    edge.dstX, edge.dstY, curNode.offsetX, curNode.offsetY,
                    3, edge.dstY + 2, 0.0F, 0.0F,
                    true);
                curNode.addEdge(mapEdge);
                curNode.edges.Sort();
                return nodes;
            }

            int row_width = nodes[edge.dstY].Count;
            int row_end_node = row_width - 1;
            if (edge.dstX == 0)
            {
                min = 0;
                max = 1;
            }
            else if (edge.dstX == row_end_node)
            {
                min = -1;
                max = 0;
            }
            else
            {
                min = -1;
                max = 1;
            }

            int newEdgeX = edge.dstX + randRange(rng, min, max);
            int newEdgeY = edge.dstY + 1;
            var targetNodeCandidate = getNode(newEdgeX, newEdgeY, nodes);
            int min_ancestor_gap = 3;
            int max_ancestor_gap = 5;
            var parents = targetNodeCandidate.parents;
            if (parents.Count > 0)
            {
                foreach (var node in parents)
                {
                    if (node == curNode)
                        continue;

                    var ancestor = getCommonAncestor(node, curNode, max_ancestor_gap);
                    if (ancestor == null)
                        continue;

                    int ancestor_gap = newEdgeY - ancestor.y;
                    if (ancestor_gap < min_ancestor_gap)
                    {
                        if (targetNodeCandidate.x > curNode.x)
                        {
                            newEdgeX = edge.dstX + randRange(rng, -1, 0);
                            if (newEdgeX < 0)
                                newEdgeX = edge.dstX;
                        }
                        else if (targetNodeCandidate.x == curNode.x)
                        {
                            newEdgeX = edge.dstX + randRange(rng, -1, 1);
                            if (newEdgeX > row_end_node)
                            {
                                newEdgeX = edge.dstX - 1;
                            }
                            else if (newEdgeX < 0)
                            {
                                newEdgeX = edge.dstX + 1;
                            }
                        }
                        else
                        {
                            newEdgeX = edge.dstX + randRange(rng, 0, 1);
                            if (newEdgeX > row_end_node)
                                newEdgeX = edge.dstX;
                        }

                        targetNodeCandidate = getNode(newEdgeX, newEdgeY, nodes);
                        continue;
                    }

                    if (ancestor_gap >= max_ancestor_gap)
                        continue;
                }
            }

            if (edge.dstX != 0)
            {
                MapRoomNode left = nodes[edge.dstY][edge.dstX - 1];
                if (left.hasEdges())
                {
                    MapEdge right_edge_of_left_node = getMaxEdge(left.edges);
                    if (right_edge_of_left_node.dstX > newEdgeX)
                        newEdgeX = right_edge_of_left_node.dstX;
                }
            }

            if (edge.dstX < row_end_node)
            {
                MapRoomNode right = nodes[edge.dstY][edge.dstX + 1];
                if (right.hasEdges())
                {
                    MapEdge left_edge_of_right_node = getMinEdge(right.edges);
                    if (left_edge_of_right_node.dstX < newEdgeX)
                        newEdgeX = left_edge_of_right_node.dstX;
                }
            }

            targetNodeCandidate = getNode(newEdgeX, newEdgeY, nodes);
            var newEdge = new MapEdge(
                edge.dstX, edge.dstY, curNode.offsetX, curNode.offsetY,
                newEdgeX, newEdgeY, targetNodeCandidate.offsetX, targetNodeCandidate.offsetY,
                false);
            curNode.addEdge(newEdge);
            curNode.edges.Sort();
            targetNodeCandidate.addParent(curNode);
            return _createPaths(nodes, newEdge, rng);
        }

        static MapRoomNode getNode(int x, int y, List<List<MapRoomNode>> nodes)
        {
            return nodes[y][x];
        }

        static string paddingGenerator(int length)
        {
            var str = new StringBuilder();
            for (int i = 0; i < length; i++)
                str.Append(" ");
            return str.ToString();
        }

        public static string toString(List<List<MapRoomNode>> nodes)
        {
            return toString(nodes, false);
        }

        public static string toString(List<List<MapRoomNode>> nodes, bool showRoomSymbols)
        {
            var str = new StringBuilder();
            int rowNum = nodes.Count - 1;
            int leftPaddingSize = 5;
            while (rowNum >= 0)
            {
                str.Append("\n ").Append(paddingGenerator(leftPaddingSize));
                foreach (var node in nodes[rowNum])
                {
                    string right = " ", mid = right, left = mid;
                    foreach (var edge in node.edges)
                    {
                        if (edge.dstX < node.x)
                            left = @"\";
                        if (edge.dstX == node.x)
                            mid = "|";
                        if (edge.dstX > node.x)
                            right = "/";
                    }

                    str.Append(left).Append(mid).Append(right);
                }

                str.Append("\n").Append(rowNum).Append(" ");
                str.Append(paddingGenerator(leftPaddingSize - rowNum.ToString().Length));
                foreach (var node in nodes[rowNum])
                {
                    string nodeSymbol = " ";
                    if (rowNum == nodes.Count - 1)
                    {
                        foreach (var lowerNode in nodes[rowNum - 1])
                        {
                            foreach (var edge in lowerNode.edges)
                            {
                                if (edge.dstX == node.x)
                                    nodeSymbol = node.getRoomSymbol(showRoomSymbols);
                            }
                        }
                    }
                    else if (node.hasEdges())
                    {
                        nodeSymbol = node.getRoomSymbol(showRoomSymbols);
                    }

                    str.Append(" ").Append(nodeSymbol).Append(" ");
                }

                rowNum--;
            }

            return str.ToString();
        }

        static int randRange(Rand rng, int min, int max)
        {
            if (rng == null)
            {
                log("RNG WAS NULL, REPORT IMMEDIATELY");
                rng = new Rand(1L);
            }

            return rng.random(max - min) + min;
        }
    }
}