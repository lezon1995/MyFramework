using System;
using System.Collections.Generic;

namespace MoreMountains
{
    public interface IBallsContainerView
    {
        BallTooltipItem BallTooltipItem { get; }
        void SetTitle(string title);
        void BuildBallsWithIndex(List<BallInventorySlot> ballInventorySlots, Action<int, BallInventoryItem, BallInventorySlot> onBuild);
        bool GetUsedItem(int index, out BallInventoryItem o);
        void SetActive(bool active);
    }

    /// <summary>
    /// 球背包 binder —— 把 BallBag 内容同步到 BallInventoryView。
    ///
    /// 数据模型:BallBag 内部是固定 N 个 BallInventorySlot,Slot.Item == null 表示空格子。
    /// 一次 Rebuild 把 N 个 slot 全部渲染:Item != null 的显示球,Item == null 的显示空格子。
    /// 单击球 → 选中;选中态视觉用 focus 节点切换;空格子不可选中也不响应点击。
    /// 拖拽:item 自身已经在 init() 一次性订阅 UnityEvent,转发走字段读取,
    /// 所以 Rebuild 不再创建 lambda,只更新 item 的数据(slot 索引 + 当前 ball)。
    /// </summary>
    public sealed class BallInventoryBinder
    {
        IBallsContainerView _view;
        BallBag _bag;
        BallItem _selected;
        APlayer _player;

        public BallInventoryBinder(IBallsContainerView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public BallItem SelectedBall => _selected;

        /// <summary>背包数据模型。</summary>
        public BallBag Bag => _bag;

        /// <summary>背包视图。</summary>
        public IBallsContainerView View => _view;
        public APlayer Player => _player;

        public event Action<BallItem /*selected ball*/> BallSelected;
        public event Action<BallItem /*ball*/, int /*slotIndex*/> EquipRequested;
        public event Action<BallItem /*ball*/> UpgradeRequested;
        public event Action<BallItem /*ball*/> SellRequested;

        public void Attach(APlayer p, BallBag bag)
        {
            if (_bag != null)
                Detach();

            _player = p;
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
            _player = null;
        }

        void OnBagItemAdded(BallItem _) => Rebuild();
        void OnBagItemRemoved(BallItem _) => Rebuild();
        void OnBagAnyChanged() => Rebuild();

        public void Rebuild()
        {
            if (_bag == null)
                return;

            // 把固定 N 个 slot 直接交给 View;View 自己判断 Slot.Item == null 决定显示空格子还是叠加 item。
            // binder 不在中间建一个 List<BallItem>,避免 Rebuild 时的中间分配。
            _view.BuildBallsWithIndex(_bag.SlotList, (index, item, slot) =>
            {
                // 把数据塞到 item 上(item 内部保存一份,作为兜底转发路径)
                // UnityEvent 订阅在 item.init() 中已经一次性完成,这里只更新数据字段,
                // 不创建任何 lambda。
                item.SetSlotData(index, this);
                item.SetBallInventorySlot(slot);

                var ball = slot.Item; // 可能为 null
                bool isEmpty = slot.IsEmpty;
                bool isOccupied = slot.IsOccupied;
                bool isSel = !isEmpty && ReferenceEquals(ball, _selected);

                item.SetSelected(isSel);
                if (isOccupied)
                {
                    item.SetBallItem(ball);
                    item.SetBallIcon(ball.Def.Icon);
                    item.SetRarity(ball.getLevelToRarity());
                }
                else
                {
                    item.SetBallItem(null);
                    item.SetBallIcon(null);
                    item.SetRarity(ItemRarity.Tier1);
                }

                item.SetIconVisible(!isEmpty);
                item.SetEnabledState(!isEmpty);
            });
        }

        // ------------- item 事件转发入口（item 直接调过来,无 lambda 中转）-------------

        /// <summary>由 BallInventoryItem.onBtnClick 转发。
        /// 无参数,ball 从 Bag 的 SlotList 中按 slotIndex 实时读取,避免 item 持有过期 ball 引用。</summary>
        public void OnBallBtnClicked(int slotIndex)
        {
            if (_bag == null)
                return;

            if (slotIndex < 0 || slotIndex >= _bag.SlotList.Count)
                return;

            var ball = _bag.SlotList[slotIndex].Item;
            OnBallClicked(ball);
        }

        /// <summary>
        /// 根据 BallInventoryItem 查找对应的 slotIndex。
        /// 用于操作状态中点击 BallInventoryItem 时确定目标背包格子。
        /// </summary>
        public void GetSlotIndexForItem(BallInventoryItem item, out int slotIndex)
        {
            slotIndex = -1;
            if (_bag == null || item == null)
                return;

            int i = 0;
            foreach (var slot in _bag.SlotList)
            {
                if (_view.GetUsedItem(i, out var usedItem) && usedItem == item)
                {
                    slotIndex = slot.Index;
                    return;
                }

                i++;
            }
        }

        // ------------- 选择/出售/升级事件(由外部按钮触发)-------------

        void OnBallClicked(BallItem ball)
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
            foreach (var slot in _bag.SlotList)
            {
                if (_view.GetUsedItem(i, out var item))
                {
                    var b = slot.Item;
                    item.SetSelected(b != null && ReferenceEquals(b, _selected));
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

        public void SetViewActive(bool active) => _view.SetActive(active);
    }
}