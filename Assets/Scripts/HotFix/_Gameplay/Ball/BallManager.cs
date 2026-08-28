using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    // 角色管理器
    public class BallManager : MainManagerBehaviour
    {
        [SerializeField]
        List<BallDef> Defs = new();

        [SerializeField]
        List<MergedBallDef> MergedDefs = new();

        [SerializeField]
        BallMergeRecipe BallMergeRecipe;

        List<BallDef> AllDefs = new();
        Dictionary<BallType, BallDef> defDict = new();
        Dictionary<ComposedBallTypeKey, BallType> mergedDict = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            defDict.Clear();
            foreach (var def in Defs)
                defDict[def.Type] = def;

            foreach (var def in MergedDefs)
                defDict[def.Type] = def;
            
            AllDefs.AddRange(Defs);
            AllDefs.AddRange(MergedDefs);

            mergedDict.Clear();
            foreach (var recipe in BallMergeRecipe.Recipes)
            {
                var types = recipe.ComposedBallTypes;
                mergedDict[new(types[0], types[1])] = recipe.MergedBallType;
            }
            
            foreach (var mergedDef in MergedDefs)
            {
                var types = mergedDef.BuildTypes;
                mergedDict[new(types[0], types[1])] = mergedDef.Type;
            }
        }

        public bool getDef(BallType type, out BallDef def)
        {
            return defDict.TryGetValue(type, out def);
        }

        public bool containsDef(BallType type)
        {
            return defDict.ContainsKey(type);
        }

        public BallDef getDef(BallType type)
        {
            defDict.TryGetValue(type, out var def);
            return def;
        }

        public List<BallDef> getDefs()
        {
            return AllDefs;
        }
        
        public bool containsMergedDef(BallType t1, BallType t2)
        {
            if (mergedDict.TryGetValue(new(t1, t2), out var mergedBallType))
            {
                if (containsDef(mergedBallType))
                {
                    return true;
                }
            }

            return false;
        }

        public bool tryGetMergedDef(BallType t1, BallType t2, out BallDef def)
        {
            def = null;
            if (mergedDict.TryGetValue(new(t1, t2), out var mergedBallType))
            {
                if (getDef(mergedBallType, out def))
                {
                    return true;
                }
            }

            return false;
        }
    }
}