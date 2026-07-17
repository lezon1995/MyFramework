using UnityEngine;

namespace MoreMountains
{
    #region Character

    public struct DoHeal
    {
        public Health Health;
        public Heal Heal;

        public DoHeal(Health health, Heal heal)
        {
            Health = health;
            Heal = heal;
        }
    }

    public struct OnHeal
    {
        public Character Source;
        public Heal Heal;

        public OnHeal(Character source, Heal heal)
        {
            Source = source;
            Heal = heal;
        }
    }

    public struct DoKill
    {
        public Character Character;
        public GameObject Instigator;

        public DoKill(Character character, GameObject instigator)
        {
            Character = character;
            Instigator = instigator;
        }
    }

    public struct DoDash
    {
    }

    public struct DoDashDodge
    {
    }

    public struct OnCombat
    {
        public Character Character { get; set; }
        const float DisengageTime = 6F;
        public bool IsOn { get; private set; }

        float elapsed;

        public void Turn(bool active)
        {
            IsOn = active;

            if (active)
            {
                elapsed = 0F;
            }

            Character.Event.trigger(this);
        }

        public void Check(float dt)
        {
            if (IsOn)
            {
                elapsed += dt;
                if (elapsed > DisengageTime)
                {
                    Turn(false);
                }
            }
        }
    }

    public struct DoAttackEffect
    {
        public Character Character;

        public DoAttackEffect(Character character)
        {
            Character = character;
        }
    }

    public struct DoAbilityEffect
    {
        public Character Character;

        public DoAbilityEffect(Character character)
        {
            Character = character;
        }
    }

    public struct DoMove
    {
        public Vector3 Movement;

        public DoMove(Vector3 movement)
        {
            Movement = movement;
        }
    }

    #endregion

    #region Health

    public struct OnHit
    {
    }

    public struct OnRevive
    {
    }

    public struct OnDeath
    {
        public OnDeath()
        {
        }
    }

    public struct DoDmg
    {
        public Character Character;
        public Dmg Dmg;

        public DoDmg(Character character, Dmg dmg)
        {
            Character = character;
            Dmg = dmg;
        }
    }

    public struct OnDmg
    {
        public Character Source;
        public Dmg Dmg;

        public OnDmg(Character source, Dmg dmg)
        {
            Source = source;
            Dmg = dmg;
        }
    }

    #endregion

    #region Weapon

    public struct OnWindup
    {
        public enum States
        {
            Start,
            Finish,
            Cancel,
        }

        public Weapon Weapon;
        public States State;
        public float Delay;

        OnWindup(Weapon weapon, States state, float delay = 0F)
        {
            Weapon = weapon;
            State = state;
            Delay = delay;
        }

        public static OnWindup Start(Weapon weapon, float delayBeforeUse) => new(weapon, States.Start, delayBeforeUse);
        public static OnWindup Finish(Weapon weapon) => new(weapon, States.Finish);
        public static OnWindup Cancel(Weapon weapon) => new(weapon, States.Cancel);
    }

    public struct DoShoot
    {
    }

    #endregion
}