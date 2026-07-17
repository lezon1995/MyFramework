using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    [Serializable]
    public class Inventory : MonoBehaviour,
        IEvent<InventoryEvent>,
        IEvent<MMGameEvent>
    {
        public enum Events
        {
            Pick,
            Select,
            Click,
            Move,
            Use,
            ItemUsed,
            Equip,
            ItemEquipped,
            UnEquip,
            ItemUnEquipped,
            Drop,
            Destroy,
            Error,
            Redraw,
            ContentChanged,
            InventoryOpens,
            InventoryCloseRequest,
            InventoryCloses,
            InventoryLoaded
        }

        public static Dictionary<string, Inventory> Inventories;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void InitializeStatics() => Inventories = null;

        public enum Types
        {
            Main,
            Equipment
        }

        public string Name;
        public string PlayerID = "Player1";
        public int DefaultSize = 20;
        public int Size => Content.Length;

        [ShowInInspector, ReadOnly]
        public InventoryItem[] Content { get; set; } = Array.Empty<InventoryItem>();

        public InventoryItem this[int index]
        {
            get => Content.Get(index);
            set
            {
                var newItem = value;
                var oldItem = this[index];
                if (newItem == oldItem)
                    return;

                Content.Set(index, newItem);
                if (oldItem) 
                    oldItem.RemoveFrom(this, PlayerID);

                if (newItem) 
                    newItem.AddTo(this, PlayerID);
            }
        }

        [Tooltip("Here you can define your inventory's type. Main are 'regular' inventories. Equipment inventories will be bound to a certain item class and have dedicated options.")]
        public Types InventoryType;

        [Tooltip("The TargetTransform is any transform in your scene at which objects dropped from the inventory will spawn.")]
        public Transform TargetTransform;

        [Title("Persistence")]
        public bool Persistent = true;

        public bool ResetThisInventorySaveOnStart;

        [Title("Debug")]
        public bool DrawContentInInspector;

        public virtual GameObject Owner { get; set; }
        public virtual int FreeCount => Size - FilledCount;
        public virtual bool IsFull => FreeCount <= 0;

        public virtual int FilledCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < Size; i++)
                {
                    if (!this[i].IsNull())
                        count++;
                }

                return count;
            }
        }


        public static string _resourceItemPath = "Items/";
        public static string _saveFolderName = "InventoryEngine/";
        public static string _saveFileExtension = ".inventory";

        public static Inventory Get(string inventoryName, string playerID)
        {
            if (inventoryName == null)
                return null;

            if (Inventories.TryGetValue(inventoryName, out var inventory))
            {
                if (inventory.PlayerID == playerID)
                    return inventory;
            }

            return null;
        }

        protected virtual void Awake()
        {
            Content = new InventoryItem[DefaultSize];
            RegisterInventory();
        }

        protected virtual void RegisterInventory()
        {
            Inventories ??= new();
            Inventories[name] = this;
        }

        public void SetOwner(GameObject owner) => Owner = owner;

        public void SetTarget(Transform target) => TargetTransform = target;

        #region Add

        public bool AddItem(InventoryItem item)
        {
            if (item.ForceSlotIndex)
                return AddItemAt(item, item.TargetIndex, 1);

            return AddItem(item, 1);
        }

        /// <summary>
        /// Tries to add an item of the specified type. Note that this is name based.
        /// </summary>
        /// <returns><c>true</c>, if item was added, <c>false</c> if it couldn't be added (item null, inventory full).</returns>
        public virtual bool AddItem(InventoryItem item, int quantity)
        {
            // if the item to add is null, we do nothing and exit
            if (item.IsNull())
            {
                Debug.LogWarning(name + " : The item you want to add to the inventory is null");
                return false;
            }

            quantity = GetMaxQuantity(item, quantity);

            // if there's at least one item like this already in the inventory and it's stackable
            var maxStack = item.MaximumStack;
            if (maxStack > 1)
            {
                for (int i = 0; i < Size; i++)
                {
                    var t = this[i];

                    if (t.IsNull())
                        continue;

                    if (t.ItemID != item.ItemID)
                        continue;

                    // if there's still room in one of these items of this kind in the inventory, we add to it
                    if (t.Quantity < maxStack)
                    {
                        // we increase the quantity of our item
                        t.Quantity += quantity;
                        // if this exceeds the maximum stack
                        var maxStackT = t.MaximumStack;
                        if (t.Quantity > maxStackT)
                        {
                            int exceedCount = t.Quantity - maxStackT;
                            // we clamp the quantity and add the rest as a new item
                            t.Quantity = maxStackT;
                            AddItem(item, exceedCount);
                        }

                        InventoryEvent.Trigger(Events.ContentChanged, name, PlayerID);
                        return true;
                    }
                }
            }

            // if we've reached the max size of our inventory, we don't add the item
            if (FilledCount >= Size)
                return false;

            while (quantity > 0)
            {
                if (quantity > maxStack)
                {
                    AddItem(item, maxStack);
                    quantity -= maxStack;
                }
                else
                {
                    AddItemToArray(item, quantity);
                    quantity = 0;
                }
            }

            // if we're still here, we add the item in the first available slot
            InventoryEvent.Trigger(Events.ContentChanged, name, PlayerID);
            return true;
        }

        /// <summary>
        /// Adds the specified quantity of the specified item to the inventory, at the destination index of choice
        /// </summary>
        public virtual bool AddItemAt(InventoryItem item, int quantity, int index)
        {
            var tempQuantity = GetMaxQuantity(item, quantity);

            var indexItem = this[index];
            if (!indexItem.IsNull())
            {
                if (indexItem.ItemID != item.ItemID)
                    return false;

                if (indexItem.MaximumStack <= 1)
                    return false;

                tempQuantity += indexItem.Quantity;
            }

            tempQuantity = Mathf.Clamp(tempQuantity, 0, item.MaximumStack);

            var copiedItem = item.Copy();
            copiedItem.Quantity = tempQuantity;
            this[index] = copiedItem;

            // if we're still here, we add the item in the first available slot
            InventoryEvent.Trigger(Events.ContentChanged, name, PlayerID);
            return true;
        }

        protected virtual bool AddItemToArray(InventoryItem item, int quantity)
        {
            if (FreeCount == 0)
                return false;

            for (int i = 0; i < Size; i++)
            {
                if (this[i].IsNull())
                {
                    var copiedItem = item.Copy();
                    copiedItem.Quantity = quantity;
                    this[i] = copiedItem;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Move

        /// <summary>
        /// Tries to move the item at the first parameter slot to the second slot
        /// </summary>
        /// <returns><c>true</c>, if item was moved, <c>false</c> otherwise.</returns>
        public virtual bool MoveItem(int startIndex, int endIndex)
        {
            // if what we're trying to move is null, this means we're trying to move an empty slot
            var startItem = this[startIndex];
            if (startItem.IsNull())
            {
                Debug.LogWarning("InventoryEngine : you're trying to move an empty slot.");
                return false;
            }

            // if both objects are swappable, we'll swap them
            var endItem = this[endIndex];
            // if the target slot is empty
            if (endItem.IsNull())
            {
                // we create a copy of our item to the destination
                this[endIndex] = startItem.Copy();
                return DestroyItem(startIndex);
            }

            // we swap our items
            InventoryItem tempItem = endItem.Copy();
            this[endIndex] = startItem.Copy();
            this[startIndex] = tempItem;
            InventoryEvent.Trigger(Events.ContentChanged, name, PlayerID);
            return true;
        }

        /// <summary>
        /// This method lets you move the item at startIndex to the chosen targetInventory, at an optional endIndex there
        /// </summary>
        public virtual bool MoveItemToInventory(int startIndex, Inventory inventory, int endIndex = -1)
        {
            // if what we're trying to move is null, this means we're trying to move an empty slot
            var moveItem = this[startIndex];
            if (moveItem.IsNull())
            {
                Debug.LogWarning("InventoryEngine : you're trying to move an empty slot.");
                return false;
            }

            // if our destination isn't empty, we exit too
            if (endIndex >= 0 && !inventory[endIndex].IsNull())
            {
                Debug.LogWarning("InventoryEngine : the destination slot isn't empty, can't move.");
                return false;
            }

            var item = moveItem.Copy();

            // if we've specified a destination index, we use it, otherwise we add normally
            if (endIndex >= 0)
                inventory.AddItemAt(item, endIndex, item.Quantity);
            else
                inventory.AddItem(item, item.Quantity);

            // we then remove from the original inventory
            RemoveItem(startIndex, item.Quantity);

            return true;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes the specified item from the inventory.
        /// </summary>
        /// <returns><c>true</c>, if item was removed, <c>false</c> otherwise.</returns>
        public virtual bool RemoveItem(int index, int quantity)
        {
            if (index < 0 || index >= Size)
            {
                Debug.LogWarning("InventoryEngine : you're trying to remove an item from an invalid index.");
                return false;
            }

            var item = this[index];
            if (item.IsNull())
            {
                Debug.LogWarning("InventoryEngine : you're trying to remove from an empty slot.");
                return false;
            }

            quantity = Mathf.Max(0, quantity);

            item.Quantity -= quantity;
            if (item.Quantity <= 0)
            {
                return DestroyItem(index);
            }

            InventoryEvent.Trigger(Events.ContentChanged, name, PlayerID);
            return true;
        }

        /// <summary>
        /// Removes the specified quantity of the item matching the specified itemID
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public virtual bool RemoveItemByID(string itemID, int quantity)
        {
            if (quantity < 1)
            {
                Debug.LogWarning("InventoryEngine : you're trying to remove an incorrect quantity (" + quantity + ") from your inventory.");
                return false;
            }

            if (string.IsNullOrEmpty(itemID))
            {
                Debug.LogWarning("InventoryEngine : you're trying to remove an item but itemID hasn't been specified.");
                return false;
            }

            int quantityLeftToRemove = quantity;

            for (int i = 0; i < Size; i++)
            {
                var item = this[i];
                if (item.IsNull())
                    continue;

                if (item.ItemID == itemID)
                {
                    int quantityAtIndex = item.Quantity;
                    RemoveItem(i, quantityLeftToRemove);
                    quantityLeftToRemove -= quantityAtIndex;
                    if (quantityLeftToRemove <= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Search

        public int GetStackCount(string itemID, int maxStack)
        {
            int count = 0;

            for (int i = 0; i < Size; i++)
            {
                var item = this[i];
                if (item.IsNull())
                    count += maxStack;
                else if (item.ItemID == itemID)
                    count += maxStack - item.Quantity;
            }

            return count;
        }

        public virtual int GetQuantity(string itemID)
        {
            int total = 0;
            for (var i = 0; i < Size; i++)
            {
                var item = this[i];
                if (item.IsNull())
                    continue;

                if (item.ItemID == itemID)
                    total += item.Quantity;
            }

            return total;
        }

        public virtual int GetMaxQuantity(InventoryItem item, int quantity)
        {
            return Mathf.Min(quantity, item.MaximumQuantity - GetQuantity(item.ItemID));
        }

        public virtual bool Contains(string itemID)
        {
            for (int i = 0; i < Size; i++)
            {
                if (this[i].IsNull())
                    continue;

                if (this[i].ItemID == itemID)
                    return true;
            }

            return false;
        }

        public virtual void Search(string itemID, ref List<int> list)
        {
            list.Clear();
            for (int i = 0; i < Size; i++)
            {
                if (this[i].IsNull())
                    continue;

                if (this[i].ItemID == itemID)
                    list.Add(i);
            }
        }

        public virtual void Search(ItemClasses searchedClass, ref List<int> list)
        {
            list.Clear();
            for (int i = 0; i < Size; i++)
            {
                if (this[i].IsNull())
                    continue;

                if (this[i].ItemClass == searchedClass)
                    list.Add(i);
            }
        }

        #endregion

        #region Save & Load

        public virtual void SaveInventory()
        {
            var info = new SerializedInventory();
            FillSerializedInventory(info);
            MMSaveLoadManager.Save(info, DetermineSaveName(), _saveFolderName);
        }

        public virtual void LoadInventory()
        {
            var info = MMSaveLoadManager.Load<SerializedInventory>(DetermineSaveName(), _saveFolderName);
            ExtractSerializedInventory(info);
            InventoryEvent.Trigger(Events.InventoryLoaded, null, name, null, 0, 0, PlayerID);
        }

        protected virtual void FillSerializedInventory(SerializedInventory info)
        {
            info.InventoryType = InventoryType;
            info.DrawContentInInspector = DrawContentInInspector;
            info.ContentType = new string[Size];
            info.ContentQuantity = new int[Size];
            for (int i = 0; i < Size; i++)
            {
                var item = this[i];
                if (item.IsNull())
                {
                    info.ContentType[i] = null;
                    info.ContentQuantity[i] = 0;
                }
                else
                {
                    info.ContentType[i] = item.ItemID;
                    info.ContentQuantity[i] = item.Quantity;
                }
            }
        }

        protected virtual void ExtractSerializedInventory(SerializedInventory info)
        {
            if (info == null)
                return;

            InventoryType = info.InventoryType;
            DrawContentInInspector = info.DrawContentInInspector;
            Content = new InventoryItem[info.ContentType.Length];
            for (int i = 0; i < info.ContentType.Length; i++)
            {
                var itemId = info.ContentType[i];
                if (!string.IsNullOrEmpty(itemId))
                {
                    var item = Resources.Load<InventoryItem>(_resourceItemPath + itemId);
                    if (item == null)
                    {
                        Debug.LogError("InventoryEngine : Couldn't find any inventory item to load at Resources/" + _resourceItemPath
                                                                                                                  + " named " + itemId + ". Make sure all your items definitions names (the name of the InventoryItem scriptable " +
                                                                                                                  "objects) are exactly the same as their ItemID string in their inspector. Make sure they are in a  Resources/" + _resourceItemPath + " folder. " +
                                                                                                                  "Once that's done, also make sure you reset all saved inventories as the mismatched names and IDs may have " +
                                                                                                                  "corrupted them.");
                    }
                    else
                    {
                        var copiedItem = item.Copy();
                        copiedItem.Quantity = info.ContentQuantity[i];
                        this[i] = copiedItem;
                    }
                }
                else
                {
                    this[i] = null;
                }
            }
        }

        protected virtual string DetermineSaveName()
        {
            return gameObject.name + "_" + PlayerID + _saveFileExtension;
        }

        public virtual void ResetInventory()
        {
            MMSaveLoadManager.DeleteSave(DetermineSaveName(), _saveFolderName);
        }

        #endregion

        #region Function

        public virtual bool UseItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (item.IsNull())
            {
                Error(index, slot);
                return false;
            }

            if (!item.IsUsable)
                return false;

            if (item.Use(PlayerID))
            {
                var quantity = item.ConsumeQuantity;
                if (quantity > 0)
                    RemoveItem(index, quantity);

                InventoryEvent.Trigger(Events.ItemUsed, slot, name, item.Copy(), 0, index, PlayerID);
            }

            return true;
        }

        public virtual bool UseItem(string itemID)
        {
            for (var i = 0; i < Size; i++)
            {
                var item = this[i];
                if (item.IsNull())
                    continue;

                if (item.ItemID == itemID)
                    return UseItem(item, i);
            }

            return false;
        }

        public virtual void EquipItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryType != Types.Main)
                return;

            InventoryItem oldItem = null;
            if (item.IsNull())
            {
                Error(index, slot);
                return;
            }

            if (!item.IsEquippable)
                return;

            if (!item.GetInventory(PlayerID, out var inventory, Types.Equipment))
            {
                Debug.LogWarning("InventoryEngine Warning : " + this[index].ItemName + "'s target equipment inventory couldn't be found.");
                return;
            }

            // if this is a mono slot inventory, we prepare to swap
            if (inventory.Size == 1)
            {
                var onlyItem = inventory[0];
                if (!onlyItem.IsNull())
                {
                    // we store the item in the equipment inventory
                    oldItem = onlyItem.Copy();
                    oldItem.Unequip(PlayerID);
                    InventoryEvent.Trigger(Events.ItemUnEquipped, slot, name, oldItem, oldItem.Quantity, index, PlayerID);
                    inventory.ClearInventory();
                }
            }

            // we add one to the target equipment inventory
            inventory.AddItem(item.Copy(), item.Quantity);

            // remove 1 from quantity
            if (item.MoveWhenEquipped)
                RemoveItem(index, item.Quantity);

            if (oldItem)
            {
                oldItem.Swap(PlayerID);
                if (oldItem.ForceSlotIndex)
                    AddItemAt(oldItem, oldItem.TargetIndex, oldItem.Quantity);
                else
                    AddItem(oldItem, oldItem.Quantity);
            }

            // call the equip method of the item
            if (item.Equip(PlayerID))
                InventoryEvent.Trigger(Events.ItemEquipped, slot, name, item, item.Quantity, index, PlayerID);
        }

        public virtual void DropItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (item.IsNull())
            {
                Error(index, slot);
                return;
            }

            item.SpawnPrefab(PlayerID);

            if (name == item.TargetEquipmentInventoryName)
            {
                if (item.Unequip(PlayerID))
                {
                    DestroyItem(index);
                }
            }
            else
            {
                DestroyItem(index);
            }
        }

        public virtual void DestroyItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (item.IsNull())
            {
                Error(index, slot);
                return;
            }

            DestroyItem(index);
        }

        public virtual void UnEquipItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            // if there's no item at this slot, we trigger an error
            if (item.IsNull())
            {
                Error(index, slot);
                return;
            }

            // if we're not in an equipment inventory, we trigger an error
            if (InventoryType != Types.Equipment)
            {
                Error(index, slot);
                return;
            }

            // we trigger the unequip effect of the item
            if (!item.Unequip(PlayerID))
                return;

            InventoryEvent.Trigger(Events.ItemUnEquipped, slot, name, item, item.Quantity, index, PlayerID);

            // if there's a target inventory, we'll try to add the item back to it
            if (item.GetInventory(PlayerID, out var inventory))
            {
                bool itemAdded;
                if (item.ForceSlotIndex)
                {
                    itemAdded = inventory.AddItemAt(item, item.TargetIndex, item.Quantity);
                    if (!itemAdded)
                    {
                        itemAdded = inventory.AddItem(item, item.Quantity);
                    }
                }
                else
                {
                    itemAdded = inventory.AddItem(item, item.Quantity);
                }

                // if we managed to add the item
                if (itemAdded)
                {
                    DestroyItem(index);
                }
                else
                {
                    // if we couldn't (inventory full for example), we drop it to the ground
                    InventoryEvent.Trigger(Events.Drop, slot, name, item, item.Quantity, index, PlayerID);
                }
            }
        }

        #endregion

        public virtual void onEvent(InventoryEvent e)
        {
            // if this event doesn't concern our inventory display, we do nothing and exit
            if (e.InventoryName != name)
                return;

            if (e.PlayerID != PlayerID)
                return;

            switch (e.Events)
            {
                case Events.Pick:
                    if (e.Item.ForceSlotIndex)
                        AddItemAt(e.Item, e.Item.TargetIndex, e.Quantity);
                    else
                        AddItem(e.Item, e.Quantity);
                    break;
                case Events.Use:
                    UseItem(e.Item, e.Index, e.Slot);
                    break;
                case Events.Equip:
                    EquipItem(e.Item, e.Index, e.Slot);
                    break;
                case Events.UnEquip:
                    UnEquipItem(e.Item, e.Index, e.Slot);
                    break;
                case Events.Destroy:
                    DestroyItem(e.Item, e.Index, e.Slot);
                    break;
                case Events.Drop:
                    DropItem(e.Item, e.Index, e.Slot);
                    break;
            }
        }

        public virtual void onEvent(MMGameEvent e)
        {
            switch (e.EventName)
            {
                case "Save" when Persistent:
                    SaveInventory();
                    break;
                case "Load" when Persistent:
                    if (ResetThisInventorySaveOnStart)
                        ResetInventory();

                    LoadInventory();
                    break;
            }
        }

        protected virtual void OnEnable()
        {
            this.addListener<MMGameEvent>();
            this.addListener<InventoryEvent>();
        }

        protected virtual void OnDisable()
        {
            this.removeListener<MMGameEvent>();
            this.removeListener<InventoryEvent>();
        }

        void Error(int index, InventorySlot slot)
        {
            InventoryEvent.Trigger(Events.Error, slot, name, null, 0, index, PlayerID);
        }

        public virtual bool DestroyItem(int index)
        {
            this[index] = null;
            InventoryEvent.Trigger(Events.ContentChanged, name, PlayerID);
            return true;
        }

        public virtual void ClearInventory()
        {
            Content = new InventoryItem[Size];
            InventoryEvent.Trigger(Events.ContentChanged, name, PlayerID);
        }

        public virtual void ResizeArray(int newSize)
        {
            var temp = new InventoryItem[newSize];
            for (int i = 0; i < Mathf.Min(newSize, Size); i++)
                temp[i] = this[i];

            Content = temp;
        }
    }
}