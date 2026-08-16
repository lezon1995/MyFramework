namespace MoreMountains;

public enum RelicType
{
    None = 30000,
    // ============================================
    // 基础遗物 (Tier 1 - Common)
    // ============================================
    AmmoSupply,
    Blender,
    BrokenTripod,
    BurlapBag,
    FishingNets,
    FreeBall,
    ImpactHammer,
    LakeMirror,
    MilkShake,
    Origami,
    Rattle,
    RhombicDarts,
    RoughCelling,
    RoughWall,
    SideBorderPortal,
    TacticalShield,
    BaseMagazine,
    ExtremelyUnstableBattery,
    RoundBattery,
    UnstableBattery,

    // ============================================
    // Tier 1 遗物 (基础)
    // ============================================
    SharpSphere,           // 锐利球体 - 命中伤害+8%
    RubberBall,            // 橡皮球 - 球弹速+5%
    LuckyDice,             // 幸运骰子 - 幸运+5
    TinyShield,            // 小护盾 - 护甲+1
    SpeedPotion,           // 速度药水 - 移速+3%
    CottonPadded,           // 棉垫 - 闪避+2%
    IronCore,              // 铁核 - 生命上限+10
    QuickReflexes,         // 快速反应 - 攻速+5%
    SteelBall,             // 钢球 - 球伤害+5%
    LightWeight,           // 轻量级 - 球弹速+3%

    // ============================================
    // Tier 2 遗物 (普通)
    // ============================================
    GoldenSphere,          // 金色球体 - 暴击率+8%
    ChainReaction,         // 链式反应 - 击杀砖块时+2%球伤害
    HeavyBall,             // 重型球 - 球伤害+12%，弹速-5%
    ElasticString,         // 弹性绳 - 球反弹次数+1
    LuckyCharm,            // 幸运符 - 幸运+10
    ArmorPlate,            // 装甲板 - 护甲+3
    SpeedBoots,            // 速靴 - 移速+8%
    VampiricTouch,         // 吸血 - 生命偷取+3%
    PiercingGaze,          // 穿刺凝视 - 命中特效概率+10%
    GlassArmor,            // 玻璃甲 - 护甲+5，生命上限-10

    // ============================================
    // Tier 3 遗物 (罕见)
    // ============================================
    BerserkerSoul,         // 狂战士之魂 - 血量越低伤害越高（低于50%时+20%伤害）
    MirrorImage,           // 镜像 - 发射的球有15%概率产生一个复制球
    LightningCore,         // 闪电核心 - 10%概率造成连锁闪电
    TimeWarp,              // 时间扭曲 - 每击杀5个砖块，攻速临时+5%
    DragonHeart,           // 龙心 - 生命上限+25，生命回复+2
    ShadowCloak,           // 暗影斗篷 - 闪避+10%
    ThunderHammer,          // 雷神之锤 - 暴击伤害+30%
    GravityWell,           // 重力井 - 球在边界停留时间延长
    MysticOrb,             // 神秘法球 - 法术强度+15
    DiamondCore,           // 钻石核心 - 所有正面属性+5%

    // ============================================
    // Tier 4 遗物 (稀有)
    // ============================================
    CosmicCube,            // 宇宙魔方 - 每场战斗随机获得一个属性增益
    PhoenixFeather,        // 凤凰羽毛 - 死亡时复活一次（恢复30%血量）
    VoidEssence,           // 虚空精华 - 击杀砖块时有5%概率召唤一个友方球
    ChronoGear,            // 时间齿轮 - 波次开始时重置所有球的属性加成
    OmegaParticle,          // 欧米茄粒子 - 伤害+30%，但球会逐渐变慢
    SingularityCore,        // 奇点核心 - 所有属性+15%
    DivineBlessing,         // 神圣祝福 - 每30秒自动获得一层护盾
    InfernalContract,       // 地狱契约 - 伤害+50%，击杀不给予生命偷取
    AngelicProtection,      // 天使守护 - 免疫一次致命伤害（每波次一次）
    RealityFragment,       // 现实碎片 - 随机改变球的轨迹

