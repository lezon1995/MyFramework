namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class BallInventoryItem : WindowRecyclableUGUI
// auto generate classname end
{
    // auto generate member start
	protected myUGUIButton btn;
	protected myUGUIObject highlight;
	protected myUGUIObject highlightHovered;
	protected myUGUIObject normal;
	protected myUGUIImageSimple itemBorder;
	protected myUGUIImageSimple iconBg;
	protected myUGUIObject disable;
	protected myUGUIObject focus;
	protected myUGUIImageSimple icon;
	protected myUGUIObject[] stars = new myUGUIObject[3];
    // auto generate member end
    public BallInventoryItem(IWindowObjectOwner parent) : base(parent)
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
		newObject(out itemBorder, "Btn/Normal/Border");
		newObject(out iconBg, "Btn/Normal/Bg");
		newObject(out disable, "Btn/Disable");
		newObject(out focus, "Btn/Focus");
		newObject(out icon, "Btn/Icon");
		for (int i = 0; i < stars.Length; ++i)
		{
			newObject(out stars[i], "Btn/Icon/Grade_Star_01/Star" + i.IToS());
		}
        // auto generate assignWindowInternal end
    }

    public int slotIndex = -1;
    public BallInventoryBinder slotBinder;
    public BallTooltipTrigger tooltipTrigger;

    public override void init()
    {
        base.init();
		// auto generate init start
		// auto generate init end

        // 挂 Bridge 组件
        var go = mRoot.getGameObject();
        if (!go.TryGetComponent<BallOperationTargetBridge>(out var bridge))
        {
            bridge = go.AddComponent<BallOperationTargetBridge>();
        }
        bridge.Target = this;

        go.TryGetComponent(out tooltipTrigger);

        // 初始化 highlight 状态
        highlight?.setActive(false);
        highlightHovered?.setActive(false);

        // 订阅左键按下事件
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

    public void SetSlotData(int index, BallInventoryBinder binder)
    {
        slotIndex = index;
        slotBinder = binder;
    }

    void OnPointerPressed(UnityEngine.EventSystems.PointerEventData data)
    {
        if (data.button != UnityEngine.EventSystems.PointerEventData.InputButton.Left) 
            return;

        if (!BallOperationStateManager.Instance.IsActive && isOccupied)
        {
            var iconRect = icon?.getGameObject()?.GetComponent<UnityEngine.RectTransform>();
            if (iconRect != null)
                BallOperationStateManager.Instance.TryEnter(this, iconRect);
        }
    }

    // 原有事件转发(仅在非操作状态时)
    void onBtnClick()
    {
        if (_eventBlocked)
            return;
        slotBinder?.OnBallBtnClicked(slotIndex);
    }

    internal bool _eventBlocked;
}
