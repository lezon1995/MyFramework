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

    [CreateAssetMenu(fileName = "BallMergeRecipe", menuName = "BallMergeRecipe", order = 0)]
    public class BallMergeRecipe : ScriptableObject
    {
        public List<MergeRecipe> Recipes = new();
    }
}