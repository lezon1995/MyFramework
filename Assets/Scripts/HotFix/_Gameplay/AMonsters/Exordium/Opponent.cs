namespace MoreMountains
{
    public class Opponent : Brick
    {
        public static string ID = "Opponent";

        // static MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings("Opponent");
        // public static string NAME = monsterStrings.NAME;
        // public static string[] MOVES = monsterStrings.MOVES;
        // public static string[] DIALOG = monsterStrings.DIALOG;

        protected override void Initialization()
        {
            base.Initialization();
            setName(ID);
            id = ID;
        }
    }
}