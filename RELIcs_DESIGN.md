# 弹球幸存者 - 遗物设计文档

本文档汇总了为弹球幸存者游戏设计的 200 个新遗物（Relic）。

## 设计理念

参考土豆兄弟（Brotato）的道具设计机制，结合本游戏的弹球核心玩法，设计了多样化的遗物效果。

## 遗物稀有度

| 稀有度 | Rarity值 | 颜色 | 说明 |
|--------|----------|------|------|
| Tier 1 (基础) | 0 | 灰色 | 基础属性增益，价格便宜 |
| Tier 2 (普通) | 1 | 绿色 | 适中属性增益，有特殊效果 |
| Tier 3 (罕见) | 2 | 蓝色 | 强力属性增益或特殊效果 |
| Tier 4 (稀有) | 3 | 紫色/橙色 | 传说级效果，极大改变玩法 |

## 遗物分类

### 1. 基础属性类遗物 (Tier 1)

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| SharpSphere (锐利球体) | 命中伤害+8% | Tier 1 | 25 |
| RubberBall (橡皮球) | 球弹速+5% | Tier 1 | 20 |
| LuckyDice (幸运骰子) | 幸运+5 | Tier 1 | 25 |
| TinyShield (小护盾) | 护甲+1 | Tier 1 | 15 |
| SpeedPotion (速度药水) | 移速+3% | Tier 1 | 20 |
| CottonPadded (棉垫) | 闪避+2% | Tier 1 | 20 |
| IronCore (铁核) | 生命上限+10 | Tier 1 | 30 |
| QuickReflexes (快速反应) | 攻速+5% | Tier 1 | 25 |
| SteelBall (钢球) | 球伤害+5% | Tier 1 | 25 |
| LightWeight (轻量级) | 球弹速+3% | Tier 1 | 18 |

### 2. 进阶属性类遗物 (Tier 2)

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| GoldenSphere (金色球体) | 暴击率+8% | Tier 2 | 45 |
| ChainReaction (链式反应) | 击杀砖块时+2%球伤害 | Tier 2 | 50 |
| HeavyBall (重型球) | 球伤害+12%，弹速-5% | Tier 2 | 40 |
| ElasticString (弹性绳) | 球反弹次数+1 | Tier 2 | 35 |
| LuckyCharm (幸运符) | 幸运+10 | Tier 2 | 40 |
| ArmorPlate (装甲板) | 护甲+3 | Tier 2 | 45 |
| SpeedBoots (速靴) | 移速+8% | Tier 2 | 40 |
| VampiricTouch (吸血) | 生命偷取+3% | Tier 2 | 45 |
| PiercingGaze (穿刺凝视) | 命中特效概率+10% | Tier 2 | 40 |
| GlassArmor (玻璃甲) | 护甲+5，生命上限-10 | Tier 2 | 35 |

### 3. 高级效果类遗物 (Tier 3)

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| BerserkerSoul (狂战士之魂) | 血量低于50%时+20%伤害 | Tier 3 | 65 |
| MirrorImage (镜像) | 15%概率产生复制球 | Tier 3 | 70 |
| LightningCore (闪电核心) | 10%概率造成连锁闪电 | Tier 3 | 65 |
| TimeWarp (时间扭曲) | 每击杀5砖块，攻速+5% | Tier 3 | 60 |
| DragonHeart (龙心) | 生命上限+25，生命回复+2 | Tier 3 | 70 |
| ShadowCloak (暗影斗篷) | 闪避+10% | Tier 3 | 65 |
| ThunderHammer (雷神之锤) | 暴击伤害+30% | Tier 3 | 70 |
| GravityWell (重力井) | 球在边界停留时间延长 | Tier 3 | 60 |
| MysticOrb (神秘法球) | 法术强度+15 | Tier 3 | 70 |
| DiamondCore (钻石核心) | 所有正面属性+5% | Tier 3 | 75 |

