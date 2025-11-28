public class MainSceneLoading : SceneProcedure
{
    protected override void onInit(SceneProcedure lastProcedure)
    {
        changeProcedure<MainSceneGaming>();
    }

    protected override void onExit(SceneProcedure nextProcedure)
    {
    }
}