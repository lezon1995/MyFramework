using UnityEngine.Localization.Components;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class PlayerStatItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple statIcon;
	protected myUGUITextTMP statName;
	protected myUGUITextTMP statValue;
	// auto generate member end
	
	LocalizeStringEvent _stringEvent;
	
	public PlayerStatItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out statIcon, "H/Icon/StatIcon");
		newObject(out statName, "H/Name/StatName");
		newObject(out statValue, "H/Value/StatValue");
		// auto generate assignWindowInternal end
		
		statName.tryGetUnityComponent(out _stringEvent);
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
	
	public void setStringReference(string table, string entry)
	{
		_stringEvent.SetTable(table);
		_stringEvent.SetEntry(entry);
		// _stringEvent.RefreshString();
	}
}
