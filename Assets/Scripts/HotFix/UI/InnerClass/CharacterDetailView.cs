
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/SelectPlayerPanel.prefab
// 

using MoreMountains;

public class CharacterDetailView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple characterIcon;
	protected myUGUITextTMP characterName;
	protected myUGUITextTMP characterStats;
	protected myUGUITextTMP characterDesc;
	// auto generate member end
	public CharacterDetailView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out characterIcon, "V/Info/H/Avatar/Icon/Image");
		newObject(out characterName, "V/Info/H/Title/V/Name/Text");
		newObject(out characterStats, "V/Stats/Text");
		newObject(out characterDesc, "V/Desc/Text");
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
	
	public void RefreshCharacterDetail(PlayerDef def)
	{
		characterIcon.setSpriteOnly(def.Icon);
		characterName.setText(def.DisplayName);
		characterStats.setText(def.DisplayStats);
		characterDesc.setText(def.DisplayDesc);
	}
}
