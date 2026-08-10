using System;
using UnityEngine;

namespace MoreMountains
{
    public enum VolumeShapeType
    {
        Circle,
        Rectangle
    }

    /// <summary>
    /// 体积碰撞形状。矩形始终与坐标轴平行，不使用 Transform 的旋转。
    /// </summary>
    [Serializable]
    public class VolumeShape
    {
        [Tooltip("体积形状：圆形或与坐标轴平行的矩形")]
        public VolumeShapeType Shape = VolumeShapeType.Circle;

        [Tooltip("相对于 TopDownController2D.Position 的中心偏移")]
        public Vector2 Offset;

        [Tooltip("圆形半径")]
        [Min(0f)]
        public float Radius = 0.5f;

        [Tooltip("矩形尺寸（宽、高）")]
        public Vector2 Size = Vector2.one;

        public Vector2 GetWorldCenter(Vector2 position)
        {
            return position + Offset;
        }

        /// <summary>
        /// 形状在指定轴上的投影半径。轴必须是归一化向量。
        /// </summary>
        public float GetProjectionRadius(Vector2 axis)
        {
            axis = axis.sqrMagnitude > 0.000001f ? axis.normalized : Vector2.right;
            if (Shape == VolumeShapeType.Rectangle)
            {
                Vector2 halfSize = GetHalfSize();
                return Mathf.Abs(axis.x) * halfSize.x + Mathf.Abs(axis.y) * halfSize.y;
            }

            return Mathf.Max(0f, Radius);
        }

        public Vector2 GetHalfSize()
        {
            return new Vector2(Mathf.Max(0f, Size.x), Mathf.Max(0f, Size.y)) * 0.5f;
        }

        /// <summary>
        /// 用于空间分区的包围圆半径。
        /// </summary>
        public float BoundingRadius
        {
            get
            {
                return Shape == VolumeShapeType.Rectangle
                    ? GetHalfSize().magnitude
                    : Mathf.Max(0f, Radius);
            }
        }

        public Vector2 GetWorldSize()
        {
            return Shape == VolumeShapeType.Rectangle ? Size : Vector2.one * (Radius * 2f);
        }
    }
}
