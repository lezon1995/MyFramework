using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

// 通过MeshRenderer的方式进行画线条
public class UGUILineMesh : ClassObject
{
    protected List<Vector3> points = new(); // 点列表
    protected MeshRenderer meshRenderer; // 线条的mesh渲染组件
    protected Transform transform; // 线条变换组件
    protected GameObject go; // 线条物体
    protected Mesh mesh; // 显示的Mesh
    protected float width = 10; // 宽度的一半

    public void init(GameObject obj)
    {
        go = obj;
        go.SetActive(true);
        go.TryGetComponent(out transform);
        go.TryGetComponent(out meshRenderer);
        go.TryGetComponent<MeshFilter>(out var filter);
        mesh = filter.mesh;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        points.Clear();
        meshRenderer = null;
        transform = null;
        go = null;
        mesh = null;
        width = 10.0f;
    }

    public override void destroy()
    {
        base.destroy();
        mesh.Clear();
        if (go)
            go.SetActive(false);
    }

    public Material getMaterial()
    {
        return meshRenderer.material;
    }

    public void setActive(bool active)
    {
        go.SetActive(active);
    }

    public void setWidth(float width)
    {
        this.width = width;
    }

    public void setPointList(List<Vector3> list)
    {
        points.Clear();
        foreach (Vector3 pos in list.safe())
        {
            if (points.Count > 0 && isVectorEqual(pos, points[^1]))
                continue;

            points.Add(pos);
        }

        onPointsChanged();
    }

    public void setPointList(Span<Vector3> list)
    {
        points.Clear();
        foreach (Vector3 pos in list)
        {
            if (points.Count > 0 && isVectorEqual(pos, points[^1]))
                continue;

            points.Add(pos);
        }

        onPointsChanged();
    }

    public void setPointList(Vector3[] list)
    {
        points.Clear();
        foreach (Vector3 pos in list.safe())
        {
            if (points.Count > 0 && isVectorEqual(pos, points[^1]))
            {
                continue;
            }

            points.Add(pos);
        }

        onPointsChanged();
    }

    public void onPointsChanged()
    {
        // 先将模型数据清空
        mesh.Clear();
        int pointCount = points.Count;
        if (pointCount < 2)
            return;

        // 计算顶点,纹理坐标,颜色
        var vertices = new Vector3[pointCount * 2];
        var colors = new Color[pointCount * 2];
        var uv = new Vector2[pointCount * 2];
        for (int i = 0; i < pointCount; ++i)
        {
            // 如果当前点跟上一个点相同,则取上一点计算出的结果
            if (i > 0 && i < pointCount - 1 && isVectorEqual(points[i - 1], points[i]))
            {
                vertices[2 * i + 0] = vertices[2 * (i - 1) + 0];
                vertices[2 * i + 1] = vertices[2 * (i - 1) + 1];
                colors[2 * i + 0] = colors[2 * (i - 1) + 0];
                colors[2 * i + 1] = colors[2 * (i - 1) + 1];
                uv[2 * i + 0] = uv[2 * (i - 1) + 0];
                uv[2 * i + 1] = uv[2 * (i - 1) + 1];
            }
            else
            {
                if (i == 0)
                {
                    Vector3 dir = (points[i + 1] - points[i]).normalized;
                    float halfAngle = HALF_PI_RADIAN;
                    Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
                    Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
                    vertices[2 * i + 0] = rotateVector3(dir, q0) * divide(width, sin(halfAngle));
                    vertices[2 * i + 1] = rotateVector3(dir, q1) * divide(width, sin(halfAngle));
                }
                else if (i > 0 && i < pointCount - 1)
                {
                    Vector3 dir = (points[i + 1] - points[i]).normalized;
                    Vector3 dir1 = (points[i - 1] - points[i]).normalized;
                    float halfAngle = getAngleVectorToVector(dir, dir1, false) * 0.5f;
                    Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
                    Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
                    if (halfAngle >= 0.0f)
                    {
                        vertices[2 * i + 0] = rotateVector3(dir, q0) * width;
                        vertices[2 * i + 1] = rotateVector3(dir, q1) * width;
                    }
                    else
                    {
                        vertices[2 * i + 0] = rotateVector3(dir, q1) * width;
                        vertices[2 * i + 1] = rotateVector3(dir, q0) * width;
                    }
                }
                else if (i == pointCount - 1)
                {
                    Vector3 dir = (points[i] - points[i - 1]).normalized;
                    float halfAngle = HALF_PI_RADIAN;
                    Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
                    Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
                    vertices[2 * i + 0] = rotateVector3(dir, q0) * divide(width, sin(halfAngle));
                    vertices[2 * i + 1] = rotateVector3(dir, q1) * divide(width, sin(halfAngle));
                }

                vertices[2 * i + 0] += points[i];
                vertices[2 * i + 1] += points[i];
                colors[2 * i + 0] = Color.green;
                colors[2 * i + 1] = Color.green;
                uv[2 * i + 0] = Vector2.zero;
                uv[2 * i + 1] = Vector2.zero;
            }
        }

        // 计算顶点索引,每两个点之间两个三角面，每个三角面三个顶点
        int[] triangles = new int[(pointCount - 1) * 6];
        for (int i = 0; i < pointCount - 1; ++i)
        {
            int startIndex = i * 6;
            int indexValue = i * 2;
            triangles[startIndex + 0] = indexValue + 0;
            triangles[startIndex + 1] = indexValue + 1;
            triangles[startIndex + 2] = indexValue + 2;
            triangles[startIndex + 3] = indexValue + 2;
            triangles[startIndex + 4] = indexValue + 1;
            triangles[startIndex + 5] = indexValue + 3;
        }

        // 将顶点数据设置到模型中
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
}