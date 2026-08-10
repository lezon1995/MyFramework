
namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class ShopSellZoneView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject highlight;
	protected myUGUIObject highlightHovered;
	protected myUGUITextTMP textSellPrice;
	// auto generate member end
	public ShopSellZoneView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out highlight, "Highlight");
		newObject(out highlightHovered, "HighlightHovered");
		newObject(out textSellPrice, "SellTips/Price/TextSellPrice");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
		
		// 挂 Bridge 组件
		var go = mRoot.getGameObject();
		if (!go.TryGetComponent<ItemOperationTargetBridge>(out var bridge))
		{
			bridge = go.AddComponent<ItemOperationTargetBridge>();
		}
		bridge.Target = this;
		Target = this;

		// 初始化 highlight 状态
		highlight?.setActive(false);
		highlightHovered?.setActive(false);
	}
	public override void onShow()
	{
		base.onShow();
		BallOperationStateManager.BroadcastHighlightEvent += OnHighlightChanged;
		RelicOperationStateManager.BroadcastHighlightEvent += OnHighlightChanged;
	}

	public override void onHide()
	{
		base.onHide();
		BallOperationStateManager.BroadcastHighlightEvent -= OnHighlightChanged;
		RelicOperationStateManager.BroadcastHighlightEvent -= OnHighlightChanged;
	}

	public IItemOperationTarget Target;

	void OnHighlightChanged(IBallOperationTarget source, bool visible)
	{
		Target?.SetHighlightVisible(visible);
		if (visible && source == Target)
		{
			Target?.SetHovered(true);
		}
	}

	void OnHighlightChanged(IRelicOperationTarget source, bool visible)
	{
		Target?.SetHighlightVisible(visible);
		if (visible && source == Target)
		{
			Target?.SetHovered(true);
		}
	}
}
