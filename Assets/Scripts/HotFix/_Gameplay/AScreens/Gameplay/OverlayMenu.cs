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

        expBar?.refresh(player.exp);
        playerInfo?.refresh(player);
        enemyInfo?.refresh(enemy);
    }

    public void hideCombatPanels()
    {
    }

    public void showCombatPanels()
    {
    }

    public class PlayerInfo : UIObject, IRefresh<APlayer>
    {
        myUGUIText health, healthMax, ballCount;
        myUGUIImageSimple healthBar;

        public PlayerInfo(myUGUIObject t) : base(t)
        {
            t.newObject(out health, "TextCurHealth");
            t.newObject(out healthMax, "TextMaxHealth");
            t.newObject(out healthBar, "HealthBar");
            t.newObject(out ballCount, "TextBallCount");
        }

        public void refresh(APlayer p)
        {
            if (p == null)
                return;

            setHealth(p.currentHealth);
            setHealthPct(p.currentHealthPct);
            setHealthMax(p.maxHealth);
            setBallCount(p.ballCount);
        }

        public void setHealth(int v) => health.setText(v);
        public void setHealthPct(float v) => healthBar.setFillPercent(v);
        public void setHealthMax(int v) => healthMax.setText(v);
        public void setBallCount(int v) => ballCount.setText(v);
    }

    public class EnemyInfo : UIObject, IRefresh<AMonster>
    {
        myUGUIText health, healthMax;
        myUGUIImageSimple healthBar;

        public EnemyInfo(myUGUIObject t) : base(t)
        {
            t.newObject(out health, "TextCurHealth");
            t.newObject(out healthMax, "TextMaxHealth");
            t.newObject(out healthBar, "HealthBar");
        }

        public void refresh(AMonster m)
        {
            if (m == null)
                return;

            setHealth(m.currentHealth);
            setHealthPct(m.currentHealthPct);
            setHealthMax(m.maxHealth);
        }

        public void setHealth(int v) => health.setText(v);
        public void setHealthPct(float v) => healthBar.setFillPercent(v);
        public void setHealthMax(int v) => healthMax.setText(v);
    }
}