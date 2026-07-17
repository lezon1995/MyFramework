namespace MoreMountains;

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

        ExpView?.refresh(dt, player.Exp);
        PlayerHealthView?.refresh(player);
        // EnemyHealthView?.refresh(enemy);
    }

    public void hideCombatPanels()
    {
    }

    public void showCombatPanels()
    {
    }
}