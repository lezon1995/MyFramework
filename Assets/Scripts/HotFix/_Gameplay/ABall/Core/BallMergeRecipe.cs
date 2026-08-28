using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    [Serializable]
    public struct MergeRecipe
    {
        public BallType MergedBallType;

        public List<BallType> ComposedBallTypes;
    }

    [Serializable]
    public struct ComposedBallTypeKey : IEquatable<ComposedBallTypeKey>
    {
        public BallType t1, t2;

        public ComposedBallTypeKey(BallType _t1, BallType _t2)
        {
            t1 = _t1;
            t2 = _t2;
        }

        public bool Equals(ComposedBallTypeKey other)
        {
            return (t1 == other.t1 && t2 == other.t2) || (t1 == other.t2 && t2 == other.t1);
        }

        public override bool Equals(object obj)
        {
            return obj is ComposedBallTypeKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            // 关键：保证 t1/t2 互换时哈希也相同
            return t1 <= t2
                ? HashCode.Combine((int)t1, (int)t2)
                : HashCode.Combine((int)t2, (int)t1);
        }

        public static bool operator ==(ComposedBallTypeKey a, ComposedBallTypeKey b) => a.Equals(b);
        public static bool operator !=(ComposedBallTypeKey a, ComposedBallTypeKey b) => !a.Equals(b);
    }

    [CreateAssetMenu(fileName = "BallMergeRecipe", menuName = "BallMergeRecipe", order = 0)]
    public class BallMergeRecipe : ScriptableObject
    {
        public List<MergeRecipe> Recipes = new();
    }
}