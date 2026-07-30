using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 球槽位组 binder —— 把 BallSlotGroup 的状态同步到 BallSlotGroupView。
    ///
    /// 槽位 item 也有 Btn 字段，所以这里在 Rebuild 时把点击接进来：
    ///   • 点击空槽 / 装备槽 → 切换选中态（聚焦于"装备/升级/卸下"按钮的 target）。
    ///   • 选中态由 _selectedSlotIndex 决定；外部 OperationPanelBinder 监听 SelectionChanged。
    ///
    /// 拖拽：
    ///   • 槽位 → 槽位：Swap。
    ///   • 槽位 → SellZone：卸下 + 出售（OnSlotDragReleased 转交 OperationPanelBinder）。
    /// </summary>
    public sealed class BallSlotGroupBinder
    {
        readonly BallSlotGroupView _view;
        BallSlotGroup _model;
        int _selectedSlotIndex = -1;
        OperationPanelBinder _owner;

        public BallSlotGroupBinder(BallSlotGroupView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        internal void SetOwner(OperationPanelBinder owner) => _owner = owner;

        public BallSlotGroup Model => _model;
        public int SelectedSlotIndex => _selectedSlotIndex;
        public event Action<int /*slotIndex*/> SelectionChanged;

        public void Attach(BallSlotGroup model)
        {
            if (_model != null) Detach();
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view.SetTitle("SLOTS");
            _model.OnSlotsChanged += OnModelChanged;
            Rebuild();
        }

        public void Detach()
        {
            if (_model != null)
                _model.OnSlotsChanged -= OnModelChanged;
            _model = null;
        }

        public void SetSelectedSlot(int index)
        {
            if (_selectedSlotIndex == index) return;
            _selectedSlotIndex = index;
            UpdateSelectionVisuals();
            SelectionChanged?.Invoke(_selectedSlotIndex);
        }

        void OnModelChanged() => Rebuild();

        public void Rebuild()
        {
            if (_model == null) return;
            var slots = new List<BallSlot>(_model.Slots);

            _view.BuildSlots(slots, (item, slot) =>
            {
                var ball = slot.Current;
                item.SetBallIcon(ball.Def.Icon);
                item.SetIconVisible(true);
                item.SetStarCount(ClampStars(ball.Level));
                item.SetSelected(_selectedSlotIndex == slot.Index);
                item.SetOnClick(() => OnSlotClicked(slot.Index));

                item.SetOnDragReleased((src, data) =>
                {
                    if (_model == null) return;
                    var b = slot.Current;
                    if (b != null) _owner?.OnSlotDragReleased(src, slot.Index, b, data);
                });
            });
        }

        void UpdateSelectionVisuals()
        {
            if (_model == null) return;
            int i = 0;
            foreach (var slot in _model.Slots)
            {
                if (_view.GetUsedItem(i, out var item))
                {
                    item.SetSelected(_selectedSlotIndex == slot.Index);
                }
                i++;
            }
        }

        void OnSlotClicked(int slotIndex)
        {
            // 单击切换选中：再点同一个槽位取消选中
            SetSelectedSlot(_selectedSlotIndex == slotIndex ? -1 : slotIndex);
        }

        static int ClampStars(int level)
        {
            if (level < 0) return 0;
            if (level > 3) return 3;
            return level;
        }
    }
}
