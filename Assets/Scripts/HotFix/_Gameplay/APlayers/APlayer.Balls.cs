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
        public Vector3 nextPosition;
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
            guideLine.setObject(getGameObject("GuideLine", getObject()));
            guideLine.setName("GuideLine");
            guideLine.setPlayer(this);
            guideLine.init();

            exp = CLASS<Exp>();
            var path = $"{GAMEPLAY_PATH}/ExpData.asset";
            var data = mResourceManager.loadGameResource<ExpData>(path);
            exp.setData(data);
            exp.resetLevel();

            nextPosition = getWorldPosition();
            setNextPositionX(nextPosition.x);

            // buffs.add(CLASS<LightingStrike>()).setBrickManager(brickManager);
            // buffs.add(CLASS<LightingStrike3>()).setBrickManager(brickManager);
            // buffs.add(CLASS<LaserHorizontal>()).setBrickManager(brickManager);
            // buffs.add(CLASS<LaserVertical>()).setBrickManager(brickManager);

            ballBuffs.add(typeof(LaserHorizontal));
            ballBuffs.add(typeof(LaserVertical));
            ballBuffs.add(typeof(LightingStrike));
            ballBuffs.add(typeof(LightingStrike3));

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

        public bool anyActiveBall()
        {
            return activeBalls.any();
        }

        IEnumerator shootBallCo(Vector3 shootPosition, Vector3 shootDirection)
        {
            guideLine.setIndicatorBallActive(false);
            var selectIndexes = ListPool<int>.Get();

            for (int i = 0; i < ballMaxCount; i++)
            {
                //SoundManager.Instance.PlayEffect(Sound.sound_play_sfx_ball_launch);
                // CtrGame.instance.ShotSound();

                var ball = ballManager.acquireBall(shootPosition, 0.14F, shootDirection, 8F);
                selectIndexes.Clear();
                /*randomSelect(ballBuffs.count(), 1, selectIndexes);
                foreach (var index in selectIndexes)
                {
                    var buffType = ballBuffs.get(index);
                    var buff = CLASS<Buff>(buffType);
                    buff.setBrickManager(brickManager);
                    ball.addBuff(buff);
                }*/

                // ball.setPenetrable(true);
                // ball.setHorizontalBorderTeleportable(true);

                ballCount -= 1;
                // CtrUI.instance.SetBallCount(ballCount);

                if (i == 0)
                {
                    // ball.isFirst = true;
                }

                activeBalls.Add(ball);
                yield return new WaitForSeconds(0.05f);

                if (ballCount < 0)
                    ballCount = 0;
            }

            yield return new WaitForSeconds(0.05f);
            // CtrUI.instance.textBallCount.DOFade(0f, 0.1f).SetEase(Ease.OutCubic);

            ListPool<int>.Release(selectIndexes);
        }

        IEnumerator readyPlayerCo()
        {
            // CtrUI.instance.SetReturnBallButton(false);
            // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
            // CtrUI.instance.textBallCount.DOFade(1f, 0.1f).SetEase(Ease.OutCubic);
            // CtrUI.instance.textBallCount.transform.DOMoveX(nextPosition.x, 0f);

            //Initialize the number of balls
            ballCount = ballMaxCount;
            //Ball count UI applied
            // CtrUI.instance.SetBallCount(ballMaxCount);

            //Additional Ball Animation
            // for (int i = 0; i < addBallBlock.Count; i++)
            // {
            //     addBallBlock[i].transform.DOKill();
            //     addBallBlock[i].transform.DOMove(nextPosition, 0.1f);
            //     SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
            //     fxGet.Play();
            // }

            yield return new WaitForSeconds(0.15f);
            // if (addBallBlock.Count > 0)
            // {
            //     textGetBallCount.text = $"+{addBallBlock.Count}";
            //     textGetBallCount.transform.DOMove(nextPosition, 0f);
            //     textGetBallCount.DOFade(1f, 0f);
            //     textGetBallCount.transform.DOMoveY(0.5f, 0.2f).SetEase(Ease.OutCubic).SetRelative(true);
            //     textGetBallCount.DOFade(0f, 1f).SetEase(Ease.Linear).SetDelay(0.2f);
            // }

            //Delete added ball blocks
            // for (int i = 0; i < addBallBlock.Count; i++)
            // {
            //     addBallBlock[i].Destory();
            // }

            //Existing Ball += Added Ball
            // ballMaxCount += addBallBlock.Count;

            if (randomHit(0.5F))
                ballMaxCount++;

            ballCount = ballMaxCount;
            // CtrUI.instance.SetBallCount(ballMaxCount);

            //Initialize the list of added balls
            // addBallBlock.Clear();

            // inBall.Clear();
            isFirstBallReturn = false;
        }

        public void returnBall()
        {
            // CtrUI.instance.SetReturnBallButton(false);
            actionManager.addToTop<ReturnBallsAction>().with(nextPosition);
        }

        public void setNextPositionX(float posX)
        {
            nextPosition.x = posX;
            guideLine.setShootPosition(nextPosition, true);
            guideLine.setIndicatorBallPosition(nextPosition);
            guideLine.setIndicatorBallActive(true);

            // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
        }

        public void moveNextPositionX(float deltaX)
        {
            nextPosition.x += deltaX;
            guideLine.setShootPosition(nextPosition, false);
            guideLine.setIndicatorBallPosition(nextPosition);
            guideLine.setIndicatorBallActive(true);
        }

        public void setBallReturn(Ball ball)
        {
            if (!isFirstBallReturn)
            {
                isFirstBallReturn = true;
                setNextPositionX(ball.getWorldPosition().x);
                activeBalls.Remove(ball);
                ballManager.releaseBall(ball);
                return;
            }

            ball.setEnabled(false);
            activeBalls.Remove(ball);
            Tween
                .Position(ball.getTransform(), endValue: nextPosition, duration: 0.15F, ease: Ease.OutCubic)
                .OnComplete(ball, b =>
                {
                    ballManager.releaseBall(b);
                });

            return;
        }

        public void addExp(int turnScore)
        {
            exp.addXp(turnScore);
        }
    }
}