public class GameEntryDerived : GameEntry
{
    public override void Awake()
    {
        base.Awake();
        var framework = new Game();
        framework.init();
        setFrameworkAOT(framework);
    }
}