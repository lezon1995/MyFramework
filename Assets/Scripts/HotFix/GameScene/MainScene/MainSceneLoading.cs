public class MainSceneLoading : SceneProcedure
{
    protected override void onInit(SceneProcedure lastProcedure)
    {
        brickManager.load();
        comboManager.load();
        mPrefabPoolManager.setTimerInterval(60);
        
        changeProcedure<MainSceneMenu>();
    }

    protected override void onExit(SceneProcedure nextProcedure)
    {
    }
}