### 4. 传说级遗物 (Tier 4)

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| CosmicCube (宇宙魔方) | 每场战斗随机获得属性增益 | Tier 4 | 100 |
| PhoenixFeather (凤凰羽毛) | 死亡时复活一次 | Tier 4 | 110 |
| VoidEssence (虚空精华) | 5%概率召唤友方球 | Tier 4 | 95 |
| ChronoGear (时间齿轮) | 波次开始时重置球的属性加成 | Tier 4 | 100 |
| OmegaParticle (欧米茄粒子) | 伤害+30%，球逐渐变慢 | Tier 4 | 105 |
| SingularityCore (奇点核心) | 所有属性+15% | Tier 4 | 110 |
| DivineBlessing (神圣祝福) | 每30秒自动获得护盾 | Tier 4 | 100 |
| InfernalContract (地狱契约) | 伤害+50%，击杀无生命偷取 | Tier 4 | 115 |
| AngelicProtection (天使守护) | 免疫致命伤害（每波次一次） | Tier 4 | 105 |
| RealityFragment (现实碎片) | 随机改变球的轨迹 | Tier 4 | 100 |

### 5. 球属性相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| BallOfFlames (火焰球) | 球附带灼烧效果 | Tier 2 | 50 |
| IceBall (冰霜球) | 球命中时减速目标 | Tier 2 | 45 |
| ThunderBall (雷电球) | 球命中时造成眩晕 | Tier 3 | 70 |
| PoisonBall (毒球) | 球命中时附加中毒 | Tier 2 | 45 |
| LightningBall (闪电球) | 球命中时释放闪电链 | Tier 3 | 70 |
| LaserBall (激光球) | 球变为穿透型 | Tier 3 | 75 |
| ExplosiveBall (爆炸球) | 球命中时造成范围伤害 | Tier 2 | 55 |
| HomingBall (追踪球) | 球自动追踪最近目标 | Tier 3 | 80 |
| RicochetBall (弹跳球) | 球额外反弹2次 | Tier 2 | 50 |
| SplittingBall (分裂球) | 球分裂成多个小球 | Tier 3 | 75 |

### 6. 反弹相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| ElasticRacket (弹性球拍) | 每次反弹伤害+3% | Tier 1 | 30 |
| Trampoline (蹦床) | 底部反弹时伤害翻倍 | Tier 2 | 40 |
| BouncyWall (弹力墙) | 球反弹时弹速+8% | Tier 1 | 30 |
| PinballWizard (弹珠达人) | 连续反弹超3次时触发暴击 | Tier 3 | 65 |
| FlipperMaster (翻转大师) | 球反弹角度更精准 | Tier 2 | 45 |
| ChaosMirror (混乱之镜) | 球反弹方向随机化 | Tier 3 | 70 |
| PrecisionAngle (精准角度) | 反弹方向优化 | Tier 2 | 45 |
| WallRunner (攀墙者) | 球沿墙移动时速度不减 | Tier 2 | 40 |
| ReflectionMaster (反射大师) | 反弹伤害+10% | Tier 3 | 70 |
| RubberBand (橡皮筋) | 球反弹时额外获得穿透 | Tier 3 | 65 |

### 7. 击杀相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| Executioner (处刑者) | 低血量敌人受到+50%伤害 | Tier 3 | 65 |
| FirstBlood (先手必杀) | 每波次首杀获得双倍奖励 | Tier 2 | 45 |
| ComboMaster (连击大师) | 连击达10时伤害+20% | Tier 3 | 70 |
| Massacre (大屠杀) | 快速击杀5敌人后短暂无敌 | Tier 3 | 75 |
| LethalPrecision (致命精准) | 暴击必定击杀普通敌人 | Tier 3 | 70 |
| Slayer (杀手) | 击杀精英敌人后伤害+15% | Tier 3 | 70 |
| Destroyer (毁灭者) | 击杀敌人时产生小爆炸 | Tier 2 | 55 |
| Reaper (收割者) | 每击杀10敌人恢复5%血量 | Tier 3 | 70 |
| Hunter (猎人) | 对特定类型敌人伤害+25% | Tier 2 | 50 |
| Annihilator (歼灭者) | 20%概率清空屏幕 | Tier 4 | 120 |

