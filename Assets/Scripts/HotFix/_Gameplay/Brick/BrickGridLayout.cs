using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public class BrickGridLayout
{
    List<Rect> botRowGrids = new();
    List<Rect> topRowGrids = new();
    List<Rect> leftColGrids = new();
    List<Rect> grids = new();
    Dictionary<(int col, int row), Rect> gridDict = new();

    BrickGridTemplate template;

    public BrickGridLayout(Vector2 _size, int _col, int _row)
    {
        template = new(_row, _col, _size);
        getGrids();
    }

    public BrickGridLayout(Vector2 _size, int _col, int _row, Vector2 _spacing, Vector2 _padding)
    {
        template = new(_row, _col, _size, _spacing, _padding);
        getGrids();
    }

    public int getCols() => cols;
    public int getRows() => rows;
    public Vector2 getSize() => size;

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

    public Vector2 getCellSize()
    {
        getCellSize(out var cellSize);
        return cellSize;
    }

    public List<Rect> getGrids()
    {
        grids.Clear();
        gridDict.Clear();
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
        // float startY = halfH - padY - cellSize.y * 0.5f;//以左上为坐标轴原点
        float startY = -halfH + padY + cellSize.y * 0.5f; //以左下为坐标轴原点

        botRowGrids.Clear();
        topRowGrids.Clear();
        leftColGrids.Clear();
        for (int row = 0; row < rows; row++)
        {
            // float cy = startY - row * (cellSize.y + spacing.y);//以左上为坐标轴原点
            float cy = startY + row * (cellSize.y + spacing.y); //以左下为坐标轴原点

            for (int col = 0; col < cols; col++)
            {
                float cx = startX + col * (cellSize.x + spacing.x);

                // 创建以 Cell 中心为原点的 Rect
                Rect rect = new(0, 0, cellSize.x, cellSize.y);
                rect.center = new Vector2(cx, cy) + offset;

                grids.Add(rect);
                gridDict[(col, row)] = rect;
                if (row == 0)
                    botRowGrids.add(rect);
                else if (row == rows - 1)
                    topRowGrids.add(rect);

                if (col == 0)
                    leftColGrids.add(rect);
            }
        }

        return grids;
    }

    public List<Rect> getTopRowGrids()
    {
        return topRowGrids;
    }

    public void getGridsAtRow(ref List<Rect> list, int row)
    {
        foreach (var ((_col, _row), rect) in gridDict)
        {
            if (_row == row)
            {
                list.add(rect);
            }
        }
    }

    public void getGridsAtCol(ref List<Rect> list, int col)
    {
        foreach (var ((_col, _row), rect) in gridDict)
        {
            if (_col == col)
            {
                list.add(rect);
            }
        }
    }

    public List<Rect> getAllGrids()
    {
        return grids;
    }

    public float getPosXAtCol(int col)
    {
        var rect = botRowGrids.get(col);
        return rect.center.x;
    }

    public float getPosYAtRow(int row)
    {
        if (row < 0)
        {
            var rect1 = leftColGrids.get(1);
            var rect0 = leftColGrids.get(0);
            return rect0.center.y - abs(rect1.center.y - rect0.center.y);
        }

        var rect = leftColGrids.get(row);
        return rect.center.y;
    }

    public Vector2 getPos(int col, int row)
    {
        return new(getPosXAtCol(col), getPosYAtRow(row));
    }

    public int getRowAtPosY(float y)
    {
        for (var row = 0; row < leftColGrids.Count; row++)
        {
            var rect = leftColGrids[row];
            if (rect.yMin <= y && y <= rect.yMax)
            {
                return row;
            }
        }

        return 0;
    }

    public int getColAtPosX(float x)
    {
        for (var col = 0; col < botRowGrids.Count; col++)
        {
            var rect = botRowGrids[col];
            if (rect.xMin <= x && x <= rect.xMax)
            {
                return col;
            }
        }

        return 0;
    }

    public Rect getRectAtPos(Vector2 pos)
    {
        var col = getColAtPosX(pos.x);
        var row = getRowAtPosY(pos.y);
        if (gridDict.TryGetValue((col, row), out var rect))
        {
            return rect;
        }

        return grids.get(0);
    }

    public int getSortingOrderAtPosY(float y)
    {
        var rowIndex = getRowAtPosY(y);
        var sortingOrder = rows - rowIndex;
        return sortingOrder;
    }

    public Vector2 size
    {
        get => template.size;
        set => template.size = value;
    }

    public Vector2 spacing
    {
        get => template.spacing;
        set => template.spacing = value;
    }

    public Vector2 padding
    {
        get => template.padding;
        set => template.padding = value;
    }

    public Vector2 offset
    {
        get => template.offset;
        set => template.offset = value;
    }

    public int cols
    {
        get => template.cols;
        set => template.cols = value;
    }

    public int rows
    {
        get => template.rows;
        set => template.rows = value;
    }

    public void setSize(float w, float h) => size = new(w, h);
    public void setWidth(float w) => size = size with { x = w };
    public void setHeight(float h) => size = size with { y = h };
    public void setRows(int row) => rows = row;
    public void setCols(int col) => cols = col;
    public void setSpacingX(float x) => spacing = spacing with { x = x };
    public void setSpacingY(float y) => spacing = spacing with { y = y };
    public void setSpacing(Vector2 s) => spacing = s;
    public void setPadding(Vector2 p) => padding = p;
    public void setOffset(Vector2 o) => offset = o;
}