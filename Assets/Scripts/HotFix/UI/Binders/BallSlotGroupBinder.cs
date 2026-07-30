using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 球槽位组 binder —— 把 BallSlotGroup 的状态同步到 BallSlotGroupView。
    ///
    /// 槽位 item 也有 Btn 字段，item 自身在 init() 时一次性订阅 UnityEvent,转发走字段,
    /// 所以 Rebuild 不再创建 lambda,只更新 item 的数据(slot 索引)。
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

            // 直接把 model 的 Slots 列表交给 View,binder 不在中间建一个 List<BallSlot>。
            _view.BuildSlotsWithIndex(_model.Slots, (index, item, slot) =>
            {
                var ball = slot.Item;
                var isOccupied = slot.IsOccupied;
                item.SetIconVisible(isOccupied);
                if (isOccupied)
                {
                    item.SetBallIcon(ball.Def.Icon);
                }

                item.SetStarCount(isOccupied ? ClampStars(ball.Level) : 0);
                item.SetSelected(_selectedSlotIndex == slot.Index);

                // 不创建 lambda;item 的 UnityEvent 在 init() 中已一次性订阅,
                // 这里只更新数据字段,转发走 item 自身的 onBtnClick / onDragReleased。
                item.SetSlotData(index, this);
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

        // ------------- item 事件转发入口(item 直接调过来,无 lambda 中转)-------------

        /// <summary>由 BallSlotItem.onBtnClick 转发。</summary>
        public void OnSlotBtnClicked(int slotIndex)
        {
            // 单击切换选中:再点同一个槽位取消选中
            SetSelectedSlot(_selectedSlotIndex == slotIndex ? -1 : slotIndex);
        }

        /// <summary>由 BallSlotItem.onDragReleased 转发。
        /// 这里只取 slot 索引,具体 ball 通过该 slot 实时读出,避免在 Rebuild 时持有 ball 引用。</summary>
        public void OnSlotDragReleased(BallSlotItem src, int slotIndex, UIDragReleaseEventData data)
        {
            if (_model == null) 
                return;

            if (slotIndex < 0 || slotIndex >= _model.Slots.Count) 
                return;

            var slot = _model.Slots[slotIndex];
            var ball = slot.Item;
            if (ball == null) 
                return;

            _owner?.OnSlotDragReleased(src, slotIndex, ball, data);
        }

        static int ClampStars(int level)
        {
            if (level < 0) return 0;
            if (level > 3) return 3;
            return level;
        }
    }
}