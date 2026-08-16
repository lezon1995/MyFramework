using UniStats;

namespace MoreMountains;

/// <summary>
/// GoldenSphere - 金色球体
/// 暴击率+8%
/// </summary>
public class GoldenSphere : ARelic
{
    public static string ID = "GoldenSphere";

    public GoldenSphere() : base(ID, "GoldenSphere.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new GoldenSphere();
}

/// <summary>
/// ChainReaction - 链式反应
/// 击杀砖块时+2%球伤害
/// </summary>
public class ChainReaction : ARelic
{
    public static string ID = "ChainReaction";
    private int killCount = 0;

    public ChainReaction() : base(ID, "ChainReaction.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override void onBallKillBrick(APlayer p, Ball ball, Brick brick)
    {
        killCount++;
        ball.GetStat(Ball.Stat.HitDamageRate, out var stat);
        stat.AddPct(0.02f);
    }

    public override ARelic makeCopy() => new ChainReaction();
}

/// <summary>
/// HeavyBall - 重型球
/// 球伤害+12%，弹速-5%
/// </summary>
public class HeavyBall : ARelic
{
    public static string ID = "HeavyBall";

    public HeavyBall() : base(ID, "HeavyBall.png", RelicTier.UNCOMMON, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new HeavyBall();
}

/// <summary>
/// ElasticString - 弹性绳
/// 球反弹次数+1
/// [设计文案] 球在反弹时额外获得一次反弹次数，可以通过修改球的MaxBounceCount来实现
/// </summary>
public class ElasticString : ARelic
{
    public static string ID = "ElasticString";

    public ElasticString() : base(ID, "ElasticString.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    // TODO: 需要在Ball类中添加MaxBounceCount属性
    // public override void onShootBall(Ball ball)
    // {
    //     ball.MaxBounceCount += 1;
    // }

    public override ARelic makeCopy() => new ElasticString();
}

/// <summary>
/// LuckyCharm - 幸运符
/// 幸运+10
/// </summary>
public class LuckyCharm : ARelic
{
    public static string ID = "LuckyCharm";

    public LuckyCharm() : base(ID, "LuckyCharm.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new LuckyCharm();
}

/// <summary>
/// ArmorPlate - 装甲板
/// 护甲+3
/// </summary>
public class ArmorPlate : ARelic
{
    public static string ID = "ArmorPlate";

    public ArmorPlate() : base(ID, "ArmorPlate.png", RelicTier.UNCOMMON, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new ArmorPlate();
}

/// <summary>
/// SpeedBoots - 速靴
/// 移速+8%
/// </summary>
public class SpeedBoots : ARelic
{
    public static string ID = "SpeedBoots";

    public SpeedBoots() : base(ID, "SpeedBoots.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new SpeedBoots();
}

/// <summary>
/// VampiricTouch - 吸血
/// 生命偷取+3%
/// </summary>
public class VampiricTouch : ARelic
{
    public static string ID = "VampiricTouch";

    public VampiricTouch() : base(ID, "VampiricTouch.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new VampiricTouch();
}

/// <summary>
/// PiercingGaze - 穿刺凝视
/// 命中特效概率+10%
/// </summary>
public class PiercingGaze : ARelic
{
    public static string ID = "PiercingGaze";

    public PiercingGaze() : base(ID, "PiercingGaze.png", RelicTier.UNCOMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new PiercingGaze();
}

/// <summary>
/// GlassArmor - 玻璃甲
/// 护甲+5，生命上限-10
/// [设计文案] 高护甲但脆弱的护甲
/// </summary>
public class GlassArmor : ARelic
{
    public static string ID = "GlassArmor";

    public GlassArmor() : base(ID, "GlassArmor.png", RelicTier.UNCOMMON, LandingSound.HEAVY)
    {
    }

    public override ARelic makeCopy() => new GlassArmor();
}
