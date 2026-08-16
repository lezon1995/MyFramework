using UnityEngine;

namespace MoreMountains;

/// <summary>
/// BlackHole - 黑洞
/// 吸引附近砖块
/// [设计文案] 引力的深渊，吸聚敌人
/// </summary>
public class BlackHole : ARelic
{
    public static string ID = "BlackHole";

    public BlackHole() : base(ID, "BlackHole.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现黑洞吸引效果
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     var center = p.Position;
    //     var radius = 5f;
    //     foreach (var brick in GetNearbyBricks(center, radius))
    //     {
    //         var dir = (center - brick.Position).normalized;
    //         brick.transform.position += dir * 2f * dt;
    //     }
    // }

    public override ARelic makeCopy() => new BlackHole();
}

/// <summary>
/// TimeFreeze - 时间冻结
/// 暂停所有敌人1秒
/// [设计文案] 冻结时间的魔法
/// </summary>
public class TimeFreeze : ARelic
{
    public static string ID = "TimeFreeze";

    public TimeFreeze() : base(ID, "TimeFreeze.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现时间冻结
    // private bool canFreeze = true;
    //
    // public override void onPlayerTurnBegin(APlayer p)
    // {
    //     if (canFreeze)
    //     {
    //         FreezeAllBricks(1f);
    //         canFreeze = false;
    //     }
    // }
    //
    // public override void onFightingPhaseEnd(APlayer p)
    // {
    //     canFreeze = true;
    // }

    public override ARelic makeCopy() => new TimeFreeze();
}

/// <summary>
/// ChainLightning - 链式闪电
/// 伤害在敌人间传递
/// [设计文案] 闪电在敌人间跳跃
/// </summary>
public class ChainLightning : ARelic
{
    public static string ID = "ChainLightning";

    public ChainLightning() : base(ID, "ChainLightning.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现链式闪电
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     // 从命中点释放闪电链，最多跳跃5个敌人
    //     // Damage = ball.Damage * 0.5
    // }

    public override ARelic makeCopy() => new ChainLightning();
}

/// <summary>
/// MeteorStrike - 陨石打击
/// 随机位置降下陨石
/// [设计文案] 天降正义的陨石
/// </summary>
public class MeteorStrike : ARelic
{
    public static string ID = "MeteorStrike";

    public MeteorStrike() : base(ID, "MeteorStrike.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现陨石效果
    // private float meteorCooldown = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     meteorCooldown -= dt;
    //     if (meteorCooldown <= 0f)
    //     {
    //         meteorCooldown = 10f;
    //         var pos = GetRandomScreenPosition();
    //         SpawnMeteor(pos, 50f); // 伤害50
    //     }
    // }

    public override ARelic makeCopy() => new MeteorStrike();
}

/// <summary>
/// NovaBlast - 新星爆发
/// 清屏技能（每波次一次）
/// [设计文案] 毁灭性的清屏技能
/// </summary>
public class NovaBlast : ARelic
{
    public static string ID = "NovaBlast";

    public NovaBlast() : base(ID, "NovaBlast.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现清屏
    // private bool canUse = true;
    //
    // public override void onPlayerTurnBegin(APlayer p)
    // {
    //     if (canUse)
    //     {
    //         // 显示技能按钮
    //     }
    // }
    //
    // public void TriggerNova(APlayer p)
    // {
    //     // 伤害屏幕内所有敌人
    //     foreach (var brick in GetAllBricks())
    //     {
    //         brick.TakeDamage(100f);
    //     }
    //     canUse = false;
    // }

    public override ARelic makeCopy() => new NovaBlast();
}

/// <summary>
/// Earthquake - 地震
/// 击退所有敌人
/// [设计文案] 大地的愤怒
/// </summary>
public class Earthquake : ARelic
{
    public static string ID = "Earthquake";

    public Earthquake() : base(ID, "Earthquake.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现地震击退
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     if (Random.value < 0.1f)
    //     {
    //         foreach (var b in GetAllBricks())
    //         {
    //             var dir = (b.Position - p.Position).normalized;
    //             b.Knockback(dir, 5f);
    //         }
    //     }
    // }

    public override ARelic makeCopy() => new Earthquake();
}

/// <summary>
/// Blizzard - 暴风雪
/// 减速所有敌人
/// [设计文案] 冰霜的领域
/// </summary>
public class Blizzard : ARelic
{
    public static string ID = "Blizzard";

    public Blizzard() : base(ID, "Blizzard.png", RelicTier.RARE, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现减速效果
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     foreach (var brick in GetAllBricks())
    //     {
    //         brick.AddStatusEffect(StatusEffect.Slow, 0.5f, 1f);
    //     }
    // }

    public override ARelic makeCopy() => new Blizzard();
}

/// <summary>
/// Inferno - 地狱火
/// 全屏灼烧
/// [设计文案] 燃烧一切的地狱之火
/// </summary>
public class Inferno : ARelic
{
    public static string ID = "Inferno";

    public Inferno() : base(ID, "Inferno.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现全屏灼烧
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     foreach (var brick in GetAllBricks())
    //     {
    //         brick.AddStatusEffect(StatusEffect.Burning, 5f, 3f);
    //     }
    // }

    public override ARelic makeCopy() => new Inferno();
}

/// <summary>
/// Tsunami - 海啸
/// 波浪式伤害
/// [设计文案] 海浪般的连续伤害
/// </summary>
public class Tsunami : ARelic
{
    public static string ID = "Tsunami";

    public Tsunami() : base(ID, "Tsunami.png", RelicTier.RARE, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现波浪式伤害
    // private float waveTimer = 0f;
    //
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     waveTimer += dt;
    //     if (waveTimer >= 2f)
    //     {
    //         waveTimer = 0f;
    //         // 从一侧到另一侧的波浪伤害
    //     }
    // }

    public override ARelic makeCopy() => new Tsunami();
}

/// <summary>
/// VoidBlast - 虚空冲击
/// 造成纯粹伤害
/// [设计文案] 无视防御的虚空之力
/// </summary>
public class VoidBlast : ARelic
{
    public static string ID = "VoidBlast";

    public VoidBlast() : base(ID, "VoidBlast.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现纯粹伤害
    // public override void onBallHitBrick(APlayer p, Ball ball, Brick brick, Vector2 normal, ref bool triggerRegularHit)
    // {
    //     brick.TakeTrueDamage(ball.Damage * 0.3f); // 纯粹伤害
    // }

    public override ARelic makeCopy() => new VoidBlast();
}
