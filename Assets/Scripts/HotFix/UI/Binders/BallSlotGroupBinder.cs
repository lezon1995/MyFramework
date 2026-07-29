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
    /// 槽位间交换 / 拖拽到 sellZone 由你后续在框架层（drag system）加入，binder 只暴露 API。
    /// </summary>
    public sealed class BallSlotGroupBinder
    {
        BallSlotGroupView _view;
        BallSlotGroup _model;
        int _selectedSlotIndex = -1;

        public BallSlotGroupBinder(BallSlotGroupView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public BallSlotGroup Model => _model;
        public int SelectedSlotIndex => _selectedSlotIndex;
        public event Action<int /*slotIndex*/> SelectionChanged;

        public void Attach(BallSlotGroup model)
        {
            if (_model != null) 
                Detach();

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
            if (_selectedSlotIndex == index) 
                return;

            _selectedSlotIndex = index;
            UpdateSelectionVisuals();
            SelectionChanged?.Invoke(_selectedSlotIndex);
        }

        void OnModelChanged() => Rebuild();

        public void Rebuild()
        {
            if (_model == null) 
                return;

            var slots = new List<BallSlot>(_model.Slots);

            _view.BuildSlots(slots, (item, slot) =>
            {
                var ball = slot.Current;
                item.SetIconVisible(ball != null);
                item.SetStarCount(ball != null ? ClampStars(ball.Level) : 0);
                item.SetSelected(_selectedSlotIndex == slot.Index);
                item.SetOnClick(() => OnSlotClicked(slot.Index));
            });
        }

        void UpdateSelectionVisuals()
        {
            if (_model == null) 
                return;

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
            if (level < 0) 
                return 0;
            if (level > 3) 
                return 3;
            return level;
        }
    }
}