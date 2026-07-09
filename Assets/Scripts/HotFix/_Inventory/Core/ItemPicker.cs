using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// Add this component to an object so it can be picked and added to an inventory
    /// </summary>
    public class ItemPicker : MonoBehaviour
    {
        public InventoryItem Item;
        public int Quantity = 1;
        public bool PickableIfInventoryIsFull;

        [Tooltip("if you set this to true, the object will be disabled when picked")]
        public bool DisableObjectWhenDepleted;

        [Tooltip("if this is true, this object will only be allowed to be picked by colliders with a Player tag")]
        public bool RequirePlayerTag = true;

        [Title("Debug")]
        [Tooltip("the current quantity of that item that should be added to the inventory when picked")]
        [ShowInInspector, ReadOnly]
        public int RemainingQuantity { get; set; } = 1;

        protected int _pickedQuantity;
        protected Inventory _inventory;
        protected List<int> searchResults = new();

        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            FindTargetInventory(Item.TargetInventoryName);
            ResetQuantity();
        }

        /// <summary>
        /// Resets the remaining quantity to the initial quantity
        /// </summary>
        public virtual void ResetQuantity()
        {
            RemainingQuantity = Quantity;
        }

        public virtual void OnTriggerEnter(Collider collider) => DoPick(collider.gameObject);
        public virtual void OnTriggerEnter2D(Collider2D collider) => DoPick(collider.gameObject);

        void DoPick(GameObject go)
        {
            if (RequirePlayerTag && !go.CompareTag("Player"))
                return;

            var playerID = "Player1";
            var identifier = go.GetComponent<InventoryCharacterIdentifier>();
            if (identifier)
            {
                playerID = identifier.PlayerID;
            }

            Pick(Item.TargetInventoryName, playerID);
        }

        public void Pick()
        {
            Pick(Item.TargetInventoryName);
        }

        public virtual void Pick(string inventoryName, string playerID = "Player1")
        {
            FindTargetInventory(inventoryName, playerID);
            if (_inventory == null)
                return;

            if (!Pickable())
            {
                OnPickFail();
                return;
            }

            DetermineMaxQuantity();
            if (Application.isPlaying)
                InventoryEvent.Trigger(Inventory.Events.Pick, null, Item.TargetInventoryName, Item, _pickedQuantity, 0, playerID);
            else
                _inventory.AddItem(Item);

            if (Item.Pick(playerID))
            {
                RemainingQuantity -= _pickedQuantity;
                OnPickSuccess();
                DisableObjectIfNeeded();
            }
        }

        /// <summary>
        /// Describes what happens when the object is successfully picked
        /// </summary>
        protected virtual void OnPickSuccess()
        {
        }

        /// <summary>
        /// Describes what happens when the object fails to get picked (inventory full, usually)
        /// </summary>
        protected virtual void OnPickFail()
        {
        }

        /// <summary>
        /// Disables the object if needed.
        /// </summary>
        protected virtual void DisableObjectIfNeeded()
        {
            // we deactivate the gameobject
            if (DisableObjectWhenDepleted && RemainingQuantity <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Determines the max quantity of item that can be picked from this
        /// </summary>
        protected virtual void DetermineMaxQuantity()
        {
            int maxQuantity = _inventory.GetMaxQuantity(Item, Quantity);
            int stackQuantity = _inventory.GetStackCount(Item.ItemID, Item.MaximumStack);

            _pickedQuantity = Mathf.Min(maxQuantity, stackQuantity);

            if (RemainingQuantity < _pickedQuantity)
            {
                _pickedQuantity = RemainingQuantity;
            }
        }


        /// <summary>
        /// Returns true if this item can be picked, false otherwise
        /// </summary>
        public virtual bool Pickable()
        {
            if (PickableIfInventoryIsFull)
                return true;

            if (_inventory.FreeCount > 0)
                return true;

            // we make sure that there isn't a place where we could store it
            int spaceAvailable = 0;
            _inventory.Search(Item.ItemID, ref searchResults);
            foreach (int index in searchResults)
            {
                spaceAvailable += Item.MaximumStack - _inventory[index].Quantity;
            }

            if (Item.Quantity <= spaceAvailable)
                return true;

            return false;
        }

        /// <summary>
        /// Finds the target inventory based on its name
        /// </summary>
        /// <param name="inventoryName">Target inventory name.</param>
        public virtual void FindTargetInventory(string inventoryName, string playerID = "Player1")
        {
            _inventory = null;
            _inventory = Inventory.Get(inventoryName, playerID);
        }
    }
}