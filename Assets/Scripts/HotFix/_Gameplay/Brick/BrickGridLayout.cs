using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public class BrickGridLayout
{
    Vector2 size, spacing, padding;
    int cols, rows;

    public BrickGridLayout(Vector2 _size, int col, int row)
    {
        size = _size;
        cols = col;
        rows = row;
        spacing = new(0.05F, 0.05F);
    }

    public void refreshSize(float w, float h)
    {
        size.x = w;
        size.y = h;
    }

    public void refreshWidth(float w)
    {
        size.x = w;
    }

    public void refreshHeight(float h)
    {
        size.y = h;
    }

    public void refreshRows(int row)
    {
        rows = row;
    }

    public void refreshCols(int col)
    {
        cols = col;
    }

    public void refreshSpacingX(float x)
    {
        spacing.x = x;
    }

    public void refreshSpacingY(float y)
    {
        spacing.y = y;
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

    public void getGrids(ref List<Rect> grids)
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
        float startY =  halfH - padY - cellSize.y * 0.5f;

        int index = 0;

        for (int r = 0; r < rows; r++)
        {
            float cy = startY - r * (cellSize.y + spacing.y);

            for (int c = 0; c < cols; c++)
            {
                float cx = startX + c * (cellSize.x + spacing.x);

                // 创建以 Cell 中心为原点的 Rect
                Rect rect = new(0, 0, cellSize.x, cellSize.y);
                rect.center = new(cx, cy);

                grids.Add(rect);
            }
        }
    }
}