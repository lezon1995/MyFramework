using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 将造成伤害的15%转为血量储存起来
    /// 最多储存5+0.2AP点
    /// 玩家碰到时恢复已存储的生命值。
    /// </summary>
    public class Ball_Vampire : Ball
    {
        public override BallType BallType => BallType.Vampire;

        TextMeshPro textHealthStorage;

        public float ConvertRate = 0.5F;
        public float MaxHealthRate = 0.25F;
        public float curHealthStorage;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.TryGetComponentInChildren(out textHealthStorage);
        }

        public override void onAcquire()
        {
            base.onAcquire();

            setCurHealthStorage(0F);
        }

        public override void onRelease()
        {
            base.onRelease();
            setCurHealthStorage(0F);
        }

        protected override void CheckBallExpiration(float dt)
        {
        }

        public override void onRecollected()
        {
            var value = curHealthStorage.toInt();
            if (value > 0)
            {
                var heal = new Heal((int)value);
                _player.Health.ReceiveHealth(heal, source: _player);
                sound.play(SoundDefine.HEAL_EFFECT);
            }
        }

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            var rate = ConvertRate + _player.ap / 1000F;
            var healthStorage = rate * e.dmg.DamageDealt;
            var value = Mathf.Clamp(curHealthStorage + healthStorage, 0, getMaxHealthStorage());
            setCurHealthStorage(value);
        }

        float getMaxHealthStorage()
        {
            var healthRate = MaxHealthRate + _player.ap / 1000F;
            var healthStorage = _player.GetStat(Character.Stat.HealthMax).Value * healthRate;
            var maxHealthStorage = healthStorage;
            return maxHealthStorage;
        }

        void setCurHealthStorage(float value)
        {
            curHealthStorage = value;
            if (textHealthStorage)
            {
                var intValue = value.toInt();
                textHealthStorage.text = intValue == 0 ? null : intValue.IToS();
            }
        }
    }
}