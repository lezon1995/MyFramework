using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class EnemyParabolicMissileWeapon : ProjectileWeapon
    {
        public static Action<ParabolicProjectile, Vector2, Vector2, float> onStartFlying = OnStartFlying;
        public static Action<ParabolicProjectile, float> onFlying = OnFlying;
        public static Action<ParabolicProjectile> onGrounded = OnGrounded;

        [MMInspectorGroup("ID")]
        public float ArcRegionRadius = 2F;

        protected override int Damage
        {
            get
            {
                var damage = (float)BaseDamage;
                return (int)damage;
                // return (int)DamageModifier.SafeInvoke(ref damage);
            }
        }

        public override Dmg Dmg => new(Damage, Dmg.Types.AP, IsCritThisFrame, CritDamageThisFrame);

        protected override void OnOwnerStatsSet()
        {
            var characterAS = Owner.GetStat(Character.Stat.AS);
            var weaponAS = GetStat(Stat.AS);
            //Weapon的DelayBeforeUseF = (1 + Character.AS + Weapon.AS) * Weapon.DelayBeforeUseF
            // DelayBeforeUseModifier = (ref float raw) =>
            // {
            //     float totalAS = 0F;
            //     if (characterAS)
            //         totalAS += characterAS.Value;
            //     if (weaponAS)
            //         totalAS += weaponAS.Value;
            //
            //     var baseWindupTime = DelayBeforeUsePct / characterAS.Initial;
            //     var currentAttackTotalTime = 1 / totalAS;
            //
            //     var windupTime = baseWindupTime + DelayBeforeUseMultiplier * (currentAttackTotalTime * DelayBeforeUsePct - baseWindupTime);
            //     raw = windupTime;
            // };

            //Weapon的TimeBetweenUsesF = (1 + Character.AS + Weapon.AS) * Weapon.TimeBetweenUsesF
            // TimeBetweenUsesModifier = (ref float raw) =>
            // {
            //     float totalAS = 0F;
            //     if (characterAS)
            //         totalAS += characterAS.Value;
            //     if (weaponAS)
            //         totalAS += weaponAS.Value;
            //
            //     float baseWindupTime = 0F;
            //     if (characterAS.Initial > 0)
            //         baseWindupTime = DelayBeforeUsePct / characterAS.Initial;
            //
            //     float currentAttackTotalTime = 0F;
            //     if (totalAS > 0)
            //         currentAttackTotalTime = 1 / totalAS;
            //
            //     var windupTime = baseWindupTime + DelayBeforeUseMultiplier * (currentAttackTotalTime * DelayBeforeUsePct - baseWindupTime);
            //     raw = currentAttackTotalTime - windupTime;
            // };

            var characterAP = Owner.GetStat(Character.Stat.AP);
            var weaponAP = GetStat(Stat.AP);
            //Weapon的Damage = (Character.AD + Weapon.AD) * Weapon.AD_Coeff
            DamageModifier = (ref float raw) =>
            {
                float v1 = 0F, v2 = 0F;

                if (characterAP)
                    v1 = characterAP.Value;

                if (weaponAP)
                    v2 = weaponAP.Value;

                raw = v1 + v2;
            };
        }

        protected override void OnFixedUpdate(float dt)
        {
        }

        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            // we get the next object in the pool and make sure it's not null
            var nextGameObject = ObjectPooler.GetPooledGameObject();
            // we position the object
            nextGameObject.transform.position = spawnPosition;
            // we activate the object
            nextGameObject.SetActive(true);

            var success = nextGameObject.TryGetComponent<ParabolicProjectile>(out var projectile);
            if (success)
            {
                projectile.OnStartFlying = onStartFlying;
                projectile.OnFlying = onFlying;
                projectile.OnGrounded = onGrounded;
                projectile.SetWeapon(this);
                if (Owner)
                {
                    projectile.SetOwner(Owner);
                    projectile.SetDamage(Dmg);
                }

                projectile.SetTarget(_aimTarget);

                var aimPosition = Owner.CharacterBrain.Target.position;
                projectile.Initialize(spawnPosition, aimPosition, ArcRegionRadius);
            }

            return nextGameObject;
        }

        static void OnStartFlying(ParabolicProjectile p, Vector2 position, Vector2 targetPosition, float radius)
        {
            var path = $"{GAMEPLAY_PATH}/Indicators/FxIndicator_Circle.prefab";
            if (p.indicatorEffect == null)
                p.indicatorEffect = mEffectManager.createEffect(path, position);

            p.aimPosition = targetPosition;
            if (p.indicatorEffect.tryGetUnityComponent(out p.circleRegion))
            {
                p.circleRegion.transform.position = p.aimPosition;
                p.circleRegion.Radius = radius;
                p.circleRegion.FillProgress = 0F;
            }
        }

        static void OnFlying(ParabolicProjectile p, float progress)
        {
            p.circleRegion.FillProgress = progress;
        }

        static void OnGrounded(ParabolicProjectile p)
        {
            p.circleRegion.FillProgress = 1F;

            if (p.circleRegion)
            {
                mEffectManager.destroyEffect(ref p.indicatorEffect);
                p.circleRegion = null;
            }
        }
    }
}