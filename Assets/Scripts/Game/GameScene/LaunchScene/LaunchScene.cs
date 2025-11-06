public class LaunchScene : GameScene
{
    public override void assignStartExitProcedure()
    {
        startProcedure = typeof(LaunchSceneVersion);
        exitProcedure = typeof(LaunchSceneExit);
    }

    public override void createSceneProcedure()
    {
        addProcedure<LaunchSceneVersion>();
        addProcedure<LaunchSceneFileList>();
        addProcedure<LaunchSceneDownload>();
        addProcedure<LaunchSceneExit>();
    }
}