using System;
using MoreMountains.Tools;

namespace MoreMountains
{
    /// <summary>
    /// 金币掉落配置 - 用于配置金币的掉落动画表现
    ///
    /// 椭圆轨迹模型说明：
    /// 在3D空间中想象一个倾斜的正圆（圆平面与XY平面成一个倾角），
    /// 金币沿这个正圆走一段弧，投影到XY平面就是一段椭圆弧。
    /// - 椭圆长半轴 a = HorizontalSpread   （控制落点的水平距离）
    /// - 椭圆短半轴 b = DropHeight         （控制抛物线的Y方向高度）
    /// - 落地点和起点都在同一Y（地面），所以 GroundHeight 不再是配置项
    /// - 反弹时 R（= HorizontalSpread）按 BounceDecayRatio 衰减，自动形成"高度同步衰减"的效果
    /// </summary>
    [Serializable]
    public class CoinDropConfig
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
        /// 椭圆长半轴 - 即3D正圆半径 R
        /// 等于落点的水平距离（X方向投影）。
        /// 即"在XY平面上，从起点到最远落点的距离"。
        /// </summary>
        public float HorizontalSpread = 1.5f;

        /// <summary>
        /// 椭圆短半轴 b - 即抛物线在Y方向上的最大高度
        /// 由3D正圆相对XY平面的倾角θ决定：b = R * sin(θ)
        /// 反弹时 R 衰减，高度 b 也按相同比例自动衰减
        /// </summary>
        public float DropHeight = 1.2f;

        /// <summary>
        /// 水平方向扩散范围（多枚金币散射时的随机角度范围，度）
        /// 注意：这是多枚金币掉落的"散射方向"角度，不是椭圆本身参数
        /// </summary>
        public float DirectionSpreadAngle = 30f;

        /// <summary>
        /// 反弹方向随机偏转角度范围（度，对称分布）
        /// 反弹时新方向在原方向附近 ±BounceSpreadAngle 之间随机偏移
        /// 默认 = DirectionSpreadAngle，这样反弹和多枚散射保持一致
        /// </summary>
        public float BounceSpreadAngle = -1f; // -1 表示使用 DirectionSpreadAngle

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static CoinDropConfig Default => new()
        {
            DropDuration = 0.5f,
            BounceCount = 2,
            BounceDecayRatio = 0.6f,
            HorizontalSpread = 1.5f,
            DropHeight = 1.2f,
            DirectionSpreadAngle = 30f,
            BounceSpreadAngle = -1f, // 使用 DirectionSpreadAngle
        };

        /// <summary>
        /// 创建快速掉落配置（短时间，少反弹）
        /// </summary>
        public static CoinDropConfig QuickDrop => new()
        {
            DropDuration = 0.3f,
            BounceCount = 1,
            BounceDecayRatio = 0.5f,
            HorizontalSpread = 1.0f,
            DropHeight = 0.8f,
            DirectionSpreadAngle = 20f,
            BounceSpreadAngle = -1f,
        };

        /// <summary>
        /// 创建华丽掉落配置（长时间，多反弹）
        /// </summary>
        public static CoinDropConfig FancyDrop => new()
        {
            DropDuration = 0.8f,
            BounceCount = 2,
            BounceDecayRatio = 0.7f,
            HorizontalSpread = 2.0f,
            DropHeight = 1.6f,
            DirectionSpreadAngle = 45f,
            BounceSpreadAngle = -1f,
        };

        /// <summary>
        /// 创建大额金币掉落配置（更远的落点，更高的弹跳）
        /// </summary>
        public static CoinDropConfig LargeDrop => new()
        {
            DropDuration = 0.7f,
            BounceCount = 2,
            BounceDecayRatio = 0.65f,
            HorizontalSpread = 2.5f,
            DropHeight = 2.0f,
            DirectionSpreadAngle = 40f,
            BounceSpreadAngle = -1f,
        };
    }

    /// <summary>
    /// 金币拾取配置 - 用于配置金币被玩家吸引的动画表现
    /// </summary>
    [Serializable]
    public class CoinPickupConfig
    {
        /// <summary>
        /// 拾取动画时长
        /// </summary>
        public float PickupDuration = 0.3f;

        /// <summary>
        /// 拾取动画曲线（当前用于缩放/旋转/淡出，位置使用二次贝塞尔曲线）
        /// </summary>
        public MMTween.MMTweenCurve PickupCurve = MMTween.MMTweenCurve.EaseOutCubic;

        /// <summary>
        /// 拾取时旋转总角度（度）
        /// </summary>
        public float RotationDegrees = 720f;

        /// <summary>
        /// 拾取时最小缩放
        /// </summary>
        public float MinScale = 0.3f;

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static CoinPickupConfig Default => new()
        {
            PickupDuration = 0.3f,
            PickupCurve = MMTween.MMTweenCurve.EaseOutCubic,
            RotationDegrees = 720f,
            MinScale = 0.3f
        };

        /// <summary>
        /// 创建快速拾取配置
        /// </summary>
        public static CoinPickupConfig Quick => new()
        {
            PickupDuration = 0.2f,
            PickupCurve = MMTween.MMTweenCurve.EaseOutQuadratic,
            RotationDegrees = 540f,
            MinScale = 0.2f
        };

        /// <summary>
        /// 创建华丽拾取配置（带更长动画）
        /// </summary>
        public static CoinPickupConfig Fancy => new()
        {
            PickupDuration = 0.5f,
            PickupCurve = MMTween.MMTweenCurve.EaseInOutCubic,
            RotationDegrees = 1080f,
            MinScale = 0.4f
        };
    }
}
