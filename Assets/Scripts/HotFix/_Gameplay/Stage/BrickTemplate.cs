using System;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public struct BrickTemplate : IEquatable<BrickTemplate>
{
    public static Vector2 cellSize = new(0.675F, 0.675F);
    public Vector2 position;
    public Vector2Int size;
    public int health;

    public Rect rect => getRect();

    public Rect getRect()
    {
        var offset = new Vector2((size.x - 1) * cellSize.x * 0.5F, (size.y - 1) * cellSize.y * 0.5F);
        var p = position + offset - size * cellSize * 0.5F;
        return new(p, size * cellSize);
    }

    public BrickTemplate()
    {
        position = default;
        size = default;
        health = 1;
    }

    public BrickTemplate(Vector2 _position, Vector2Int _size, int _health)
    {
        position = _position;
        size = _size;
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