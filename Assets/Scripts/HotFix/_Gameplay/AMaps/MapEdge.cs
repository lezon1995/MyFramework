using System;
using UnityEngine;

namespace MoreMountains
{
    [Serializable]
    public class MapEdge : IComparable<MapEdge>
    {
        public int dstX, dstY;
        public int srcX, srcY;

        public bool taken;

        // public List<MapDot> dots = new();
        // public Vector2 src => new(srcX, srcY);
        // public Vector2 dst => new(dstX, dstY);
        // public Vector2 src => dots[0].pos;
        // public Vector2 dst => dots[^1].pos;
        static float ICON_SRC_RADIUS = 29.0F * Settings.scale;
        static float ICON_DST_RADIUS = 20.0F * Settings.scale;
        static float SPACING = Settings.isMobile ? (20.0F * Settings.xScale) : (17.0F * Settings.xScale);
        static Color DISABLED_COLOR = new Color(0.0F, 0.0F, 0.0F, 0.25F);
        public Color color = DISABLED_COLOR;
        static float SPACE_X = Settings.isMobile ? (140.8F * Settings.xScale) : (128.0F * Settings.xScale);

        public MapEdge(
            int _srcX, int _srcY,
            int _dstX, int _dstY)
        {
            srcX = _srcX;
            srcY = _srcY;
            dstX = _dstX;
            dstY = _dstY;
        }

        public MapEdge(
            int _srcX, int _srcY, float srcOffsetX, float srcOffsetY,
            int _dstX, int _dstY, float dstOffsetX, float dstOffsetY,
            bool isBoss)
        {
            srcX = _srcX;
            srcY = _srcY;
            dstX = _dstX;
            dstY = _dstY;
            // var tmpSX = getX(srcX) + srcOffsetX;
            // var tmpDX = getX(dstX) + dstOffsetX;
            // var tmpSY = srcY * Settings.MAP_DST_Y + srcOffsetY;
            // var tmpDY = dstY * Settings.MAP_DST_Y + dstOffsetY;
            // var vec2 = new Vector2(tmpDX, tmpDY) - new Vector2(tmpSX, tmpSY);
            // var length = vec2.sqrMagnitude;
            // var START = SPACING * MathUtils.random() / 2.0F;
            // var tmpRadius = ICON_DST_RADIUS;
            // if (isBoss)
            //     tmpRadius = 164.0F * Settings.scale;

            // for (var i = START + tmpRadius; i < length - ICON_SRC_RADIUS; i += SPACING)
            // {
            //     vec2 = vec2.Clamp(length - i, length - i);
            //     if (i != START + tmpRadius && i <= length - ICON_SRC_RADIUS - SPACING)
            //     {
            //         dots.Add(new MapDot(tmpSX + vec2.x, tmpSY + vec2.y, (new Vector2(tmpSX - tmpDX, tmpSY - tmpDY)).normalized.Angle() + 90.0F, true));
            //     }
            //     else
            //     {
            //         dots.Add(new MapDot(tmpSX + vec2.x, tmpSY + vec2.y, (new Vector2(tmpSX - tmpDX, tmpSY - tmpDY)).normalized.Angle() + 90.0F, false));
            //     }
            // }
        }

        float getX(int x) => x * SPACE_X + MapRoomNode.OFFSET_X;

        public override string ToString()
        {
            return "(" + dstX + "," + dstY + ")";
        }

        public int CompareTo(MapEdge e)
        {
            if (dstX > e.dstX)
                return 1;

            if (dstX < e.dstX)
                return -1;

            if (dstY > e.dstY)
                return 1;

            if (dstY < e.dstY)
                return -1;

            if (dstY == e.dstY)
                return 0;

            return 0;
        }

        public void markAsTaken()
        {
            taken = true;
            color = MapRoomNode.AVAILABLE_COLOR;
        }
    }
}