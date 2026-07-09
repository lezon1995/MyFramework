using System;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// Serialized class to help store / load inventories from files.
    /// </summary>
    [Serializable]
    public class SerializedInventory
    {
        public int NumberOfRows;
        public int NumberOfColumns;
        public string InventoryName = "Inventory";
        public Inventory.Types InventoryType;
        public bool DrawContentInInspector;
        public string[] ContentType;
        public int[] ContentQuantity;
    }
}