### 8. 暴击相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| CriticalEye (暴击之眼) | 暴击率+10% | Tier 1 | 30 |
| SharpClaws (锋利爪子) | 暴击伤害+20% | Tier 2 | 45 |
| VampiricFangs (吸血獠牙) | 暴击时恢复生命 | Tier 3 | 70 |
| LightningStrike (雷击) | 暴击时有概率触发闪电 | Tier 3 | 70 |
| CriticalMass (暴击质变) | 暴击率超50%时额外+15% | Tier 3 | 75 |
| AssassinDagger (刺客匕首) | 暴击伤害+40% | Tier 3 | 80 |
| LuckyStar (幸运星) | 暴击时随机获得增益 | Tier 3 | 70 |
| CrimsonEdge (猩红之刃) | 暴击造成流血 | Tier 3 | 70 |
| PrecisionStrike (精准打击) | 暴击率+15%，暴击伤害-10% | Tier 2 | 45 |
| Bloodlust (嗜血) | 暴击时伤害增益可叠加 | Tier 3 | 75 |

### 9. 生命/防御相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| HeartOfGold (黄金之心) | 生命上限+20 | Tier 2 | 45 |
| RegenerationRing (再生戒指) | 每秒恢复1%最大生命 | Tier 2 | 50 |
| StoneSkin (石肤) | 护甲+8 | Tier 2 | 50 |
| BarrierWard (屏障护符) | 战斗开始时获得护盾 | Tier 2 | 50 |
| LifeDrain (生命汲取) | 造成伤害的5%转化为生命 | Tier 3 | 70 |
| ImmortalSoul (不死之魂) | 免疫致命伤害（每波次一次） | Tier 4 | 100 |
| VampireFang (吸血鬼之牙) | 生命偷取+8% | Tier 3 | 70 |
| ThornsArmor (荆棘护甲) | 受伤时反弹10%伤害 | Tier 3 | 70 |
| HealingLight (治疗之光) | 击杀敌人时恢复生命 | Tier 3 | 70 |
| ShieldGenerator (护盾发生器) | 每15秒生成护盾 | Tier 3 | 70 |

### 10. 速度/机动相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| SwiftBoots (疾风之靴) | 移速+15% | Tier 2 | 45 |
| Haste (加速) | 攻速+20% | Tier 2 | 45 |
| WindWalker (风行者) | 移动不受惩罚 | Tier 2 | 40 |
| QuickDraw (快速拔枪) | 发射间隔-15% | Tier 2 | 40 |
| Sprint (冲刺) | 移速+25% | Tier 2 | 50 |
| LightningReflexes (闪电反应) | 攻速+10%，闪避+5% | Tier 2 | 50 |
| WindCloak (风之斗篷) | 移速+10%，闪避+8% | Tier 2 | 50 |
| TurboMode (涡轮模式) | 弹速+30% | Tier 3 | 70 |
| Momentum (动量) | 移动增加弹速 | Tier 3 | 65 |
| Afterburner (加速器) | 攻速逐渐加快 | Tier 3 | 70 |

### 11. 范围/弹道相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| WideAngle (广角) | 球发射范围+30% | Tier 2 | 40 |
| LongRange (远程) | 球飞行距离+50% | Tier 2 | 40 |
| Scattershot (散弹) | 发射多枚小球 | Tier 3 | 70 |
| SniperScope (狙击镜) | 命中伤害+25%，命中率-20% | Tier 3 | 70 |
| SpreadShot (扩散射击) | 球分散成扇形 | Tier 3 | 70 |
| Railgun (电磁炮) | 单发高伤害，低射速 | Tier 3 | 75 |
| ShotgunBlast (霰弹) | 近距离高伤害 | Tier 2 | 55 |
| PrecisionBeam (精准光束) | 穿透伤害+50% | Tier 3 | 70 |
| ArcShot (弧形射击) | 球沿弧线飞行 | Tier 2 | 50 |
| TrajectoryGuide (弹道引导) | 球飞行更精准 | Tier 2 | 50 |

