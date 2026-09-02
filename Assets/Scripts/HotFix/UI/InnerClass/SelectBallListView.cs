using System.Collections.Generic;
using System.Linq;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/SelectPlayerPanel.prefab
// 
public partial class SelectBallListView : WindowObjectUGUI
// auto generate classname end
{
    // auto generate member start
	protected WindowStructPool<SelectBallItem> SelectBallItemPool;
    // auto generate member end
    public SelectBallListView(IWindowObjectOwner parent) : base(parent)
    {
        // auto generate constructor start
		SelectBallItemPool = new(this);
        // auto generate constructor end
    }

    protected override void assignWindowInternal()
    {
        // auto generate assignWindowInternal start
		SelectBallItemPool.assignTemplate(mRoot, "Scroll/Viewport/Content/SelectBallItem");
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

    SelectedCharacterDetailView detailView;
    BallTooltipItem ballTooltipItem;
    Dictionary<BallItem, SelectBallItem> ballItems = new();
    public List<BallItem> selectedBalls = new();

    public void initBallItems()
    {
        foreach (var ballItem in ballManager.getDefs().Select(def => BallItem.New(def)))
        {
            var item = SelectBallItemPool.newItem();
            bool isUnlocked = IsBallUnlocked(ballItem);
            isUnlocked = true;
            item.refresh(ballItem, isUnlocked, OnBallItemClicked, OnBallItemHovered, OnBallItemHoveredEnd);
            ballItems[ballItem] = item;
        }
    }

    public void setCharacterDetailView(SelectedCharacterDetailView v) => detailView = v;
    public void setBallTooltipItem(BallTooltipItem v) => ballTooltipItem = v;

    bool IsBallUnlocked(BallItem def)
    {
        return def.Type == BallType.Normal;
    }

    void OnBallItemClicked(SelectBallItem item)
    {
        if (!item.IsUnlocked)
            return;

        selectedBalls.Clear();
        selectedBalls.Add(item.Item);
        _charSelectInfo.balls.Clear();
        _charSelectInfo.balls.AddRange(selectedBalls);

        RefreshBallItems();
        ballTooltipItem.Refresh(item.Item);
        detailView.RefreshCharacterSelectBalls(selectedBalls);
        // selectPlayerPanel.updateNextStepButton();
    }

    void OnBallItemHovered(SelectBallItem item)
    {
        if (!item.IsUnlocked)
            return;

        ballTooltipItem.Refresh(item.Item);
        // detailView.RefreshCharacterDetail(item.Def);
    }

    void OnBallItemHoveredEnd(SelectBallItem item)
    {
        if (selectedBalls.Count > 0)
        {
            ballTooltipItem.Refresh(selectedBalls[0]);
        }

        // detailView.RefreshCharacterDetail(selectedPlayer);
    }

    public void RefreshBallItems()
    {
        foreach (var (def, item) in ballItems)
        {
            bool isSelected = selectedBalls.Contains(def);
            item.SetSelected(isSelected);
        }
    }
}