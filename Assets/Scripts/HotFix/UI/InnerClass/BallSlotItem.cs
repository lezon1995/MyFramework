using MoreMountains;
using UnityEngine;
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
			newObject(out stars[i], "Btn/Icon/Grade_Star_01/Star" + i.IToS());
		}
		// auto generate assignWindowInternal end
	}

	int slotIndex = -1;
	public BallSlotGroupBinder slotBinder;

	public override void init()
	{
		base.init();

		// 挂 Bridge 组件到根节点,用于射线检测、事件拦截和右键取消
		var go = mRoot.getGameObject();
		if (!go.TryGetComponent<BallOperationTargetBridge>(out var bridge))
		{
			bridge = go.AddComponent<BallOperationTargetBridge>();
		}
		bridge.Target = this;

		// 初始化 highlight 状态
		highlight?.setActive(false);
		highlightHovered?.setActive(false);

		// 订阅左键按下事件(进入操作状态)
		if (btn.tryGetUnityComponent(out UIEventListener listener))
		{
			listener.SetOnPointerPressed(OnPointerPressed);
		}
	}

	public override void onShow()
	{
		base.onShow();
		highlight?.setActive(false);
		highlightHovered?.setActive(false);
	}

	/// <summary>由 BallSlotGroupBinder 在 Rebuild 时写入。</summary>
	public void SetSlotData(int index, BallSlotGroupBinder binder)
	{
		slotIndex = index;
		slotBinder = binder;
	}

	// 左键按下:尝试进入操作状态
	void OnPointerPressed(PointerEventData data)
	{
		if (data.button != PointerEventData.InputButton.Left) 
			return;

		if (!BallOperationStateManager.Instance.IsActive && ballSlot.IsOccupied)
		{
			var iconRect = icon?.getGameObject()?.GetComponent<RectTransform>();
			if (iconRect != null)
				BallOperationStateManager.Instance.TryEnter(this, iconRect);
		}
	}

	// 原有事件转发(仅在非操作状态时)
	void onBtnClick()
	{
		if (_eventBlocked)
			return;
		slotBinder?.OnSlotBtnClicked(slotIndex);
	}

	// 内部标志:操作状态激活时阻止原有事件
	internal bool _eventBlocked;
}
