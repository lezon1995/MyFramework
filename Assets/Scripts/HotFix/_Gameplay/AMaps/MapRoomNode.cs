using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    [Serializable]
    public class MapRoomNode
    {
        public static float OFFSET_X = Settings.isMobile ? (496.0F * Settings.xScale) : (560.0F * Settings.xScale);
        public static Color AVAILABLE_COLOR = new(0.09F, 0.13F, 0.17F, 1.0F);

        [NonSerialized]
        public List<MapRoomNode> parents = new();

        // List<FlameAnimationEffect> fEffects = new();
        public int x, y;
        ARoom _room;

        public ARoom room
        {
            get => _room;
            set => _room = value;
        }

        static float JITTER_X = Settings.isMobile ? (13.0F * Settings.xScale) : (27.0F * Settings.xScale);
        static float JITTER_Y = Settings.isMobile ? (18.0F * Settings.xScale) : (37.0F * Settings.xScale);

        public float offsetX { get; } = (int)MathUtils.random(-JITTER_X, JITTER_X);
        public float offsetY { get; } = (int)MathUtils.random(-JITTER_Y, JITTER_Y);
        // public float offsetX { get; }
        // public float offsetY { get; }

        public List<MapEdge> edges = new();
        public bool taken;
        public bool hasEmeraldKey;

        public MapRoomNode(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public MapRoomNode(int _x, int _y, ARoom room)
        {
            x = _x;
            y = _y;
            _room = room;
        }

        public bool hasEdges() => edges.Count > 0;

        public void addEdge(MapEdge e)
        {
            bool unique = true;
            foreach (var edge in edges)
            {
                if (e.CompareTo(edge) == 0)
                {
                    unique = false;
                    break;
                }
            }

            if (unique)
                edges.Add(e);
        }

        public void delEdge(MapEdge e)
        {
            edges.Remove(e);
        }

        public bool isConnectedTo(MapRoomNode node)
        {
            foreach (var edge in edges)
            {
                if (node.x == edge.dstX && node.y == edge.dstY)
                    return true;
            }

            return false;
        }

        public bool wingedIsConnectedTo(MapRoomNode node)
        {
            foreach (var edge in edges)
            {
                if (ModHelper.isModEnabled("Flight") && node.y == edge.dstY)
                    return true;

                if (node.y == edge.dstY && player.tryGetRelic("WingedGreaves", out var relic) && relic.counter > 0)
                    return true;
            }

            return false;
        }

        public MapEdge getEdgeConnectedTo(MapRoomNode node)
        {
            foreach (var edge in edges)
            {
                if (node.x == edge.dstX && node.y == edge.dstY)
                    return edge;
            }

            return null;
        }

        public bool leftNodeAvailable()
        {
            foreach (var edge in edges)
            {
                if (edge.dstX < x)
                    return true;
            }

            return false;
        }

        public bool centerNodeAvailable()
        {
            foreach (var edge in edges)
            {
                if (edge.dstX == x)
                    return true;
            }

            return false;
        }

        public bool rightNodeAvailable()
        {
            foreach (var edge in edges)
            {
                if (edge.dstX > x)
                    return true;
            }

            return false;
        }

        public void addParent(MapRoomNode parent)
        {
            parents.Add(parent);
        }

        public string getRoomSymbol(bool showSpecificRoomSymbol)
        {
            if (room == null || !showSpecificRoomSymbol)
                return "*";
            return room.getMapSymbol();
        }

        public void markAsTaken()
        {
            taken = true;
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "):" + string.Join(", ", edges);
        }


        /*
        void updateEmerald(float dt)
        {
            if (Settings.isFinalActAvailable && hasEmeraldKey)
            {
                flameVfxTimer -= dt;
                if (flameVfxTimer < 0.0F)
                {
                    flameVfxTimer = MathUtils.random(0.2F, 0.4F);
                    fEffects.add(new FlameAnimationEffect(hb));
                }

                Iterator<FlameAnimationEffect> i;
                for (i = fEffects.iterator(); i.hasNext();)
                {
                    FlameAnimationEffect e = i.next();
                    if (e.isDone)
                    {
                        e.dispose();
                        i.remove();
                    }
                }

                for (i = fEffects.iterator(); i.hasNext();)
                {
                    FlameAnimationEffect e = i.next();
                    e.update();
                }
            }
        }
        */


        /*public void render(SpriteBatch sb)
        {
            foreach (var edge in edges)
                edge.render(sb);

            renderEmeraldVfx(sb);
            if (highlighted)
                sb.setColor(new Color(0.9F, 0.9F, 0.9F, 1.0F));
            else
                sb.setColor(OUTLINE_COLOR);

            bool legendHovered = ADungeon.dungeonMapScreen.map.legend.isIconHovered(getRoomSymbol(true));
            if (legendHovered)
            {
                scale = 0.68F;
                sb.setColor(Color.LIGHT_GRAY);
            }

            if (!Settings.isMobile)
                sb.draw(room.getMapImgOutline(), x * SPACING_X + OFFSET_X - 64.0F + offsetX, y * Settings.MAP_DST_Y + OFFSET_Y + DungeonMapScreen.offsetY - 64.0F + offsetY, 64.0F, 64.0F, W, W, scale * Settings.scale, scale * Settings.scale, 0.0F, 0, 0, W, W, false, false);
            else
                sb.draw(room.getMapImgOutline(), x * SPACING_X + OFFSET_X - 64.0F + offsetX, y * Settings.MAP_DST_Y + OFFSET_Y + DungeonMapScreen.offsetY - 64.0F + offsetY, 64.0F, 64.0F, W, W, scale * Settings.scale * 2.0F, scale * Settings.scale * 2.0F, 0.0F, 0, 0, W, W, false, false);

            if (taken)
                sb.setColor(AVAILABLE_COLOR);
            else
                sb.setColor(color);

            if (legendHovered)
                sb.setColor(AVAILABLE_COLOR);

            if (!Settings.isMobile)
                sb.draw(room.getMapImg(), x * SPACING_X + OFFSET_X - 64.0F + offsetX, y * Settings.MAP_DST_Y + OFFSET_Y + DungeonMapScreen.offsetY - 64.0F + offsetY, 64.0F, 64.0F, W, W, scale * Settings.scale, scale * Settings.scale, 0.0F, 0, 0, W, W, false, false);
            else
                sb.draw(room.getMapImg(), x * SPACING_X + OFFSET_X - 64.0F + offsetX, y * Settings.MAP_DST_Y + OFFSET_Y + DungeonMapScreen.offsetY - 64.0F + offsetY, 64.0F, 64.0F, W, W, scale * Settings.scale * 2.0F, scale * Settings.scale * 2.0F, 0.0F, 0, 0, W, W, false, false);

            if (taken || (ADungeon.firstRoomChosen && equals(mapNode)))
            {
                sb.setColor(AVAILABLE_COLOR);
                if (!Settings.isMobile)
                    sb.draw(ImageMaster.MAP_CIRCLE_5, x * SPACING_X + OFFSET_X - 96.0F + offsetX, y * Settings.MAP_DST_Y + OFFSET_Y + DungeonMapScreen.offsetY - 96.0F + offsetY, 96.0F, 96.0F, O_W, O_W, (scale * 0.95F + 0.2F) * Settings.scale, (scale * 0.95F + 0.2F) * Settings.scale, angle, 0, 0, O_W, O_W, false, false);
                else
                    sb.draw(ImageMaster.MAP_CIRCLE_5, x * SPACING_X + OFFSET_X - 96.0F + offsetX, y * Settings.MAP_DST_Y + OFFSET_Y + DungeonMapScreen.offsetY - 96.0F + offsetY, 96.0F, 96.0F, O_W, O_W, (scale * 0.95F + 0.2F) * Settings.scale * 2.0F, (scale * 0.95F + 0.2F) * Settings.scale * 2.0F, angle, 0, 0, O_W, O_W, false, false);
            }

            if (hb != null)
                hb.render(sb);
        }*/

        /*void renderEmeraldVfx(SpriteBatch sb)
        {
            if (Settings.isFinalActAvailable && hasEmeraldKey)
                foreach (FlameAnimationEffect e in fEffects)
                    e.render(sb, scale);
        }*/
        
        public static implicit operator bool(MapRoomNode self) => self != null;
    }
}