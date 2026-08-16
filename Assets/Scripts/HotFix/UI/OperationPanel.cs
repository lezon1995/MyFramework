using Obfuz;
using UnityEngine.Localization.Components;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class OperationPanel : LayoutScript
// auto generate classname end
{
	// auto generate member start
	protected RewardChooseView rewardChooseView;
	protected ShopView shopView;
	protected PlayerInfoView playerInfoView;
	protected WaveMonsterView waveMonsterView;
	protected RelicInventoryView relicInventoryView;
	protected BallInventoryView ballInventoryView;
	protected myUGUITextTMP textTitle;
	protected myUGUIButton btnNext;
	protected myUGUITextTMP textBtn;
	// auto generate member end

	LocalizeStringEvent _stringTitle;
	LocalizeStringEvent _stringNextWaveBtn;
	
	public OperationPanel()
	{
		// auto generate constructor start
		rewardChooseView = new(this);
		shopView = new(this);
		playerInfoView = new(this);
		waveMonsterView = new(this);
		relicInventoryView = new(this);
		ballInventoryView = new(this);
		// auto generate constructor end
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		rewardChooseView.assignWindow(mRoot, "RewardChooseView");
		shopView.assignWindow(mRoot, "ShopView");
		playerInfoView.assignWindow(mRoot, "PlayerInfoView");
		waveMonsterView.assignWindow(mRoot, "WaveMonsterView");
		relicInventoryView.assignWindow(mRoot, "InventoryView/RelicInventoryView");
		ballInventoryView.assignWindow(mRoot, "InventoryView/BallInventoryView");
		newObject(out textTitle, "Title/TextTitle");
		newObject(out btnNext, "BtnNext");
		newObject(out textBtn, "BtnNext/TextBtn");
		// auto generate assignWindow end

		textTitle.tryGetUnityComponent(out _stringTitle);
		textBtn.tryGetUnityComponent(out _stringNextWaveBtn);
	}
	public override void init()
	{
		initBinder();
		base.init();
		// auto generate init start
		// auto generate init end
	}
}
