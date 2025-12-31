using UnityEngine;

namespace MarbleHero
{
    public class Opponent : AMonster
    {
        public static string ID = "Opponent";

        // static MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings("Opponent");
        // public static string NAME = monsterStrings.NAME;
        // public static string[] MOVES = monsterStrings.MOVES;
        // public static string[] DIALOG = monsterStrings.DIALOG;
        public static int HP_MIN = 20;
        public static int HP_MAX = 30;
        public static int A_2_HP_MIN = 11;
        public static int A_2_HP_MAX = 15;
        public static int TACKLE_DAMAGE = 5;
        public static int A_2_TACKLE_DAMAGE = 6;
        static byte TACKLE = 1;

        int rollCounter;

        public Opponent() : base(ID, ID, HP_MAX)
        {
            if (ADungeon.ascensionLevel >= 7)
                setHp(A_2_HP_MIN, A_2_HP_MAX);
            else
                setHp(HP_MIN, HP_MAX);

            if (ADungeon.ascensionLevel >= 2)
                damageList.Add(new DamageInfo(this, A_2_TACKLE_DAMAGE));
            else
                damageList.Add(new DamageInfo(this, TACKLE_DAMAGE));

        }

        public override void takeTurn()
        {
            switch (nextMove)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    if (intentPawns != null)
                    {
                        actionManager.addToBot(new InstantiatePawnsAction(this, intentPawns));
                    }

                    break;
            }

            actionManager.addToBot(new RollMoveAction(this));
            Debug.Log($"Enemy Execute move[{nextMove}] ");
            new OnOpPlayerTakeTurn().Trigger();
        }

        protected override void getMove(int num)
        {
            var flag = rollCounter % pawnsInfos.Count + 1;
            switch (flag)
            {
                case 1:
                    setMove(1, Intent.ATTACK, pawnsInfos[1]);
                    break;
                case 2:
                    setMove(2, Intent.ATTACK, pawnsInfos[2]);
                    break;
                case 3:
                    setMove(3, Intent.ATTACK, pawnsInfos[3]);
                    break;
                case 4:
                    setMove(4, Intent.ATTACK, pawnsInfos[4]);
                    break;
                case 5:
                    setMove(5, Intent.ATTACK, pawnsInfos[5]);
                    break;
            }

            rollCounter++;
        }
    }
}