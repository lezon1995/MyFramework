using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public class LevelTemplate
{
    public int rows, cols;
    public Vector2 size, spacing, padding;

    //第0个元素为当前关卡开始时需要生成的所有砖块
    public List<BrickGroupTemplate> groups = new();
}

public class BrickGroupTemplate
{
    public List<Brick> bricks = new();

    public struct Brick
    {
        public float x, y;
        public float w, h;

        public Rect getRect()
        {
            var rect = new Rect(0, 0, w, h);
            rect.center = new(x, y);
            return rect;
        }
    }
}