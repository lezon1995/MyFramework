using System;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// Base item class, to use when your object doesn't do anything special
    /// </summary>
    [CreateAssetMenu(fileName = "BaseItem", menuName = "MoreMountains/InventoryEngine/BaseItem", order = 0)]
    [Serializable]
    public class BaseItem : InventoryItem
    {
    }
}