using MoreMountains;
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public class PlayerHealthView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple healthBar;
	protected myUGUIText curHealth;
	protected myUGUIText maxHealth;
	protected myUGUIText ballCount;
	// auto generate member end
	public PlayerHealthView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out healthBar, "HealthOutline/HealthBar");
		newObject(out curHealth, "Circle/H2/TextCurHealth");
		newObject(out maxHealth, "Circle/H2/TextMaxHealth");
		newObject(out ballCount, "Balls/TextBallCount");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
	}
	public override void onShow()
	{
		base.onShow();
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

	public void setHealth(int v) => curHealth.setText(v);
	public void setHealthPct(float v) => healthBar.setFillPercent(v);
	public void setHealthMax(int v) => maxHealth.setText(v);
	public void setBallCount(int v) => ballCount.setText(v);
}
