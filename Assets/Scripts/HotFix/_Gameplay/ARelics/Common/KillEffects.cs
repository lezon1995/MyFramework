using UniStats;

namespace MoreMountains;

/// <summary>
/// Executioner - 处刑者
/// 血量低于20%的敌人受到额外50%伤害
/// [设计文案] 杀手本能，对弱小的敌人毫不留情
/// </summary>
public class Executioner : ARelic
{
    public static string ID = "Executioner";

    public Executioner() : base(ID, "Executioner.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现对低血量敌人增伤
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (brick.CurrentHealth / brick.MaxHealth < 0.2f)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.5f);
    //     }
    // }

    public override ARelic makeCopy() => new Executioner();
}

/// <summary>
/// FirstBlood - 先手必杀
/// 每波次第一个击杀获得双倍奖励
/// [设计文案] 第一个击杀的奖励翻倍
/// </summary>
public class FirstBlood : ARelic
{
    public static string ID = "FirstBlood";

    public FirstBlood() : base(ID, "FirstBlood.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现首杀奖励翻倍
    // private bool firstKillThisWave = true;
    //
    // public override void atBattleStart()
    // {
    //     firstKillThisWave = true;
    // }
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     if (firstKillThisWave)
    //     {
    //         firstKillThisWave = false;
    //         // 奖励翻倍：经验+100%, 金币+100%
    //     }
    // }

    public override ARelic makeCopy() => new FirstBlood();
}

/// <summary>
/// ComboMaster - 连击大师
/// 连击数达到10时伤害+20%
/// [设计文案] 连击的艺术家，积累连击释放力量
/// </summary>
public class ComboMaster : ARelic
{
    public static string ID = "ComboMaster";

    public ComboMaster() : base(ID, "ComboMaster.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现连击计数和增伤
    // private int comboCount = 0;
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     comboCount++;
    //     if (comboCount >= 10)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.2f);
    //     }
    // }
    //
    // public override void onFightingPhaseEnd(APlayer p)
    // {
    //     comboCount = 0;
    // }

    public override ARelic makeCopy() => new ComboMaster();
}

/// <summary>
/// Massacre - 大屠杀
/// 快速击杀5个敌人后获得短暂无敌
/// [设计文案] 杀戮的狂潮，击杀后获得保护
/// </summary>
public class Massacre : ARelic
{
    public static string ID = "Massacre";

    public Massacre() : base(ID, "Massacre.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现快速击杀后无敌
    // private int killCount = 0;
    // private bool isInvincible = false;
    // private float invincibleTimer = 0f;
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     killCount++;
    //     if (killCount >= 5 && !isInvincible)
    //     {
    //         isInvincible = true;
    //         invincibleTimer = 2f; // 无敌2秒
    //         killCount = 0;
    //     }
    // }
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     if (isInvincible)
    //     {
    //         invincibleTimer -= dt;
    //         if (invincibleTimer <= 0f)
    //             isInvincible = false;
    //     }
    // }

    public override ARelic makeCopy() => new Massacre();
}

/// <summary>
/// LethalPrecision - 致命精准
/// 暴击必定击杀普通敌人
/// [设计文案] 一击必杀的艺术
/// </summary>
public class LethalPrecision : ARelic
{
    public static string ID = "LethalPrecision";

    public LethalPrecision() : base(ID, "LethalPrecision.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现暴击必杀机制
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (ball.IsCriticalHit && brick.IsNormalEnemy)
    //     {
    //         brick.CurrentHealth = 0; // 直接击杀
    //     }
    // }

    public override ARelic makeCopy() => new LethalPrecision();
}

/// <summary>
/// Slayer - 杀手
/// 击杀精英敌人后伤害+15%（持续至波次结束）
/// [设计文案] 精英猎手，获得击杀奖励
/// </summary>
public class Slayer : ARelic
{
    public static string ID = "Slayer";

    public Slayer() : base(ID, "Slayer.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现精英击杀增伤
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     if (brick.IsElite)
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.15f);
    //     }
    // }
    //
    // public override void atBattleStart()
    // {
    //     // 清除之前的增伤效果
    // }

    public override ARelic makeCopy() => new Slayer();
}

/// <summary>
/// Destroyer - 毁灭者
/// 击杀敌人时产生小爆炸
/// [设计文案] 毁灭的力量，击杀引发爆炸
/// </summary>
public class Destroyer : ARelic
{
    public static string ID = "Destroyer";

    public Destroyer() : base(ID, "Destroyer.png", RelicTier.UNCOMMON, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现击杀爆炸
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     // 在砖块位置产生小爆炸
    //     // Radius = 1, Damage = ball.Damage * 0.3
    // }

    public override ARelic makeCopy() => new Destroyer();
}

/// <summary>
/// Reaper - 收割者
/// 每击杀10个敌人恢复5%最大生命
/// [设计文案] 生命的收割者，用敌人的生命治愈自己
/// </summary>
public class Reaper : ARelic
{
    public static string ID = "Reaper";

    public Reaper() : base(ID, "Reaper.png", RelicTier.RARE, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现击杀回血
    // private int killCount = 0;
    //
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     killCount++;
    //     if (killCount >= 10)
    //     {
    //         killCount = 0;
    //         p.Heal(p.MaxHealth * 0.05f);
    //     }
    // }

    public override ARelic makeCopy() => new Reaper();
}

/// <summary>
/// Hunter - 猎人
/// 对特定类型敌人伤害+25%
/// [设计文案] 专业的猎人，针对特定目标
/// </summary>
public class Hunter : ARelic
{
    public static string ID = "Hunter";

    public Hunter() : base(ID, "Hunter.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要实现对特定敌人类型的增伤
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (brick.BrickType == TargetType) // 假设有目标类型
    //     {
    //         ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
    //         stat.AddPct(0.25f);
    //     }
    // }

    public override ARelic makeCopy() => new Hunter();
}

/// <summary>
/// Annihilator - 歼灭者
/// 击杀时有20%概率清空屏幕
/// [设计文案] 毁灭一切的力量
/// </summary>
public class Annihilator : ARelic
{
    public static string ID = "Annihilator";

    public Annihilator() : base(ID, "Annihilator.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现清屏机制
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     if (UnityEngine.Random.value < 0.2f)
    //     {
    //         // 清除屏幕所有敌人
    //         foreach (var enemy in p.GetActiveBricks())
    //         {
    //             enemy.Destroy();
    //         }
    //     }
    // }

    public override ARelic makeCopy() => new Annihilator();
}
