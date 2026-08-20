using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public partial class Brick : AMonster
    {
        protected BoxCollider2D boxCollider;
        protected Vector2 colliderOffset, colliderSize;
        protected DamageOnTouch damageOnTouch;
        public new BrickStats Stats => stats as BrickStats;
        public CharacterHandleWeapon handleWeapon;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            instanceID = GetInstanceID();
            TryGetComponent(out brickRenderer);
            TryGetComponent(out damageOnTouch);
            TryGetComponent(out volumeCollider);
            damageOnTouch.SetOwner(this);
            brickRenderer.setOnBornAnimationComplete(OnBornCompleted);
            brickRenderer.Awake();
        }

        protected override void Initialization()
        {
            base.Initialization();
            boxCollider = _controller2D.boxCollider;
            colliderOffset = boxCollider.offset;
            colliderSize = boxCollider.size;

            this.TryGetComponentInChildren(out handleWeapon);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 砖块启用时, 刷新 mover anchor 让它从当前位置开始
            if (TryGetComponent<BrickGridMover>(out var mover))
                mover.RefreshAnchor();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // 砖块被回收/禁用时, 强制停止移动并释放锁
            if (TryGetComponent<BrickGridMover>(out var mover))
                mover.ForceStop();
        }

        public Rect getRect()
        {
            Vector2 pos = transform.position;
            return new(pos + colliderOffset - colliderSize * 0.5F, colliderSize);
        }

        public Vector2 getCenterPosition()
        {
            return getRect().center;
        }

        public override void SetColliderEnabled(bool enable)
        {
            base.SetColliderEnabled(enable);
            new OnBrickColliderChanged().trigger();
        }

        protected override void UpdateAnimators()
        {
        }

        public override void onEvent(OnRevive e)
        {
            CharacterBrain.enabled = true;
        }

        public override void RespawnAt(Vector3 spawnPosition, FacingDirections facingDirection = FacingDirections.South)
        {
            _controller.SetPosition(spawnPosition);
            setWorldPosition(spawnPosition);

            conditionState.ChangeState(Conditions.Normal);

            _controller.enabled = true;
            _controller.Reset();

            Reset();
            UnFreeze();

            if (Health)
            {
                Health.ResetHealthToMaxHealth();
                Health.Resurrect();
            }

            CharacterBrain.enabled = true;
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            // DebugDrawRect();
        }

        void DebugDrawRect()
        {
            Drawing.Draw.ingame.xy.WireRectangle(getRect(), Color.red);
        }
    }
}