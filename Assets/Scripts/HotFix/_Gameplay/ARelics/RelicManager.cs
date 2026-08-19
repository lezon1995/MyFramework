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
        Dictionary<string, RelicDef> defDict2 = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            defDict.Clear();
            foreach (var def in Defs)
            {
                defDict[def.Type] = def;
            }

            defDict2.Clear();
            foreach (var def in Defs)
            {
                defDict2[def.RelicName] = def;
            }
        }

        public void refreshRelicSeenStatus()
        {
            RelicLibrary.seenRelics = 0;
            foreach (var def in Defs)
            {
                if (UnlockTracker.isRelicSeen(def.RelicName))
                    RelicLibrary.seenRelics++;
                def.isSeen = UnlockTracker.isRelicSeen(def.RelicName); 
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
        
        public RelicDef getDef(string relicName)
        {
            defDict2.TryGetValue(relicName, out var def);
            return def;
        }
        
        public List<RelicDef> getDefs()
        {
            return Defs;
        }
    }
}