### 12. 特殊效果遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| BlackHole (黑洞) | 吸引附近砖块 | Tier 4 | 100 |
| TimeFreeze (时间冻结) | 暂停所有敌人1秒 | Tier 4 | 95 |
| ChainLightning (链式闪电) | 伤害在敌人间传递 | Tier 4 | 90 |
| MeteorStrike (陨石打击) | 随机位置降下陨石 | Tier 4 | 100 |
| NovaBlast (新星爆发) | 清屏技能（每波次一次） | Tier 4 | 120 |
| Earthquake (地震) | 击退所有敌人 | Tier 3 | 80 |
| Blizzard (暴风雪) | 减速所有敌人 | Tier 3 | 80 |
| Inferno (地狱火) | 全屏灼烧 | Tier 4 | 100 |
| Tsunami (海啸) | 波浪式伤害 | Tier 3 | 85 |
| VoidBlast (虚空冲击) | 造成纯粹伤害 | Tier 4 | 110 |

### 13. 持续时间相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| ExtendedFlight (延长飞行) | 球持续时间+30% | Tier 2 | 40 |
| EternalBall (永恒之球) | 球存在时间翻倍 | Tier 3 | 65 |
| QuickReturn (快速返回) | 球提前返回 | Tier 2 | 35 |
| LongLasting (持久) | 球持续时间+50% | Tier 2 | 45 |
| FadingEcho (消逝回声) | 球逐渐衰减但伤害增加 | Tier 3 | 65 |
| Overcharged (过载) | 球时间越长伤害越高 | Tier 3 | 70 |
| StableOrbit (稳定轨道) | 球轨道更稳定 | Tier 2 | 45 |
| EnduringBlow (持久打击) | 命中伤害随时间增加 | Tier 3 | 70 |
| TimelessShot (无时间射击) | 球不受时间限制 | Tier 3 | 75 |
| MomentumShift (动量转移) | 球速度随距离增加 | Tier 3 | 70 |

### 14. 商店/经济相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| MoneyBag (钱袋) | 金币+20% | Tier 2 | 45 |
| DiscountCoupon (折扣券) | 商店价格-15% | Tier 2 | 40 |
| TreasureHunter (寻宝者) | 稀有物品出现概率+10% | Tier 3 | 70 |
| GoldenTouch (点金手) | 击杀敌人额外获得金币 | Tier 2 | 50 |
| TaxCollector (税务员) | 每波次结束时获得金币 | Tier 2 | 45 |
| MerchantSoul (商人灵魂) | 商店物品价格-10% | Tier 3 | 65 |
| LuckyCoin (幸运硬币) | 金币+30 | Tier 2 | 50 |
| BankAccount (银行账户) | 携带金币上限翻倍 | Tier 2 | 50 |
| Investment (投资) | 每10秒获得金币 | Tier 3 | 65 |
| BlackMarket (黑市) | 可以用金币刷新商店 | Tier 3 | 75 |

### 15. 经验/升级相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| WisdomScroll (智慧卷轴) | 经验+25% | Tier 2 | 50 |
| ScholarHat (学者之帽) | 升级所需经验-15% | Tier 2 | 50 |
| KnowledgeGem (知识宝石) | 每级额外获得属性 | Tier 3 | 70 |
| AncientTome (古老典籍) | 技能解锁更快 | Tier 3 | 70 |
| StudyGlasses (学习眼镜) | 经验+15% | Tier 2 | 40 |
| MentorSpirit (导师之魂) | 升级时获得双倍属性 | Tier 3 | 75 |
| Enlightenment (启迪) | 每3级获得额外技能 | Tier 4 | 90 |
| PhilosopherStone (贤者之石) | 经验转化为生命 | Tier 4 | 100 |
| LibraryCard (图书证) | 经验+20% | Tier 2 | 45 |
| BrainFood (补脑食品) | 经验+10%，幸运+5 | Tier 2 | 40 |

