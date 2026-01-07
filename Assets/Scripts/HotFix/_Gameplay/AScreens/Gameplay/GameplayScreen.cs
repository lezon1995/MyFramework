namespace MarbleHero;

public class GameplayScreen : ClassObject
{
    GameplayPanel panel;
    
    public override void onCtor()
    {
        base.onCtor();
        panel = LT.LOAD<GameplayPanel>();
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