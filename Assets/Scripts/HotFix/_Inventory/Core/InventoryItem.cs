using System;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    [Serializable]
    public class InventoryItemDisplayProperties
    {
        public bool DisplayEquipUseButton = true;
        public bool DisplayMoveButton = true;
        public bool DisplayDropButton = true;
        public bool DisplayEquipButton = true;
        public bool DisplayUseButton = true;
        public bool DisplayUnequipButton = true;
        public bool AllowEquipUseShortcut = true;
        public bool AllowMoveShortcut = true;
        public bool AllowDropShortcut = true;
        public bool AllowEquipShortcut = true;
        public bool AllowUseShortcut = true;
    }

    /// <summary>
    /// Base class for inventory items, meant to be extended.
    /// Will handle base properties and drop spawn
    /// </summary>
    [Serializable]
    public abstract class InventoryItem : SerializedScriptableObject
    {
        public string ItemID;
        public string ItemName;
        public Sprite Icon;

        [TextArea]
        public string ShortDescription;

        [TextArea]
        public string Description;

        [Tooltip("if this is true, the item won't be added anywhere's there's room in the inventory, but instead at the specified TargetIndex slot")]
        [HorizontalGroup("SlotIndex")]
        [ToggleLeft]
        public bool ForceSlotIndex;

        [Tooltip("if ForceSlotIndex is true, this is the index at which the item will be added in the target inventory")]
        [EnableIf(nameof(ForceSlotIndex)), HideLabel]
        [HorizontalGroup("SlotIndex")]
        public int TargetIndex;

        [Tooltip("whether or not this item can be 'used' (via the Use method) - important, this is only the INITIAL state of this object, IsUsable is to be used anytime after that")]
        [HorizontalGroup("Usable")]
        [ToggleLeft]
        public bool Usable;

        [Tooltip("if this item is consumable, determines how many will be consumed per use (usually one)")]
        [EnableIf(nameof(Usable))]
        [HorizontalGroup("Usable")]
        public int ConsumeQuantity = 1;

        [Tooltip("whether or not this item can be equipped - important, this is only the INITIAL state of this object, IsEquippable is to be used anytime after that")]
        [ToggleLeft]
        public bool Equippable;

        [Tooltip("if this is true, this item will be removed from its original inventory when equipped, and moved to its EquipmentInventory")]
        [EnableIf(nameof(Equippable))]
        public bool MoveWhenEquipped = true;

        [ToggleLeft]
        public bool Droppable = true;

        [Tooltip("the inventory name into which this item will be stored")]
        [BoxGroup("Inventory")]
        public string TargetInventoryName = "MainInventory";

        [Tooltip("If this item is equippable, you can set here its target inventory name (for example ArmorInventory). Of course you'll need an inventory with a matching name in your scene.")]
        [BoxGroup("Inventory")]
        public string TargetEquipmentInventoryName;

        [Tooltip("a set of properties defining whether or not to show inventory action buttons when that item is selected")]
        [FoldoutGroup("DisplayProperties"), HideLabel, HideInInspector]
        public InventoryItemDisplayProperties DisplayProperties;

        [Tooltip("the prefab to instantiate when the item is dropped")]
        [BoxGroup("Drop")]
        public GameObject Prefab;

        [Tooltip("if this is true, the quantity of the object will be forced to PrefabDropQuantity when dropped")]
        [BoxGroup("Drop")]
        public bool ForcePrefabDropQuantity;

        [Tooltip("the quantity to force on the spawned item if ForcePrefabDropQuantity is true")]
        [EnableIf(nameof(ForcePrefabDropQuantity))]
        [BoxGroup("Drop")]
        public int PrefabDropQuantity = 1;

        [Tooltip("the minimal distance at which the object should be spawned when dropped")]
        [BoxGroup("Drop")]
        [FoldoutGroup("Drop/Properties"), HideLabel]
        public MMSpawnAroundProperties DropProperties;

        [Tooltip("If this object can be stacked (multiple instances in a single inventory slot), you can specify here the maximum size of that stack.")]
        public int MaximumStack = 1;

        [Tooltip("the maximum quantity allowed of this item in the target inventory")]
        public int MaximumQuantity = 999999999;

        [Tooltip("the class of the item")]
        public ItemClasses ItemClass;

        public AudioClip EquippedSound;
        public AudioClip UsedSound;
        public AudioClip MovedSound;
        public AudioClip DroppedSound;

        [Tooltip("if this is set to false, default sounds won't be used and no sound will be played")]
        public bool UseDefaultSoundsIfNull = true;

        public bool IsUsable => Usable;
        public bool IsEquippable => Equippable;
        public int Quantity { get; set; } = 1;

        public virtual bool AddTo(Inventory inventory, string playerID) => true;
        public virtual bool RemoveFrom(Inventory inventory, string playerID) => true;
        public virtual bool Pick(string playerID) => true;
        public virtual bool Use(string playerID) => true;
        public virtual bool Equip(string playerID) => true;
        public virtual bool Unequip(string playerID) => true;
        public virtual bool Swap(string playerID) => true;
        public virtual bool Drop(string playerID) => true;

        public virtual bool GetInventory(string playerID, out Inventory inventory, Inventory.Types type = Inventory.Types.Main)
        {
            var inventoryName = type switch
            {
                Inventory.Types.Main => TargetInventoryName,
                Inventory.Types.Equipment => TargetEquipmentInventoryName,
                _ => TargetInventoryName
            };

            if (string.IsNullOrEmpty(inventoryName))
            {
                inventory = null;
                return false;
            }

            inventory = Inventory.Get(inventoryName, playerID);
            if (inventory)
                return true;

            return false;
        }

        public virtual bool GetOwner(string playerID, out GameObject owner)
        {
            if (string.IsNullOrEmpty(TargetInventoryName))
            {
                owner = null;
                return false;
            }

            var inventory = Inventory.Get(TargetInventoryName, playerID);
            if (inventory)
            {
                owner = inventory.Owner;
                return owner != null;
            }

            owner = null;
            return false;
        }

        public virtual InventoryItem Copy()
        {
            var clone = Instantiate(this);
            clone.name = name;
            return clone;
        }

        public virtual GameObject SpawnPrefab(string playerID)
        {
            if (GetInventory(playerID, out var inventory))
            {
                // if there's a prefab set for the item at this slot, we instantiate it at the specified offset
                if (Prefab && inventory.TargetTransform)
                {
                    var droppedObject = Instantiate(Prefab);
                    if (droppedObject.TryGetComponent<ItemPicker>(out var itemPicker))
                    {
                        if (ForcePrefabDropQuantity)
                        {
                            itemPicker.Quantity = PrefabDropQuantity;
                            itemPicker.RemainingQuantity = PrefabDropQuantity;
                        }
                        else
                        {
                            itemPicker.Quantity = Quantity;
                            itemPicker.RemainingQuantity = Quantity;
                        }
                    }

                    MMSpawnAround.ApplySpawnAroundProperties(droppedObject, DropProperties, inventory.TargetTransform.position);
                    return droppedObject;
                }
            }

            return null;
        }
    }
}