using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains;

public struct BallDuplicateArg
{
    public BallType Type;
    public Vector3 Pos;
    public Vector2 Normal;
    public int Level;
    public float Duration;
}

public class BallDuplicateEffect : ALogicEffect, IArgs<APlayer, Weapon, BallDuplicateArg>
{
    APlayer _player;
    Weapon _weapon;
    BallDuplicateArg _arg;

    public void onCreate(APlayer p1, Weapon w, BallDuplicateArg arg)
    {
        _player = p1;
        _weapon = w;
        _arg = arg;
        duration = 0.1F;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        _player = null;
        _weapon = null;
        _arg = default;
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            int count = 1;
            foreach (var p in _player.powers)
            {
                if (p is IBallDuplicateEffectModifier m)
                {
                    m.modifyCount(ref count);
                }
            }

            for (int i = 0; i < count; i++)
            {
                var angle = randomFloat(-60F, 60F);
                var direction = _arg.Normal.MMRotate(angle);
                var ball = _player.BallManagement.Instance.acquireBall(_arg.Type, _arg.Pos, direction, _arg.Level, duration: _arg.Duration);
                ball.setTeleportPosition(_arg.Pos);
                ball.setTemp(true);

                // we activate the object
                ball.setActive(true);

                ball.SetWeapon(_weapon);
                if (_player)
                {
                    ball.SetOwner(_player);
                    ball.SetPlayer(_player);
                    var dmg = _weapon.Dmg;
                    // dmg.SetDmgRate(0.5F);
                    ball.SetDamage(dmg);
                }

                ball.setShootDirection(direction);
                ball.SetDirection(direction, Quaternion.identity);
            }

            fx.play(FxDefine.BALL_DUPLICATE, _arg.Pos);
            isDone = true;
        }

        return base.fixedUpdate(dt);
    }
}