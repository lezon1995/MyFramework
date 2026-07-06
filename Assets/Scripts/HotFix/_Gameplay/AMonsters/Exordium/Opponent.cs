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

        rollCounter = -1;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        rollCounter = -1;
    }

    protected override void getMove(int num)
    {
        var flag = rollCounter % 4 + 1;
        switch (flag)
        {
            case 0:
                setMove(1, Intent.BRICK_STAGE_INITIALIZATION);
                break;
            case 1:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                // setMove(3, Intent.BRICK_HEALING_X);
                break;
            case 2:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                // setMove(3, Intent.BRICK_HEALING_X);
                break;
            case 3:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                // setMove(3, Intent.BRICK_HEALING_X);
                // setMove(3, Intent.BRICK_GENERATE_X);
                break;
            case 4:
                setMove(1, Intent.BRICK_GENERATE_X);
                setMove(2, Intent.BRICK_MOVE_DOWN_X);
                // setMove(3, Intent.BRICK_HEALING_X);
                // setMove(3, Intent.BRICK_GENERATE_X);
                // setMove(4, Intent.BRICK_MOVE_DOWN_X);
                break;
        }

        rollCounter++;
    }

    public override void takeMove(EnemyMoveInfo moveInfo)
    {
        switch (moveInfo.intent)
        {
            case Intent.BRICK_STAGE_INITIALIZATION:
                var group = createBrickGroup<StageTemplateBrickGroup>();
                var path = $"{GAMEPLAY_PATH}/SO/StageTemplates/StageTemplate.asset";
                var res = mResourceManager.loadGameResource<StageTemplate>(path);
                group.with(res);
                actionManager.addToBot<BrickGroupGenerateAction>().with(this, group);
                break;
            case Intent.BRICK_GENERATE_X:
                actionManager.addToBot<BrickGroupGenerateAction>().with(this, createBrickGroup());
                break;
            case Intent.BRICK_HEALING_X:
                actionManager.addToBot<BrickHealingAction>().with(3, 1);
                break;
            case Intent.BRICK_MOVE_DOWN_X:
                actionManager.addToBot<BrickGroupMoveDownAction>().with(this);
                break;
            case Intent.BRICK_MOVE_TO_BORDER_LEFT:
                actionManager.addToBot<BrickGroupMoveToBorderAction>().with(levelManager.borderLeft);
                break;
            case Intent.BRICK_MOVE_TO_BORDER_RIGHT:
                actionManager.addToBot<BrickGroupMoveToBorderAction>().with(levelManager.borderRight);
                break;
            case Intent.BRICK_MOVE_TO_BORDER_TOP:
                actionManager.addToBot<BrickGroupMoveToBorderAction>().with(levelManager.borderTop);
                break;
            case Intent.BRICK_MOVE_TO_BORDER_BOT:
                actionManager.addToBot<BrickGroupMoveToBorderAction>().with(levelManager.borderBot);
                break;
        }
    }
    
    BrickGroup createBrickGroup<T>() where T : BrickGroup, new()
    {
        BrickGroup brickGroup = CLASS<T>();
        brickGroup.setBrickManager(brickManager);
        brickGroup.setLevelManager(levelManager);
        brickGroup.setOnBricksClear(onBrickGroupClear);
        brickGroups.add(brickGroup);
        return brickGroup;
    }

    

    BrickGroup createBrickGroup()
    {
        var num = GameActionManager.turn % 4;
        BrickGroup brickGroup = num switch
        {
            // 0 => createBrickGroup<TopRowRandomBrickGroup>(),
            // 1 => createBrickGroup<RandomRowRandomBrickGroup>(),
            // 2 => createBrickGroup<RandomColRandomBrickGroup>(),
            // 3 => createBrickGroup<RandomAnyEmptyBrickGroup>(),
            _ => createBrickGroup<TopRowRandomBrickGroup>()
        };

        return brickGroup;
    }

    public override void takeTurn()
    {
        base.takeTurn();

        actionManager.addToBot<RollMoveAction>().with(this);
        new OnOpPlayerTakeTurn().trigger();
    }
}