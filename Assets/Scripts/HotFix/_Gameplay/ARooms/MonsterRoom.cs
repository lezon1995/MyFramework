using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public record struct OnPlayerEnterBattleRoom;

    public partial class MonsterRoom : ARoom, IEvent<OnBrickDeath>
    {
        public const float COMBAT_WAIT_TIME = 0.1F;
        public override RoomType Type => RoomType.MONSTER;

        // public DiscardPileViewScreen discardPileViewScreen = new DiscardPileViewScreen();
        protected static Queue<OnBrickDeath> brickDeathQueue = new();
        Timer brickDeathTimer;

        public WaveGameMode waveGameMode;
        public WaveLevelConfig waveLevelConfig;

        public MonsterRoom()
        {
            _phases[RoomPhaseType.SELECT_CHARACTER] = new SelectCharacterPhase(this);
            _phases[RoomPhaseType.SELECT_WEAPON] = new SelectWeaponPhase(this);
            _phases[RoomPhaseType.SELECT_DIFFICULTY] = new SelectDifficultyPhase(this);
            _phases[RoomPhaseType.PREPARE] = new PreparePhase(this);
            _phases[RoomPhaseType.BATTLE] = new BattlePhase(this);
            _phases[RoomPhaseType.BATTLE_PASS_CLEANUP] = new BattlePassCleanupPhase(this);
            _phases[RoomPhaseType.LEVEL_UP_REWARD] = new LevelUpRewardPhase(this);
            _phases[RoomPhaseType.SHOPPING] = new ShoppingPhase(this);
            _phases[RoomPhaseType.GAME_SETTLEMENT] = new GameSettlementPhase(this);
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

            loadWaveManager();
        }

        void loadWaveManager()
        {
            string path1 = $"{GAMEPLAY_PATH}/Levels/WaveGameMode.prefab";
            string path2 = $"{GAMEPLAY_PATH}/Levels/WaveLevelConfig.asset";
            var res = resource.loadGameResource<WaveGameMode>(path1);
            waveGameMode = Object.Instantiate(res.getResource());
            waveLevelConfig = resource.loadGameResource<WaveLevelConfig>(path2);
            waveGameMode.StartGame(waveLevelConfig);
        }

        public override void onPlayerExit()
        {
            this.removeListener();
            base.onPlayerExit();
        }

        protected override void onEnemyTurnStart(int turn)
        {
            base.onEnemyTurnStart(turn);
            // nextPhase(PhaseType.ENEMY_TURN);
        }

        protected override void onEnemyTurnEnd()
        {
            base.onEnemyTurnEnd();
        }

        protected override void onPlayerTurnStart(int turn)
        {
            base.onPlayerTurnStart(turn);
            // nextPhase(PhaseType.PLAYER_TURN);
        }

        protected override void onPlayerTurnEnd()
        {
            base.onPlayerTurnEnd();
        }

        public override void onCombatFightStart()
        {
            // nextPhase(PhaseType.FIGHTING);
        }

        protected override void changePhase(RoomPhaseType type)
        {
            base.changePhase(type);
            nextPhase(type);
        }

        public void onEvent(OnBrickDeath e)
        {
            var combo = ++GameActionManager.turnCombo;
            var baseExp = gameDesign.baseExpStandard;
            int extraExp = gameDesign.getExtraExpAtCombo(combo);
            var totalExp = baseExp + extraExp;
            actionManager.addToBot<GainExpAction>().with(totalExp);
            GameActionManager.turnExp += totalExp;
            e.combo = combo;

            brickDeathTimer = 0.15F;
            brickDeathQueue.Enqueue(e);
        }

        protected override void onFightPhaseEnd()
        {
            // nextPhase(PhaseType.SETTLEMENT);
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
            return 40 + blizzardPotionMod;
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
                list.add(m as Brick);
            }
        }

        void handleBrickDeathEvent(float elapsedTime)
        {
            if (brickDeathTimer.update(elapsedTime))
            {
                if (brickDeathQueue.TryDequeue(out var e))
                {
                    brickDeathTimer = 0.15F;
                    // comboManager.createComboEffect(e.combo, e.deathPosition);

                    //Camera shaking
                    Game.screenShake.shakeCamera(e.combo * 0.005f, 0.15F);
                }
            }

            return;
        }
    }
}