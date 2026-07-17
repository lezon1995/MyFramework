using System;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains
{
    public class Buff_Rune_DarkHarvest : Buff, IEvent<DoAttackEffect>
    {
        [Serializable]
        public new class DmgMag : Buff.DmgMag
        {
            public Mag DamagePerStack;

            public override float Value(Buff buff)
            {
                var stackedDamage = DamagePerStack.Value(buff) * buff.Stack;
                var baseDamage = base.Value(buff);
                var damage = baseDamage + stackedDamage;
                return damage;
            }
        }

        public DmgMag Damage;

        public Buff Cooldown;

        public int StackIncrement;
        public float StackIncrementDelay;
        public float HealthPctThreshold;

        protected override void OnAfterAdd()
        {
            Source.Event.addListener(this);
        }

        protected override void OnBeforeRemove()
        {
            Source.Event.removeListener(this);
        }

        public void onEvent(DoAttackEffect e)
        {
            var health = e.Character.Health;
            if (health.HealthPct > HealthPctThreshold)
                return;

            if (Target.HasBuff(Cooldown.BuffType))
                return;
            
            Target.ApplyBuff(Cooldown);

            var mag = Damage;
            var value = mag.Value(this);
            if (value > 0)
            {
                var dmg = new Dmg(value, mag.DmgType, mag.DmgAlgo);
                dmg.SetEffect(Dmg.Effects.Skill);
                health.Damage(ref dmg, gameObject, source: Target.Character);
            }

            Timing.RunCoroutine(DelayIncreaseStack());

        }

        IEnumerator<float> DelayIncreaseStack()
        {
            yield return Timing.WaitForSeconds(StackIncrementDelay);
            IncreaseStack(StackIncrement);
        }
    }
}