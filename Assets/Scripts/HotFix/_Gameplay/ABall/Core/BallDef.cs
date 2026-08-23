using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace MoreMountains
{
    /// <summary>
    /// 球的静态定义 —— 由策划用 CSV / SO 维护。
    /// 系统初始化时把所有 BallDef 注册到 BallDefLibrary。
    /// </summary>
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/BallDef")]
    public sealed class BallDef : ScriptableObject, IRarityObject
    {
        public int BallDefId => (int)Type;
        public BallType Type;

        [Header("Price")]
        public int BasePrice = 10; // 商店售价 / 售出回收基于它

        [Header("Level")]
        public int MaxLevel = 3;

        [Header("Upgrade Recipe")]
        public int UpgradeCombineCount = 2;

        public int UpgradeGoldCost;

        [Header("Merge Recipe")]
        /// <summary>融合后产物的 def id。0 / -1 表示不可融合。</summary>
        public int MergeResultDefId;

        public int MergeGoldCost = 100;

        [Header("Visual")]
        public Sprite Icon;

        public Color LevelColor = Color.white;
        public ItemRarity Rarity;
        public BallStatsTemplate StatsTemplate;
        public ItemRarity rarity => Rarity;

        public LocalizedString DisplayName;
        public LocalizedString DisplayDescription;

        public MetaHandleWeapon MetaHandleWeapon;

        public static void BuildDescription(MyStringBuilder sb, BallDef def, APlayer p = null)
        {
            var configs = def.StatsTemplate.Configs;

            build_HitDamage(sb, configs, p);
            build_AttackSpeed(sb, configs, p);
            build_Knockback(sb, configs, p);
            // build_BallisticSpeed(sb, configs, p);
            build_Crit(sb, configs, p);
            // build_HitEffectChance(sb, configs, p);
            build_Duration(sb, configs, p);
            build_DisplayDescription(sb, def, p);
        }

        static void build_DisplayDescription(MyStringBuilder sb, BallDef def, APlayer p = null)
        {
            sb.addLine();
            var localizedString = def.DisplayDescription.GetLocalizedString();
            sb.add(localizedString);
        }

        static void build_Duration(MyStringBuilder sb, Dictionary<string, float> configs, APlayer p = null)
        {
            //持续时间
            {
                var statKey = Ball.Stat.Duration.Key();
                var statName = LocalizedStats.getName(statKey);
                var universalColor = gameDesign.universalColor;
                sb.add(statName.color(universalColor.statEntry), " : ");

                var ballDuration = configs.get(statKey);
                UniStats.Stat playerDuration = null;
                if (p)
                {
                    playerDuration = p.GetStat(Character.Stat.Duration);
                }

                var duration = ballDuration * (1F + ( playerDuration?.Value ?? 0F));
                if (playerDuration is { BonusValue: > 0 })
                {
                    sb.add(duration.FToS(1).color(universalColor.enhanced), "s");
                }
                else
                {
                    sb.add(duration.FToS(1), "s");
                }

                sb.addLine();
            }
        }

        static void build_HitEffectChance(MyStringBuilder sb, Dictionary<string, float> configs, APlayer p = null)
        {
            //撞击特效概率
            {
                var statKey = Ball.Stat.HitEffectChance.Key();
                var statName = LocalizedStats.getName(statKey);
                var universalColor = gameDesign.universalColor;
                sb.add(statName.color(universalColor.statEntry), " : ");

                var ballHitEffectChance = configs.get(statKey);
                UniStats.Stat playerHitEffectChance = null;
                if (p)
                {
                    playerHitEffectChance = p.GetStat(Character.Stat.HitEffectChance);
                }

                var hitEffectChance = ballHitEffectChance + (playerHitEffectChance?.Value ?? 0F);
                if (playerHitEffectChance is { BonusValue: > 0 })
                {
                    sb.add(hitEffectChance.toPercent(0).color(universalColor.enhanced));
                }
                else
                {
                    sb.add(hitEffectChance.toPercent(0));
                }

                sb.addLine();
            }
        }

        static void build_Crit(MyStringBuilder sb, Dictionary<string, float> configs, APlayer p = null)
        {
            //暴击
            {
                var critName = LocalizedStats.getName("Crit");
                var universalColor = gameDesign.universalColor;
                sb.add(critName.color(universalColor.statEntry), " : ");

                var statCritChance = Ball.Stat.CritChance.Key();
                var statCritDamage = Ball.Stat.CritDamage.Key();
                var ballCritChance = configs.get(statCritChance);
                var ballCritDamage = configs.get(statCritDamage);

                UniStats.Stat playerCritChance = null;
                UniStats.Stat playerCritDamage = null;
                if (p)
                {
                    playerCritChance = p.GetStat(Character.Stat.CritChance);
                    playerCritDamage = p.GetStat(Character.Stat.CritDamage);
                }

                var critDamage = ballCritDamage + (playerCritDamage?.Value ?? 0F);
                if (playerCritDamage is { BonusValue: > 0 })
                {
                    sb.add("x ", critDamage.FToS(2).color(universalColor.enhanced));
                }
                else
                {
                    sb.add("x ", critDamage.FToS(2));
                }

                sb.add(" ");
                var critChance = ballCritChance + (playerCritChance?.Value ?? 0F);
                if (playerCritChance is { BonusValue: > 0 })
                {
                    sb.add("(", critChance.toPercent(0).color(universalColor.enhanced), ")");
                }
                else
                {
                    sb.add("(", critChance.toPercent(0), ")");
                }

                sb.addLine();
            }
        }

        static void build_BallisticSpeed(MyStringBuilder sb, Dictionary<string, float> configs, APlayer p = null)
        {
            //弹道速度
            {
                var statKey = Ball.Stat.BallisticSpeed.Key();
                var statName = LocalizedStats.getName(statKey);
                var universalColor = gameDesign.universalColor;
                sb.add(statName.color(universalColor.statEntry), " : ");

                var ballBallisticSpeed = configs.get(statKey);
                UniStats.Stat playerBallisticSpeed = null;
                if (p)
                {
                    playerBallisticSpeed = p.GetStat(Character.Stat.BallisticSpeed);
                }

                var logicBallisticSpeed = ballBallisticSpeed * (1 + (playerBallisticSpeed?.Value ?? 0F));
                if (playerBallisticSpeed is { BonusValue: > 0 })
                {
                    sb.add(logicBallisticSpeed.FToS(0).color(universalColor.enhanced));
                }
                else
                {
                    sb.add(logicBallisticSpeed.FToS(0));
                }

                sb.addLine();
            }
        }

        static void build_Knockback(MyStringBuilder sb, Dictionary<string, float> configs, APlayer p = null)
        {
            //击退
            {
                var statKey = Ball.Stat.Knockback.Key();
                var statName = LocalizedStats.getName(statKey);
                var universalColor = gameDesign.universalColor;
                sb.add(statName.color(universalColor.statEntry), " : ");

                var ballKnockback = configs.get(statKey);
                UniStats.Stat playerKnockback = null;
                if (p)
                {
                    playerKnockback = p.GetStat(Character.Stat.Knockback);
                }

                var logicKnockback = ballKnockback + (playerKnockback?.Value ?? 0F);
                if (playerKnockback is { BonusValue: > 0 })
                {
                    sb.add(logicKnockback.FToS(1).color(universalColor.enhanced));
                }
                else
                {
                    sb.add(logicKnockback.FToS(1));
                }

                sb.addLine();
            }
        }

        static void build_AttackSpeed(MyStringBuilder sb, Dictionary<string, float> configs, APlayer p = null)
        {
            //攻击速度
            {
                var statKey = Ball.Stat.AS.Key();
                var statName = LocalizedStats.getName(statKey);
                var universalColor = gameDesign.universalColor;
                sb.add(statName.color(universalColor.statEntry), " : ");

                var ballAttackSpeed = configs.get(statKey);
                UniStats.Stat playerAttackSpeed = null;
                if (p)
                {
                    playerAttackSpeed = p.GetStat(Character.Stat.AS);
                }

                var attackSpeed = ballAttackSpeed * (1 + (playerAttackSpeed?.Value ?? 0F));
                if (playerAttackSpeed is { BonusValue: > 0 })
                {
                    sb.add(attackSpeed.FToS(2).color(universalColor.enhanced));
                }
                else
                {
                    sb.add(attackSpeed.FToS(2));
                }

                sb.add(" ");
                sb.add("(", (1 / attackSpeed).FToS(2), "s", ")");

                sb.addLine();
            }
        }

        static void build_HitDamage(MyStringBuilder sb, Dictionary<string, float> configs, APlayer p = null)
        {
            //撞击伤害
            {
                var statKey = Ball.Stat.HitDamage.Key();
                var statBallDmgRate = Ball.Stat.DmgRate.Key();
                var statName = LocalizedStats.getName(statKey);
                var universalColor = gameDesign.universalColor;
                sb.add(statName.color(universalColor.statEntry), " : ");

                var rawHitDamage = configs.get(statKey);
                var ballDmgRate = configs.get(statBallDmgRate);
                var statHitDamageRate = Ball.Stat.HitDamageRate.Key();
                var hitDamageRate = configs.get(statHitDamageRate);

                UniStats.Stat ad = null;
                UniStats.Stat playerDmgRate = null;
                if (p)
                {
                    ad = p.GetStat(Character.Stat.AD);
                    playerDmgRate = p.GetStat(Character.Stat.DmgRate);
                }

                var dmgRate = ballDmgRate * (1F + (playerDmgRate?.Value ?? 0F));
                var hitDamage = rawHitDamage + hitDamageRate * (ad?.Value ?? 0F);
                hitDamage *= dmgRate;
                if (ad is { BonusValue: > 0 } || playerDmgRate is { BonusValue: > 0 })
                {
                    sb.add(hitDamage.FToS(0).color(universalColor.enhanced));
                }
                else
                {
                    sb.add(hitDamage.FToS(0));
                }

                sb.add(" = ");
                sb.add(rawHitDamage.FToS().color(universalColor.statValueRaw), " + ", hitDamageRate.toPercent(), Character.Stat.AD.Key().toSprite());
                sb.addLine();
            }
        }
    }
}