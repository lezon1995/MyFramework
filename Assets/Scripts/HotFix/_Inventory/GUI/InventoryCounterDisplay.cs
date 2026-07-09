using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// Use this class to display the total quantity of one or more given items in one or more given inventories
    /// </summary>
    public class InventoryCounterDisplay : MonoBehaviour, IEvent<InventoryEvent>
    {
        // a list of the items to count
        [Header("Items & Inventories")]
        public List<InventoryItem> Item;

        /// the inventories in which to count the items
        public List<Inventory> TargetInventories;

        // the text UI to update with the total quantity of the item(s) in the target inventories
        [Header("Display")]
        public Text TargetText;

        /// the format to apply to the text
        public string DisplayFormat = "0";

        /// <summary>
        /// A public method used to update the target text with the total quantity of the item(s) in the target inventories
        /// </summary>
        public void UpdateText()
        {
            TargetText.text = ComputeQuantity().ToString(DisplayFormat);
        }

        /// <summary>
        /// Handles inventory events, updates the text if needed
        /// </summary>
        /// <param name="e"></param>
        public virtual void onEvent(InventoryEvent e)
        {
            if (!ShouldUpdate(e.InventoryName))
                return;

            switch (e.Events)
            {
                case Inventory.Events.ContentChanged:
                    UpdateText();
                    break;
                case Inventory.Events.InventoryLoaded:
                    UpdateText();
                    break;
            }
        }

        /// <summary>
        /// Computes the quantity of the item in the target inventories
        /// </summary>
        /// <returns></returns>
        protected virtual int ComputeQuantity()
        {
            int count = 0;
            foreach (Inventory inventory in TargetInventories)
            {
                foreach (InventoryItem item in Item)
                {
                    count += inventory.GetQuantity(item.ItemID);
                }
            }

            return count;
        }

        /// <summary>
        /// Returns true if the inventory name passed in parameters is one of the target inventories, false otherwise
        /// </summary>
        /// <param name="inventoryName"></param>
        /// <returns></returns>
        protected virtual bool ShouldUpdate(string inventoryName)
        {
            bool shouldUpdate = false;
            foreach (Inventory inventory in TargetInventories)
            {
                if (inventory.name == inventoryName)
                {
                    shouldUpdate = true;
                }
            }

            return shouldUpdate;
        }

        /// <summary>
        /// On Enable, we start listening for MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
        {
            this.addListener<InventoryEvent>();
        }

        /// <summary>
        /// On Disable, we stop listening for MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
        {
            this.removeListener<InventoryEvent>();
        }
    }
}