### 16. 负面效果遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| CursedCoin (被诅咒硬币) | 金币+50%但经验-20% | Tier 2 | 35 |
| BrokenShield (破损护盾) | 护甲-5但伤害+10% | Tier 2 | 40 |
| RustedArmor (生锈护甲) | 护甲-3但移速+15% | Tier 2 | 35 |
| WeakenedHeart (衰弱之心) | 生命上限-20但伤害+15% | Tier 3 | 50 |
| SlowCannon (慢速大炮) | 攻速-30%但伤害+40% | Tier 3 | 55 |
| HeavyBurden (重负) | 移速-20%但生命上限+50 | Tier 3 | 55 |
| ChaosCurse (混乱诅咒) | 随机属性波动 | Tier 3 | 60 |
| DeathWish (死亡之愿) | 伤害+30%但无法生命偷取 | Tier 3 | 65 |
| MidasTouch (点金术) | 金币+100%但击杀不恢复生命 | Tier 3 | 70 |
| PowerDrain (力量流失) | 属性逐渐下降 | Tier 3 | 60 |

### 17. 互动/触发类遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| ReactiveArmor (反应护甲) | 受伤时反弹伤害 | Tier 3 | 70 |
| ThornedSkin (荆棘皮肤) | 近距离反弹伤害 | Tier 3 | 70 |
| MagicMirror (魔法镜) | 反射敌方攻击 | Tier 3 | 75 |
| CounterStrike (反击) | 闪避后立即攻击 | Tier 3 | 70 |
| Riposte (格挡反击) | 完美闪避后造成双倍伤害 | Tier 3 | 70 |
| LuckyDodge (幸运闪避) | 闪避时触发暴击 | Tier 3 | 70 |
| Opportunist (机会主义者) | 敌人破绽时伤害翻倍 | Tier 3 | 70 |
| FirstStrike (先发制人) | 战斗开始时伤害+30% | Tier 2 | 50 |
| LastStand (背水一战) | 生命低于30%时伤害翻倍 | Tier 3 | 75 |
| SecondWind (第二呼吸) | 生命低于50%时攻速翻倍 | Tier 3 | 70 |

### 18. 叠加/累积类遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| RageStack (怒气叠加) | 每击杀敌人伤害+2%（可叠10层） | Tier 3 | 70 |
| MomentumGain (动量积累) | 持续射击伤害递增 | Tier 3 | 70 |
| WarriorSpirit (战士之魂) | 每波次击杀增加属性 | Tier 3 | 70 |
| BeastMode (野兽模式) | 血量越低属性越高 | Tier 3 | 80 |
| RisingPower (上升之力) | 战斗越久伤害越高 | Tier 3 | 75 |
| EscalatingForce (升级力量) | 每次命中增加伤害 | Tier 3 | 70 |
| InfiniteGrowth (无限成长) | 属性持续增长 | Tier 4 | 100 |
| Overdrive (过载) | 持续战斗获得增益 | Tier 3 | 75 |
| BattleRage (战斗狂热) | 战斗中获得攻速 | Tier 3 | 70 |
| BloodPact (血之契约) | 消耗生命换取力量 | Tier 4 | 90 |

### 19. 稀有/传奇遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| CrownOfThorns (荆棘之冠) | 受伤时周围敌人受伤 | Tier 4 | 100 |
| RingOfFire (火焰戒指) | 周围持续造成伤害 | Tier 4 | 110 |
| BootsOfHermes (赫尔墨斯之靴) | 无敌帧+30% | Tier 4 | 105 |
| ShieldOfJustice (正义之盾) | 完美格挡触发反击 | Tier 4 | 100 |
| SwordOfDamocles (达摩克利斯之剑) | 高风险高回报 | Tier 4 | 120 |
| AmuletOfLife (生命护符) | 生命偷取+15% | Tier 4 | 100 |
| OrbOfProtection (保护之球) | 周围敌人攻击减半 | Tier 4 | 100 |
| CloakOfShadows (暗影斗篷) | 隐身时伤害翻倍 | Tier 4 | 105 |
| GauntletOfStrength (力量护手) | 伤害+50% | Tier 4 | 110 |
| HeartOfTheMountain (山之心) | 每秒恢复最大生命1% | Tier 4 | 100 |

