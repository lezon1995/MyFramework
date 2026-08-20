using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 背包系统 —— 球背包 + 遗物背包。
    /// 继承 FrameSystem，由 GameHotFix.initFrameSystem 注册。
    /// </summary>
    public class InventorySystem : PlayerAbility
    {
        [Header("Bag Capacity")]
        [Tooltip("球背包默认格数")]
        public int BallBagCapacity = 6;

        [Tooltip("遗物背包默认格数")]
        public int RelicBagCapacity = 6;

        [Header("Expansion Cap")]
        [Tooltip("球背包容量上限")]
        public int MaxBallBagCapacity = 6;

        [Tooltip("遗物背包容量上限")]
        public int MaxRelicBagCapacity = 6;
        
        BallBag _ballBag;
        RelicBag _relicBag;

        public BallBag BallBag => _ballBag;
        public RelicBag RelicBag => _relicBag;

        bool _systemReadyRaised;

        protected override void Initialization()
        {
            base.Initialization();

            _ballBag = new(_player, BallBagCapacity, MaxBallBagCapacity);
            _relicBag = new(_player, RelicBagCapacity, MaxRelicBagCapacity);

            // 把背包变更桥接到 InventoryEvents，便于跨模块订阅。
            _ballBag.OnItemAdded += InventoryEvents.RaiseBallAdded;
            _ballBag.OnItemRemoved += InventoryEvents.RaiseBallRemoved;
            _ballBag.OnBagChanged += InventoryEvents.RaiseBallBagChanged;
            _ballBag.OnSlotChanged = _player.OnBallInventorySlotChanged;

            _relicBag.OnItemAdded += InventoryEvents.RaiseRelicAdded;
            _relicBag.OnItemRemoved += InventoryEvents.RaiseRelicRemoved;
            _relicBag.OnBagChanged += InventoryEvents.RaiseRelicBagChanged;

            if (!_systemReadyRaised)
            {
                _systemReadyRaised = true;
                InventoryEvents.RaiseSystemReady(this);
            }
        }

        protected override void OnDestroy()
        {
            if (_systemReadyRaised)
            {
                _systemReadyRaised = false;
                InventoryEvents.RaiseSystemDestroy(this);
            }
            _ballBag = null;
            _relicBag = null;
        }

        // ---------------- 便利方法 ----------------

        public bool CanAddBall() => _ballBag != null && _ballBag.CanAdd();
        public bool CanAddRelic() => _relicBag != null && _relicBag.CanAdd();

        public bool AddBall(BallItem ball)
        {
            if (_ballBag == null || ball == null) 
                return false;

            try
            {
                _ballBag.Add(ball);
                return true;
            }
            catch (InventoryFullException)
            {
                return false;
            }
        }
        
        public bool AddRelic(RelicDef def)
        {
            if (_relicBag == null || def == null) 
                return false;

            RelicItem item = null;
            try
            {
                item = RelicService.CreateItem(def);
                _relicBag.Add(item);
                return true;
            }
            catch (InventoryFullException)
            {
                RelicItem.Release(item);
                return false;
            }
        }

        public bool AddRelic(RelicItem item)
        {
            if (_relicBag == null || item == null) 
                return false;

            try
            {
                _relicBag.Add(item);
                return true;
            }
            catch (InventoryFullException)
            {
                RelicItem.Release(item);
                return false;
            }
        }

        public void ExpandBallBag(int delta) => _ballBag?.Expand(delta);
        public void ExpandRelicBag(int delta) => _relicBag?.Expand(delta);

        public void RemoveBall(BallItem b) => _ballBag?.Remove(b);
        public void RemoveRelic(RelicItem r) => _relicBag?.Remove(r);
    }
}