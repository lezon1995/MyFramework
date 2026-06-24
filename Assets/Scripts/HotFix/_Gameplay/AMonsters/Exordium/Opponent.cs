namespace MarbleHero;

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

    protected override void getMove(int num)
    {
        var flag = rollCounter % 5 + 1;
        switch (flag)
        {
            case 1:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                setMove(3, Intent.BRICK_HEALING_X);
                break;
            case 2:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                setMove(3, Intent.BRICK_HEALING_X);
                break;
            case 3:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                setMove(3, Intent.BRICK_HEALING_X);
                // setMove(3, Intent.BRICK_GENERATE_X);
                break;
            case 4:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                setMove(3, Intent.BRICK_HEALING_X);
                // setMove(3, Intent.BRICK_GENERATE_X);
                // setMove(4, Intent.BRICK_MOVE_DOWN_X);
                break;
            case 5:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                setMove(3, Intent.BRICK_HEALING_X);
                // setMove(3, Intent.BRICK_GENERATE_X);
                // setMove(4, Intent.BRICK_MOVE_DOWN_X);
                // setMove(5, Intent.BRICK_GENERATE_X);
                break;
        }

        rollCounter++;
    }

    public override void takeMove(EnemyMoveInfo moveInfo)
    {
        switch (moveInfo.intent)
        {
            case Intent.BRICK_GENERATE_X:
                actionManager.addToBot<BrickGroupGenerateAction>().with(this, createBrickGroup());
                break;
            case Intent.BRICK_HEALING_X:
                actionManager.addToBot<BrickHealingAction>().with(3, 10);
                break;
            case Intent.BRICK_MOVE_DOWN_X:
                actionManager.addToBot<BrickGroupMoveDownAction>().with(this);
                break;
        }
    }

    BrickGroup createBrickGroup()
    {
        var num = GameActionManager.turn % 4;
        BrickGroup brickGroup = num switch
        {
            // 0 => CLASS<TopRowRandomBrickGroup>(),
            // 1 => CLASS<RandomRowRandomBrickGroup>(),
            // 2 => CLASS<RandomColRandomBrickGroup>(),
            // 3 => CLASS<RandomAnyEmptyBrickGroup>(),
            _ => CLASS<TopRowRandomBrickGroup>()
        };

        brickGroup.setBrickManager(brickManager);
        brickGroup.setLevelManager(levelManager);
        brickGroup.setOnBricksClear(onBrickGroupClear);
        brickGroups.add(brickGroup);
        return brickGroup;
    }

    public override void takeTurn()
    {
        base.takeTurn();

        actionManager.addToBot<RollMoveAction>().with(this);
        new OnOpPlayerTakeTurn().trigger();
    }
}