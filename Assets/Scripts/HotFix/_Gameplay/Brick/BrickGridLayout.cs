using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public class BrickGridLayout
{
    List<Rect> firstRowGrids = new();
    List<Rect> firstColGrids = new();
    List<Rect> grids = new();

    Vector2 size, spacing, padding;
    int cols, rows;

    public BrickGridLayout(Vector2 _size, int col, int row)
    {
        size = _size;
        cols = col;
        rows = row;
        spacing = new(0.05F, 0.05F);
        getGrids();
    }

    public void getCellSize(out Vector2 gridSize)
    {
        //左右各 padding.x，上下各 padding.y
        float padX = padding.x;
        float padY = padding.y;

        // 可用宽高 = 总宽高 - 左右 padding - 水平间距 - 同理高度
        float usableWidth = size.x - padX * 2 - spacing.x * Mathf.Max(0, cols - 1);
        float usableHeight = size.y - padY * 2 - spacing.y * Mathf.Max(0, rows - 1);

        // 防止负数（若 padding/spacing 太大）和除以 0
        if (cols <= 0 || usableWidth <= 0f)
            gridSize = new(0f, Mathf.Max(0f, usableHeight / Mathf.Max(1, rows)));
        else if (rows <= 0 || usableHeight <= 0f)
            gridSize = new(Mathf.Max(0f, usableWidth / Mathf.Max(1, cols)), 0f);
        else
            gridSize = new(Mathf.Max(0f, usableWidth / cols), Mathf.Max(0f, usableHeight / rows));
    }

    public List<Rect> getGrids()
    {
        grids.Clear();
        // 计算每个 Cell 的 size
        getCellSize(out Vector2 cellSize);

        int total = cols * rows;
        float gridWidth = size.x;
        float gridHeight = size.y;

        // 整个 Grid 的左下角相对于中心点的位置 (中心为 0,0)
        float halfW = gridWidth * 0.5f;
        float halfH = gridHeight * 0.5f;

        // padding
        float padX = padding.x;
        float padY = padding.y;

        // 左上角 Cell 的中心位置（因为要按行列往下排）
        float startX = -halfW + padX + cellSize.x * 0.5f;
        float startY = halfH - padY - cellSize.y * 0.5f;

        firstRowGrids.Clear();
        firstColGrids.Clear();
        for (int row = 0; row < rows; row++)
        {
            float cy = startY - row * (cellSize.y + spacing.y);

            for (int col = 0; col < cols; col++)
            {
                float cx = startX + col * (cellSize.x + spacing.x);

                // 创建以 Cell 中心为原点的 Rect
                Rect rect = new(0, 0, cellSize.x, cellSize.y);
                rect.center = new(cx, cy);

                grids.Add(rect);

                if (row == 0)
                    firstRowGrids.add(rect);

                if (col == 0)
                    firstColGrids.add(rect);
            }
        }

        return grids;
    }

    public float getPosXAtCol(int col)
    {
        var rect = firstRowGrids.get(col);
        return rect.center.x;
    }

    public float getPosYAtRow(int row)
    {
        var rect = firstColGrids.get(row);
        return rect.center.y;
    }

    public Vector2 getPos(int col, int row)
    {
        return new(getPosXAtCol(col), getPosYAtRow(row));
    }

    public int getRowAtPosY(float y)
    {
        for (var row = 0; row < firstColGrids.Count; row++)
        {
            var rect = firstColGrids[row];
            if (rect.yMin <= y && y <= rect.yMax)
            {
                return row;
            }
        }

        return -1;
    }

    public void setSize(float w, float h) => (size.x, size.y) = (w, h);
    public void setWidth(float w) => size.x = w;
    public void setHeight(float h) => size.y = h;
    public void setRows(int row) => rows = row;
    public void setCols(int col) => cols = col;
    public void setSpacingX(float x) => spacing.x = x;
    public void setSpacingY(float y) => spacing.y = y;
}