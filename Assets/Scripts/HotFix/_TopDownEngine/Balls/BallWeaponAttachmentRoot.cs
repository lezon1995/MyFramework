using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    /// <summary>
    /// 圆形布局控制器 - 子节点在圆弧上平均分布
    /// 基准节点位置固定，其他节点绕其排列
    /// </summary>
    public class BallWeaponAttachmentRoot : MonoBehaviour
    {
        [Header("布局设置")]
        [Tooltip("圆的半径")]
        public float radius = 5f;

        [Tooltip("基准节点索引（该节点位置保持不变）")]
        public int baseIndex;

        [Header("调试")]
        public bool drawGizmos = true;

        // 缓存所有子节点
        List<Transform> _children = new();

        void Start()
        {
            RefreshLayout();
        }

        void OnValidate()
        {
            RefreshLayout();
        }

        void OnTransformChildrenChanged()
        {
            // 当子节点变化时，重新计算布局
            RefreshLayout();
        }

        /// <summary>
        /// 刷新布局 - 一次性设置所有子节点位置
        /// </summary>
        [Button]
        public void RefreshLayout()
        {
            // 收集所有子节点
            _children.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    _children.Add(child);
                }
            }

            if (_children.Count == 0)
                return;

            // 计算每个节点之间的角度间隔
            float angleStep = 360f / _children.Count;

            // 确保基准索引在有效范围内
            int baseIdx = Mathf.Clamp(baseIndex, 0, _children.Count - 1);
            // 计算该节点的角度 = 基准角度 + 步数 * 角度间隔
            float baseAngle = Vector3.SignedAngle(_children[baseIdx].localPosition, Vector3.up, Vector3.forward);

            for (int i = 0; i < _children.Count; i++)
            {
                // 基准节点保持不动
                if (i == baseIdx)
                {
                    // 转换为弧度并计算位置
                    float rad = baseAngle * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(
                        Mathf.Sin(rad) * radius,
                        Mathf.Cos(rad) * radius,
                        0
                    );

                    // 设置位置（相对于父对象的本地坐标）
                    _children[baseIdx].localPosition = offset;
                    continue;
                }

                {
                    // 计算到基准节点的步数（顺时针或逆时针取较短路径）
                    int steps = CalculateSteps(i, baseIdx, _children.Count);
                    float angle = baseAngle + steps * angleStep;

                    // 转换为弧度并计算位置
                    float rad = angle * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(
                        Mathf.Sin(rad) * radius,
                        Mathf.Cos(rad) * radius,
                        0
                    );

                    // 设置位置（相对于父对象的本地坐标）
                    _children[i].localPosition = offset;
                }
            }
        }

        /// <summary>
        /// 计算从基准节点到目标节点的步数
        /// 选择顺时针或逆时针中较短的路
        /// </summary>
        int CalculateSteps(int targetIndex, int baseIndex, int totalCount)
        {
            int diff = targetIndex - baseIndex;

            // 如果差值超过一半总数，取反方向
            if (diff > totalCount / 2)
            {
                diff -= totalCount;
            }
            else if (diff < -totalCount / 2)
            {
                diff += totalCount;
            }

            return diff;
        }

        /// <summary>
        /// 运行时动态添加节点（不触发多次设置）
        /// </summary>
        public void AddChild(Transform child)
        {
            child.SetParent(transform);
            RefreshLayout(); // 只调用一次
        }

        /// <summary>
        /// 运行时移除节点（不触发多次设置）
        /// </summary>
        public void RemoveChild(Transform child)
        {
            child.SetParent(null);
            RefreshLayout(); // 只调用一次
        }

        /// <summary>
        /// 改变半径
        /// </summary>
        public void SetRadius(float newRadius)
        {
            radius = newRadius;
            RefreshLayout();
        }

        /// <summary>
        /// 改变基准节点
        /// </summary>
        public void SetBaseIndex(int newBaseIndex)
        {
            baseIndex = newBaseIndex;
            RefreshLayout();
        }

        void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            // 绘制圆
            Gizmos.color = Color.cyan;
            int segments = 64;
            Vector3 prevPoint = transform.position + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
                Vector3 point = transform.position + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            // 绘制半径线
            Gizmos.color = Color.yellow;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    Gizmos.DrawLine(transform.position, child.position);
                }
            }
        }
    }
}