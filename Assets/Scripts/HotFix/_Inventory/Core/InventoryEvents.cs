using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// Inventory events are used throughout the Inventory Engine to let other interested classes know that something happened to an inventory.  
    /// </summary>
    public struct InventoryEvent
    {
        public Inventory.Events Events;
        public InventoryItem Item;
        public InventorySlot Slot;
        public string InventoryName;
        public int Quantity;
        public int Index;
        public string PlayerID;

        public InventoryEvent(Inventory.Events events, InventorySlot slot, string inventoryName, InventoryItem item, int quantity, int index, string playerID)
        {
            Events = events;
            Slot = slot;
            InventoryName = inventoryName;
            Item = item;
            Quantity = quantity;
            Index = index;
            PlayerID = playerID != "" ? playerID : "Player1";
        }

        static InventoryEvent e;

        public static void Trigger(Inventory.Events events, InventorySlot slot, string inventoryName, InventoryItem item, int quantity, int index, string playerID)
        {
            e.Events = events;
            e.Slot = slot;
            e.InventoryName = inventoryName;
            e.Item = item;
            e.Quantity = quantity;
            e.Index = index;
            e.PlayerID = playerID != "" ? playerID : "Player1";
            MMEventManager.TriggerEvent(e);
        }

        public static void Trigger(Inventory.Events events, string inventoryName, string playerID)
        {
            e.Events = events;
            e.Slot = null;
            e.InventoryName = inventoryName;
            e.Item = null;
            e.Quantity = 0;
            e.Index = 0;
            e.PlayerID = playerID != "" ? playerID : "Player1";
            MMEventManager.TriggerEvent(e);
        }
    }
}