### 20. 终极/传说遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| Excalibur (圣剑) | 所有属性+30% | Tier 4 | 150 |
| Mjolnir (雷神之锤) | 闪电造成巨大伤害 | Tier 4 | 150 |
| AegisOfOlympus (奥林匹斯之盾) | 免疫所有负面效果 | Tier 4 | 150 |
| WingsOfIcarus (伊卡洛斯之翼) | 飞行（无敌）+30%移速 | Tier 4 | 150 |
| EyeOfProvidence (普罗维登斯之眼) | 全屏显示敌人 | Tier 4 | 150 |
| PhilosopherStoneUltimate (贤者之石终极版) | 所有属性+50% | Tier 4 | 160 |
| CrownOfTheUniverse (宇宙之冠) | 获得所有遗物效果（减半） | Tier 4 | 200 |
| HolyGrail (圣杯) | 无限生命回复 | Tier 4 | 150 |
| InfinityGauntlet (无限手套) | 集齐六颗宝石的力量 | Tier 4 | 180 |
| CelestialCore (天体核心) | 时间倒流（重置波次） | Tier 4 | 170 |

### 21. 球数量相关遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| MultiBall (多球) | 额外获得1个球 | Tier 2 | 50 |
| BallSquad (球小队) | 额外获得2个球 | Tier 2 | 60 |
| BallSwarm (球群) | 额外获得3个球 | Tier 3 | 70 |
| BallBarrage (球弹幕) | 额外获得4个球 | Tier 3 | 80 |
| BallStorm (球风暴) | 额外获得5个球 | Tier 4 | 100 |
| BallRain (球雨) | 每秒额外发射1个球 | Tier 3 | 80 |
| BallFountain (球喷泉) | 球数量上限+1 | Tier 3 | 75 |
| BallSwarmController (球群控制器) | 球循环发射 | Tier 4 | 100 |
| BallFactory (球工厂) | 每波次补充球数量 | Tier 3 | 80 |
| BallReplication (球复制) | 球有概率分裂 | Tier 4 | 120 |

### 22. 元素属性遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| FireElemental (火元素) | 灼烧伤害+30% | Tier 3 | 70 |
| IceElemental (冰元素) | 冻结时间+1秒 | Tier 3 | 70 |
| LightningElemental (雷元素) | 闪电链伤害+50% | Tier 3 | 75 |
| PoisonElemental (毒元素) | 中毒伤害+40% | Tier 3 | 70 |
| EarthElemental (土元素) | 护甲+10 | Tier 3 | 70 |
| WindElemental (风元素) | 移速+20% | Tier 3 | 70 |
| WaterElemental (水元素) | 生命回复+100% | Tier 3 | 70 |
| LightElemental (光元素) | 暴击伤害+50% | Tier 3 | 80 |
| DarkElemental (暗元素) | 生命偷取+20% | Tier 3 | 80 |
| ChaosElemental (混沌元素) | 所有元素伤害+15% | Tier 4 | 100 |

### 23. 地形互动遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| WallCrawler (爬墙者) | 球可以攀爬边界 | Tier 2 | 50 |
| BouncePad (弹跳垫) | 特定区域弹速增加 | Tier 2 | 45 |
| PortalWand (传送棒) | 球可以传送 | Tier 3 | 75 |
| MagnetBall (磁力球) | 球吸引附近敌人 | Tier 2 | 50 |
| GravityBoots (重力靴) | 可以改变重力方向 | Tier 3 | 70 |
| TeleportBeacon (传送信标) | 球可以瞬移 | Tier 3 | 75 |
| Wormhole (虫洞) | 球可以从任意位置出现 | Tier 3 | 80 |
| BouncyCastle (蹦床城堡) | 所有反弹伤害翻倍 | Tier 3 | 75 |
| TrampolinePark (蹦床公园) | 反弹次数+3 | Tier 4 | 100 |
| WallJumpMaster (墙壁跳跃大师) | 反弹伤害递增 | Tier 3 | 70 |

