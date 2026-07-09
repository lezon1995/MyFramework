using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A basic melee weapon class, that will activate a "hurt zone" when the weapon is used
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/MeleeWeapon")]
    public class MeleeWeapon : Weapon
    {
        /// the possible shapes for the melee weapon's damage area
        public enum Shapes
        {
            Box2D,
            Circle2D,
            Box,
            Sphere
        }

        public enum Modes
        {
            Generated,
            Existing
        }

        [MMInspectorGroup("Damage Area")]
        [Tooltip("the possible modes to handle the damage area. In Generated, the MeleeWeapon will create it, in Existing, you can bind an existing damage area - usually nested under the weapon")]
        public Modes Mode = Modes.Generated;

        [Tooltip("the shape of the damage area (rectangle or circle)")]
        [MMEnumCondition(nameof(Mode), (int)Modes.Generated)]
        public Shapes DamageAreaShape = Shapes.Box2D;

        [Tooltip("the offset to apply to the damage area (from the weapon's attachment position")]
        [MMEnumCondition(nameof(Mode), (int)Modes.Generated)]
        public Vector3 AreaOffset = new Vector3(1, 0);

        [Tooltip("the size of the damage area")]
        [MMEnumCondition(nameof(Mode), (int)Modes.Generated)]
        public Vector3 AreaSize = new Vector3(1, 1);

        [Tooltip("the trigger filters this melee weapon should apply damage on (by default, it'll apply damage on everything, but you can change this to only apply when targets enter the area, for example)")]
        [MMEnumCondition(nameof(Mode), (int)Modes.Generated)]
        public DamageOnTouch.TriggerMask TriggerFilter = DamageOnTouch.AllowedTrigger;

        [Tooltip("the feedback to play when hitting a Damageable")]
        [MMEnumCondition(nameof(Mode), (int)Modes.Generated)]
        public MMFeedbacks HitDamageableFeedback;

        [Tooltip("the feedback to play when hitting a non Damageable")]
        [MMEnumCondition(nameof(Mode), (int)Modes.Generated)]
        public MMFeedbacks HitNonDamageableFeedback;

        [Tooltip("an existing damage area to activate/handle as the weapon is used")]
        [MMEnumCondition(nameof(Mode), (int)Modes.Existing)]
        public DamageOnTouch ExistingDamageArea;

        [MMInspectorGroup("Damage Area Timing")]
        public float InitialDelay;

        public float ActiveDuration = 1f;

        [MMInspectorGroup("Damage Caused")]
        public LayerMask TargetLayerMask;

        public KnockbackStyles Knockback;
        public Vector3 KnockbackForce = new Vector3(10, 2, 0);
        public KnockbackDirections KnockbackDirection;
        public float InvincibilityDuration = 0.5f;

        protected Collider _damageAreaCollider;
        protected Collider2D _damageAreaCollider2D;
        protected bool _attackInProgress;
        protected CircleCollider2D _circleCollider2D;
        protected BoxCollider2D _boxCollider2D;
        protected BoxCollider _boxCollider;
        protected SphereCollider _sphereCollider;
        protected DamageOnTouch _damageOnTouch;
        protected GameObject _damageArea;

        public override void Initialization()
        {
            base.Initialization();

            if (_damageArea == null)
            {
                CreateDamageArea();
                DisableDamageArea();
            }

            if (Owner)
            {
                _damageOnTouch.SetOwner(Owner.gameObject);
            }
        }


        /// <summary>
        /// Creates the damage area.
        /// </summary>
        protected virtual void CreateDamageArea()
        {
            if (Mode == Modes.Existing && ExistingDamageArea)
            {
                _damageArea = ExistingDamageArea.gameObject;
                _damageAreaCollider = _damageArea.GetComponent<Collider>();
                _damageAreaCollider2D = _damageArea.GetComponent<Collider2D>();
                _damageOnTouch = ExistingDamageArea;
                return;
            }

            _damageArea = new GameObject
            {
                name = name + "HitBox",
                transform =
                {
                    position = transform.position,
                    rotation = transform.rotation,
                }
            };
            _damageArea.transform.SetParent(transform);
            _damageArea.transform.localScale = Vector3.one;
            _damageArea.layer = gameObject.layer;

            switch (DamageAreaShape)
            {
                case Shapes.Box2D:
                    _boxCollider2D = _damageArea.AddComponent<BoxCollider2D>();
                    _boxCollider2D.offset = AreaOffset;
                    _boxCollider2D.size = AreaSize;
                    _damageAreaCollider2D = _boxCollider2D;
                    _damageAreaCollider2D.isTrigger = true;
                    break;
                case Shapes.Circle2D:
                    _circleCollider2D = _damageArea.AddComponent<CircleCollider2D>();
                    _circleCollider2D.transform.position = transform.position;
                    _circleCollider2D.offset = AreaOffset;
                    _circleCollider2D.radius = AreaSize.x / 2;
                    _damageAreaCollider2D = _circleCollider2D;
                    _damageAreaCollider2D.isTrigger = true;
                    break;
                case Shapes.Box:
                    _boxCollider = _damageArea.AddComponent<BoxCollider>();
                    _boxCollider.center = AreaOffset;
                    _boxCollider.size = AreaSize;
                    _damageAreaCollider = _boxCollider;
                    _damageAreaCollider.isTrigger = true;
                    break;
                case Shapes.Sphere:
                    _sphereCollider = _damageArea.AddComponent<SphereCollider>();
                    _sphereCollider.transform.position = transform.position + transform.rotation * AreaOffset;
                    _sphereCollider.radius = AreaSize.x / 2;
                    _damageAreaCollider = _sphereCollider;
                    _damageAreaCollider.isTrigger = true;
                    break;
            }

            switch (DamageAreaShape)
            {
                case Shapes.Box2D:
                case Shapes.Circle2D:
                    var rigidBody2D = _damageArea.AddComponent<Rigidbody2D>();
                    rigidBody2D.isKinematic = true;
                    rigidBody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
                    break;
                case Shapes.Box:
                case Shapes.Sphere:
                    var rigidBody = _damageArea.AddComponent<Rigidbody>();
                    rigidBody.isKinematic = true;
                    rigidBody.gameObject.AddComponent<MMRagdollerIgnore>();
                    break;
            }

            var damageOnTouch = _damageArea.AddComponent<DamageOnTouch>();
            _damageOnTouch = damageOnTouch;
            damageOnTouch.SetGizmoSize(AreaSize);
            damageOnTouch.SetGizmoOffset(AreaOffset);
            damageOnTouch.TargetLayerMask = TargetLayerMask;
            damageOnTouch.DamageDirectionMode = DamageOnTouch.DamageDirections.BasedOnOwnerPosition;
            damageOnTouch.DamageCausedKnockbackType = Knockback;
            damageOnTouch.DamageCausedKnockbackForce = KnockbackForce;
            damageOnTouch.DamageCausedKnockbackDirection = KnockbackDirection;
            damageOnTouch.InvincibilityDuration = InvincibilityDuration;
            damageOnTouch.HitDamageableFeedback = HitDamageableFeedback;
            damageOnTouch.HitNonDamageableFeedback = HitNonDamageableFeedback;
            damageOnTouch.TriggerFilter = TriggerFilter;

            if (Owner)
            {
                damageOnTouch.AddIgnore(Owner.gameObject);
            }
        }

        /// <summary>
        /// When the weapon is used, we trigger our attack routine
        /// </summary>
        public override void WeaponUse()
        {
            base.WeaponUse();
            Timing.RunCoroutine(MeleeWeaponAttack(Dmg));
        }

        /// <summary>
        /// Triggers an attack, turning the damage area on and then off
        /// </summary>
        /// <returns>The weapon attack.</returns>
        protected virtual IEnumerator<float> MeleeWeaponAttack(Dmg dmg)
        {
            if (_attackInProgress)
                yield break;

            _damageOnTouch.SetDmg(dmg);
            _attackInProgress = true;
            yield return Timing.WaitForSeconds(InitialDelay);
            EnableDamageArea();
            yield return Timing.WaitForSeconds(ActiveDuration);
            DisableDamageArea();
            _attackInProgress = false;
        }

        /// <summary>
        /// Enables the damage area.
        /// </summary>
        protected virtual void EnableDamageArea()
        {
            if (_damageAreaCollider2D)
                _damageAreaCollider2D.enabled = true;

            if (_damageAreaCollider)
                _damageAreaCollider.enabled = true;
        }


        /// <summary>
        /// Disables the damage area.
        /// </summary>
        protected virtual void DisableDamageArea()
        {
            if (_damageAreaCollider2D)
                _damageAreaCollider2D.enabled = false;

            if (_damageAreaCollider)
                _damageAreaCollider.enabled = false;
        }

        /// <summary>
        /// When selected, we draw a bunch of gizmos
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                DrawGizmos();
        }

        protected virtual void DrawGizmos()
        {
            if (Mode == Modes.Existing)
                return;

            switch (DamageAreaShape)
            {
                case Shapes.Box:
                    Gizmos.DrawWireCube(transform.position + AreaOffset, AreaSize);
                    break;
                case Shapes.Circle2D:
                    Gizmos.DrawWireSphere(transform.position + AreaOffset, AreaSize.x / 2);
                    break;
                case Shapes.Box2D:
                    MMDebug.DrawGizmoRectangle(transform.position + AreaOffset, AreaSize, Color.red);
                    break;
                case Shapes.Sphere:
                    Gizmos.DrawWireSphere(transform.position + AreaOffset, AreaSize.x / 2);
                    break;
            }
        }

        /// <summary>
        /// On disable we set our flag to false
        /// </summary>
        protected virtual void OnDisable()
        {
            _attackInProgress = false;
        }
    }
}