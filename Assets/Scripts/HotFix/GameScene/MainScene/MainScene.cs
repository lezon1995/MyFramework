public class MainScene : GameScene
{
    COMGameSceneAudio sceneAudio;
    COMGameSceneVolume sceneVolume;

    public override void resetProperty()
    {
        base.resetProperty();
        UN_CLASS(ref sceneAudio);
        UN_CLASS(ref sceneVolume);
    }

    protected override void initComponents()
    {
        base.initComponents();
        getOrAddComponent(out sceneAudio);
        getOrAddComponent(out sceneVolume);
    }

    public override void init()
    {
        base.init();
        sceneAudio.initAudioSource();
    }

    public override void assignStartExitProcedure()
    {
        mStartProcedure = typeof(MainSceneLoading);
        mExitProcedure = typeof(MainSceneExit);
    }

    public override void createSceneProcedure()
    {
        addProcedure(typeof(MainSceneLoading));
        addProcedure(typeof(MainSceneLogin));
        addProcedure(typeof(MainSceneGaming));
        addProcedure(typeof(MainSceneExit));
    }
}