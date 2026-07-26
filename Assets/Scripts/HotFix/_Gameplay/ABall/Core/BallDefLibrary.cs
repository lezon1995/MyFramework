/*using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 球 def 注册中心 —— 系统启动时一次性把所有 BallDef 注册进来。
    /// 业务侧只用 Get(int id) 取，O(1)。
    /// </summary>
    public sealed class BallDefLibrary : FrameSystem
    {
        public static BallDefLibrary Instance { get; private set; }

        Dictionary<BallType, BallDef> _defs = new();

        public override void init()
        {
            base.init();
            Instance = this;
        }

        public override void willDestroy()
        {
            base.willDestroy();
            if (Instance == this) 
                Instance = null;
        }

        public void Register(BallDef def)
        {
            if (def == null || def.BallDefId <= 0) 
                return;

            if (_defs.ContainsKey(def.Type))
            {
                logWarning($"BallDefLibrary: duplicate id {def.BallDefId}");
                return;
            }

            _defs.Add(def.Type, def);
        }

        public void RegisterAll(BallDef[] defs)
        {
            if (defs == null) 
                return;

            foreach (var d in defs) 
                Register(d);
        }

        public BallDef Get(BallType type)
        {
            _defs.TryGetValue(type, out var def);
            if (def == null) 
                logError($"BallDefLibrary: missing def id {type}");
            return def;
        }

        public bool TryGet(BallType type, out BallDef def)
        {
            return _defs.TryGetValue(type, out def);
        }

        public IEnumerable<BallDef> All => _defs.Values;

        public void Clear()
        {
            _defs.Clear();
        }
    }
}*/