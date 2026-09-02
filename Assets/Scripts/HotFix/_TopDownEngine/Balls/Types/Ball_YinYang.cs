using System.Collections.Generic;
using MoreMountains.Tools;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    public class Ball_YinYang : Ball, IEvent<OnBrickDeath>
    {
        public override BallType BallType => BallType.YinYang;

        float bonusARDamage;
        float bonusMRDamage;
        const string mod_key = nameof(Ball_YinYang);

        static Dictionary<Brick, int> stolenBricks = new();

        protected override bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            if (brick.GetStat(Brick.Stat.AR, out var ar))
            {
                if (!ar.BonusFlat.GetMod(mod_key, out var mod))
                    ar.BonusFlat.AddFlat(-1, mod_key);
                else
                    mod.Value -= 1;
            }

            if (brick.GetStat(Brick.Stat.MR, out var mr))
            {
                if (!mr.BonusFlat.GetMod(mod_key, out var mod))
                    mr.BonusFlat.AddFlat(-1, mod_key);
                else
                    mod.Value -= 1;
            }

            if (_player.GetStat(Character.Stat.AR, out var playerAR))
            {
                if (!playerAR.BonusFlat.GetMod(mod_key, out var mod))
                    playerAR.BonusFlat.AddFlat(1, mod_key);
                else
                    mod.Value += 1;
            }

            if (_player.GetStat(Character.Stat.MR, out var playerMR))
            {
                if (!playerMR.BonusFlat.GetMod(mod_key, out var mod))
                    playerMR.BonusFlat.AddFlat(1, mod_key);
                else
                    mod.Value += 1;
            }

            if (!stolenBricks.ContainsKey(brick))
            {
                brick.Event.addListener<OnBrickDeath>(this);
                stolenBricks[brick] = 1;
            }
            else
            {
                stolenBricks[brick]++;
            }

            return base.onHitEnter(brick, normal, out triggerRegularHit);
        }

        public override int getHitDamage()
        {
            float hitDamage = base.getHitDamage();

            if (Player.GetStat(Character.Stat.AR, out var characterAR))
            {
                bonusARDamage = characterAR.Value * 0.5F;
                hitDamage += bonusARDamage;
            }

            if (Player.GetStat(Character.Stat.MR, out var characterMR))
            {
                bonusMRDamage = characterMR.Value * 0.5F;
                hitDamage += bonusMRDamage;
            }

            return (int)hitDamage;
        }

        public override Dmg getHitDmg(Brick brick, Vector2 normal)
        {
            var d = getHitDamage();
            var dmg = Dmg.AD(d);

            var bonusDamage = bonusARDamage + bonusMRDamage;
            if (bonusDamage.isZero())
                dmg.setMixed(0.5F, 0.5F);
            else
                dmg.setMixed(bonusARDamage / bonusDamage, bonusMRDamage / bonusDamage);

            dmg.setAttackEffect();
            _player.GetStat(Character.Stat.DmgRate, out var playerDmgRate);
            GetStat(Stat.DmgRate, out var ballDmgRate);
            var dmgRate = (1 + playerDmgRate.Value) * ballDmgRate.Value;
            dmg.SetDmgRate(dmgRate);
            dmg.setHitNormal(normal);

            _player.GetStat(Character.Stat.CritChance, out var playerCritChance);
            GetStat(Stat.CritChance, out var ballCritChance);
            var critChange = playerCritChance.Value + ballCritChance.Value;
            if (randomHit(critChange))
                dmg.Crit();

            _player.GetStat(Character.Stat.CritDamage, out var playerCritDamage);
            GetStat(Stat.CritDamage, out var ballCritDamage);
            var critDamage = (1 + playerCritDamage.Value) * ballCritDamage.Value;
            dmg.SetCritDamage(critDamage);

            return dmg;
        }

        public void onEvent(OnBrickDeath e)
        {
            e.brick.Event.removeListener<OnBrickDeath>(this);
            stolenBricks.Remove(e.brick, out var stolenValue);

            if (_player.GetStat(Character.Stat.AR, out var playerAR))
            {
                if (playerAR.BonusFlat.GetMod(mod_key, out var mod))
                    mod.Value -= stolenValue;
            }

            if (_player.GetStat(Character.Stat.MR, out var playerMR))
            {
                if (playerMR.BonusFlat.GetMod(mod_key, out var mod))
                    mod.Value -= stolenValue;
            }
        }
    }
}