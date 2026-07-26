using UnityEngine;
using static FrameBaseUtility;

namespace MoreMountains
{
    /// <summary>
    /// 背包系统 —— 球背包 + 遗物背包。
    /// 继承 FrameSystem，由 GameHotFix.initFrameSystem 注册。
    /// </summary>
    public class InventorySystem : FrameSystem
    {
        public static InventorySystem Instance { get; private set; }

        BallBag  _ballBag;
        RelicBag _relicBag;

        public BallBag  BallBag  => _ballBag;
        public RelicBag RelicBag => _relicBag;

        public int BallBagCapacity  => _ballBag?.Capacity  ?? 0;
        public int RelicBagCapacity => _relicBag?.Capacity ?? 0;

        public override void init()
        {
            base.init();
            Instance = this;

            var cfg = InventorySystemConfig.Instance;
            if (cfg == null)
            {
                logError("InventorySystem: missing InventorySystemConfig asset.");
                return;
            }

            _ballBag  = new BallBag (cfg.BallBagCapacity,  cfg.MaxBallBagCapacity);
            _relicBag = new RelicBag(cfg.RelicBagCapacity, cfg.MaxRelicBagCapacity);

            // 把背包变更桥接到 InventoryEvents，便于跨模块订阅。
            _ballBag .OnItemAdded   += item => InventoryEvents.RaiseBallAdded(item);
            _ballBag .OnItemRemoved += item => InventoryEvents.RaiseBallRemoved(item);
            _ballBag .OnBagChanged  += ()    => InventoryEvents.RaiseBallBagChanged();

            _relicBag.OnItemAdded   += item => InventoryEvents.RaiseRelicAdded(item);
            _relicBag.OnItemRemoved += item => InventoryEvents.RaiseRelicRemoved(item);
            _relicBag.OnBagChanged  += ()    => InventoryEvents.RaiseRelicBagChanged();

            InventoryEvents.RaiseSystemReady(this);
        }

        public override void willDestroy()
        {
            base.willDestroy();
            InventoryEvents.RaiseSystemDestroy(this);
            if (Instance == this) Instance = null;
            _ballBag  = null;
            _relicBag = null;
        }

        // ---------------- 便利方法 ----------------

        public bool CanAddBall()  => _ballBag  != null && _ballBag.CanAdd();
        public bool CanAddRelic() => _relicBag != null && _relicBag.CanAdd();

        public bool AddBall(BallInstance ball)
        {
            if (_ballBag == null || ball == null) return false;
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

        public bool AddRelic(RelicItem relic)
        {
            if (_relicBag == null || relic == null) return false;
            try
            {
                _relicBag.Add(relic);
                return true;
            }
            catch (InventoryFullException)
            {
                return false;
            }
        }

        public void ExpandBallBag (int delta) => _ballBag ?.Expand(delta);
        public void ExpandRelicBag(int delta) => _relicBag?.Expand(delta);

        public void RemoveBall (BallInstance b)  => _ballBag ?.Remove(b);
        public void RemoveRelic(RelicItem r)      => _relicBag?.Remove(r);
    }
}
