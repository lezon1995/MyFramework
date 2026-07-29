using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    // 角色管理器
    public class BallManager : MainManagerBehaviour
    {
        [SerializeField]
        List<BallDef> Defs = new();

        Dictionary<BallType, BallDef> defDict = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            defDict.Clear();
            foreach (var def in Defs)
            {
                defDict[def.Type] = def;
            }
        }

        public bool getDef(BallType type, out BallDef def)
        {
            return defDict.TryGetValue(type, out def);
        }
        public BallDef getDef(BallType type)
        {
            defDict.TryGetValue(type, out var def);
            return def;
        }
        
        public List<BallDef> getDefs()
        {
            return Defs;
        }
    }
}