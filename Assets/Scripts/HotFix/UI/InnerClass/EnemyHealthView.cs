using MarbleHero;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public class EnemyHealthView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple mHealthBar;
	protected myUGUIText mTextCurHealth;
	protected myUGUIText mTextMaxHealth;
	// auto generate member end
	public EnemyHealthView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out mHealthBar, "HealthOutline/HealthBar");
		newObject(out mTextCurHealth, "Circle/H/TextCurHealth");
		newObject(out mTextMaxHealth, "Circle/H/TextMaxHealth");
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
	
	public void refresh(AMonster m)
	{
		if (m == null)
			return;

		setHealth(m.currentHealth);
		setHealthPct(m.currentHealthPct);
		setHealthMax(m.maxHealth);
	}

	public void setHealth(int v) => mTextCurHealth.setText(v);
	public void setHealthPct(float v) => mHealthBar.setFillPercent(v);
	public void setHealthMax(int v) => mTextMaxHealth.setText(v);
}
