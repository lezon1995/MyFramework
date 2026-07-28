
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public class ShopView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUITextTMP shopTitle;
	protected myUGUITextTMP remainCoin;
	protected myUGUIObject shopItems;
	protected myUGUIButton btnReroll;
	protected myUGUIButton btnBuyExp;
	protected myUGUIObject sellZone;
	protected WindowStructPool<BallPurchaseItem> BallPurchaseItemPool;
	protected WindowStructPool<RelicPurchaseItem> RelicPurchaseItemPool;
	// auto generate member end
	public ShopView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
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
		newObject(out sellZone, "SellZone");
		BallPurchaseItemPool.assignTemplate(mRoot, "ShopItems/BallPurchaseItem");
		RelicPurchaseItemPool.assignTemplate(mRoot, "ShopItems/RelicPurchaseItem");
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
}
