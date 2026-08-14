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
	protected myUGUIImageSimple itemBorder;
	protected myUGUIImageSimple iconBg;
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
		newObject(out itemBorder, "Btn/Normal/Border");
		newObject(out iconBg, "Btn/Normal/Bg");
		newObject(out disable, "Btn/Disable");
		newObject(out focus, "Btn/Focus");
		newObject(out icon, "Btn/Icon");
        // auto generate assignWindowInternal end
    }

    // 槽位 index 由 binder Rebuild 时写入,转发事件时从字段读。
    // 避免每次 Rebuild 创建闭包 lambda。
    public int slotIndex { get; private set; } = -1;
    RelicInventoryBinder relicBinder;

    public override void init()
    {
        base.init();
        // auto generate init start
        // auto generate init end

        // 挂 Bridge 组件
        var go = mRoot.getGameObject();
        if (!go.TryGetComponent<RelicOperationTargetBridge>(out var bridge))
        {
            bridge = go.AddComponent<RelicOperationTargetBridge>();
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

        // 一次性订阅原有 btn click 转发
        btn.setUGUIButtonClick(onBtnClick);
    }

    /// <summary>由 RelicInventoryBinder 在 Rebuild 时写入本 item 的数据。</summary>
    public void SetSlotData(int index, RelicInventoryBinder binder)
    {
        slotIndex = index;
        relicBinder = binder;
    }

    void OnPointerPressed(UnityEngine.EventSystems.PointerEventData data)
    {
        if (data.button != UnityEngine.EventSystems.PointerEventData.InputButton.Left)
            return;

        if (!RelicOperationStateManager.Instance.IsActive && isOccupied)
        {
            var iconRect = icon?.getGameObject()?.GetComponent<UnityEngine.RectTransform>();
            if (iconRect != null)
                RelicOperationStateManager.Instance.TryEnter(this, iconRect);
        }
    }

    // 原有事件转发(仅在非操作状态时)
    void onBtnClick()
    {
        if (_eventBlocked)
            return;
        relicBinder?.OnRelicBtnClicked(slotIndex);
    }

    internal bool _eventBlocked;

    public override void onShow()
    {
        base.onShow();
        highlight?.setActive(false);
        highlightHovered?.setActive(false);
    }
}