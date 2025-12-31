namespace MarbleHero
{
    public record struct OnPlayerEnterBattleRoom;

    public partial class MonsterRoom : ARoom
    {
        public override RoomType Type => RoomType.MONSTER;

        // public DiscardPileViewScreen discardPileViewScreen = new DiscardPileViewScreen();
        public const float COMBAT_WAIT_TIME = 0.1F;

        public MonsterRoom()
        {
            _phases = new APhase[]
            {
                new PlayerTurnPhase(this),
                new EnemyTurnPhase(this),
                new FightingPhase(this),
                new SettlementPhase(this),
            };
        }

        public override void onPlayerEntry()
        {
            playBGM(null);

            if (monsters == null)
            {
                monsters = _dungeon.getMonsterForRoomCreation();
                monsters.init();
            }

            waitTimer = COMBAT_WAIT_TIME;
            new OnPlayerEnterBattleRoom().Trigger();
        }

        public override void onCombatFightStart()
        {
            fightResult = RoundResult.None;
            nextPhase(PhaseType.Fighting);
        }

        protected override void onPlayerTurnStart(int turn)
        {
            nextPhase(PhaseType.PlayerTurn);
        }

        protected override void onPlayerTurnEnd()
        {
            nextPhase(PhaseType.EnemyTurn);
        }

        protected override void onPlayerFightEnd(RoundResult result)
        {
            fightResult = result;
            nextPhase(PhaseType.Settlement);
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
    }
}