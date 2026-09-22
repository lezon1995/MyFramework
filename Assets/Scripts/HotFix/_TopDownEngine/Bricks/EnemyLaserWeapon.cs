using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class EnemyLaserWeapon : ProjectileWeapon
    {
        [MMInspectorGroup("ID")]
        public Movements Direction;

        public float ArcRegionArc = 60F;
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

        GameEffect indicatorEffect;
        ArcRegion arcRegion;
        Vector2 aimDirection;

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
        
        protected override bool CanShootBetweenUses(float now) => true;

        protected override void OnEnterDelayBeforeUse()
        {
            Owner.CharacterBrain.IsDecisionLocked = true;
            var path = $"{GAMEPLAY_PATH}/Indicators/FxIndicator_Arc.prefab";
            if (indicatorEffect == null)
                indicatorEffect = mEffectManager.createEffect(path, transform.position);

            aimDirection = _weaponAim.CurrentAim;
            aimDirection = ConstrainDirection(aimDirection);
            if (indicatorEffect.tryGetUnityComponent(out arcRegion))
            {
                arcRegion.Angle = -aimDirection.getAngle(ANGLE.DEGREE);
                arcRegion.Arc = ArcRegionArc;
                arcRegion.Radius = ArcRegionRadius;
                arcRegion.FillProgress = 0F;
            }
        }
        
        protected virtual Vector2 ConstrainDirection(Vector2 dir)
        {
            if (dir == Vector2.zero)
                return dir;

            switch (Direction)
            {
                case Movements.Free:
                    return dir;

                case Movements.Strict2DirectionsHorizontal:
                    return new(Mathf.Sign(dir.x), 0f);

                case Movements.Strict2DirectionsVertical:
                    return new(0f, Mathf.Sign(dir.y));

                case Movements.Strict4Directions:
                {
                    // 取 x/y 中绝对值更大的分量作为主方向
                    if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
                        return new(Mathf.Sign(dir.x), 0f);
                    return new(0f, Mathf.Sign(dir.y));
                }

                case Movements.Strict8Directions:
                {
                    // 将方向量化为 8 个方向之一
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    // 归一化到 [-180, 180]
                    angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
                    // 每 45° 一个区间，量化到最近的 8 方向
                    int sector = Mathf.RoundToInt(angle / 45f);
                    float quantizedAngle = sector * 45f * Mathf.Deg2Rad;
                    return new(Mathf.Cos(quantizedAngle), Mathf.Sin(quantizedAngle));
                }

                default:
                    return dir;
            }
        }


        public override bool CaseWeaponDelayBeforeUse(float dt)
        {
            var finished = _delayBeforeUseTimer.update(dt, true);
            if (finished || _delayBeforeUseTimer.duration.isZero())
            {
                arcRegion.transform.position = transform.position;
                arcRegion.FillProgress = 1;
                arcRegion.Arc = 0;
                DoShootRequest();
            }
            else
            {
                var pct = _delayBeforeUseTimer.pct;
                arcRegion.transform.position = transform.position;
                arcRegion.FillProgress = pct;
                arcRegion.Arc = ArcRegionArc * (1 - pct);
            }

            return true;
        }

        public override void OnWeaponUse()
        {
            base.OnWeaponUse();

            Owner.CharacterBrain.IsDecisionLocked = false;

            ReleaseEffect();
        }

        void ReleaseEffect()
        {
            if (arcRegion)
            {
                mEffectManager.destroyEffect(ref indicatorEffect);
                arcRegion = null;
            }
        }

        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            // we get the next object in the pool and make sure it's not null
            var nextGameObject = ObjectPooler.GetPooledGameObject();
            // we position the object
            nextGameObject.transform.position = spawnPosition;
            // we activate the object
            nextGameObject.SetActive(true);

            var success = nextGameObject.TryGetComponent<Projectile>(out var projectile);
            if (success)
            {
                projectile.SetWeapon(this);
                if (Owner)
                {
                    projectile.SetOwner(Owner);
                    projectile.SetDamage(Dmg);
                }

                projectile.SetTarget(_aimTarget);
            }

            if (success)
            {
                if (totalProjectiles > 1)
                {
                    var dir = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread, Spread);
                    _randomSpreadDirection = dir;
                }
                else
                {
                    _randomSpreadDirection = Vector3.zero;
                }

                var spread = Quaternion.Euler(_randomSpreadDirection);
                var rotation = transform.rotation;
                if (Owner.Orientation2D)
                {
                    projectile.SetDirection(aimDirection, spread * rotation, Owner.Orientation2D.IsFacingRight);
                }
                else
                {
                    projectile.SetDirection(aimDirection, spread * rotation);
                }

                if (RotateWeaponOnSpread)
                {
                    transform.rotation *= spread;
                }
            }

            return nextGameObject;
        }
        
        protected override void OnDestroy()
        {
            ReleaseEffect();
        }
    }
}