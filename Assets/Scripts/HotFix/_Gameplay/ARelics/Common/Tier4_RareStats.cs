using UniStats;

namespace MoreMountains;

/// <summary>
/// CosmicCube - 宇宙魔方
/// 每场战斗随机获得一个属性增益
/// [设计文案] 蕴含宇宙力量的魔方，每场战斗赐予随机祝福
/// </summary>
public class CosmicCube : ARelic
{
    public static string ID = "CosmicCube";

    public CosmicCube() : base(ID, "CosmicCube.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现每场战斗随机属性增益
    // private enum BuffType { Damage, Speed, Crit, Health, Armor }
    // private BuffType currentBuff;
    //
    // public override void atBattleStart()
    // {
    //     currentBuff = (BuffType)Random.Range(0, 5);
    //     switch (currentBuff)
    //     {
    //         case BuffType.Damage: /* +20% Damage */ break;
    //         case BuffType.Speed: /* +20% Speed */ break;
    //         case BuffType.Crit: /* +20% Crit */ break;
    //         case BuffType.Health: /* +20 MaxHealth */ break;
    //         case BuffType.Armor: /* +5 Armor */ break;
    //     }
    // }

    public override ARelic makeCopy() => new CosmicCube();
}

/// <summary>
/// PhoenixFeather - 凤凰羽毛
/// 死亡时复活一次（恢复30%血量）
/// [设计文案] 凤凰的羽毛蕴含重生之力
/// </summary>
public class PhoenixFeather : ARelic
{
    public static string ID = "PhoenixFeather";

    public PhoenixFeather() : base(ID, "PhoenixFeather.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现复活机制
    // private bool hasRevived = false;
    //
    // public override void onEquip(APlayer p)
    // {
    //     hasRevived = false;
    // }
    //
    // public override void onMonsterDeath(AMonster m)
    // {
    //     // 在玩家死亡时触发复活
    // }
    //
    // public override void onLoseHp(int damageAmount)
    // {
    //     if (owner.CurrentHealth <= 0 && !hasRevived)
    //     {
    //         hasRevived = true;
    //         owner.CurrentHealth = owner.MaxHealth * 0.3f;
    //         // 播放复活特效
    //     }
    // }

    public override ARelic makeCopy() => new PhoenixFeather();
}

/// <summary>
/// VoidEssence - 虚空精华
/// 击杀砖块时有5%概率召唤一个友方球
/// [设计文案] 虚空的力量，可以从虚无中召唤球
/// </summary>
public class VoidEssence : ARelic
{
    public static string ID = "VoidEssence";

    public VoidEssence() : base(ID, "VoidEssence.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现召唤友方球的机制
    // public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    // {
    //     if (Random.value < 0.05f)
    //     {
    //         // 在砖块位置召唤一个友方球
    //     }
    // }

    public override ARelic makeCopy() => new VoidEssence();
}

/// <summary>
/// ChronoGear - 时间齿轮
/// 波次开始时重置所有球的属性加成
/// [设计文案] 操控时间的齿轮，让球恢复初始状态
/// </summary>
public class ChronoGear : ARelic
{
    public static string ID = "ChronoGear";

    public ChronoGear() : base(ID, "ChronoGear.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现重置球属性的机制
    // public override void atBattleStart()
    // {
    //     foreach (var ball in owner.Balls)
    //     {
    //         ball.ResetToBaseStats();
    //     }
    // }

    public override ARelic makeCopy() => new ChronoGear();
}

/// <summary>
/// OmegaParticle - 欧米茄粒子
/// 伤害+30%，但球会逐渐变慢
/// [设计文案] 强大的粒子能量，但有时间限制
/// </summary>
public class OmegaParticle : ARelic
{
    public static string ID = "OmegaParticle";

    public OmegaParticle() : base(ID, "OmegaParticle.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    // TODO: 需要实现逐渐减速的效果
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     foreach (var ball in p.Balls)
    //     {
    //         ball.GetStat(Ball.Stat.BallisticSpeed, out var stat);
    //         stat.AddPct(-0.01f * dt);
    //     }
    // }

    public override ARelic makeCopy() => new OmegaParticle();
}

/// <summary>
/// SingularityCore - 奇点核心
/// 所有属性+15%
/// </summary>
public class SingularityCore : ARelic
{
    public static string ID = "SingularityCore";

    public SingularityCore() : base(ID, "SingularityCore.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    public override ARelic makeCopy() => new SingularityCore();
}

/// <summary>
/// DivineBlessing - 神圣祝福
/// 每30秒自动获得一层护盾
/// [设计文案] 神圣的祝福，持续提供保护
/// </summary>
public class DivineBlessing : ARelic
{
    public static string ID = "DivineBlessing";
    private float timer = 0f;

    public DivineBlessing() : base(ID, "DivineBlessing.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现自动护盾生成
    // public override void onPlayerTurnUpdate(APlayer p, float dt)
    // {
    //     timer += dt;
    //     if (timer >= 30f)
    //     {
    //         timer = 0f;
    //         p.AddShield(1);
    //     }
    // }

    public override ARelic makeCopy() => new DivineBlessing();
}

/// <summary>
/// InfernalContract - 地狱契约
/// 伤害+50%，击杀不给予生命偷取
/// [设计文案] 与地狱签订的契约，强大的力量代价
/// </summary>
public class InfernalContract : ARelic
{
    public static string ID = "InfernalContract";

    public InfernalContract() : base(ID, "InfernalContract.png", RelicTier.SPECIAL, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new InfernalContract();
}

/// <summary>
/// AngelicProtection - 天使守护
/// 免疫一次致命伤害（每波次一次）
/// [设计文案] 天使的保护，抵挡致命一击
/// </summary>
public class AngelicProtection : ARelic
{
    public static string ID = "AngelicProtection";

    public AngelicProtection() : base(ID, "AngelicProtection.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现免疫致命伤害的机制
    // private bool hasProtection = true;
    //
    // public override void atBattleStart()
    // {
    //     hasProtection = true;
    // }
    //
    // public override int onLoseHpLast(int damageAmount)
    // {
    //     if (hasProtection && damageAmount >= owner.CurrentHealth)
    //     {
    //         hasProtection = false;
    //         return owner.CurrentHealth - 1; // 保留1点血
    //     }
    //     return damageAmount;
    // }

    public override ARelic makeCopy() => new AngelicProtection();
}

/// <summary>
/// RealityFragment - 现实碎片
/// 随机改变球的轨迹
/// [设计文案] 碎裂的现实，让球的轨迹变得不可预测
/// </summary>
public class RealityFragment : ARelic
{
    public static string ID = "RealityFragment";

    public RealityFragment() : base(ID, "RealityFragment.png", RelicTier.SPECIAL, LandingSound.MAGICAL)
    {
    }

    // TODO: 需要实现随机改变轨迹的机制
    // public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    // {
    //     if (Random.value < 0.2f)
    //     {
    //         float angle = Random.Range(-30f, 30f);
    //         reflectDir = reflectDir.Rotate(angle);
    //     }
    // }

    public override ARelic makeCopy() => new RealityFragment();
}