### 24. 视觉/特效遗物

| 遗物名称 | 效果 | 稀有度 | 价格 |
|----------|------|--------|------|
| PrismSphere (棱镜球) | 球发出彩虹光 | Tier 1 | 15 |
| CrystalBall (水晶球) | 显示未来事件 | Tier 2 | 30 |
| MagicLantern (魔法灯笼) | 照亮隐藏区域 | Tier 2 | 30 |
| MirrorBall (镜球) | 产生闪光特效 | Tier 1 | 15 |
| DiscoBall (迪斯科球) | 音乐节拍增强 | Tier 2 | 35 |
| AuroraSphere (极光球) | 美丽极光效果 | Tier 2 | 35 |
| StarDust (星尘) | 留下星光轨迹 | Tier 1 | 20 |
| RainbowTrail (彩虹尾迹) | 球带有彩虹 | Tier 1 | 20 |
| SparkleEffect (闪亮特效) | 华丽的粒子效果 | Tier 1 | 15 |
| FireworkBall (烟花球) | 爆炸时产生烟花 | Tier 2 | 35 |

## 代码实现说明

### ARelic 类结构

所有遗物的 ARelic 类都继承自 `ARelic` 基类，存放在 `Assets/Scripts/HotFix/_Gameplay/ARelics/Common/` 目录下。

每个遗物类需要实现以下方法：

```csharp
public class ExampleRelic : ARelic
{
    public static string ID = "ExampleRelic";

    public ExampleRelic() : base(ID, "ExampleRelic.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override ARelic makeCopy() => new ExampleRelic();
}
```

### RelicDef ScriptableObject

每个遗物都有对应的 RelicDef ScriptableObject，存放在 `Assets/GameResources/_Gameplay/Relics/` 目录下。

属性修正通过 `PlayerStatMods` 数组配置：

```yaml
PlayerStatMods:
- StatName: DmgRate
  BonusFlat: 0
  BonusPct: 8
```

### 可用的回调方法

| 方法 | 触发时机 |
|------|----------|
| `onEquip(APlayer p)` | 遗物装备时 |
| `onUnequip(APlayer p)` | 遗物卸下时 |
| `onShootBall(Ball ball)` | 球发射时 |
| `onBallReflect(...)` | 球反弹时 |
| `onBallKillBrick(...)` | 球击杀砖块时 |
| `onBallHitBrick(...)` | 球命中砖块时 |
| `onBallHitBorderBot/Top/Left/Right(...)` | 球碰到各边界时 |
| `onPlayerTurnUpdate(APlayer p, float dt)` | 每帧玩家回合更新 |
| `onPlayerTurnBegin(APlayer p)` | 玩家回合开始时 |
| `onPlayerTurnEnd(APlayer p)` | 玩家回合结束时 |
| `onFightingPhaseEnd(APlayer p)` | 战斗阶段结束时 |

## 特殊效果设计待实现

以下遗物需要额外的游戏系统支持才能完全实现：

1. **MirrorImage (镜像)** - 需要球复制系统
2. **BlackHole (黑洞)** - 需要吸引机制
3. **TimeFreeze (时间冻结)** - 需要敌人冻结系统
4. **NovaBlast (新星爆发)** - 需要主动技能系统
5. **PhoenixFeather (凤凰羽毛)** - 需要复活系统
6. **CelestialCore (天体核心)** - 需要时间倒流系统

这些遗物的设计文案已写在代码注释中，可以在实现相应系统后再进行开发。
