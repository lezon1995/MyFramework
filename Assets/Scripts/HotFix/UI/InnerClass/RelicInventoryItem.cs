namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class RelicInventoryItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton btn;
	protected myUGUIObject highlight;
	protected myUGUIObject highlightHovered;
	protected myUGUIObject normal;
	protected myUGUIObject disable;
	protected myUGUIObject focus;
	protected myUGUIImageSimple icon;
	// auto generate member end
	public RelicInventoryItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out btn, "Btn");
		newObject(out highlight, "Btn/Highlight");
		newObject(out highlightHovered, "Btn/HighlightHovered");
		newObject(out normal, "Btn/Normal");
		newObject(out disable, "Btn/Disable");
		newObject(out focus, "Btn/Focus");
		newObject(out icon, "Btn/Icon");
		// auto generate assignWindowInternal end
	}

	// 槽位 index 由 binder Rebuild 时写入,转发事件时从字段读。
	// 避免每次 Rebuild 创建闭包 lambda。
	int slotIndex = -1;
	RelicInventoryBinder relicBinder;

	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end

		if (btn.tryGetUnityComponent(out UIEventListener listener))
		{
			listener.SetOnPotentialDragInitialized(onPotentialDragInitialized);
			listener.SetOnDragStarted(onDragStarted);
			listener.SetOnDragging(onDragging);
			listener.SetOnDragEnded(onDragEnded);
			listener.SetOnDragReleasedOverUI(onDragReleasedOverUI);
		}

		// 一次性订阅,转发走字段,不创建 lambda。
		btn.setUGUIButtonClick(onBtnClick);
		SetOnDragReleased(onDragReleased);
	}

	/// <summary>由 RelicInventoryBinder 在 Rebuild 时写入本 item 的数据。</summary>
	public void SetSlotData(int index, RelicInventoryBinder binder)
	{
		slotIndex = index;
		relicBinder = binder;
	}

	void onBtnClick()
	{
		relicBinder?.OnRelicBtnClicked(slotIndex);
	}

	void onDragReleased(RelicInventoryItem src, UIDragReleaseEventData data)
	{
		relicBinder?.OnRelicDragReleased(src, slotIndex, data);
	}

	public override void onShow()
	{
		base.onShow();
	}
}