using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A loot table helper that can be used to randomly pick objects out of a weighted list
    /// This design pattern was described in more details by Daniel Cook in 2014 in his blog :
    /// https://lostgarden.home.blog/2014/12/08/loot-drop-tables/
    ///
    /// This generic LootTable defines a list of objects to loot, each of them weighted.
    /// The weights don't have to add to a certain number, they're relative to each other.
    /// The ComputeWeights method determines, based on these weights, the chance percentage of each object to be picked
    /// The GetLoot method returns one object, picked randomly from the table
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class MMLootTable<T, V> where T : MMLoot<V>
    {
        /// the list of objects that have a chance of being returned by the table
        public List<T> ObjectsToLoot;

        /// the total amount of weights, for debug purposes only
        [Header("Debug")]
        [MMReadOnly]
        public float WeightsTotal;

        protected float _maximumWeightSoFar;
        protected bool _weightsComputed;

        /// <summary>
        /// Determines, for each object in the table, its chance percentage, based on the specified weights
        /// </summary>
        public virtual void ComputeWeights()
        {
            if (ObjectsToLoot == null)
                return;

            if (ObjectsToLoot.Count == 0)
                return;

            _maximumWeightSoFar = 0f;

            foreach (T item in ObjectsToLoot)
            {
                if (item.Weight >= 0f)
                {
                    item.RangeFrom = _maximumWeightSoFar;
                    _maximumWeightSoFar += item.Weight;
                    item.RangeTo = _maximumWeightSoFar;
                }
                else
                {
                    item.Weight = 0f;
                }
            }

            WeightsTotal = _maximumWeightSoFar;

            foreach (T item in ObjectsToLoot)
            {
                item.ChancePercentage = item.Weight / WeightsTotal * 100;
            }

            _weightsComputed = true;
        }

        /// <summary>
        /// Returns one object from the table, picked randomly
        /// </summary>
        /// <returns></returns>
        public virtual T GetLoot()
        {
            if (ObjectsToLoot == null)
                return null;

            if (ObjectsToLoot.Count == 0)
                return null;

            if (!_weightsComputed)
            {
                ComputeWeights();
            }

            float index = Random.Range(0, WeightsTotal);

            foreach (T item in ObjectsToLoot)
            {
                if (index > item.RangeFrom && index < item.RangeTo)
                {
                    return item;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// A MMLootTable implementation for GameObjects
    /// </summary>
    [Serializable]
    public class MMLootTableGameObject : MMLootTable<MMLootGameObject, GameObject>
    {
    }

    /// <summary>
    /// A MMLootTable implementation for floats
    /// </summary>
    [Serializable]
    public class MMLootTableFloat : MMLootTable<MMLootFloat, float>
    {
    }

    /// <summary>
    /// A MMLootTable implementation for strings
    /// </summary>
    [Serializable]
    public class MMLootTableString : MMLootTable<MMLootString, string>
    {
    }
}