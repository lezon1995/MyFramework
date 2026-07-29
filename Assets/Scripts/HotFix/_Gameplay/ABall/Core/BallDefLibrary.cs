using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 球 def 注册中心 —— 不依赖 FrameSystem，保留为一个静态字典。
    /// 启动入口（GameHotFix / BallManagementSystem）把所有 BallDef 注册进来，业务侧按 Type 取。
    /// </summary>
    public sealed class BallDefLibrary
    {
        static BallDefLibrary sInstance;

        public static BallDefLibrary Instance => sInstance ??= new BallDefLibrary();

        readonly Dictionary<BallType, BallDef> _defs = new();

        public void Register(BallDef def)
        {
            if (def == null)
                return;

            if (_defs.ContainsKey(def.Type))
            {
                logWarning($"BallDefLibrary: duplicate BallType {def.Type}");
                return;
            }

            _defs.Add(def.Type, def);
        }

        public void RegisterAll(IEnumerable<BallDef> defs)
        {
            if (defs == null) return;
            foreach (var d in defs)
                Register(d);
        }

        public BallDef Get(BallType type)
        {
            _defs.TryGetValue(type, out var def);
            if (def == null)
                logError($"BallDefLibrary: missing def for BallType {type}");
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
}
