using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;

namespace MarbleHero
{
    public partial class APlayer
    {
        public bool isReturnBall;
        public int ballMaxCount = 1;
        public int ballCount = 1;

        protected GuideLine guideLine;
        protected Exp exp;
        public List<Ball> activeBalls = new();
        public Vector3 originalShootPosition, shootPosition;
        public bool isFirstBallReturn;

        protected List<Buff> buffs = new();
        protected List<Type> ballBuffs = new();

        public override void setObject(GameObject obj)
        {
            base.setObject(obj);
        }

        public override void init()
        {
            base.init();
            setWorldPositionY(-4.7F);

            guideLine = CLASS<GuideLine>();
            guideLine.setObject(gameObject.find("GuideLine"));
            guideLine.setName("GuideLine");
            guideLine.setPlayer(this);
            guideLine.init();

            exp = CLASS<Exp>();
            var path = $"{GAMEPLAY_PATH}/ExpData.asset";
            var data = mResourceManager.loadGameResource<ExpData>(path);
            exp.setData(data.getResource());
            exp.resetLevel();
            exp.setOnLevelUp(onLevelUp);

            originalShootPosition = shootPosition = getWorldPosition();
            setOriginalShootPositionX(shootPosition.x);

            // buffs.add(CLASS<LightingStrike>()).setBrickManager(brickManager);
            // buffs.add(CLASS<LightingStrike3>()).setBrickManager(brickManager);
            // buffs.add(CLASS<LaserHorizontal>()).setBrickManager(brickManager);
            // buffs.add(CLASS<LaserVertical>()).setBrickManager(brickManager);

            ballBuffs.add(typeof(LaserHorizontal));
            ballBuffs.add(typeof(LaserVertical));
            ballBuffs.add(typeof(LightningStrike));
            ballBuffs.add(typeof(LightningStrike3));

            addListeners();
        }

        public override void destroy()
        {
            base.destroy();

            UN_CLASS(ref guideLine);
            UN_CLASS(ref exp);

            removeListeners();
        }

        public GuideLine getGuideLine() => guideLine;

        public void shootBalls(Vector3 shootPosition, Vector3 shootDirection)
        {
            // ballMaxCount++;
            // CtrUI.instance.SetReturnBallButton(true);
            isReturnBall = false;
            guideLine.setIndicatorBallActive(false);
            guideLine.guidelineOff();
            actionManager.addToBot<ShootBallsAction>().with(shootPosition, shootDirection);
        }

        public void returnBall()
        {
            // CtrUI.instance.SetReturnBallButton(false);
            actionManager.addToTop<ReturnBallsAction>().with(shootPosition);
        }

        public void setCurrentShootPosition(Vector2 p)
        {
            shootPosition = p;
            guideLine.setShootPosition(shootPosition, true);
            guideLine.setIndicatorBallPosition(shootPosition);
            guideLine.setIndicatorBallActive(true);

            // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
        }

        public void setOriginalShootPositionX(float posX)
        {
            shootPosition = originalShootPosition;
            shootPosition.x = posX;
            originalShootPosition.x = posX;
            guideLine.setOriginalShootPosition(originalShootPosition);
            guideLine.setShootPosition(originalShootPosition, true);
            guideLine.setIndicatorBallPosition(originalShootPosition);
            guideLine.setIndicatorBallActive(true);

            // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
        }

        public void moveShootPositionX(float deltaX)
        {
            shootPosition.x += deltaX;
            originalShootPosition.x += deltaX;
            guideLine.setOriginalShootPosition(originalShootPosition);
            guideLine.setShootPosition(originalShootPosition, false);
            guideLine.setIndicatorBallPosition(originalShootPosition);
            guideLine.setIndicatorBallActive(true);
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
            exp.addXp(xp);
        }

        protected void onLevelUp()
        {
            actionManager.addToBot<OpenRewardChoosePanelAction>();
        }
    }
}