namespace MarbleHero;

public class DungeonMapScreen : ClassObject
{
    GameplayPanel panel;
    
    public override void onCtor()
    {
        base.onCtor();
        panel = LT.LOAD_HIDE<GameplayPanel>();
    }

    public override void onCreate()
    {
        base.onCreate();
        LT.SHOW<GameplayPanel>();
    }

    public override void destroy()
    {
        LT.HIDE<GameplayPanel>();
            
        base.destroy();
    }
}