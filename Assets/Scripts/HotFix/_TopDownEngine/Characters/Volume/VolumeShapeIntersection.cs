using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 两个 <see cref="VolumeShape"/> 相交判定工具。
    /// 矩形始终按坐标轴对齐（与 <see cref="VolumeShape"/> 一致，不使用旋转）。
    /// 提供 <c>scaling</c> 参数同时对两个形状进行缩放后再做相交判定：
    /// <c>scaling &gt; 1</c> 表示两个形状均被放大（判定更宽松），
    /// <c>scaling &lt; 1</c> 表示两个形状均被缩小（判定更严格），
    /// 默认 1 表示按形状原始尺寸判定。
    /// </summary>
    public static class VolumeShapeIntersection
    {
        private const float k_Epsilon = 1e-6f;

        /// <summary>
        /// 判断两个 VolumeShape 是否相交。
        /// </summary>
        /// <param name="a">形状 A，null 时视为不相交</param>
        /// <param name="centerA">A 的世界中心</param>
        /// <param name="b">形状 B，null 时视为不相交</param>
        /// <param name="centerB">B 的世界中心</param>
        /// <param name="scaling">
        /// 同时作用于 A 与 B 的缩放系数。
        /// 作用于 Circle 的 Radius 与 Rectangle 的 Size。
        /// 默认 1 即按原始尺寸判定。
        /// </param>
        public static bool Intersects(
            VolumeShape a, Vector2 centerA,
            VolumeShape b, Vector2 centerB,
            float scaling = 1f)
        {
            if (a == null || b == null)
                return false;

            float s = Mathf.Max(scaling, 0f);

            switch (a.Shape)
            {
                case VolumeShapeType.Circle when b.Shape == VolumeShapeType.Circle:
                {
                    float rA = Mathf.Max(0f, a.Radius) * s;
                    float rB = Mathf.Max(0f, b.Radius) * s;
                    return CircleCircle(centerA, rA, centerB, rB);
                }
                case VolumeShapeType.Rectangle when b.Shape == VolumeShapeType.Rectangle:
                {
                    Vector2 sizeA = a.Size * s;
                    Vector2 sizeB = b.Size * s;
                    return RectangleRectangle(centerA, sizeA, centerB, sizeB);
                }
            }

            // Circle-Rectangle 任意顺序，归一化为 (圆, 矩)
            bool aIsCircle = a.Shape == VolumeShapeType.Circle;
            VolumeShape circleV = aIsCircle ? a : b;
            Vector2 circleCenter = aIsCircle ? centerA : centerB;
            VolumeShape rectV = aIsCircle ? b : a;
            Vector2 rectCenter = aIsCircle ? centerB : centerA;

            float radius = Mathf.Max(0f, circleV.Radius) * s;
            Vector2 size = rectV.Size * s;
            return CircleRectangle(circleCenter, radius, rectCenter, size);
        }

        /// <summary>
        /// 使用 <see cref="VolumeShape.Offset"/> 自动计算世界中心的便捷重载。
        /// </summary>
        public static bool Intersects2(
            VolumeShape a, Vector2 aPosition,
            VolumeShape b, Vector2 bPosition,
            float scaling = 1f)
        {
            Vector2 centerA = a.GetWorldCenter(aPosition);
            Vector2 centerB = b.GetWorldCenter(bPosition);
            return Intersects(a, centerA, b, centerB, scaling);
        }

        /// <summary>
        /// 圆与圆是否相交。
        /// </summary>
        public static bool CircleCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            if (radiusA <= 0f || radiusB <= 0f)
                return false;

            float distSq = (centerA - centerB).sqrMagnitude;
            float radiusSum = radiusA + radiusB;
            return distSq <= radiusSum * radiusSum;
        }

        /// <summary>
        /// 轴对齐矩形与轴对齐矩形是否相交（AABB 相交）。
        /// </summary>
        public static bool RectangleRectangle(Vector2 centerA, Vector2 sizeA, Vector2 centerB, Vector2 sizeB)
        {
            if (sizeA.x <= 0f || sizeA.y <= 0f || sizeB.x <= 0f || sizeB.y <= 0f)
                return false;

            Vector2 halfA = sizeA * 0.5f;
            Vector2 halfB = sizeB * 0.5f;
            return Mathf.Abs(centerA.x - centerB.x) <= halfA.x + halfB.x
                && Mathf.Abs(centerA.y - centerB.y) <= halfA.y + halfB.y;
        }

        /// <summary>
        /// 圆与轴对齐矩形是否相交。
        /// 算法：把圆心钳制到矩形内部，得到矩形上离圆心最近的点，
        /// 再判断该点与圆心的距离是否小于等于半径。
        /// </summary>
        public static bool CircleRectangle(Vector2 circleCenter, float radius, Vector2 rectCenter, Vector2 rectSize)
        {
            if (radius <= 0f || rectSize.x <= 0f || rectSize.y <= 0f)
                return false;

            Vector2 half = rectSize * 0.5f;
            Vector2 closest = new Vector2(
                Mathf.Clamp(circleCenter.x, rectCenter.x - half.x, rectCenter.x + half.x),
                Mathf.Clamp(circleCenter.y, rectCenter.y - half.y, rectCenter.y + half.y));

            Vector2 delta = circleCenter - closest;
            return delta.sqrMagnitude <= radius * radius + k_Epsilon;
        }

        /// <summary>
        /// 在判断是否相交的同时，计算两个形状的穿透深度（沿中心连线方向，从 A 指向 B）。
        /// 不相交时返回 false，<paramref name="overlap"/> 为 0。
        /// </summary>
        public static bool TryGetOverlap(
            VolumeShape a, Vector2 centerA,
            VolumeShape b, Vector2 centerB,
            out float overlap,
            float scaling = 1f)
        {
            overlap = 0f;
            if (a == null || b == null)
                return false;

            float s = Mathf.Max(scaling, 0f);

            switch (a.Shape)
            {
                case VolumeShapeType.Circle when b.Shape == VolumeShapeType.Circle:
                {
                    float rA = Mathf.Max(0f, a.Radius) * s;
                    float rB = Mathf.Max(0f, b.Radius) * s;
                    if (!CircleCircle(centerA, rA, centerB, rB))
                        return false;
                    overlap = Mathf.Max(0f, (rA + rB) - (centerA - centerB).magnitude);
                    return true;
                }
                case VolumeShapeType.Rectangle when b.Shape == VolumeShapeType.Rectangle:
                {
                    Vector2 sizeA = a.Size * s;
                    Vector2 sizeB = b.Size * s;
                    if (!RectangleRectangle(centerA, sizeA, centerB, sizeB))
                        return false;
                    Vector2 halfA = sizeA * 0.5f;
                    Vector2 halfB = sizeB * 0.5f;
                    float overlapX = (halfA.x + halfB.x) - Mathf.Abs(centerA.x - centerB.x);
                    float overlapY = (halfA.y + halfB.y) - Mathf.Abs(centerA.y - centerB.y);
                    overlap = Mathf.Min(overlapX, overlapY);
                    return true;
                }
            }

            // Circle-Rectangle 穿透深度：圆心到矩形最近点的距离与半径之差
            bool aIsCircle = a.Shape == VolumeShapeType.Circle;
            VolumeShape circleV = aIsCircle ? a : b;
            Vector2 circleCenter = aIsCircle ? centerA : centerB;
            VolumeShape rectV = aIsCircle ? b : a;
            Vector2 rectCenter = aIsCircle ? centerB : centerA;

            float radius = Mathf.Max(0f, circleV.Radius) * s;
            Vector2 size = rectV.Size * s;
            if (!CircleRectangle(circleCenter, radius, rectCenter, size))
                return false;

            Vector2 half = size * 0.5f;
            Vector2 closest = new Vector2(
                Mathf.Clamp(circleCenter.x, rectCenter.x - half.x, rectCenter.x + half.x),
                Mathf.Clamp(circleCenter.y, rectCenter.y - half.y, rectCenter.y + half.y));
            float dist = (circleCenter - closest).magnitude;
            overlap = Mathf.Max(0f, radius - dist);
            return true;
        }
    }
}