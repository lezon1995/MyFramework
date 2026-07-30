using System;

namespace MoreMountains
{
    /// <summary>
    /// 球背包 binder —— 把 BallBag 内容同步到 BallInventoryView。
    /// 单击球 → 选中；选中态视觉用 focus 节点切换。
    /// 不在这里做 Equip / Upgrade / Sell —— 而是抛事件,由 OperationPanelBinder 把按钮接到对应操作。
    /// 拖拽：通过 SetOnDragReleased 把「球被拖到哪了」交给 OperationPanelBinder 处理(装备/出售)。
    /// </summary>
    public sealed class BallInventoryBinder
    {
        Action<BallInventoryItem, BallItem> onBuild;
        BallInventoryView _view;
        BallBag _bag;

        BallItem _selected;
        OperationPanelBinder _owner; // 用于把 drag 释放事件转交顶层 binder
        public OperationPanelBinder Owner => _owner;

        public BallInventoryBinder(BallInventoryView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            onBuild = OnBuild;
        }

        /// <summary>由 OperationPanelBinder 在构造后注入,让 item 的 drag 释放回调能找到上层分派逻辑。</summary>
        internal void SetOwner(OperationPanelBinder owner) => _owner = owner;

        public BallItem SelectedBall => _selected;

        public event Action<BallItem /*selected ball*/> BallSelected;
        public event Action<BallItem /*ball*/, int /*slotIndex*/> EquipRequested;
        public event Action<BallItem /*ball*/> UpgradeRequested;
        public event Action<BallItem /*ball*/> SellRequested;

        public void Attach(BallBag bag)
        {
            if (_bag != null)
                Detach();

            _bag = bag ?? throw new ArgumentNullException(nameof(bag));
            _view.SetTitle("BALL BAG");
            _bag.OnItemAdded += OnBagItemAdded;
            _bag.OnItemRemoved += OnBagItemRemoved;
            _bag.OnBagChanged += OnBagAnyChanged;
            Rebuild();
        }

        public void Detach()
        {
            if (_bag == null)
                return;

            _bag.OnItemAdded -= OnBagItemAdded;
            _bag.OnItemRemoved -= OnBagItemRemoved;
            _bag.OnBagChanged -= OnBagAnyChanged;
            _bag = null;
            _selected = null;
        }

        void OnBagItemAdded(BallItem _) => Rebuild();
        void OnBagItemRemoved(BallItem _) => Rebuild();
        void OnBagAnyChanged() => Rebuild();

        public void Rebuild()
        {
            if (_bag == null)
                return;

            // 把 IReadOnlyList 转成 List 以匹配 WindowStructPool.newItemList 的 List<T> 要求。
            _view.BuildBalls(_bag.AllItems, onBuild);
        }

        void OnBuild(BallInventoryItem item, BallItem ball)
        {
            bool isSel = ReferenceEquals(ball, _selected);
            item.SetBallItem(ball);
            item.SetBallInventoryBinder(this);
            item.SetSelected(isSel);
            item.SetBallIcon(ball.Def.Icon);
            item.SetIconVisible(true);
            item.SetStarCount(ClampStars(ball.Level));
            item.SetEnabledState(true);
        }

        public void OnBallClicked(BallItem ball)
        {
            if (ball == null)
                return;

            _selected = ReferenceEquals(_selected, ball) ? null : ball;
            UpdateSelectionVisuals();
            if (_selected != null)
                BallSelected?.Invoke(_selected);
        }

        void UpdateSelectionVisuals()
        {
            if (_bag == null)
                return;

            int i = 0;
            foreach (var ball in _bag.AllItems)
            {
                if (_view.GetUsedItem(i, out var item))
                {
                    item.SetSelected(ReferenceEquals(ball, _selected));
                }

                i++;
            }
        }

        public void RequestEquipSelected(int slotIndex)
        {
            if (_selected == null)
                return;

            EquipRequested?.Invoke(_selected, slotIndex);
        }

        public void RequestUpgradeSelected()
        {
            if (_selected == null)
                return;

            UpgradeRequested?.Invoke(_selected);
        }

        public void RequestSellSelected()
        {
            if (_selected == null)
                return;

            SellRequested?.Invoke(_selected);
        }

        public void ClearSelection() => _selected = null;

        static int ClampStars(int level)
        {
            if (level < 0)
                return 0;
            if (level > 3)
                return 3;
            return level;
        }
    }
}