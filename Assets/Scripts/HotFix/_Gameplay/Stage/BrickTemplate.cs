using System;
using UnityEngine;

namespace MoreMountains;

[Serializable]
public struct BrickTemplate : IEquatable<BrickTemplate>
{
    public static Vector2 cellSize = new(0.675F, 0.675F);
    public BrickDef def;
    public Vector2 position;
    public Vector2Int size => def.Size;
    public int health;

    public Rect rect => getRect();

    public Rect getRect()
    {
        var offset = new Vector2((size.x - 1) * cellSize.x * 0.5F, (size.y - 1) * cellSize.y * 0.5F);
        var p = position + offset - size * cellSize * 0.5F;
        return new(p, size * cellSize);
    }

    public BrickTemplate(Vector2 _position, BrickDef _def)
    {
        position = _position;
        def = _def;
        health = 0;
    }

    public BrickTemplate(Vector2 _position, BrickDef _def, int _health)
    {
        position = _position;
        def = _def;
        health = _health;
    }

    public bool Equals(BrickTemplate other)
    {
        return position.Equals(other.position) && size.Equals(other.size);
    }

    public override bool Equals(object obj)
    {
        return obj is BrickTemplate other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(position, size);
    }
}