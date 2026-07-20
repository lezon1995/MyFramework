using System;
using UnityEngine;

namespace MoreMountains
{
    public partial class Brick : IHittable
    {
        public override string ToString() => mName;

        public int instanceID;
        public long guid; // 角色的唯一ID

        #region Stats

        public Vector2Int size;

        #endregion

        public BrickRenderer brickRenderer;
    
        public void setID(long id) => guid = id;
        public Type getType() => GetType();
        public long getGUID() => guid;

        public override void onAcquire()
        {
            base.onAcquire();
            SetColliderEnabled(false);
            CharacterBrain.BrainActive = false;
            brickRenderer.setRendererActive(true);
            brickRenderer.setHealthBarActive(false);
            brickRenderer.playBornAnimation();
            Health.onAcquire();
            _controller2D.RegisterToVolumeManager();
        }

        void OnBornCompleted()
        {
            SetColliderEnabled(true);
            CharacterBrain.ResetBrain();
            brickRenderer.setHealthBarActive(true);
        }

        public override void onRelease()
        {
            _controller2D.UnregisterToVolumeManager();
            Health.onRelease();
            base.onRelease();
        }

        public void heal(Heal heal)
        {
            Health.ReceiveHealth(heal, null, this);
        }

        public virtual bool kill() => Health.Kill();
        public bool IsDeadTotally() => Health.IsDeadTotally;

        public Vector2Int getSize() => size;
        public void setSize(Vector2Int v) => size = v;

        public void setSortingOrder(int order)
        {
            brickRenderer.setSortingOrder(order);
        }
    }
}