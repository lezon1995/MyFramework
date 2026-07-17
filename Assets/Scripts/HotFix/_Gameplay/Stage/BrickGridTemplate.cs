using System;
using UnityEngine;

namespace MoreMountains;

[Serializable]
public struct BrickGridTemplate
{
    public int rows;
    public int cols;
    public Vector2 size;
    public Vector2 spacing;
    public Vector2 padding;
    public Vector2 offset;

    public BrickGridTemplate(int _rows, int _cols, Vector2 _size) : this()
    {
        rows = _rows;
        cols = _cols;
        size = _size;
        spacing = Vector2.zero;
        padding = Vector2.zero;
        offset = Vector2.zero;
    }
    
    public BrickGridTemplate(int _rows, int _cols, Vector2 _size, Vector2 _spacing, Vector2 _padding)
    {
        rows = _rows;
        cols = _cols;
        size = _size;
        spacing = _spacing;
        padding = _padding;
        offset = Vector2.zero;
    }

    public BrickGridTemplate(int _rows, int _cols, Vector2 _size, Vector2 _spacing, Vector2 _padding, Vector2 _offset)
    {
        rows = _rows;
        cols = _cols;
        size = _size;
        spacing = _spacing;
        padding = _padding;
        offset = _offset;
    }
}