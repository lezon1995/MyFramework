using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    // 角色管理器
    public class RelicManager : MainManagerBehaviour
    {
        [SerializeField]
        List<RelicDef> Defs = new();

        Dictionary<RelicType, RelicDef> defDict = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            defDict.Clear();
            foreach (var def in Defs)
            {
                defDict[def.Type] = def;
            }
        }

        public bool getDef(RelicType type, out RelicDef def)
        {
            return defDict.TryGetValue(type, out def);
        }
        public RelicDef getDef(RelicType type)
        {
            defDict.TryGetValue(type, out var def);
            return def;
        }
        
        public List<RelicDef> getDefs()
        {
            return Defs;
        }
    }
}