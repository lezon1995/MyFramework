using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 自定义拖尾渲染器 - 使用Catmull-Rom算法实现平滑轨迹（对象池优化版）
/// </summary>
public class SmoothTrail : MonoBehaviour
{
    public int maxPathPointCount = 50;
    public float pointGap = 0.1f;
    public int lerpPrecision = 10;
    public Color color = Color.white;
    public float width = 0.1f;
    public float lifeTime = 2.0f;
    public bool isPausedTiming;
    public float tension = 0.5f;

    // 组件引用
    public LineRenderer line;
    List<Point> points = new();

    // 暂停相关变量
    float pausedStartTime;
    bool pausedState;

    public int updateInterval = 1;

    /// <summary>
    /// 拖尾点数据结构，包含位置和时间信息
    /// </summary>
    struct Point
    {
        public Vector3 pos;
        public float genTime;
        public float liveTime;

        public Point(Vector3 p, float time)
        {
            pos = p;
            genTime = time;
            liveTime = 0f;
        }
    }

    void Awake()
    {
        // 初始化拖尾渲染器
        initRenderer();
    }

    void Update()
    {
        if (Time.frameCount % updateInterval == 0)
        {
            // 更新暂停状态
            RefreshPausedState();

            // 更新轨迹点
            RefreshPathPoints();

            // 生成平滑轨迹
            if (points.Count >= 2)
            {
                genSmoothPath();
            }
            else if (points.Count == 1)
            {
                // 只有一个点时直接显示
                line.positionCount = 1;
                line.SetPosition(0, points[0].pos);
            }
            else
            {
                line.positionCount = 0;
            }
        }
    }

    /// <summary>
    /// 初始化LineRenderer组件
    /// </summary>
    void initRenderer()
    {
        line.useWorldSpace = true;
        line.positionCount = 0;
    }

    /// <summary>
    /// 更新暂停状态
    /// </summary>
    void RefreshPausedState()
    {
        var now = Time.time;
        if (isPausedTiming && !pausedState)
        {
            pausedState = true;
            pausedStartTime = now;
        }
        else if (!isPausedTiming && pausedState)
        {
            pausedState = false;
            float timeElapsedSincePaused = now - pausedStartTime;

            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];
                p.genTime += timeElapsedSincePaused;
                points[i] = p;
            }
        }
    }

    /// <summary>
    /// 更新轨迹点列表（使用对象池优化）
    /// </summary>
    void RefreshPathPoints()
    {
        var pos = transform.position;
        var now = Time.time;

        // 如果列表为空或距离足够远，添加新点
        if (points.Count == 0 || (pos - points[^1].pos).sqrMagnitude > pointGap * pointGap)
        {
            points.Add(new(pos, now));

            while (points.Count > maxPathPointCount)
            {
                points.RemoveAt(0);
            }
        }

        // 更新所有点的存活时间并移除过期点
        RefreshAliveTimeAndRemoveExpiredPoint();
    }


    /// <summary>
    /// 更新点存活时间并移除过期点（使用对象池优化）
    /// </summary>
    void RefreshAliveTimeAndRemoveExpiredPoint()
    {
        // 更新存活时间并移除过期点
        for (int i = points.Count - 1; i >= 0; i--)
        {
            var p = points[i];
            if (!pausedState)
            {
                p.liveTime = Time.time - p.genTime;
                points[i] = p;
            }

            // if (p.liveTime > lifeTime)
            // {
            //     points.RemoveAt(i);
            // }
        }
    }

    /// <summary>
    /// 使用Catmull-Rom算法生成平滑轨迹（优化内存分配）
    /// </summary>
    void genSmoothPath()
    {
        // 准备控制点
        using var _ = ListPool<Vector3>.Get(out var positions);
        getControlPositions(ref positions);

        // 清空临时列表（重用已分配的内存）
        var count = positions.Count - 3;
        var arrayCount = count * lerpPrecision;

        // 生成平滑路径点
        using (new ArrayScope<Vector3>(out var tempPoints, arrayCount))
        {
            int counter = 0;
            for (int i = 0; i < count; i++)
            {
                var p0 = positions[i];
                var p1 = positions[i + 1];
                var p2 = positions[i + 2];
                var p3 = positions[i + 3];

                for (int j = 0; j < lerpPrecision; j++)
                {
                    var t = j / (float)lerpPrecision;
                    var p = CalculatePointByCatmullRom(p0, p1, p2, p3, t);
                    counter++;
                    tempPoints[^counter] = p;
                }
            }

            // 更新LineRenderer
            line.positionCount = tempPoints.Length;
            line.SetPositions(tempPoints);
        }
    }

    /// <summary>
    /// 为Catmull-Rom插值准备控制点
    /// </summary>
    void getControlPositions(ref List<Vector3> positions)
    {
        positions.Clear();
        if (points.Count < 2)
            return;

        // 添加起始虚拟点
        var startPos = points[0].pos + (points[0].pos - points[1].pos);
        positions.Add(startPos);

        // 添加原始点
        foreach (var p in points)
        {
            positions.Add(p.pos);
        }

        // 添加结束虚拟点
        var endPos = points[^1].pos + (points[^1].pos - points[^2].pos);
        positions.Add(endPos);
    }

    /// <summary>
    /// Catmull-Rom插值核心算法
    /// </summary>
    Vector3 CalculatePointByCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float s = (1.0f - tension) * 0.5f;
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 result = p1 * (2 * s);
        result += (-p0 + p2) * (s * t);
        result += (2 * p0 - 5 * p1 + 4 * p2 - p3) * (s * t2);
        result += (-p0 + 3 * p1 - 3 * p2 + p3) * (s * t3);

        return result;
    }

    /// <summary>
    /// 清空拖尾轨迹（使用对象池优化）
    /// </summary>
    public void clearTrail()
    {
        points.Clear();

        if (line)
        {
            line.positionCount = 0;
        }
    }

    /// <summary>
    /// 设置拖尾颜色
    /// </summary>
    public void setColor(Color c)
    {
        color = c;
        if (line)
        {
            line.startColor = color;
            line.endColor = color;
        }
    }
    public void setGradientColor(Color c)
    {
        color = c;
        if (line)
        {
            var gradient = line.colorGradient;

            var alphaKeys = gradient.alphaKeys;
            alphaKeys[0].time = 0f;
            alphaKeys[0].alpha = 58F/255F;
            alphaKeys[1].time = 0.632f;
            alphaKeys[1].alpha = 0f;
            gradient.alphaKeys = alphaKeys;

            var colorKeys = gradient.colorKeys;
            colorKeys[0].time = 0f;
            colorKeys[0].color = c;
            colorKeys[1].time = 1f;
            colorKeys[1].color = c;
            gradient.colorKeys = colorKeys;

            line.colorGradient = gradient;
        }
    }

    /// <summary>
    /// 设置拖尾宽度
    /// </summary>
    public void setWidth(float w)
    {
        width = w;
        if (line)
        {
            line.startWidth = width;
            line.endWidth = width;
        }
    }

    /// <summary>
    /// 设置是否暂停计时
    /// </summary>
    public void setPaused(bool p)
    {
        isPausedTiming = p;
    }

    /// <summary>
    /// 强制恢复拖尾计时
    /// </summary>
    public void ForceRecoverTiming()
    {
        isPausedTiming = false;
        pausedState = false;

        float now = Time.time;
        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            point.liveTime = now - point.genTime;
            points[i] = point;
        }
    }
}