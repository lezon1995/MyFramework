using UniStats;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// BerserkerSoul - 狂战士之魂
/// 血量越低伤害越高（低于50%时+20%伤害）
/// </summary>
public class BerserkerSoul : ARelic
{
    public static string ID = "BerserkerSoul";

    public BerserkerSoul() : base(ID, "BerserkerSoul.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
    }

    // TODO: 需要根据玩家当前血量百分比来调整伤害
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     var healthPercent = p.CurrentHealth / p.MaxHealth;
    //     if (healthPercent < 0.5f)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.2f);
    //     }
    // }

    public override ARelic makeCopy() => new BerserkerSoul();
}

/// <summary>
/// MirrorImage - 镜像
/// 发射的球有15%概率产生一个复制球
/// [设计文案] 神秘的镜像效果，有概率复制发射的球
/// </summary>
public class MirrorImage : ARelic
{
    public static string ID = "MirrorImage";

    public MirrorImage() : base(ID, "MirrorImage.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要在发射逻辑中添加15%概率产生复制球的机制
    // public override void onShootBall(Ball ball)
    // {
    //     if (Random.value < 0.15f)
    //     {
    //         // 创建复制球
    //         var copyBall = ball.Clone();
    //         // 设置复制球的位置稍微偏移
    //         copyBall.transform.position = ball.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
    //     }
    // }

    public override ARelic makeCopy() => new MirrorImage();
}

/// <summary>
/// LightningCore - 闪电核心
/// 10%概率造成连锁闪电
/// [设计文案] 闪电在敌人间跳跃，造成连锁伤害
/// </summary>
public class LightningCore : ARelic
{
    public static string ID = "LightningCore";

    public LightningCore() : base(ID, "LightningCore.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现闪电链效果
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (Random.value < 0.1f)
    //     {
    //         // 在附近敌人间释放闪电链
    //         // Damage = ball.Damage * 0.5, 可以跳跃3-5个敌人
    //     }
    // }

    public override ARelic makeCopy() => new LightningCore();
}

/// <summary>
/// TimeWarp - 时间扭曲
/// 每击杀5个砖块，攻速临时+5%
/// </summary>
public class TimeWarp : ARelic
{
    public static string ID = "TimeWarp";
    private int killCount = 0;

    public TimeWarp() : base(ID, "TimeWarp.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    {
        killCount++;
        if (killCount >= 5)
        {
            killCount = 0;
            ball.GetStat(Ball.Stat.AS, out var stat);
            stat.AddPct(0.05f);
        }
    }

    public override ARelic makeCopy() => new TimeWarp();
}

/// <summary>
/// DragonHeart - 龙心
/// 生命上限+25，生命回复+2
/// </summary>
public class DragonHeart : ARelic
{
    public static string ID = "DragonHeart";

    public DragonHeart() : base(ID, "DragonHeart.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new DragonHeart();
}

/// <summary>
/// ShadowCloak - 暗影斗篷
/// 闪避+10%
/// </summary>
public class ShadowCloak : ARelic
{
    public static string ID = "ShadowCloak";

    public ShadowCloak() : base(ID, "ShadowCloak.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new ShadowCloak();
}

/// <summary>
/// ThunderHammer - 雷神之锤
/// 暴击伤害+30%
/// </summary>
public class ThunderHammer : ARelic
{
    public static string ID = "ThunderHammer";

    public ThunderHammer() : base(ID, "ThunderHammer.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new ThunderHammer();
}

/// <summary>
/// GravityWell - 重力井
/// 球在边界停留时间延长
/// [设计文案] 重力场效果，球在碰到边界时会缓慢减速
/// </summary>
public class GravityWell : ARelic
{
    public static string ID = "GravityWell";

    public GravityWell() : base(ID, "GravityWell.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要修改球碰到边界时的行为
    // public override void onBallHitBorderBot(APlayer p, Ball ball, BorderBot border, Vector2 normal, ref bool forceReturn)
    // {
    //     // 延长球在边界停留的时间
    //     ball.StickTime += 0.5f;
    // }

    public override ARelic makeCopy() => new GravityWell();
}

/// <summary>
/// MysticOrb - 神秘法球
/// 法术强度+15
/// </summary>
public class MysticOrb : ARelic
{
    public static string ID = "MysticOrb";

    public MysticOrb() : base(ID, "MysticOrb.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    public override ARelic makeCopy() => new MysticOrb();
}

/// <summary>
/// DiamondCore - 钻石核心
/// 所有正面属性+5%
/// [设计文案] 钻石般的坚固核心，全面提升属性
/// </summary>
public class DiamondCore : ARelic
{
    public static string ID = "DiamondCore";

    public DiamondCore() : base(ID, "DiamondCore.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new DiamondCore();
}
