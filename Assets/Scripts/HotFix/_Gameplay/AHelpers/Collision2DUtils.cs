using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 2D碰撞检测相关的静态工具类
/// </summary>
public static class Collision2DUtils
{
    /// <summary>
    /// 在目标位置附近查找随机空置位置
    /// </summary>
    /// <param name="targetPosition">目标中心位置</param>
    /// <param name="rangeWidth">水平范围宽（左右各一半）</param>
    /// <param name="rangeHeight">垂直范围高（上下各一半）</param>
    /// <param name="targetCollider">目标碰撞体（会被排除）</param>
    /// <param name="otherLayers">需要检测的其他碰撞体LayerMask</param>
    /// <param name="maxAttempts">最大尝试次数</param>
    /// <param name="sampleRadius">检测空置时的采样半径</param>
    /// <returns>随机空置位置，如果找不到空置位置返回null</returns>
    public static bool FindRandomEmptyPosition(
        Vector2 targetPosition,
        Vector2 range,
        Collider2D targetCollider,
        LayerMask otherLayers,
        out Vector2 result,
        int maxAttempts = 30)
    {
        float sampleRadius = 0.5F;
        float rangeWidth = range.x / 2F;
        float rangeHeight = range.y / 2F;

        sampleRadius = targetCollider switch
        {
            CircleCollider2D c => c.radius,
            BoxCollider2D b => Mathf.Max(b.size.x / 2F, b.size.y / 2F),
            _ => sampleRadius
        };

        // 生成随机候选位置
        using var a = new ListScope<Vector2>(out var candidates);
        using var _ = new ListScope<Collider2D>(out var colliders);
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(otherLayers);

        for (int i = 0; i < maxAttempts; i++)
        {
            // 在目标位置附近的矩形范围内生成随机点
            var randomX = Random.Range(-rangeWidth, rangeWidth);
            var randomY = Random.Range(-rangeHeight, rangeHeight);
            var candidate = targetPosition + new Vector2(randomX, randomY);

            // 检查该位置是否为空置（不与其他碰撞体相交）
            // 使用OverlapCircle检测，使用目标碰撞体的半径
            var count = Physics2D.OverlapCircle(candidate, sampleRadius, filter, colliders);
            // 如果没有碰撞体，则该位置为空置
            if (count == 0)
            {
                candidates.Add(candidate);
                result = candidate;
                return true;
            }
        }

        // 如果找到了空置位置，随机返回一个
        if (candidates.Count > 0)
        {
            result = candidates[Random.Range(0, candidates.Count)];
            return true;
        }

        result = targetPosition;
        return false;
    }

    /// <summary>
    /// 在目标位置附近查找随机空置位置（使用圆形范围）
    /// </summary>
    /// <param name="targetPosition">目标中心位置</param>
    /// <param name="radius">圆形范围半径</param>
    /// <param name="targetCollider">目标碰撞体</param>
    /// <param name="otherLayers">需要检测的其他碰撞体LayerMask</param>
    /// <param name="maxAttempts">最大尝试次数</param>
    /// <param name="sampleRadius">检测空置时的采样半径</param>
    /// <returns>随机空置位置，如果找不到空置位置返回null</returns>
    public static Vector2? FindRandomEmptyPositionInCircle(
        Vector2 targetPosition,
        float radius,
        Collider2D targetCollider,
        LayerMask otherLayers,
        int maxAttempts = 30)
    {
        float sampleRadius = 0.5F;
        sampleRadius = targetCollider switch
        {
            CircleCollider2D c => c.radius,
            BoxCollider2D b => Mathf.Max(b.size.x / 2F, b.size.y / 2F),
            _ => sampleRadius
        };

        using var a = new ListScope<Vector2>(out var candidates);
        using var _ = new ListScope<Collider2D>(out var colliders);
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(otherLayers);

        for (int i = 0; i < maxAttempts; i++)
        {
            // 在圆形范围内生成随机点
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(0f, radius);
            Vector2 candidate = targetPosition + randomDir * randomDist;

            // 检查该位置是否为空置
            var count = Physics2D.OverlapCircle(candidate, sampleRadius, filter, colliders);
            if (count == 0)
            {
                candidates.Add(candidate);
                return candidate;
            }
        }

        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        return null;
    }

    /// <summary>
    /// 查找所有在目标位置附近的空置位置
    /// </summary>
    /// <param name="targetPosition">目标中心位置</param>
    /// <param name="rangeWidth">水平范围宽</param>
    /// <param name="rangeHeight">垂直范围高</param>
    /// <param name="gridSize">网格间隔</param>
    /// <param name="targetCollider">目标碰撞体</param>
    /// <param name="otherLayers">需要检测的其他碰撞体LayerMask</param>
    /// <param name="sampleRadius">检测空置时的采样半径</param>
    /// <returns>所有空置位置的列表</returns>
    public static List<Vector2> FindAllEmptyPositions(
        Vector2 targetPosition,
        Vector2 range,
        float gridSize,
        Collider2D targetCollider,
        LayerMask otherLayers)
    {
        float sampleRadius = 0.5F;
        float rangeWidth = range.x / 2F;
        float rangeHeight = range.y / 2F;
        sampleRadius = targetCollider switch
        {
            CircleCollider2D c => c.radius,
            BoxCollider2D b => Mathf.Max(b.size.x / 2F, b.size.y / 2F),
            _ => sampleRadius
        };

        using var a = new ListScope<Vector2>(out var emptyPositions);
        using var _ = new ListScope<Collider2D>(out var colliders);
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(otherLayers);

        // 按网格遍历查找
        for (float x = -rangeWidth; x <= rangeWidth; x += gridSize)
        {
            for (float y = -rangeHeight; y <= rangeHeight; y += gridSize)
            {
                Vector2 candidate = targetPosition + new Vector2(x, y);

                var count = Physics2D.OverlapCircle(candidate, sampleRadius, filter, colliders);
                if (count == 0)
                {
                    emptyPositions.Add(candidate);
                }
            }
        }

        return emptyPositions;
    }
}