    // ============================================
    // 球属性相关遗物
    // ============================================
    BallOfFlames,          // 火焰球 - 球附带灼烧效果
    IceBall,               // 冰霜球 - 球命中时减速目标
    ThunderBall,           // 雷电球 - 球命中时造成眩晕
    PoisonBall,            // 毒球 - 球命中时附加中毒
    LightningBall,         // 闪电球 - 球命中时释放闪电链
    LaserBall,             // 激光球 - 球变为穿透型
    ExplosiveBall,         // 爆炸球 - 球命中时造成范围伤害
    HomingBall,            // 追踪球 - 球自动追踪最近目标
    RicochetBall,          // 弹跳球 - 球额外反弹2次
    SplittingBall,         // 分裂球 - 球分裂成多个小球

    // ============================================
    // 反弹相关遗物
    // ============================================
    ElasticRacket,          // 弹性球拍 - 每次反弹伤害+3%
    Trampoline,            // 蹦床 - 球从底部反弹时伤害翻倍
    BouncyWall,            // 弹力墙 - 球反弹时弹速+8%
    PinballWizard,         // 弹珠达人 - 连续反弹超过3次时触发暴击
    FlipperMaster,         // 翻转大师 - 球反弹角度更精准
    ChaosMirror,           // 混乱之镜 - 球反弹方向随机化
    PrecisionAngle,        // 精准角度 - 反弹方向优化
    WallRunner,            // 攀墙者 - 球沿墙移动时速度不减
    ReflectionMaster,      // 反射大师 - 反弹伤害+10%
    RubberBand,            // 橡皮筋 - 球反弹时额外获得一次穿透

    // ============================================
    // 击杀相关遗物
    // ============================================
    Executioner,            // 处刑者 - 血量低于20%的敌人受到额外50%伤害
    FirstBlood,            // 先手必杀 - 每波次第一个击杀获得双倍奖励
    ComboMaster,           // 连击大师 - 连击数达到10时伤害+20%
    Massacre,               // 大屠杀 - 快速击杀5个敌人后获得短暂无敌
    LethalPrecision,       // 致命精准 - 暴击必定击杀普通敌人
    Slayer,                // 杀手 - 击杀精英敌人后伤害+15%（持续至波次结束）
    Destroyer,             // 毁灭者 - 击杀敌人时产生小爆炸
    Reaper,                // 收割者 - 每击杀10个敌人恢复5%最大生命
    Hunter,                // 猎人 - 对特定类型敌人伤害+25%
    Annihilator,           // 歼灭者 - 击杀时有20%概率清空屏幕

    // ============================================
    // 暴击相关遗物
    // ============================================
    CriticalEye,           // 暴击之眼 - 暴击率+10%
    SharpClaws,            // 锋利爪子 - 暴击伤害+20%
    VampiricFangs,         // 吸血獠牙 - 暴击时恢复生命
    LightningStrike,       // 雷击 - 暴击时有概率触发闪电
    CriticalMass,          // 暴击质变 - 暴击率超过50%时额外+15%
    AssassinDagger,         // 刺客匕首 - 暴击伤害+40%
    LuckyStar,             // 幸运星 - 暴击时随机获得增益
    CrimsonEdge,           // 猩红之刃 - 暴击造成流血
    PrecisionStrike,       // 精准打击 - 暴击率+15%，暴击伤害-10%
    Bloodlust,             // 嗜血 - 暴击时伤害增益可叠加

    // ============================================
    // 生命/防御相关遗物
    // ============================================
    HeartOfGold,           // 黄金之心 - 生命上限+20
    RegenerationRing,      // 再生戒指 - 每秒恢复1%最大生命
    StoneSkin,             // 石肤 - 护甲+8
    BarrierWard,           // 屏障护符 - 战斗开始时获得护盾
    LifeDrain,             // 生命汲取 - 造成伤害的5%转化为生命
    ImmortalSoul,          // 不死之魂 - 免疫致命伤害（每波次一次）
    VampireFang,           // 吸血鬼之牙 - 生命偷取+8%
    ThornsArmor,           // 荆棘护甲 - 受到伤害时反弹10%
    HealingLight,          // 治疗之光 - 击杀敌人时恢复生命
    ShieldGenerator,       // 护盾发生器 - 每15秒生成护盾

