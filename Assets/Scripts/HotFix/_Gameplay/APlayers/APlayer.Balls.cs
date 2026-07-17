using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace MoreMountains
{
    public partial class APlayer
    {
        public bool isReturnBall;
        public int ballMaxCount = 1;
        public int ballCount = 1;

        public List<Ball> activeBalls = new();
        public Vector3 originalShootPosition, shootPosition;
        public bool isFirstBallReturn;
        public int toClaimRewardCount;

        protected List<BuffObject> buffs = new();
        protected List<Type> ballBuffs = new();

        protected override void Initialization()
        {
            base.Initialization();
            
            Exp.ResetLevel();
            Exp.SetOnLevelUp(onLevelUp);
            
            originalShootPosition = shootPosition = getWorldPosition();
            setOriginalShootPositionX(shootPosition.x);

            // buffs.add(CLASS<LightningStrike>());
            // buffs.add(CLASS<LightningStrike3>());
            // buffs.add(CLASS<LaserHorizontal>());
            // buffs.add(CLASS<LaserVertical>());

            ballBuffs.add(typeof(LaserHorizontal));
            ballBuffs.add(typeof(LaserVertical));
            ballBuffs.add(typeof(LightningStrike));
            ballBuffs.add(typeof(LightningStrike3));

            addListeners();
        }

        protected override void OnDestroy()
        {
            removeListeners();
            base.OnDestroy();
        }

        public void shootBalls(Vector3 pos, Vector3 dir)
        {
            // ballMaxCount++;
            // CtrUI.instance.SetReturnBallButton(true);
            isReturnBall = false;
            actionManager.addToBot<ShootBallsAction>().with(pos, dir);
        }

        public void returnBall()
        {
            // CtrUI.instance.SetReturnBallButton(false);
            actionManager.addToTop<ReturnBallsAction>().with(shootPosition);
        }

        public void setCurrentShootPosition(Vector2 p)
        {
            shootPosition = p;
            // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
        }

        public void setOriginalShootPositionX(float posX)
        {
            shootPosition = originalShootPosition;
            shootPosition.x = posX;
            originalShootPosition.x = posX;

            // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
        }

        public void moveShootPositionX(float deltaX)
        {
            shootPosition.x += deltaX;
            originalShootPosition.x += deltaX;
        }

        public void setBallReturn(Ball ball)
        {
            if (!isFirstBallReturn)
            {
                isFirstBallReturn = true;
                setOriginalShootPositionX(ball.getWorldPosition().x);
                activeBalls.Remove(ball);
                ballManager.releaseBall(ball);
                return;
            }

            ball.setEnabled(false);
            activeBalls.Remove(ball);
            Tween
                .Position(ball.getTransform(), endValue: shootPosition, duration: 0.15F, ease: Ease.OutCubic)
                .OnComplete(ball, b =>
                {
                    ballManager.releaseBall(b);
                });

            return;
        }

        public void gainExp(int xp)
        {
            Exp.AddXp(xp);
        }

        protected void onLevelUp()
        {
            toClaimRewardCount++;
        }
    }
}