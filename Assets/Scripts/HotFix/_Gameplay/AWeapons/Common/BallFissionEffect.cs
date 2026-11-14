using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains;

public struct BallFissionArg
{
    public BallType Type;
    public Vector3 Pos;
    public Vector2 Normal;
    public int Level;
    public float Duration;
}

public class BallFissionEffect : ALogicEffect, IArgs<APlayer, Weapon, BallFissionArg>
{
    APlayer _player;
    Weapon _weapon;
    BallFissionArg _arg;

    public void onCreate(APlayer p1, Weapon w, BallFissionArg arg)
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
                if (p is IBallFissionEffectModifier m)
                {
                    m.modifyCount(ref count);
                }
            }

            for (int i = 0; i < count; i++)
            {
                var angle = randomFloat(-60F, 60F);
                var direction = _arg.Normal.MMRotate(angle);

                var ball = _player.BallManagement.Instance.acquireBall(BallType.FissionMini, _arg.Pos, direction, _arg.Level, _arg.Duration);
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
                    dmg.SetDmgRate(0.5F);
                    ball.SetDamage(dmg);
                }

                ball.setShootDirection(direction);
                ball.SetDirection(direction, Quaternion.identity);

                fx.play(FxDefine.BALL_FISSION, _arg.Pos);
            }


            isDone = true;
        }

        return base.fixedUpdate(dt);
    }
}