    // ============================================
    // 速度/机动相关遗物
    // ============================================
    SwiftBoots,            // 疾风之靴 - 移速+15%
    Haste,                 // 加速 - 攻速+20%
    WindWalker,            // 风行者 - 移动不受惩罚
    QuickDraw,             // 快速拔枪 - 发射间隔-15%
    Sprint,                // 冲刺 - 移速+25%
    LightningReflexes,     // 闪电反应 - 攻速+10%，闪避+5%
    WindCloak,             // 风之斗篷 - 移速+10%，闪避+8%
    TurboMode,             // 涡轮模式 - 弹速+30%
    Momentum,              // 动量 - 移动增加弹速
    Afterburner,          // 加速器 - 攻速逐渐加快

    // ============================================
    // 范围/弹道相关遗物
    // ============================================
    WideAngle,             // 广角 - 球发射范围+30%
    LongRange,             // 远程 - 球飞行距离+50%
    Scattershot,           // 散弹 - 发射多枚小球
    SniperScope,           // 狙击镜 - 命中伤害+25%，命中率-20%
    SpreadShot,            // 扩散射击 - 球分散成扇形
    Railgun,               // 电磁炮 - 单发高伤害，低射速
    ShotgunBlast,          // 霰弹 - 近距离高伤害
    PrecisionBeam,         // 精准光束 - 穿透伤害+50%
    ArcShot,               // 弧形射击 - 球沿弧线飞行
    TrajectoryGuide,       // 弹道引导 - 球飞行更精准

    // ============================================
    // 特殊效果遗物
    // ============================================
    BlackHole,             // 黑洞 - 吸引附近砖块
    TimeFreeze,            // 时间冻结 - 暂停所有敌人1秒
    ChainLightning,         // 链式闪电 - 伤害在敌人间传递
    MeteorStrike,          // 陨石打击 - 随机位置降下陨石
    NovaBlast,             // 新星爆发 - 清屏技能（每波次一次）
    Earthquake,            // 地震 - 击退所有敌人
    Blizzard,              // 暴风雪 - 减速所有敌人
    Inferno,               // 地狱火 - 全屏灼烧
    Tsunami,               // 海啸 - 波浪式伤害
    VoidBlast,             // 虚空冲击 - 造成纯粹伤害

    // ============================================
    // 持续时间相关遗物
    // ============================================
    ExtendedFlight,        // 延长飞行 - 球持续时间+30%
    EternalBall,           // 永恒之球 - 球存在时间翻倍
    QuickReturn,           // 快速返回 - 球提前返回
    LongLasting,           // 持久 - 球持续时间+50%
    FadingEcho,            // 消逝回声 - 球逐渐衰减但伤害增加
    Overcharged,           // 过载 - 球时间越长伤害越高
    StableOrbit,           // 稳定轨道 - 球轨道更稳定
    EnduringBlow,          // 持久打击 - 命中伤害随时间增加
    TimelessShot,          // 无时间射击 - 球不受时间限制
    MomentumShift,         // 动量转移 - 球速度随距离增加

    // ============================================
    // 商店/经济相关遗物
    // ============================================
    MoneyBag,              // 钱袋 - 金币+20%
    DiscountCoupon,        // 折扣券 - 商店价格-15%
    TreasureHunter,        // 寻宝者 - 稀有物品出现概率+10%
    GoldenTouch,           // 点金手 - 击杀敌人额外获得金币
    TaxCollector,          // 税务员 - 每波次结束时获得金币
    MerchantSoul,          // 商人灵魂 - 商店物品价格-10%
    LuckyCoin,             // 幸运硬币 - 金币+30
    BankAccount,           // 银行账户 - 携带金币上限翻倍
    Investment,            // 投资 - 每10秒获得金币
    BlackMarket,           // 黑市 - 可以用金币刷新商店

