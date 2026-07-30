using MoreMountains;
using UnityEngine.EventSystems;
using static StringUtility;
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class BallSlotItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton btn;
	protected myUGUIObject selected;
	protected myUGUIObject highlight;
	protected myUGUIObject highlightHovered;
	protected myUGUIImageSimple icon;
	protected myUGUIObject[] stars = new myUGUIObject[3];
	// auto generate member end
	public BallSlotItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out btn, "Btn");
		newObject(out selected, "Btn/Selected");
		newObject(out highlight, "Btn/Highlight");
		newObject(out highlightHovered, "Btn/HighlightHovered");
		newObject(out icon, "Btn/Icon");
		for (int i = 0; i < stars.Length; ++i)
		{
			newObject(out stars[i], "Btn/Icon/Grade_Star_01/Star" + IToS(i));
		}
		// auto generate assignWindowInternal end
	}

	// 槽位 index 由 binder Rebuild 时写入,转发事件时从字段读。
	// 避免每次 Rebuild 创建闭包 lambda。
	int slotIndex = -1;
	BallSlotGroupBinder slotBinder;

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

	/// <summary>由 BallSlotGroupBinder 在 Rebuild 时写入本 item 的数据。</summary>
	public void SetSlotData(int index, BallSlotGroupBinder binder)
	{
		slotIndex = index;
		slotBinder = binder;
	}

	// 点击:转发到 binder 的实例方法,无闭包。
	void onBtnClick()
	{
		slotBinder?.OnSlotBtnClicked(slotIndex);
	}

	// 拖拽释放:同上。
	void onDragReleased(BallSlotItem src, UIDragReleaseEventData data)
	{
		slotBinder?.OnSlotDragReleased(src, slotIndex, data);
	}

	public override void onShow()
	{
		base.onShow();
	}
}