public class MainScene : GameScene
{
    public override void assignStartExitProcedure()
    {
        startProcedure = typeof(MainSceneLoading);
        exitProcedure = typeof(MainSceneExit);
    }

    public override void createSceneProcedure()
    {
        addProcedure(typeof(MainSceneLoading));
        addProcedure(typeof(MainSceneLogin));
        addProcedure(typeof(MainSceneGaming));
        addProcedure(typeof(MainSceneExit));
    }
}