    // ============================================
    // 经验/升级相关遗物
    // ============================================
    WisdomScroll,          // 智慧卷轴 - 经验+25%
    ScholarHat,            // 学者之帽 - 升级所需经验-15%
    KnowledgeGem,          // 知识宝石 - 每级额外获得属性
    AncientTome,           // 古老典籍 - 技能解锁更快
    StudyGlasses,          // 学习眼镜 - 经验+15%
    MentorSpirit,          // 导师之魂 - 升级时获得双倍属性
    Enlightenment,         // 启迪 - 每3级获得额外技能
    PhilosopherStone,      // 贤者之石 - 经验转化为生命
    LibraryCard,           // 图书证 - 经验+20%
    BrainFood,             // 补脑食品 - 经验+10%，幸运+5

    // ============================================
    // 负面效果遗物
    // ============================================
    CursedCoin,            // 被诅咒的硬币 - 金币+50%但经验-20%
    BrokenShield,          // 破损护盾 - 护甲-5但伤害+10%
    RustedArmor,          // 生锈护甲 - 护甲-3但移速+15%
    WeakenedHeart,         // 衰弱之心 - 生命上限-20但伤害+15%
    SlowCannon,            // 慢速大炮 - 攻速-30%但伤害+40%
    HeavyBurden,           // 重负 - 移速-20%但生命上限+50
    ChaosCurse,            // 混乱诅咒 - 随机属性波动
    DeathWish,             // 死亡之愿 - 伤害+30%但无法生命偷取
    MidasTouch,            // 点金术 - 金币+100%但击杀不恢复生命
    PowerDrain,            // 力量流失 - 属性逐渐下降

    // ============================================
    // 互动/触发类遗物
    // ============================================
    ReactiveArmor,         // 反应护甲 - 受伤时反弹伤害
    ThornedSkin,           // 荆棘皮肤 - 近距离反弹伤害
    MagicMirror,           // 魔法镜 - 反射敌方攻击
    CounterStrike,         // 反击 - 闪避后立即攻击
    Riposte,               // 格挡反击 - 完美闪避后造成双倍伤害
    LuckyDodge,           // 幸运闪避 - 闪避时触发暴击
    Opportunist,           // 机会主义者 - 敌人露出破绽时伤害翻倍
    FirstStrike,           // 先发制人 - 战斗开始时伤害+30%
    LastStand,             // 背水一战 - 生命低于30%时伤害翻倍
    SecondWind,            // 第二呼吸 - 生命低于50%时攻速翻倍

    // ============================================
    // 叠加/累积类遗物
    // ============================================
    RageStack,             // 怒气叠加 - 每击杀一个敌人伤害+2%（可叠加10层）
    MomentumGain,          // 动量积累 - 持续射击伤害递增
    WarriorSpirit,         // 战士之魂 - 每波次击杀增加属性
    BeastMode,             // 野兽模式 - 血量越低属性越高
    RisingPower,          // 上升之力 - 战斗越久伤害越高
    EscalatingForce,       // 升级力量 - 每次命中增加伤害
    InfiniteGrowth,        // 无限成长 - 属性持续增长
    Overdrive,             // 过载 - 持续战斗获得增益
    BattleRage,            // 战斗狂热 - 战斗中获得攻速
    BloodPact,             // 血之契约 - 消耗生命换取力量

    // ============================================
    // 稀有/传奇遗物
    // ============================================
    CrownOfThorns,         // 荆棘之冠 - 受伤时周围敌人受伤
    RingOfFire,            // 火焰戒指 - 周围持续造成伤害
    BootsOfHermes,         // 赫尔墨斯之靴 - 无敌帧+30%
    ShieldOfJustice,       // 正义之盾 - 完美格挡触发反击
    SwordOfDamocles,       // 达摩克利斯之剑 - 高风险高回报
    AmuletOfLife,          // 生命护符 - 生命偷取+15%
    OrbOfProtection,       // 保护之球 - 周围敌人攻击减半
    CloakOfShadows,        // 暗影斗篷 - 隐身时伤害翻倍
    GauntletOfStrength,    // 力量护手 - 伤害+50%
    HeartOfTheMountain,    // 山之心 - 每秒恢复最大生命的1%

