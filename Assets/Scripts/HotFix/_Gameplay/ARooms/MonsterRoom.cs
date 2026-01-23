using System.Collections.Generic;

namespace MarbleHero
{
    public record struct OnPlayerEnterBattleRoom;

    public partial class MonsterRoom : ARoom, IEvent<OnBrickDeath>
    {
        public override RoomType Type => RoomType.MONSTER;

        // public DiscardPileViewScreen discardPileViewScreen = new DiscardPileViewScreen();
        public const float COMBAT_WAIT_TIME = 0.1F;
        Timer brickDeathTimer;
        protected static Queue<OnBrickDeath> brickDeathQueue = new();

        public MonsterRoom()
        {
            _phases[RoomPhaseType.PLAYER_TURN] = new PlayerTurnPhase(this);
            _phases[RoomPhaseType.ENEMY_TURN] = new EnemyTurnPhase(this);
            _phases[RoomPhaseType.FIGHTING] = new FightingPhase(this);
            _phases[RoomPhaseType.SETTLEMENT] = new SettlementPhase(this);
        }

        public override void onPlayerEntry()
        {
            base.onPlayerEntry();
            playBGM(null);

            if (monsters == null)
            {
                monsters = _dungeon.getMonsterForRoomCreation();
                monsters.init();
            }

            waitTimer = COMBAT_WAIT_TIME;
            new OnPlayerEnterBattleRoom().trigger();
            this.addListener();
        }

        public override void onPlayerExit()
        {
            this.removeListener();
            base.onPlayerExit();
        }

        protected override void onEnemyTurnStart(int turn)
        {
            base.onEnemyTurnStart(turn);
            nextPhase(RoomPhaseType.ENEMY_TURN);
        }

        protected override void onEnemyTurnEnd()
        {
            base.onEnemyTurnEnd();
        }

        protected override void onPlayerTurnStart(int turn)
        {
            base.onPlayerTurnStart(turn);
            nextPhase(RoomPhaseType.PLAYER_TURN);
        }

        protected override void onPlayerTurnEnd()
        {
            base.onPlayerTurnEnd();
        }

        public override void onCombatFightStart()
        {
            nextPhase(RoomPhaseType.FIGHTING);
        }

        public void onEvent(OnBrickDeath e)
        {
            var combo = ++GameActionManager.turnCombo;
            var baseExp = gameDesign.baseExpStandard;
            int extraExp = gameDesign.getExtraExpAtCombo(combo);
            GameActionManager.turnExp += (baseExp + extraExp);
            e.combo = combo;

            brickDeathTimer = 0.15F;
            brickDeathQueue.Enqueue(e);
        }

        protected override void onFightPhaseEnd()
        {
            nextPhase(RoomPhaseType.SETTLEMENT);
        }

        public override void dropReward()
        {
            if (ModHelper.isModEnabled("Vintage") && room is not MonsterRoomElite && room is not MonsterRoomBoss)
            {
                RelicTier tier = returnRandomRelicTier();
                addRelicToRewards(tier);
            }
        }

        private RelicTier returnRandomRelicTier()
        {
            int roll = ADungeon.relicRng.random(0, 99);
            return roll switch
            {
                < 50 => RelicTier.COMMON,
                > 85 => RelicTier.RARE,
                _ => RelicTier.UNCOMMON
            };
        }

        protected override void onPlayerCompletedRewardGold()
        {
            if (monsters.haveMonstersEscaped())
                return;

            Game.monstersSlain++;
            log("Monsters Slain " + Game.monstersSlain);

            int gold;
            if (Settings.isDailyRun)
                gold = 15;
            else
                gold = ADungeon.treasureRng.random(10, 20);

            addGoldToRewards(gold);
        }

        protected override int onPlayerCompletedGetPotionChance()
        {
            if (!monsters.haveMonstersEscaped())
                return 40 + blizzardPotionMod;

            return 0;
        }

        public override void update(float dt)
        {
            base.update(dt);

            handleBrickDeathEvent(dt);
        }

        public override void getAllBricks(ref List<Brick> list)
        {
            foreach (var m in monsters.monsters)
            {
                foreach (var group in m.brickGroups)
                {
                    list.addRange(group.bricks);
                }
            }
        }

        void handleBrickDeathEvent(float elapsedTime)
        {
            if (brickDeathTimer.update(elapsedTime))
            {
                if (brickDeathQueue.TryDequeue(out var e))
                {
                    brickDeathTimer = 0.15F;
                    comboManager.createComboEffect(e.combo, e.deathPosition);

                    //Camera shaking
                    Game.screenShake.shakeCamera(e.combo * 0.01f, 0.15F);
                }
            }

            return;
        }
    }
}