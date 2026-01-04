namespace MarbleHero
{
    public class SpikeSlime_S : AMonster
    {
        public static string ID = "SpikeSlime_S";

        // static MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings("SpikeSlime_S");
        // public static string NAME = monsterStrings.NAME;
        // public static string[] MOVES = monsterStrings.MOVES;
        // public static string[] DIALOG = monsterStrings.DIALOG;
        public static int HP_MIN = 10;
        public static int HP_MAX = 14;
        public static int A_2_HP_MIN = 11;
        public static int A_2_HP_MAX = 15;
        public static int TACKLE_DAMAGE = 5;
        public static int A_2_TACKLE_DAMAGE = 6;
        static byte TACKLE = 1;

        public SpikeSlime_S() : base("SpikeSlime_S", "SpikeSlime_S", 14)
        {
            if (ADungeon.ascensionLevel >= 7)
                setHp(11, 15);
            else
                setHp(10, 14);

            if (ADungeon.ascensionLevel >= 2)
                damageList.Add(new DamageInfo(this, 6));
            else
                damageList.Add(new DamageInfo(this, 5));

            // if (poisonAmount >= 1)
            // powers.Add(new PoisonPower(this, this, poisonAmount));

            // loadAnimation("images/monsters/theBottom/slimeAltS/skeleton.atlas", "images/monsters/theBottom/slimeAltS/skeleton.json", 1.0F);
            // AnimationState.TrackEntry e = state.setAnimation(0, "idle", true);
            // e.setTime(e.getEndTime() * MathUtils.random());
            // state.addListener(new SlimeAnimListener());
        }

        public override void takeTurn()
        {
            switch (nextMove)
            {
                case 1:
                    // actionManager.addToBot(new AnimateFastAttackAction(this));
                    actionManager.addToBot(new DamageAction(player, damageList[0]));
                    actionManager.addToBot(new RollMoveAction(this));
                    break;
            }
        }

        protected override void getMove(int num)
        {
            setMove(1, Intent.ATTACK, damageList[0].Value);
        }
    }
}