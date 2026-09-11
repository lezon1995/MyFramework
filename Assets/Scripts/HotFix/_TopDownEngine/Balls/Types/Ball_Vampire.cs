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

        public float convertRate = 0.15F;
        public float baseHealthStorage = 5F;
        public float baseHealthStorageRate = 0.2F;
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

        protected override void playHitBrickSfx(Brick brick)
        {
            sound.play(SoundDefine.VAMPIRE_HIT);
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
            }
        }

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            var healthStorage = convertRate * e.dmg.DamageDealt;
            var value = Mathf.Clamp(curHealthStorage + healthStorage, 0, getMaxHealthStorage());
            setCurHealthStorage(value);
        }

        float getMaxHealthStorage()
        {
            var maxHealthStorage = baseHealthStorage + baseHealthStorageRate * _player.GetStat(Character.Stat.AP).Value;
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