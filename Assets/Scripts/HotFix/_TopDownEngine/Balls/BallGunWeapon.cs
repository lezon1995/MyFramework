using UnityEngine;

namespace MoreMountains
{
    public class BallGunWeapon : BallWeapon, IStatsGetter<Ball.Stat>
    {
        protected SpriteRenderer BallWeaponAttachmentSpriteRenderer;

        public int ShootFixedUpdateInterval = 5;

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
                    if (HideWeaponRenderer)
                    {
                        BallWeaponAttachmentSpriteRenderer.enabled = false;
                    }
                }
            }
            else
            {
                if (BallWeaponSpriteRenderer)
                {
                    BallWeaponSpriteRenderer.sprite = BallDef.Icon;
                    if (HideWeaponRenderer)
                    {
                        BallWeaponSpriteRenderer.enabled = false;
                    }
                }
            }

            Stats.InitializeStats(BallDef.StatsTemplate);
            InitializeStats();
        }

        protected override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            BallItem?.setCooldownProgress(_delayBetweenUsesTimer.pct);
        }

        protected override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);
        }

        public void SetBallAttachmentSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            BallWeaponAttachmentSpriteRenderer = spriteRenderer;
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
                    BallWeaponAttachmentSpriteRenderer.sprite = def.Icon;
                    BallWeaponAttachmentSpriteRenderer.enabled = true;

                    BallWeaponSpriteRenderer.sprite = null;
                }
                else
                {
                    BallWeaponAttachmentSpriteRenderer.sprite = null;
                    BallWeaponAttachmentSpriteRenderer.enabled = false;

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
                BallWeaponAttachmentSpriteRenderer.enabled = false;
            }

            if (BallWeaponSpriteRenderer)
                BallWeaponSpriteRenderer.enabled = false;

            if (BallWeaponAttachmentSpriteRenderer)
                BallWeaponAttachmentSpriteRenderer.enabled = false;
        }

        public override void SetBallLevel(int level)
        {
            Material material;
            if (BallDef && BallDef.PlaceIconAtWeaponAttachment)
                material = BallWeaponAttachmentSpriteRenderer.material;
            else
                material = BallWeaponSpriteRenderer.material;

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
            
        }

        public override void SetOwner(Character owner, CharacterHandleWeapon handleWeapon = null)
        {
            base.SetOwner(owner, handleWeapon);
            _player = owner as APlayer;
        }

        protected override void OnOwnerStatsSet() { }

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


        public override void ShootRequest()
        {
            // var service = _player.BallManagement.Instance;
            // if (service.CanShoot)
            {
                // service.resetShootCountdown(ShootFixedUpdateInterval);
                State.ChangeState(States.Use);
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
            var ball = _player.BallManagement.Instance.acquireBall(BallDef.Type, state.Position, BallLevel, 0F);
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

            // if (triggerObjectActivation)
            // {
            //     poolableObject.TriggerOnSpawnComplete();
            // }

            ball.onPreparedToShoot();

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