using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// This class handles the display of the items in an inventory and will trigger the various things you can do with an item (equip, use, etc.)
    /// </summary>
    public class InventorySlot : Button
    {
        /// the sprite used as a background for the slot while an item is being moved
        public Sprite MovedSprite;

        /// the inventory display this slot belongs to
        public InventoryDisplay ParentInventoryDisplay;

        /// the slot's index (its position in the inventory array)
        public int Index;

        /// whether or not this slot is currently enabled and can be interacted with
        public bool SlotEnabled = true;

        public Image TargetImage;
        public CanvasGroup TargetCanvasGroup;
        public RectTransform TargetRectTransform;
        public RectTransform IconRectTransform;
        public Image IconImage;
        public Text QuantityText;

        public InventoryItem Item
        {
            get
            {
                if (ParentInventoryDisplay)
                    return ParentInventoryDisplay.Inventory[Index];

                return null;
            }
        }
        
        public Inventory Inventory
        {
            get
            {
                if (ParentInventoryDisplay)
                    return ParentInventoryDisplay.Inventory;

                return null;
            }
        }

        public string InventoryName
        {
            get
            {
                if (ParentInventoryDisplay)
                    return ParentInventoryDisplay.TargetInventoryName;

                return null;
            }
        }
        
        public string PlayerID
        {
            get
            {
                if (ParentInventoryDisplay)
                    return ParentInventoryDisplay.PlayerID;

                return null;
            }
        }

        protected const float _disabledAlpha = 0.5f;
        protected const float _enabledAlpha = 1.0f;

        protected override void Awake()
        {
            base.Awake();
            TargetImage = GetComponent<Image>();
            TargetCanvasGroup = GetComponent<CanvasGroup>();
            TargetRectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// On Start, we start listening to click events on that slot
        /// </summary>
        protected override void Start()
        {
            base.Start();
            onClick.AddListener(SlotClicked);
        }

        /// <summary>
        /// If there's an item in this slot, draws its icon inside.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        public virtual void DrawIcon(InventoryItem item, int index)
        {
            if (ParentInventoryDisplay)
            {
                if (item.IsNull())
                {
                    DisableIconAndQuantity();
                }
                else
                {
                    SetIcon(item.Icon);
                    SetQuantity(item.Quantity);
                }
            }
        }

        public virtual void SetIcon(Sprite newSprite)
        {
            IconImage.gameObject.SetActive(true);
            IconImage.sprite = newSprite;
        }

        public virtual void SetQuantity(int quantity)
        {
            if (quantity > 1)
            {
                QuantityText.gameObject.SetActive(true);
                QuantityText.text = quantity.ToString();
            }
            else
            {
                QuantityText.gameObject.SetActive(false);
            }
        }

        public virtual void DisableIconAndQuantity()
        {
            IconImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// When that slot gets selected (via a mouse over or a touch), triggers an event for other classes to act on
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (ParentInventoryDisplay)
            {
                InventoryItem item = Item;
                InventoryEvent.Trigger(Inventory.Events.Select, this, InventoryName, item, 0, Index, PlayerID);
            }
        }

        /// <summary>
        /// When that slot gets clicked, triggers an event for other classes to act on
        /// </summary>
        public virtual void SlotClicked()
        {
            if (ParentInventoryDisplay)
            {
                if (ParentInventoryDisplay.InEquipSelection)
                    InventoryEvent.Trigger(Inventory.Events.Equip, this, InventoryName, Item, 0, Index, PlayerID);

                InventoryEvent.Trigger(Inventory.Events.Click, this, InventoryName, Item, 0, Index, PlayerID);

                // if we're currently moving an object
                if (InventoryDisplay.CurrentlyBeingMovedItemIndex != -1)
                    Move();
            }
        }

        /// <summary>
        /// Selects the item in this slot for a movement, or moves the currently selected one to that slot
        /// This will also swap both objects if possible
        /// </summary>
        public virtual void Move()
        {
            if (!SlotEnabled)
                return;

            // if we're not already moving an object
            if (InventoryDisplay.CurrentlyBeingMovedItemIndex == -1)
            {
                // if the slot we're on is empty, we do nothing
                if (Item.IsNull())
                {
                    InventoryEvent.Trigger(Inventory.Events.Error, this, InventoryName, null, 0, Index, PlayerID);
                    return;
                }

                // we change the background image
                TargetImage.sprite = ParentInventoryDisplay.MovedSlotImage;
                InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = ParentInventoryDisplay;
                InventoryDisplay.CurrentlyBeingMovedItemIndex = Index;
            }
            // if we ARE moving an object
            else
            {
                bool moveSuccessful;
                // we move the object to a new slot. 
                if (ParentInventoryDisplay == InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay)
                {
                    if (!Inventory.MoveItem(InventoryDisplay.CurrentlyBeingMovedItemIndex, Index))
                    {
                        // if the move couldn't be made (non empty destination slot for example), we play a sound
                        InventoryEvent.Trigger(Inventory.Events.Error, this, InventoryName, null, 0, Index, PlayerID);
                        moveSuccessful = false;
                    }
                    else
                    {
                        moveSuccessful = true;
                    }
                }
                else
                {
                    if (!ParentInventoryDisplay.AllowMovingObjectsToThisInventory)
                    {
                        moveSuccessful = false;
                    }
                    else
                    {
                        if (!InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay.Inventory.MoveItemToInventory(InventoryDisplay.CurrentlyBeingMovedItemIndex, Inventory, Index))
                        {
                            // if the move couldn't be made (non empty destination slot for example), we play a sound
                            InventoryEvent.Trigger(Inventory.Events.Error, this, InventoryName, null, 0, Index, PlayerID);
                            moveSuccessful = false;
                        }
                        else
                        {
                            moveSuccessful = true;
                        }
                    }
                }

                if (moveSuccessful)
                {
                    // if the move could be made, we reset our currentlyBeingMoved pointer
                    InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
                    InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = null;
                    InventoryEvent.Trigger(Inventory.Events.Move, this, InventoryName, Item, 0, Index, PlayerID);
                }
            }
        }

        public virtual void Use()
        {
            if (!SlotEnabled)
                return;

            InventoryEvent.Trigger(Inventory.Events.Use, this, InventoryName, Item, 0, Index, PlayerID);
        }

        public virtual void Equip()
        {
            if (!SlotEnabled)
                return;

            InventoryEvent.Trigger(Inventory.Events.Equip, this, InventoryName, Item, 0, Index, PlayerID);
        }

        public virtual void UnEquip()
        {
            if (!SlotEnabled)
                return;

            InventoryEvent.Trigger(Inventory.Events.UnEquip, this, InventoryName, Item, 0, Index, PlayerID);
        }

        /// <summary>
        /// Drops this item.
        /// </summary>
        public virtual void Drop()
        {
            if (!SlotEnabled)
                return;

            var item = Item;
            if (item.IsNull())
            {
                InventoryEvent.Trigger(Inventory.Events.Error, this, InventoryName, null, 0, Index, PlayerID);
                return;
            }

            if (!item.Droppable)
                return;

            if (item.Drop(PlayerID))
            {
                InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
                InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = null;
                InventoryEvent.Trigger(Inventory.Events.Drop, this, InventoryName, Item, 0, Index, PlayerID);
            }
        }

        /// <summary>
        /// Disables the slot.
        /// </summary>
        public virtual void DisableSlot()
        {
            interactable = false;
            SlotEnabled = false;
            TargetCanvasGroup.alpha = _disabledAlpha;
        }

        /// <summary>
        /// Enables the slot.
        /// </summary>
        public virtual void EnableSlot()
        {
            interactable = true;
            SlotEnabled = true;
            TargetCanvasGroup.alpha = _enabledAlpha;
        }

        /// <summary>
        /// Returns true if the item at this slot can be equipped, false otherwise
        /// </summary>
        public virtual bool Equippable()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.IsEquippable;
        }

        /// <summary>
        /// Returns true if the item at this slot can be used, false otherwise
        /// </summary>
        public virtual bool Usable()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.IsUsable;
        }

        /// <summary>
        /// Returns true if the item at this slot can be dropped, false otherwise
        /// </summary>
        public virtual bool Droppable()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.Droppable;
        }

        /// <summary>
        /// Returns true if the item at this slot can be dropped, false otherwise
        /// </summary>
        public virtual bool Unequippable()
        {
            var inventory = Inventory;
            var item = inventory[Index];
            if (item.IsNull())
                return false;

            return inventory.InventoryType == Inventory.Types.Equipment;
        }

        public virtual bool EquipUseButtonShouldShow()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.DisplayProperties.DisplayEquipUseButton;
        }

        public virtual bool MoveButtonShouldShow()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.DisplayProperties.DisplayMoveButton;
        }

        public virtual bool DropButtonShouldShow()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.DisplayProperties.DisplayDropButton;
        }

        public virtual bool EquipButtonShouldShow()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.DisplayProperties.DisplayEquipButton;
        }

        public virtual bool UseButtonShouldShow()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.DisplayProperties.DisplayUseButton;
        }

        public virtual bool UnequipButtonShouldShow()
        {
            var item = Item;
            if (item.IsNull())
                return false;

            return item.DisplayProperties.DisplayUnequipButton;
        }
    }
}