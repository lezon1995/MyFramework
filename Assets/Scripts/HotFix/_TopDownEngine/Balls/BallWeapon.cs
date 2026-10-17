using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public abstract class BallWeapon : ProjectileWeapon
    {
        protected static readonly int PixelOutlineColor = Shader.PropertyToID("_PixelOutlineColor");
        protected static readonly int PixelOutlineFade = Shader.PropertyToID("_PixelOutlineFade");

        protected APlayer _player;

        [MMInspectorGroup("ID")]
        public BallDef BallDef;

        public SpriteRenderer BallWeaponSpriteRenderer;
        public bool HideWeaponRenderer = true;

        public int BallLevel;
        public BallItem BallItem;

        public BallInventorySlot BallSlot { get; set; }

        public void SetBallSlot(BallInventorySlot slot) => BallSlot = slot;
        public void SetBallItem(BallItem item) => BallItem = item;

        public abstract void SetBallDef(BallDef def);
        public abstract void SetBallLevel(int level);
        public abstract void onBallRecollected(Ball ball);
    }
}