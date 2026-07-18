using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public partial class Brick : AMonster
    {
        protected BoxCollider2D boxCollider;
        protected Vector2 colliderOffset, colliderSize;
        protected DamageOnTouch damageOnTouch;

        protected override void OnAwake()
        {
            base.OnAwake();
            instanceID = GetInstanceID();
            TryGetComponent(out brickRenderer);
            TryGetComponent(out damageOnTouch);
            damageOnTouch.SetOwner(gameObject);
            brickRenderer.Awake();
        }

        protected override void Initialization()
        {
            base.Initialization();
            boxCollider = _controller2D.boxCollider;
            colliderOffset = boxCollider.offset;
            colliderSize = boxCollider.size;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public Rect getRect()
        {
            Vector2 pos = transform.position;
            return new(pos + colliderOffset - colliderSize * 0.5F, colliderSize);
        }

        public override void SetColliderEnabled(bool enable)
        {
            base.SetColliderEnabled(enable);
            new OnBrickColliderChanged().trigger();
        }

        protected override void UpdateAnimators()
        {
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