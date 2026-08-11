using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using PrimeTween;
using UnityEngine;

namespace MoreMountains
{
    public partial class APlayer
    {
        public bool isReturnBall;
        public int ballMaxCount = 1;
        public int ballCount = 1;

        public Vector3 originalShootPosition, shootPosition;
        public bool isFirstBallReturn;
        public int toClaimRewardCount;

        protected List<BuffObject> buffs = new();
        protected List<Type> ballBuffs = new();
        protected BallManagementSystem ballManagement;
        protected InventorySystem inventory;
        protected ShopSystem shop;
        protected PlayerWallet wallet;

        public BallManagementSystem BallManagement => ballManagement;
        public InventorySystem Inventory => inventory;
        public ShopSystem Shop => shop;
        public PlayerWallet Wallet => wallet;
        
        CharacterHandleWeapon[] handleWeapons;
        public BallWeaponAttachmentRoot ballWeaponAttachmentRoot;

        protected override void Initialization()
        {
            base.Initialization();

            Exp.ResetLevel();
            // Exp.SetOnLevelUp(onLevelUp);

            FindAbility(out playerRecollectBall);

            getOrAddUnityComponent(out ballManagement);
            getOrAddUnityComponent(out inventory);
            getOrAddUnityComponent(out shop);
            getOrAddUnityComponent(out wallet);

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

            this.TryGetComponentInChildren(out ballWeaponAttachmentRoot);
            this.TryGetComponentsInChildren(out handleWeapons);
            for (var i = 0; i < handleWeapons.Length; i++)
            {
                handleWeapons[i].SetAbilityPermitted(false);
            }
        }

        public void OnBallInventorySlotChanged(BallInventorySlot slot)
        {
            var handleWeapon = handleWeapons[slot.Index];
            handleWeapon.SetAbilityPermitted(slot.IsOccupied);
            if (handleWeapon.CurrentWeapon is BallGunWeapon ballGunWeapon)
            {
                ballGunWeapon.SetBallDef(slot.Item.Def);
            }
            
            ballWeaponAttachmentRoot.RefreshLayout();
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

        public void recollectBall(Ball ball, float collectDuration = 0.75F, bool immediately = false)
        {
            playerRecollectBall.RecollectBall(ball, collectDuration, immediately);
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
                BallManagement.Instance.releaseBall(ball);
                return;
            }

            ball.setEnabled(false);
            Tween
                .Position(ball.getTransform(), endValue: shootPosition, duration: 0.15F, ease: Ease.OutCubic)
                .OnComplete(ball, b => { BallManagement.Instance.releaseBall(b); });

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