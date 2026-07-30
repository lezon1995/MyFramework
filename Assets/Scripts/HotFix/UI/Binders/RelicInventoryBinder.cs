using System;

namespace MoreMountains
{
    /// <summary>
    /// 遗物背包 binder —— 把 RelicBag 同步到 RelicInventoryView。
    ///
    /// 数据模型:RelicBag 内部是固定 N 个 RelicInventorySlot,Slot.Item == null 表示空格子。
    /// 一次 Rebuild 把 N 个 slot 全部渲染:Item != null 的显示遗物,Item == null 的显示空格子。
    /// 单击选中,操作委托给 OperationPanelBinder。
    /// 拖拽:item 自身已经在 init() 一次性订阅 UnityEvent,转发走字段读取,
    /// 所以 Rebuild 不再创建 lambda,只更新 item 的数据(slot 索引 + 当前 relic)。
    /// </summary>
    public sealed class RelicInventoryBinder
    {
        RelicInventoryView _view;
        RelicBag _bag;

        RelicItem _selected;
        OperationPanelBinder _owner;

        public RelicInventoryBinder(RelicInventoryView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        internal void SetOwner(OperationPanelBinder owner) => _owner = owner;

        public RelicItem SelectedRelic => _selected;

        public event Action<RelicItem /*selected*/> RelicSelected;
        public event Action<RelicItem> SellRequested;

        public void Attach(RelicBag bag)
        {
            if (_bag != null)
                Detach();

            _bag = bag ?? throw new ArgumentNullException(nameof(bag));
            _view.SetTitle("RELICS");
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

        void OnBagItemAdded(RelicItem _) => Rebuild();
        void OnBagItemRemoved(RelicItem _) => Rebuild();
        void OnBagAnyChanged() => Rebuild();

        public void Rebuild()
        {
            if (_bag == null)
                return;

            // 把固定 N 个 slot 直接交给 View;View 自己判断 Slot.Item == null 决定显示空格子还是叠加 item。
            // binder 不在中间建一个 List<RelicItem>,避免 Rebuild 时的中间分配。
            _view.BuildRelicsWithIndex(_bag.SlotList, (index, item, slot) =>
            {
                var relic = slot.Item; // 可能为 null
                bool isEmpty = slot.IsEmpty;
                bool isOccupied = slot.IsOccupied;
                bool isSel = !isEmpty && ReferenceEquals(relic, _selected);

                item.SetSelected(isSel);
                if (isOccupied)
                    item.SetRelicIcon(relic.Def.Icon);

                item.SetIconVisible(!isEmpty);
                item.SetEnabled(!isEmpty);

                // 不创建 lambda;item 的 UnityEvent 在 init() 中已一次性订阅,
                // 这里只更新数据字段,转发走 item 自身的 onBtnClick / onDragReleased。
                item.SetSlotData(index, this);
            });
        }

        // ------------- item 事件转发入口(item 直接调过来,无 lambda 中转)-------------

        /// <summary>由 RelicInventoryItem.onBtnClick 转发。</summary>
        public void OnRelicBtnClicked(int slotIndex)
        {
            if (_bag == null) 
                return;

            if (slotIndex < 0 || slotIndex >= _bag.SlotList.Count) 
                return;

            var relic = _bag.SlotList[slotIndex].Item;
            OnRelicClicked(relic);
        }

        /// <summary>由 RelicInventoryItem.onDragReleased 转发。</summary>
        public void OnRelicDragReleased(RelicInventoryItem src, int slotIndex, UIDragReleaseEventData data)
        {
            if (_bag == null) 
                return;

            if (slotIndex < 0 || slotIndex >= _bag.SlotList.Count) 
                return;

            var relic = _bag.SlotList[slotIndex].Item;
            if (relic == null) 
                return;

            _owner?.OnRelicInventoryDragReleased(src, relic, data);
        }

        // ------------- 选择/出售事件(由外部按钮触发)-------------

        void OnRelicClicked(RelicItem relic)
        {
            if (relic == null)
                return;

            _selected = ReferenceEquals(_selected, relic) ? null : relic;
            UpdateSelectionVisuals();
            if (_selected != null) RelicSelected?.Invoke(_selected);
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
                    var r = slot.Item;
                    item.SetSelected(r != null && ReferenceEquals(r, _selected));
                }

                i++;
            }
        }

        public void RequestSellSelected()
        {
            if (_selected == null)
                return;

            SellRequested?.Invoke(_selected);
        }

        public void ClearSelection() => _selected = null;
    }
}