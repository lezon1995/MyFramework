using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

public partial class BallPurchaseItem
{
    public myUGUIButton Btn => btn;

    public void SetHovered(bool on)
    {
        hovered?.setActive(on);
    }

    public void SetRarity(ItemRarity rarity)
    {
        var c = gameDesign.getRarityColor(rarity);
        itemBorder.setColor(c.border);
        itemBg.setColor(c.bg);
        IconBg.setColor(c.iconBg);
        itemName.setColor(c.title);
    }

    public void SetNewTag(bool on) => newTag.setActive(on);
    public void SetIcon(Sprite s) => itemIcon?.setSpriteOnly(s);
    public void SetName(string s) => itemName.setText(s ?? string.Empty);
    public void SetPrice(int price) => itemPrice.setText(price.IToS());

    public void SetSold(bool sold)
    {
        hovered.setActive(!sold);
        itemSold.setActive(sold);
        if (btn.tryGetUnityComponent<ButtonScaleAnim>(out var btnScaleAnim))
        {
            btnScaleAnim.ResetToNormal();
            btnScaleAnim.enabled = !sold;
        }
    }

    public void SetDesc(BallDef def)
    {
        using var _ = new MyStringBuilderScope(out var sb);

        var configs = def.StatsTemplate.Configs;

        build_HitDamage(sb, configs);
        build_AttackSpeed(sb, configs);
        build_Knockback(sb, configs);
        // build_BallisticSpeed(sb, configs);
        build_Crit(sb, configs);
        // build_HitEffectChance(sb, configs);
        build_Duration(sb, configs);

        itemDesc.setText(sb.ToString());
    }

    static void build_Duration(MyStringBuilder sb, Dictionary<string, float> configs)
    {
        //持续时间
        {
            var statKey = Ball.Stat.Duration.Key();
            var statName = LocalizedStats.getName(statKey);
            var universalColor = gameDesign.universalColor;
            sb.add(statName.color(universalColor.statEntry), " : ");

            var ballDuration = configs.get(statKey);
            var playerDuration = player.GetStat(Character.Stat.Duration);
            var duration = ballDuration * (1 + playerDuration.Value);
            if (playerDuration.BonusValue > 0)
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

    static void build_HitEffectChance(MyStringBuilder sb, Dictionary<string, float> configs)
    {
        //撞击特效概率
        {
            var statKey = Ball.Stat.HitEffectChance.Key();
            var statName = LocalizedStats.getName(statKey);
            var universalColor = gameDesign.universalColor;
            sb.add(statName.color(universalColor.statEntry), " : ");

            var ballHitEffectChance = configs.get(statKey);
            var playerHitEffectChance = player.GetStat(Character.Stat.HitEffectChance);
            if (playerHitEffectChance.BonusValue > 0)
            {
                var hitEffectChance = ballHitEffectChance + playerHitEffectChance.Value;
                sb.add(hitEffectChance.toPercent(0).color(universalColor.enhanced));
            }
            else
            {
                var hitEffectChance = ballHitEffectChance + playerHitEffectChance.Value;
                sb.add(hitEffectChance.toPercent(0));
            }

            sb.addLine();
        }
    }

    static void build_Crit(MyStringBuilder sb, Dictionary<string, float> configs)
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

            var playerCritChance = player.GetStat(Character.Stat.CritChance);
            var playerCritDamage = player.GetStat(Character.Stat.CritDamage);

            if (playerCritDamage.BonusValue > 0)
            {
                var critDamage = ballCritDamage + playerCritDamage.Value;
                sb.add("x ", critDamage.FToS(2).color(universalColor.enhanced));
            }
            else
            {
                var critDamage = ballCritDamage + playerCritDamage.Value;
                sb.add("x ", critDamage.FToS(2));
            }

            sb.add(" ");
            if (playerCritChance.BonusValue > 0)
            {
                var critChance = ballCritChance + playerCritChance.Value;
                sb.add("(", critChance.toPercent(0).color(universalColor.enhanced), ")");
            }
            else
            {
                var critChance = ballCritChance + playerCritChance.Value;
                sb.add("(", critChance.toPercent(0), ")");
            }


            sb.addLine();
        }
    }

    static void build_BallisticSpeed(MyStringBuilder sb, Dictionary<string, float> configs)
    {
        //弹道速度
        {
            var statKey = Ball.Stat.BallisticSpeed.Key();
            var statName = LocalizedStats.getName(statKey);
            var universalColor = gameDesign.universalColor;
            sb.add(statName.color(universalColor.statEntry), " : ");

            var ballBallisticSpeed = configs.get(statKey);
            var playerBallisticSpeed = player.GetStat(Character.Stat.BallisticSpeed);
            if (playerBallisticSpeed.BonusValue > 0)
            {
                var logicBallisticSpeed = ballBallisticSpeed * (1 + playerBallisticSpeed.Value);
                sb.add(logicBallisticSpeed.FToS(0).color(universalColor.enhanced));
            }
            else
            {
                var logicBallisticSpeed = ballBallisticSpeed * (1 + playerBallisticSpeed.Value);
                sb.add(logicBallisticSpeed.FToS(0));
            }

            sb.addLine();
        }
    }

    static void build_Knockback(MyStringBuilder sb, Dictionary<string, float> configs)
    {
        //击退
        {
            var statKey = Ball.Stat.Knockback.Key();
            var statName = LocalizedStats.getName(statKey);
            var universalColor = gameDesign.universalColor;
            sb.add(statName.color(universalColor.statEntry), " : ");

            var ballKnockback = configs.get(statKey);
            var playerKnockback = player.GetStat(Character.Stat.Knockback);
            if (playerKnockback.BonusValue > 0)
            {
                var logicKnockback = ballKnockback + playerKnockback.Value;
                sb.add(logicKnockback.FToS(1).color(universalColor.enhanced));
            }
            else
            {
                var logicAttackSpeed = ballKnockback + playerKnockback.Value;
                sb.add(logicAttackSpeed.FToS(1));
            }

            sb.addLine();
        }
    }

    static void build_AttackSpeed(MyStringBuilder sb, Dictionary<string, float> configs)
    {
        //攻击速度
        {
            var statKey = Ball.Stat.AS.Key();
            var statName = LocalizedStats.getName(statKey);
            var universalColor = gameDesign.universalColor;
            sb.add(statName.color(universalColor.statEntry), " : ");

            var ballAttackSpeed = configs.get(statKey);
            var playerAttackSpeed = player.GetStat(Character.Stat.AS);
            var attackSpeed = ballAttackSpeed * (1 + playerAttackSpeed.Value);
            if (playerAttackSpeed.BonusValue > 0)
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

    static void build_HitDamage(MyStringBuilder sb, Dictionary<string, float> configs)
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

            var ad = player.GetStat(Character.Stat.AD);
            var playerDmgRate = player.GetStat(Character.Stat.DmgRate);
            var dmgRate = ballDmgRate * (1 + playerDmgRate.Value);
            if (ad.BonusValue > 0 || playerDmgRate.BonusValue > 0)
            {
                var hitDamage = rawHitDamage + hitDamageRate * ad.Value;
                hitDamage *= dmgRate;
                sb.add(hitDamage.FToS(0).color(universalColor.enhanced));
            }
            else
            {
                var hitDamage = rawHitDamage + hitDamageRate * ad.Value;
                hitDamage *= dmgRate;
                sb.add(hitDamage.FToS(0));
            }

            sb.add(" = ");
            sb.add(rawHitDamage.FToS().color(universalColor.statValueRaw), " + ", hitDamageRate.toPercent(), ad.Name.toSprite());
            sb.addLine();
        }
    }
}