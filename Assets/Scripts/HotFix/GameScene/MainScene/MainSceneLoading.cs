public class MainSceneLoading : SceneProcedure
{
    protected override void onInit(SceneProcedure lastProcedure)
    {
        brickManager.load();
        
        changeProcedure<MainSceneGaming>();
    }

    protected override void onExit(SceneProcedure nextProcedure)
    {
    }
}