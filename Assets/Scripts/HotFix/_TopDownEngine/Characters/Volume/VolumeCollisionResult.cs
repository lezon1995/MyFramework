using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 碰撞结果数据结构
    /// </summary>
    public struct VolumeCollisionResult
    {
        public TopDownController2D Other;// 碰撞检测到的其他实体
        public Vector2 Direction;// 从本实体指向对方的归一化方向
        public float Distance;// 两实体之间的距离
        public float CenterDistance;// 两实体圆心之间的距离（未减去半径）
        public float CombinedRadius;// 两实体的半径和
        public float Overlap;// 重叠量（正值表示重叠，负值表示有空隙）
        public float MaxAllowedOverlap;// 重叠方向上的最大可允许重叠量
        public float RequiredSeparation;// 实际需要修正的重叠量
        public float OtherEffectiveRadius;// 对方实体的有效半径
        public bool IsColliding => Overlap > 0;// 是否发生碰撞
        public bool IsExceedingMaxOverlap => RequiredSeparation > 0;// 是否超出最大允许重叠

        public VolumeCollisionResult(TopDownController2D self, TopDownController2D other)
        {
            Other = other;
            CenterDistance = Vector2.Distance(self.Position, other.Position);
            CombinedRadius = self.Radius + other.Radius;
            Overlap = CombinedRadius - CenterDistance;

            if (Overlap > 0)
            {
                Direction = (other.Position - self.Position).normalized;
                Distance = CenterDistance;
            }
            else
            {
                Direction = Vector2.zero;
                Distance = CenterDistance;
            }

            OtherEffectiveRadius = other.Radius * (1f - other.MaxOverlapRatio);
            MaxAllowedOverlap = self.MaxOverlapDistance + other.MaxOverlapDistance;
            RequiredSeparation = Mathf.Max(0, Overlap - MaxAllowedOverlap);
        }
    }

    /// <summary>
    /// 击退链式传播结果
    /// </summary>
    public struct KnockbackChainResult
    {
        public TopDownController2D Target; // 被击退的实体
        public float OriginalForce; // 原始击退力
        public float ActualForce; // 实际受到的击退力（经过衰减后）
        public Vector2 Direction; // 击退方向
        public int ChainLevel; // 传播层级（0=直接击退，1=被连带击退，2=再下一级...）
        public bool IsValid => Target && ActualForce > 0; // 击退是否有效
    }

    /// <summary>
    /// 体积碰撞事件数据
    /// </summary>
    public struct VolumeCollisionEvent
    {
        public TopDownController2D Self;
        public TopDownController2D Other;
        public VolumeCollisionResult Result;
        public float DeltaTime;
    }

    /// <summary>
    /// 击退事件数据
    /// </summary>
    public struct KnockbackEvent
    {
        public TopDownController2D Source;
        public TopDownController2D Target;
        public Vector2 Direction;
        public float OriginalForce;
        public float ActualForce;
        public int ChainLevel;
    }
}
