using System;
using System.Collections.Generic;

namespace MarbleHero
{
    public class RoomTypeAssigner
    {
        public static void AssignRowAsRoomType<T>(List<MapRoomNode> row) where T : ARoom, new()
        {
            foreach (var node in row)
            {
                if (node.room != null)
                    continue;

                try
                {
                    node.room = new T();
                }
                catch (Exception e)
                {
                    logException(e);
                }
            }
        }

        static int GetConnectedNonAssignedNodeCount(List<List<MapRoomNode>> map)
        {
            int count = 0;
            foreach (var row in map)
            {
                foreach (var node in row)
                {
                    if (node.hasEdges() && node.room == null)
                        count++;
                }
            }

            return count;
        }

        static List<MapRoomNode> GetSiblings(List<List<MapRoomNode>> map, List<MapRoomNode> parents, MapRoomNode n)
        {
            List<MapRoomNode> siblings = new();
            foreach (var node in parents)
            {
                foreach (var edge in node.edges)
                {
                    MapRoomNode siblingNode = map[edge.dstY][edge.dstX];
                    if (siblingNode != n)
                        siblings.Add(siblingNode);
                }
            }

            return siblings;
        }

        static bool RuleSiblingMatches(List<MapRoomNode> siblings, ARoom roomToBeSet)
        {
            var applicableRooms = new List<Type>
            {
                typeof(RestRoom),
                typeof(MonsterRoom),
                typeof(EventRoom),
                typeof(MonsterRoomElite),
                typeof(ShopRoom),
            };
            foreach (MapRoomNode siblingNode in siblings)
            {
                if (siblingNode.room != null && applicableRooms.Contains(roomToBeSet.GetType()) && roomToBeSet.GetType() == siblingNode.room.GetType())
                    return true;
            }

            return false;
        }

        static bool RuleParentMatches(List<MapRoomNode> parents, ARoom roomToBeSet)
        {
            var applicableRooms = new List<Type>
            {
                typeof(RestRoom),
                typeof(TreasureRoom),
                typeof(ShopRoom),
                typeof(MonsterRoomElite),
            };
            foreach (MapRoomNode parentNode in parents)
            {
                ARoom parentRoom = parentNode.room;
                if (parentRoom != null && applicableRooms.Contains(roomToBeSet.GetType()) && roomToBeSet.GetType() == (parentRoom.GetType()))
                    return true;
            }

            return false;
        }

        static bool RuleAssignableToRow(MapRoomNode n, ARoom roomToBeSet)
        {
            var applicableRooms = new List<Type>
            {
                typeof(RestRoom),
                typeof(MonsterRoomElite),
            };
            var applicableRooms2 = new List<Type>
            {
                typeof(RestRoom),
            };
            if (n.y <= 4 && applicableRooms.Contains(roomToBeSet.GetType()))
                return false;
            return n.y < 13 || !applicableRooms2.Contains(roomToBeSet.GetType());
        }

        static ARoom GetNextRoomTypeAccordingToRules(List<List<MapRoomNode>> map, MapRoomNode n, List<ARoom> roomList)
        {
            List<MapRoomNode> parents = n.parents;
            List<MapRoomNode> siblings = GetSiblings(map, parents, n);
            foreach (ARoom roomToBeSet in roomList)
            {
                if (RuleAssignableToRow(n, roomToBeSet))
                {
                    if (!RuleParentMatches(parents, roomToBeSet) && !RuleSiblingMatches(siblings, roomToBeSet))
                        return roomToBeSet;
                    if (n.y == 0)
                        return roomToBeSet;
                }
            }

            return null;
        }

        static void LastMinuteNodeChecker(List<List<MapRoomNode>> map, MapRoomNode n)
        {
            foreach (var row in map)
            {
                foreach (var node in row)
                {
                    if (node != null && node.hasEdges() && node.room == null)
                    {
                        log("INFO: Node=" + node + " was null. Changed to a MonsterRoom.");
                        node.room = new MonsterRoom();
                    }
                }
            }
        }

        static void AssignRoomsToNodes(List<List<MapRoomNode>> map, List<ARoom> roomList)
        {
            foreach (var row in map)
            {
                foreach (var node in row)
                {
                    if (node != null && node.hasEdges() && node.room == null)
                    {
                        ARoom roomToBeSet = GetNextRoomTypeAccordingToRules(map, node, roomList);
                        if (roomToBeSet != null)
                        {
                            roomList.RemoveAt(roomList.IndexOf(roomToBeSet));
                            node.room = roomToBeSet;
                        }
                    }
                }
            }
        }

        public static void DistributeRoomsAcrossMap(Rand rng, ref List<List<MapRoomNode>> map, ref List<ARoom> roomList)
        {
            int nodeCount = GetConnectedNonAssignedNodeCount(map);
            while (roomList.Count < nodeCount)
                roomList.Add(new MonsterRoom());

            if (roomList.Count > nodeCount)
                log("WARNING: the roomList is larger than the number of connected nodes. Not all desired roomTypes will be used.");

            Shuffle(roomList, rng._random);
            AssignRoomsToNodes(map, roomList);
            log("#### Unassigned Rooms:");
            foreach (var room in roomList)
                log(room.GetType().Name);

            LastMinuteNodeChecker(map, null);
        }

        public static void Shuffle<T>(IList<T> list, RandomXS128 rnd)
        {
            for (int i = list.Count; i > 1; i--)
            {
                var next = rnd.nextInt(i);
                (list[i - 1], list[next]) = (list[next], list[i - 1]);
            }
        }
    }
}