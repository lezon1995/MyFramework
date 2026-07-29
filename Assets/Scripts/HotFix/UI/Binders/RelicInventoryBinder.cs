using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 遗物背包 binder —— 把 RelicBag 同步到 RelicInventoryView。
    /// 行为与 BallInventoryBinder 一致：单击选中，操作委托给 OperationPanelBinder。
    /// </summary>
        public sealed class RelicInventoryBinder
    {
        readonly RelicInventoryView _view;
        RelicBag _bag;

        RelicItem _selected;

        public RelicInventoryBinder(RelicInventoryView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public RelicItem SelectedRelic => _selected;

        public event Action<RelicItem /*selected*/> RelicSelected;
        public event Action<RelicItem> SellRequested;

        public void Attach(RelicBag bag)
        {
            if (_bag != null) Detach();
            _bag = bag ?? throw new ArgumentNullException(nameof(bag));
            _view.SetTitle("RELICS");
            _bag.OnItemAdded += OnBagItemAdded;
            _bag.OnItemRemoved += OnBagItemRemoved;
            _bag.OnBagChanged += OnBagAnyChanged;
            Rebuild();
        }

        public void Detach()
        {
            if (_bag == null) return;
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
            if (_bag == null) return;
            var items = new List<RelicItem>(_bag.AllItems);

            _view.BuildRelics(items, (item, relic) =>
            {
                item.SetSelected(ReferenceEquals(relic, _selected));
                item.SetIconVisible(relic != null);
                item.SetEnabled(true);
                item.SetOnClick(() => OnRelicClicked(relic));
            });
        }

        void OnRelicClicked(RelicItem relic)
        {
            if (relic == null) return;
            _selected = ReferenceEquals(_selected, relic) ? null : relic;
            UpdateSelectionVisuals();
            if (_selected != null) RelicSelected?.Invoke(_selected);
        }

        void UpdateSelectionVisuals()
        {
            if (_bag == null) return;
            int i = 0;
            foreach (var relic in _bag.AllItems)
            {
                if (_view.GetUsedItem(i, out var item))
                {
                    item.SetSelected(ReferenceEquals(relic, _selected));
                }
                i++;
            }
        }

        public void RequestSellSelected()
        {
            if (_selected == null) return;
            SellRequested?.Invoke(_selected);
        }

        public void ClearSelection() => _selected = null;
    }
}
