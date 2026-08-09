namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class ShopView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUITextTMP shopTitle;
	protected myUGUITextTMP remainCoin;
	protected myUGUIObject shopItems;
	protected myUGUIButton btnReroll;
	protected myUGUIButton btnBuyExp;
	protected ShopSellZoneView shopSellZoneView;
	protected WindowStructPool<BallPurchaseItem> BallPurchaseItemPool;
	protected WindowStructPool<RelicPurchaseItem> RelicPurchaseItemPool;
	// auto generate member end
	public ShopView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		shopSellZoneView = new(this);
		BallPurchaseItemPool = new(this);
		RelicPurchaseItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out shopTitle, "Title/TextTitle");
		newObject(out remainCoin, "TotalCoin/Price/TextRemainCoin");
		newObject(out shopItems, "ShopItems");
		newObject(out btnReroll, "BtnReroll");
		newObject(out btnBuyExp, "BtnBuyExp");
		shopSellZoneView.assignWindow(mRoot, "ShopSellZoneView");
		BallPurchaseItemPool.assignTemplate(mRoot, "ShopItems/BallPurchaseItem");
		RelicPurchaseItemPool.assignTemplate(mRoot, "ShopItems/RelicPurchaseItem");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end

		shopSellZoneView.shopBinder = binder;
	}
	public override void onShow()
	{
		base.onShow();
	}
}
