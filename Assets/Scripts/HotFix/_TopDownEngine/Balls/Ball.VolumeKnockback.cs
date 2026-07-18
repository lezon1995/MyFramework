using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 球的击退能力接口
    /// 当球击退怪物时，触发链式击退效果
    /// </summary>
    public interface IBallKnockbackSource
    {
        float GetKnockbackForce();// 获取击退力
        Vector2 GetKnockbackDirection();// 获取击退方向
        bool IsChainKnockbackEnabled();// 是否启用链式击退
    }

    /// <summary>
    /// 球的链式击退处理扩展
    /// 当球击中怪物并击退时，自动触发链式击退
    /// </summary>
    public static class BallVolumeKnockbackExtensions
    {
        /// <summary>
        /// 当球击退一个怪物时，触发链式击退
        /// </summary>
        /*public static void TriggerChainKnockbackOnHit(this Ball ball, Vector2 hitNormal)
        {
            if (ball == null || VolumeManager.Instance == null)
                return;

            if (!ball.IsChainKnockbackEnabled())
                return;

            // 获取球的击退力
            float knockbackForce = ball.GetKnockbackForce();
            if (knockbackForce < VolumeManager.Instance.MinChainKnockbackForce)
                return;

            // 从碰撞法线获取击退方向
            Vector2 knockbackDir = -hitNormal;

            // 查找被击中的怪物
            if (ball.lastHittable is Brick brick)
            {
                if (brick.TryGetComponent<TopDownController2D>(out var body))
                {
                    // 对被击中的怪物施打击退
                    VolumeManager.Instance.ApplyKnockback(body, knockbackDir, knockbackForce);
                }
            }
        }*/

        /// <summary>
        /// 当球对怪物造成伤害时，同时施打击退
        /// </summary>
        /*public static void ApplyKnockbackWithDamage(this Ball ball, Brick brick, Vector2 hitNormal)
        {
            if (ball == null || brick == null) 
                return;

            // 触发链式击退
            ball.TriggerChainKnockbackOnHit(hitNormal);
        }*/
    }

    /// <summary>
    /// 球的链式击退Power
    /// 附加到球上以启用链式击退能力
    /// </summary>
    public class BallPower_ChainKnockback : BallPower
    {
        [NonSerialized] public float KnockbackForce = 5f;
        [NonSerialized] public bool ChainEnabled = true;
        [NonSerialized] public float ChainDecay = 0.6f;

        public override void onCreate()
        {
            base.onCreate();
            ID = "ChainKnockback";
            KnockbackForce = 5f;
            ChainEnabled = true;
            ChainDecay = 0.6f;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            KnockbackForce = 0;
            ChainEnabled = false;
            ChainDecay = 0;
        }

        public override void onGainPower(Ball ball)
        {
        }

        public override void onHitBrick(Brick brick, Vector2 hitNormal)
        {
            if (!ChainEnabled || VolumeManager.Instance == null)
                return;

            float force = KnockbackForce;
            Vector2 knockbackDir = -hitNormal;
            VolumeManager.Instance.ApplyKnockback(brick.Controller2D, knockbackDir, force);
        }

        public override void onLosePower(Ball ball)
        {
        }
    }

    /// <summary>
    /// 球的体积感扩展
    /// 允许球与怪物之间的交互计算
    /// </summary>
    public static class BallVolumeExtensions
    {
        /// <summary>
        /// 获取球的有效碰撞半径
        /// </summary>
        public static float GetEffectiveRadius(this Ball ball)
        {
            return ball?.Radius ?? 0f;
        }

        /// <summary>
        /// 获取球的当前位置
        /// </summary>
        public static Vector2 GetPosition(this Ball ball)
        {
            return ball?.curPos ?? Vector2.zero;
        }

        /// <summary>
        /// 检查球是否在指定实体的碰撞范围内
        /// </summary>
        public static bool IsInRangeOf(this Ball ball, TopDownController2D entity, float extraRadius = 0f)
        {
            if (ball == null || entity == null) return false;

            float dist = Vector2.Distance(ball.curPos, entity.Position);
            return dist <= ball.Radius + entity.Radius + extraRadius;
        }

        /// <summary>
        /// 计算球对实体的击退方向
        /// </summary>
        public static Vector2 CalculateKnockbackDirectionTo(this Ball ball, TopDownController2D entity)
        {
            if (ball == null || entity == null) return Vector2.zero;

            Vector2 dir = entity.Position - (Vector2)ball.curPos;
            return dir.normalized;
        }
    }
}