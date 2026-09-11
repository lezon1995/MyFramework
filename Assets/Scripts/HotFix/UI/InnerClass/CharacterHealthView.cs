
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/CharacterInfoView.prefab
// 
public partial class CharacterHealthView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject expBar;
	protected myUGUITextTMP curExp;
	protected myUGUITextTMP maxExp;
	protected myUGUITextTMP curShield;
	// auto generate member end
	
	protected DamageChunkHealthBarUI damageChunkHealthBarUI;
	public CharacterHealthView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out expBar, "HealthBarRenderer");
		newObject(out curExp, "Health/TextCurHealth");
		newObject(out maxExp, "Health/TextMaxHealth");
		newObject(out curShield, "Shield/TextCurShield");
		// auto generate assignWindowInternal end
		
		expBar.tryGetUnityComponent(out damageChunkHealthBarUI);
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
}
