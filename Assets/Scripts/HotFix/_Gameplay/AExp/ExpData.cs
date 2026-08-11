using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 经验值掉落配置 - 用于配置经验值物品的掉落动画表现
    ///
    /// 椭圆轨迹模型说明：
    /// 在3D空间中想象一个倾斜的正圆（圆平面与XY平面成一个倾角），
    /// 经验值物品沿这个正圆走一段弧，投影到XY平面就是一段椭圆弧。
    /// - 椭圆长半轴 a = HorizontalSpread   （控制落点的水平距离）
    /// - 椭圆短半轴 b = DropHeight         （控制抛物线的Y方向高度）
    /// - 落地点和起点都在同一Y（地面），所以 GroundHeight 不再是配置项
    /// - 反弹时 R（= HorizontalSpread）按 BounceDecayRatio 衰减，自动形成"高度同步衰减"的效果
    /// </summary>
    [Serializable]
    public class ExpDropConfig
    {
        /// <summary>
        /// 掉落动画总时长（所有抛物线段累加）
        /// </summary>
        public float DropDuration = 0.5f;

        /// <summary>
        /// 反弹次数（2-3段抛物线）
        /// 注意：实际段数 = BounceCount + 1（初始抛物线 + 反弹次数）
        /// 例如 BounceCount=2 表示 3段抛物线（1次初始 + 2次反弹）
        /// </summary>
        public int BounceCount = 2;

        /// <summary>
        /// 反弹衰减比例（0-1），值越小衰减越快
        /// 每段反弹后椭圆长半轴 R 乘以此系数（高度随之同步缩小）
        /// </summary>
        public float BounceDecayRatio = 0.6f;

        /// <summary>
        /// 掉落每段动画的缓动曲线（默认 EaseOutCubic）
        /// 控制每段贝塞尔曲线的 t 输入：t = MMTween.Evaluate(linearT, BounceCurve)
        /// 影响经验值物品在每段中的位置、视觉高度、缩放统一缓动
        /// </summary>
        public MMTween.MMTweenCurve BounceCurve = MMTween.MMTweenCurve.EaseOutCubic;

        /// <summary>
        /// 椭圆长半轴 - 即3D正圆半径 R
        /// 等于落点的水平距离（X方向投影）。
        /// 即"在XY平面上，从起点到最远落点的距离"。
        /// </summary>
        public float HorizontalSpread = 1.2f;

        /// <summary>
        /// 椭圆短半轴 b - 即抛物线在Y方向上的最大高度
        /// 由3D正圆相对XY平面的倾角θ决定：b = R * sin(θ)
        /// 反弹时 R 衰减，高度 b 也按相同比例自动衰减
        /// </summary>
        public float DropHeight = 1.2f;

        /// <summary>
        /// 水平方向扩散范围（多个经验值物品散射时的随机角度范围，度）
        /// 注意：这是多个经验值物品掉落的"散射方向"角度，不是椭圆本身参数
        /// </summary>
        public float DirectionSpreadAngle = 25f;

        /// <summary>
        /// 障碍物层（经验值物品最终落点不能穿越的层，例如墙壁、地面边缘）
        /// 用于 Physics2D.CircleCast 裁剪最终落点
        /// 设为 0 表示不启用障碍物检测
        /// </summary>
        public LayerMask ObstacleLayers = 0;

        /// <summary>
        /// 经验值物品碰撞半径（用于 CircleCast 半径）
        /// 让经验值物品的中心 + 半径不会穿进障碍物
        /// </summary>
        public float ExpRadius = 0.16f;

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static ExpDropConfig Default => new()
        {
            DropDuration = 0.6f,
            BounceCount = 2,
            BounceDecayRatio = 0.6f,
            HorizontalSpread = 1.2f,
            DropHeight = 1.2f,
            DirectionSpreadAngle = 25f,
            ObstacleLayers = LayerManager.Obstacles_Mask,
            ExpRadius = 0.14F,
        };

        /// <summary>
        /// 创建快速掉落配置（短时间，少反弹）
        /// </summary>
        public static ExpDropConfig QuickDrop => new()
        {
            DropDuration = 0.3f,
            BounceCount = 1,
            BounceDecayRatio = 0.5f,
            HorizontalSpread = 1.0f,
            DropHeight = 0.8f,
            DirectionSpreadAngle = 20f,
            ObstacleLayers = LayerManager.Obstacles_Mask,
            ExpRadius = 0.14F,
        };

        /// <summary>
        /// 创建华丽掉落配置（长时间，多反弹）
        /// </summary>
        public static ExpDropConfig FancyDrop => new()
        {
            DropDuration = 0.8f,
            BounceCount = 2,
            BounceDecayRatio = 0.7f,
            HorizontalSpread = 1.8f,
            DropHeight = 1.5f,
            DirectionSpreadAngle = 40f,
            ObstacleLayers = LayerManager.Obstacles_Mask,
            ExpRadius = 0.14F,
        };

        /// <summary>
        /// 创建大额经验值掉落配置（更远的落点，更高的弹跳）
        /// </summary>
        public static ExpDropConfig LargeDrop => new()
        {
            DropDuration = 0.7f,
            BounceCount = 2,
            BounceDecayRatio = 0.65f,
            HorizontalSpread = 2.2f,
            DropHeight = 1.8f,
            DirectionSpreadAngle = 35f,
            ObstacleLayers = LayerManager.Obstacles_Mask,
            ExpRadius = 0.14F,
        };
    }

    /// <summary>
    /// 经验值拾取配置 - 用于配置经验值物品被玩家吸引时的动画表现
    /// 拾取动画分为两段：
    ///   1) 远离阶段：经验值物品先朝着远离玩家(敌人被击杀的)方向飞行一小段距离
    ///   2) 飞向阶段：再折返飞向玩家位置，吸取到玩家身上并消失
    /// 经验值实际到账发生在飞向玩家阶段结束之后（表现更接近直觉，体验更好）
    /// </summary>
    [Serializable]
    public class ExpPickupConfig
    {
        /// <summary>
        /// 远离阶段时长（秒）
        /// 经验值物品先飞离玩家一小段距离
        /// </summary>
        public float FleeDuration = 0.15f;

        /// <summary>
        /// 飞向玩家阶段速度（世界单位/秒）
        /// 经验值物品从远离点飞向玩家位置时的飞行速度
        /// 到位置后触发实际到账
        /// </summary>
        public float FlyToPlayerSpeed = 12f;

        /// <summary>
        /// 远离阶段距离（世界单位）
        /// 经验值物品飞离玩家的距离（从当前位置沿远离玩家方向）
        /// </summary>
        public float FleeDistance = 1.2f;

        /// <summary>
        /// 远离阶段缓动曲线
        /// </summary>
        public MMTween.MMTweenCurve FleeCurve = MMTween.MMTweenCurve.EaseOutQuadratic;

        /// <summary>
        /// 飞向阶段缓动曲线
        /// </summary>
        public MMTween.MMTweenCurve FlyToPlayerCurve = MMTween.MMTweenCurve.EaseInQuadratic;

        /// <summary>
        /// 拾取时旋转总角度（度，飞向阶段累积）
        /// </summary>
        public float RotationDegrees = 540f;

        /// <summary>
        /// 拾取时最小缩放（视为缩小消失，0-1）
        /// </summary>
        public float MinScale = 0.2f;

        /// <summary>
        /// 飞向玩家的过程中是否锁定追踪玩家实时位置（追踪会更自然）
        /// </summary>
        public bool TrackPlayerDuringFly = true;

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static ExpPickupConfig Default => new()
        {
            FleeDuration = 0.25f,
            FlyToPlayerSpeed = 12f,
            FleeDistance = 1.0f,
            FleeCurve = MMTween.MMTweenCurve.EaseOutQuadratic,
            FlyToPlayerCurve = MMTween.MMTweenCurve.EaseInQuadratic,
            RotationDegrees = 0f,
            MinScale = 1f,
            TrackPlayerDuringFly = true,
        };

        /// <summary>
        /// 快速拾取配置
        /// </summary>
        public static ExpPickupConfig Quick => new()
        {
            FleeDuration = 0.08f,
            FlyToPlayerSpeed = 18f,
            FleeDistance = 0.8f,
            FleeCurve = MMTween.MMTweenCurve.EaseOutQuadratic,
            FlyToPlayerCurve = MMTween.MMTweenCurve.EaseInQuadratic,
            RotationDegrees = 360f,
            MinScale = 0.2f,
            TrackPlayerDuringFly = true,
        };

        /// <summary>
        /// 华丽拾取配置
        /// </summary>
        public static ExpPickupConfig Fancy => new()
        {
            FleeDuration = 0.25f,
            FlyToPlayerSpeed = 8f,
            FleeDistance = 1.6f,
            FleeCurve = MMTween.MMTweenCurve.EaseOutCubic,
            FlyToPlayerCurve = MMTween.MMTweenCurve.EaseInCubic,
            RotationDegrees = 720f,
            MinScale = 0.1f,
            TrackPlayerDuringFly = true,
        };
    }
}