    // ============================================
    // 终极/传说遗物
    // ============================================
    Excalibur,             // 圣剑 - 所有属性+30%
    Mjolnir,               // 雷神之锤 - 闪电造成巨大伤害
    AegisOfOlympus,        // 奥林匹斯之盾 - 免疫所有负面效果
    WingsOfIcarus,         // 伊卡洛斯之翼 - 飞行（无敌）+30%移速
    EyeOfProvidence,       // 普罗维登斯之眼 - 全知（全屏显示敌人）
    PhilosopherStoneUltimate, // 贤者之石终极版 - 所有属性+50%
    CrownOfTheUniverse,    // 宇宙之冠 - 获得其他所有遗物的效果（减半）
    HolyGrail,             // 圣杯 - 无限生命回复
    InfinityGauntlet,       // 无限手套 - 集齐六颗宝石的力量
    CelestialCore,         // 天体核心 - 时间倒流（重置当前波次）

    // ============================================
    // 球数量相关遗物
    // ============================================
    MultiBall,             // 多球 - 额外获得1个球
    BallSquad,             // 球小队 - 额外获得2个球
    BallSwarm,             // 球群 - 额外获得3个球
    BallBarrage,           // 球弹幕 - 额外获得4个球
    BallStorm,             // 球风暴 - 额外获得5个球
    BallRain,              // 球雨 - 每秒额外发射1个球
    BallFountain,          // 球喷泉 - 球数量上限+1
    BallSwarmController,   // 球群控制器 - 球循环发射
    BallFactory,           // 球工厂 - 每波次补充球数量
    BallReplication,        // 球复制 - 球有概率分裂

    // ============================================
    // 元素属性遗物
    // ============================================
    FireElemental,         // 火元素 - 灼烧伤害+30%
    IceElemental,          // 冰元素 - 冻结时间+1秒
    LightningElemental,    // 雷元素 - 闪电链伤害+50%
    PoisonElemental,       // 毒元素 - 中毒伤害+40%
    EarthElemental,        // 土元素 - 护甲+10
    WindElemental,         // 风元素 - 移速+20%
    WaterElemental,        // 水元素 - 生命回复+100%
    LightElemental,        // 光元素 - 暴击伤害+50%
    DarkElemental,         // 暗元素 - 生命偷取+20%
    ChaosElemental,        // 混沌元素 - 所有元素伤害+15%

    // ============================================
    // 地形互动遗物
    // ============================================
    WallCrawler,           // 爬墙者 - 球可以攀爬边界
    BouncePad,             // 弹跳垫 - 特定区域弹速增加
    PortalWand,            // 传送棒 - 球可以传送
    MagnetBall,            // 磁力球 - 球吸引附近敌人
    GravityBoots,          // 重力靴 - 可以改变重力方向
    TeleportBeacon,        // 传送信标 - 球可以瞬移
    Wormhole,              // 虫洞 - 球可以从任意位置出现
    BouncyCastle,          // 蹦床城堡 - 所有反弹伤害翻倍
    TrampolinePark,        // 蹦床公园 - 反弹次数+3
    WallJumpMaster,        // 墙壁跳跃大师 - 反弹伤害递增

    // ============================================
    // 视觉/特效遗物（纯装饰性，可能有隐藏效果）
    // ============================================
    PrismSphere,           // 棱镜球 - 球发出彩虹光
    CrystalBall,           // 水晶球 - 显示未来事件
    MagicLantern,          // 魔法灯笼 - 照亮隐藏区域
    MirrorBall,            // 镜球 - 产生闪光特效
    DiscoBall,             // 迪斯科球 - 音乐节拍增强
    AuroraSphere,          // 极光球 - 美丽极光效果
    StarDust,              // 星尘 - 留下星光轨迹
    RainbowTrail,          // 彩虹尾迹 - 球带有彩虹
    SparkleEffect,         // 闪亮特效 - 华丽的粒子效果
    FireworkBall,          // 烟花球 - 爆炸时产生烟花
}
