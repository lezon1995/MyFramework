namespace MarbleHero;

public partial class OverlayMenu
{
    public override void onCtor()
    {
        base.onCtor();
    }

    public override void onCreate()
    {
        base.onCreate();
    }

    public override void destroy()
    {
        base.destroy();
    }

    public override void update(float dt)
    {
        base.update(dt);

        mExpView?.refresh(dt, player.exp);
        mPlayerHealthView?.refresh(player);
        mEnemyHealthView?.refresh(enemy);
    }

    public void hideCombatPanels()
    {
    }

    public void showCombatPanels()
    {
    }
}