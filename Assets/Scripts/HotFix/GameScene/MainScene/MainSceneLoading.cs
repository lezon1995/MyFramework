public class MainSceneLoading : SceneProcedure
{
    protected override void onInit(SceneProcedure lastProcedure)
    {
        // changeProcedure<MainSceneLogin>();
        comboManager.load();
        prefabPool.setTimerInterval(60);
        
        changeProcedure<MainSceneGaming>();
    }

    protected override void onExit(SceneProcedure nextProcedure)
    {
    }
}