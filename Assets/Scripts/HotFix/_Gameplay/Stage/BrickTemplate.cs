using System;
using UnityEngine;

namespace MarbleHero;

[Serializable]
public struct BrickTemplate : IEquatable<BrickTemplate>
{
    public Vector2 position;
    public Vector2 size;
    public int health;

    public Rect rect
    {
        get
        {
            var r = new Rect(position, size);
            r.center = position;
            return r;
        }
    }

    public BrickTemplate()
    {
        position = default;
        size = default;
        health = 1;
    }

    public BrickTemplate(Rect _rect, int _health)
    {
        position = _rect.center;
        size = _rect.size;
        health = _health;
    }

    public BrickTemplate(Vector2 _position, Vector2 _size, int _health)
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