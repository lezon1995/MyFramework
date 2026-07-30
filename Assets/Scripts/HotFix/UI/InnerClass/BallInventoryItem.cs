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
		newObject(out disable, "Btn/Disable");
		newObject(out focus, "Btn/Focus");
		newObject(out icon, "Btn/Icon");
		for (int i = 0; i < stars.Length; ++i)
		{
			newObject(out stars[i], "Btn/Icon/Grade_Star_01/Star" + IToS(i));
		}
        // auto generate assignWindowInternal end
    }

    // 由 binder Rebuild 时写入:slot 索引 + binder 引用。
    // 转发事件时从字段读,避免每次 Rebuild 创建 lambda 闭包。
    int slotIndex = -1;
    BallInventoryBinder slotBinder;

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

        // 一次性订阅 UnityEvent,转发走字段读,不创建 lambda。
        btn.setUGUIButtonClick(onBtnClick);
        SetOnDragReleased(onDragReleased);
    }

    /// <summary>由 BallInventoryBinder 在 Rebuild 时写入本 item 的数据。
    /// 写入后,转发事件会根据 slotIndex 实时从 BallBag.SlotList 取 ball,避免 item 持有过期 ball 引用。</summary>
    public void SetSlotData(int index, BallInventoryBinder binder)
    {
        slotIndex = index;
        slotBinder = binder;
    }

    // 拖拽释放:把 item + slotIndex + data 转给 binder,binder 拿到 index 后再去 SlotList 取 ball。
    void onDragReleased(BallInventoryItem inventoryItem, UIDragReleaseEventData data)
    {
        slotBinder?.OnBallDragReleased(inventoryItem, slotIndex, data);
    }

    void onBtnClick()
    {
        slotBinder?.OnBallBtnClicked(slotIndex);
    }

    public override void onShow()
    {
        base.onShow();
    }
}