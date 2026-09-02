using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    public class BallGunWeapon : ProjectileWeapon, IStatsGetter<Ball.Stat>
    {
        APlayer _player;

        [MMInspectorGroup("ID")]
        public BallDef BallDef;

        public int BallLevel;
        public BallInventorySlot BallSlot;

        [MMInspectorGroup("ID")]
        public SpriteRenderer BallWeaponSpriteRenderer;

        protected SpriteRenderer BallWeaponAttachmentSpriteRenderer;

        public override bool requireTargetToShoot
        {
            get
            {
                if (BallDef)
                    return BallDef.RequireTargetToShoot;

                return RequireAimTarget;
            }
        }

        public override void Initialization()
        {
            base.Initialization();

            if (BallDef.PlaceIconAtWeaponAttachment)
            {
                if (BallWeaponAttachmentSpriteRenderer)
                {
                    BallWeaponAttachmentSpriteRenderer.sprite = BallDef.Icon;
                }
            }
            else
            {
                if (BallWeaponSpriteRenderer)
                {
                    BallWeaponSpriteRenderer.sprite = BallDef.Icon;
                }
            }

            Stats.InitializeStats(BallDef.StatsTemplate);
            InitializeStats();
        }

        protected override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
        }

        protected override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);
        }

        public void SetBallAttachmentSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            BallWeaponAttachmentSpriteRenderer = spriteRenderer;
        }

        public void SetBallSlot(BallInventorySlot slot)
        {
            BallSlot = slot;
        }

        public void SetBallDef(BallDef def)
        {
            BallDef = def;

            if (def)
            {
                Stats.InitializeStats(def.StatsTemplate);
                InitializeStats();

                if (def.PlaceIconAtWeaponAttachment)
                {
                    BallWeaponAttachmentSpriteRenderer.sprite = def.Icon;
                    BallWeaponAttachmentSpriteRenderer.enabled = true;

                    BallWeaponSpriteRenderer.sprite = null;
                    BallWeaponSpriteRenderer.gameObject.SetActive(true);
                }
                else
                {
                    BallWeaponAttachmentSpriteRenderer.sprite = null;
                    BallWeaponAttachmentSpriteRenderer.enabled = false;

                    BallWeaponSpriteRenderer.sprite = def.Icon;
                    BallWeaponSpriteRenderer.gameObject.SetActive(true);
                }
            }
            else
            {
                BallWeaponSpriteRenderer.gameObject.SetActive(false);
                BallWeaponAttachmentSpriteRenderer.enabled = false;
            }
        }

        static readonly int PixelOutlineColor = Shader.PropertyToID("_PixelOutlineColor");
        static readonly int PixelOutlineFade = Shader.PropertyToID("_PixelOutlineFade");

        public void SetBallLevel(int level)
        {
            Material material;
            if (BallDef && BallDef.PlaceIconAtWeaponAttachment)
                material = BallWeaponAttachmentSpriteRenderer.material;
            else
                material = BallWeaponSpriteRenderer.material;

            if (level > 1)
            {
                BallLevel = level;

                var rarity = Mathf.Clamp(level - 1, 0, 3);
                var color = gameDesign.getRarityColor((ItemRarity)rarity);
                material.SetColor(PixelOutlineColor, color.title);
                material.SetFloat(PixelOutlineFade, 1F);
            }
            else
            {
                BallLevel = 1;
                material.SetColor(PixelOutlineColor, Color.clear);
                material.SetFloat(PixelOutlineFade, 0F);
            }
        }

        public override void SetOwner(Character owner, CharacterHandleWeapon handleWeapon = null)
        {
            base.SetOwner(owner, handleWeapon);
            _player = owner as APlayer;
        }

        public override void ShootRequest()
        {
            State.ChangeState(States.Use);
        }

        protected override void OnOwnerStatsSet()
        {
        }

        void InitializeStats()
        {
            var characterAS = Owner.GetStat(Character.Stat.AS);
            var ballAS = GetStat(Ball.Stat.AS);
            //Weapon的DelayBeforeUseF = (1 + Character.AS + Weapon.AS) * Weapon.DelayBeforeUseF
            DelayBeforeUseModifier = (ref float raw) =>
            {
                float totalAS = 0F;
                if (ballAS)
                    totalAS += ballAS.Value;

                if (characterAS)
                    totalAS *= (1 + characterAS.Value);

                float baseWindupTime = 0F;
                if (ballAS.Initial > 0)
                    baseWindupTime = DelayBeforeUsePct / ballAS.Initial;

                var currentAttackTotalTime = 1 / totalAS;
                var windupTime = baseWindupTime + DelayBeforeUseMultiplier * (currentAttackTotalTime * DelayBeforeUsePct - baseWindupTime);
                raw = windupTime;
            };

            //Weapon的TimeBetweenUsesF = (1 + Character.AS + Weapon.AS) * Weapon.TimeBetweenUsesF
            TimeBetweenUsesModifier = (ref float raw) =>
            {
                float totalAS = 0F;
                if (ballAS)
                    totalAS += ballAS.Value;

                if (characterAS)
                    totalAS *= (1 + characterAS.Value);

                float baseWindupTime = 0F;
                if (ballAS.Initial > 0)
                    baseWindupTime = DelayBeforeUsePct / ballAS.Initial;

                float currentAttackTotalTime = 0F;
                if (totalAS > 0)
                    currentAttackTotalTime = 1 / totalAS;

                var windupTime = baseWindupTime + DelayBeforeUseMultiplier * (currentAttackTotalTime * DelayBeforeUsePct - baseWindupTime);
                raw = currentAttackTotalTime - windupTime;
            };

            var characterAD = Owner.GetStat(Character.Stat.AD);
            var weaponAD = GetStat(Ball.Stat.HitDamage);
            //Weapon的Damage = (Character.AD + Weapon.AD) * Weapon.AD_Coeff
            DamageModifier = (ref float raw) =>
            {
                float v1 = 0F, v2 = 0F;

                if (characterAD)
                    v1 = characterAD.Value;

                if (weaponAD)
                    v2 = weaponAD.Value;

                raw = v1 + v2;
            };
        }

        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            var ball = _player.BallManagement.Instance.acquireBall(BallDef.Type, spawnPosition, BallLevel);
            var success = ball != null;
            // mandatory checks
            if (!success)
                return null;

            ball.setTeleportPosition(spawnPosition);
            if (_projectileSpawnTransform)
            {
                ball.setTeleportPosition(_projectileSpawnTransform.position);
            }

            // we activate the object
            ball.setActive(true);

            ball.SetWeapon(this);
            if (Owner)
            {
                ball.SetOwner(Owner);
                ball.SetPlayer(_player);
                ball.SetDamage(Dmg);
            }

            ball.SetBallSlot(BallSlot);
            ball.SetTarget(_aimTarget);

            if (RandomSpread)
            {
                var x = Random.Range(-Spread.x, Spread.x);
                var y = Random.Range(-Spread.y, Spread.y);
                var z = Random.Range(-Spread.z, Spread.z);
                _randomSpreadDirection = new(x, y, z);
            }
            else
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
            }

            var spread = Quaternion.Euler(_randomSpreadDirection);
            if (Owner == null)
            {
                var direction = spread * transform.rotation * DefaultProjectileDirection;
                ball.setShootDirection(direction);
                ball.SetDirection(direction, transform.rotation);
            }
            else
            {
                Vector3 newDirection = spread * transform.right * (Flipped ? -1 : 1);
                if (Owner.Orientation2D)
                {
                    ball.setShootDirection(newDirection);
                    ball.SetDirection(newDirection, spread * transform.rotation, Owner.Orientation2D.IsFacingRight);
                }
                else
                {
                    ball.setShootDirection(newDirection);
                    ball.SetDirection(newDirection, spread * transform.rotation);
                }
            }

            if (RotateWeaponOnSpread)
            {
                transform.rotation *= spread;
            }

            // if (triggerObjectActivation)
            // {
            //     poolableObject.TriggerOnSpawnComplete();
            // }

            return ball.getGameObject();
        }

        public UniStats.Stat GetStat(Ball.Stat key)
        {
            if (Stats == null)
                _hasStats = TryGetComponent(out Stats);
            else
                _hasStats = true;

            return _hasStats ? Stats.GetStat(key.Key()) : null;
        }

        public bool GetStat(Ball.Stat key, out UniStats.Stat stat)
        {
            if (!_hasStats)
            {
                stat = null;
                return false;
            }

            return Stats.GetStat(key.Key(), out stat);
        }
    }
}