using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    public class BallSlotWeapon : BallWeapon, IStatsGetter<Ball.Stat>
    {
        public override void Initialization()
        {
            base.Initialization();

            if (BallWeaponSpriteRenderer)
            {
                BallWeaponSpriteRenderer.sprite = BallDef.Icon;
                if (HideWeaponRenderer)
                {
                    BallWeaponSpriteRenderer.enabled = false;
                }
            }

            Stats.InitializeStats(BallDef.StatsTemplate);
            InitializeStats();
        }

        void InitializeStats()
        {
            var characterAS = Owner.GetStat(Character.Stat.AS);
            var ballAS = GetStat(Ball.Stat.AS);
            DelayBeforeUseModifier = (ref float raw) => raw = 0F;
            TimeBetweenUsesModifier = (ref float raw) => raw = 0F;

            var characterAD = Owner.GetStat(Character.Stat.AD);
            var weaponAD = GetStat(Ball.Stat.AD);
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

        public override void SetBallDef(BallDef def)
        {
            BallDef = def;

            if (def)
            {
                Stats.InitializeStats(def.StatsTemplate);
                InitializeStats();

                if (def.PlaceIconAtWeaponAttachment)
                {
                    BallWeaponSpriteRenderer.sprite = null;
                }
                else
                {
                    BallWeaponSpriteRenderer.sprite = def.Icon;
                }

                BallWeaponSpriteRenderer.gameObject.SetActive(true);
                if (HideWeaponRenderer)
                {
                    BallWeaponSpriteRenderer.enabled = false;
                }
            }
            else
            {
                BallWeaponSpriteRenderer.gameObject.SetActive(false);
            }
        }

        public override void SetBallLevel(int level)
        {
            var material = BallWeaponSpriteRenderer.material;
            if (level > 1)
            {
                BallLevel = level;
                if (!HideWeaponRenderer)
                {
                    var rarity = Mathf.Clamp(level - 1, 0, 3);
                    var color = gameDesign.getRarityColor((ItemRarity)rarity);
                    material.SetColor(PixelOutlineColor, color.title);
                    material.SetFloat(PixelOutlineFade, 1F);
                }
            }
            else
            {
                BallLevel = 1;
                if (!HideWeaponRenderer)
                {
                    material.SetColor(PixelOutlineColor, Color.clear);
                    material.SetFloat(PixelOutlineFade, 0F);
                }
            }
        }

        public override void onBallRecollected(Ball ball)
        {
            if (_player.Inventory.BallBag.TryGetAlreadyShootSlotByBallInstance(ball, out var slot))
            {
                slot.TryReload(_player, ball);
            }
        }

        public override void SetOwner(Character owner, CharacterHandleWeapon handleWeapon = null)
        {
            base.SetOwner(owner, handleWeapon);
            _player = owner as APlayer;
        }

        public override void ShootRequest()
        {
            if (_player.Inventory.BallBag.TryGetSlot(BallItem, out var slot) && slot.ReadyToShoot)
            {
                State.ChangeState(States.Use);
            }
            else
            {
                State.ChangeState(States.Idle);
            }
        }

        protected override void OnWeaponUseSpawnProjectile()
        {
            var perShot = ProjectilesPerShot;
            var newDirection = transform.right * (Flipped ? -1 : 1);
            for (int i = 0; i < perShot; i++)
            {
                var state = SpawnState;
                if (SpawnStateGetter != null)
                    state = SpawnStateGetter(state, newDirection, i, perShot);

                state.ProjectileIndex = i;
                state.TotalProjectiles = perShot;
                SpawnProjectile(state);
                PlaySpawnFeedbacks();
            }
        }

        public override GameObject SpawnProjectile(SpawnState state)
        {
            Ball ball = null;
            var success = _player.Inventory.BallBag.TryGetSlot(BallItem, out var slot);
            if (success)
                success &= slot.TryShoot(_player, state.Position, out ball);

            // mandatory checks
            if (!success)
                return null;

            ball.setTeleportPosition(state.Position);
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

            Vector3 newDirection = transform.right * (Flipped ? -1 : 1);
            if (!state.Direction.isZero())
            {
                newDirection = state.Direction;
            }

            if (Owner.Orientation2D)
            {
                ball.setShootDirection(newDirection);
                ball.SetDirection(newDirection, transform.rotation, Owner.Orientation2D.IsFacingRight);
            }
            else
            {
                ball.setShootDirection(newDirection);
                ball.SetDirection(newDirection, transform.rotation);
            }

            ball.onPreparedToShoot();
            ball.setInfiniteDuration();

            _player.onShootBall(ball);

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