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
        public VolumeCollider volumeCollider;
        public VolumeType RegisteredVolumeType;

        protected Action<Brick> onBornCompleted;
        protected APlayer player;
        protected BrickDef def;

        public void setID(long id) => guid = id;
        public Type getType() => GetType();
        public BrickDef getDef() => def;
        public long getGUID() => guid;
        public APlayer getTargetPlayer() => player;

        public override void onAcquire()
        {
            base.onAcquire();
            SetColliderEnabled(false);
            CharacterBrain.SetTarget(player.getTransform());
            CharacterBrain.BrainActive = false;
            brickRenderer.setRendererActive(true);
            brickRenderer.setHealthBarActive(false);
            brickRenderer.playBornAnimation();
            Health.onAcquire();

            switch (RegisteredVolumeType)
            {
                case VolumeType.Entity:
                    _controller2D.RegisterToVolumeManager();
                    break;
                case VolumeType.Collider:
                    volumeCollider.RegisterToVolumeManager();
                    break;
            }
        }

        public void setOnBornCompleted(Action<Brick> a)
        {
            onBornCompleted = a;
        }

        void OnBornCompleted()
        {
            SetColliderEnabled(true);
            CharacterBrain.ResetBrain();
            CharacterBrain.SetTarget(player.getTransform());
            brickRenderer.setHealthBarActive(true);
            onBornCompleted?.Invoke(this);
        }

        public override void onRelease()
        {
            switch (RegisteredVolumeType)
            {
                case VolumeType.Entity:
                    _controller2D.UnregisterToVolumeManager();
                    break;
                case VolumeType.Collider:
                    volumeCollider.UnregisterToVolumeManager();
                    break;
            }

            Health.onRelease();
            base.onRelease();
        }

        public void heal(Heal heal)
        {
            Health.ReceiveHealth(heal, null, this);
        }

        public virtual bool kill(Character killBySource = null) => Health.Kill(killBySource);
        public bool IsDeadTotally() => Health.IsDeadTotally;

        public Vector2Int getSize() => size;
        public void setSize(Vector2Int v) => size = v;

        public float getMaxWidthOrHeight()
        {
            return Mathf.Max(BrickUnitSize * size.x, BrickUnitSize * size.y);
        }

        public void setSortingOrder(int order)
        {
            brickRenderer.setSortingOrder(order);
        }

        public void setPlayerAsTarget(APlayer p)
        {
            player = p;
        }

        public virtual void setBrickDef(BrickDef d)
        {
            def = d;
            brickRenderer.setBlockSprite(d.BlockIcon);
            brickRenderer.setUnitSprite(d.UnitIcon);
            brickRenderer.onBrickDefSet(d);
        }

        public void setupBrain(EnemyBrain brainTemplate)
        {
            if (brainTemplate == null)
                return;

            var brain = CharacterBrain;
            if (brain)
            {
                Destroy(brain);
            }

            if (brainTemplate)
            {
                InstantiateBrain(brainTemplate);
            }
            else
            {
                CharacterBrain = null;
            }

            // OnBrainChange?.Invoke();
        }
        
        protected virtual void InstantiateBrain(EnemyBrain newBrain)
        {
            EnemyBrain curBrain = Instantiate(newBrain, transform);
            CharacterBrain = curBrain;

            if (curBrain)
            {
                curBrain.name = newBrain.name;
                curBrain.transform.localPosition = Vector3.zero;
                curBrain.SetOwner(gameObject);
            }
        }

        public void setupWeapon(CharacterHandleWeapon weaponTemplate)
        {
            if (weaponTemplate == null)
                return;

            var ability = CharacterHandleWeapon;
            if (ability)
            {
                RemoveAbility(ability);
                Destroy(ability.gameObject);
            }

            if (weaponTemplate)
            {
                InstantiateWeapon(weaponTemplate);
            }
            else
            {
                CharacterHandleWeapon = null;
            }

            // OnBrainChange?.Invoke();
        }
        
        protected virtual void InstantiateWeapon(CharacterHandleWeapon newBrain)
        {
            var ability = Instantiate(newBrain, transform);
            CharacterHandleWeapon = ability;

            if (ability)
            {
                AddAbility(ability);
            }
        }
    }
}