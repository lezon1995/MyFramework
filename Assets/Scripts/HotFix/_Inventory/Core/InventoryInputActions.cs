using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// This class lets you bind keys to specific slots in a target inventory, and associate an action to execute when that key is pressed.
    /// A typical use case would be a weapon bar, where pressing 1 equips a gun, pressing 2 equips a shotgun, etc.
    /// Coincidentally, that's what the PixelRogueWeaponBar demo scene demonstrates.
    /// </summary>
    public class InventoryInputActions : MonoBehaviour
    {
        /// <summary>
        /// A class used to store slot / key / action bindings 
        /// </summary>
        [Serializable]
        public class Binding
        {
            /// the slot in the target inventory to bind an action to 
            public int SlotIndex;

            /// the key that should trigger the action
            public KeyCode InputBinding = KeyCode.Alpha0;

            /// an alt key that will also trigger the action
            public KeyCode AltInputBinding = KeyCode.None;

            /// the action to trigger when pressing the input
            public Actions Action = Actions.Equip;

            /// whether or not this action should be triggered
            public bool Active = true;
        }

        /// <summary>
        /// The possible actions that can be caused when activating input
        /// </summary>
        public enum Actions
        {
            Equip,
            Use,
            Drop,
            UnEquip
        }

        /// the name of the inventory to pilot with these bindings
        public string TargetInventoryName = "MainInventory";

        /// the unique ID of the Player associated to this component
        public string PlayerID = "Player1";

        /// a list of bindings to go through when looking for input
        public List<Binding> InputBindings;

        protected Inventory _targetInventory;

        /// <summary>
        /// Returns the target inventory of this component
        /// </summary>
        public Inventory TargetInventory
        {
            get
            {
                if (TargetInventoryName == null)
                    return null;

                if (_targetInventory == null)
                    _targetInventory = Inventory.Get(TargetInventoryName, PlayerID);

                return _targetInventory;
            }
        }

        /// <summary>
        /// On Start we initialize our inventory reference
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Makes sure we have a target inventory
        /// </summary>
        protected virtual void Initialization()
        {
            if (TargetInventoryName == "")
            {
                Debug.LogError("The " + name + " Inventory Input Actions component doesn't have a TargetInventoryName set. You need to set one from its inspector, matching an Inventory's name.");
                return;
            }

            if (TargetInventory == null)
            {
                Debug.LogError("The " + name + " Inventory Input Actions component couldn't find a TargetInventory. You either need to create an inventory with a matching inventory name (" + TargetInventoryName + "), or set that TargetInventoryName to one that exists.");
            }
        }

        /// <summary>
        /// On Update we look for input
        /// </summary>
        protected virtual void Update()
        {
            DetectInput();
        }

        /// <summary>
        /// Every frame we look for input for each of our bindings
        /// </summary>
        protected virtual void DetectInput()
        {
            foreach (Binding binding in InputBindings)
            {
                if (binding == null)
                    continue;

                if (!binding.Active)
                    continue;

                if (Input.GetKeyDown(binding.InputBinding) || Input.GetKeyDown(binding.AltInputBinding))
                {
                    ExecuteAction(binding);
                }
            }
        }

        /// <summary>
        /// Executes the corresponding action for the specified binding
        /// </summary>
        protected virtual void ExecuteAction(Binding binding)
        {
            var slotIndex = binding.SlotIndex;
            if (slotIndex > _targetInventory.Size)
                return;

            var item = _targetInventory[slotIndex];
            if (item == null)
                return;

            var inventoryName = _targetInventory.name;
            var playerID = _targetInventory.PlayerID;
            switch (binding.Action)
            {
                case Actions.Equip:
                    InventoryEvent.Trigger(Inventory.Events.Equip, null, inventoryName, item, 0, slotIndex, playerID);
                    break;
                case Actions.Use:
                    InventoryEvent.Trigger(Inventory.Events.Use, null, inventoryName, item, 0, slotIndex, playerID);
                    break;
                case Actions.Drop:
                    InventoryEvent.Trigger(Inventory.Events.Drop, null, inventoryName, item, 0, slotIndex, playerID);
                    break;
                case Actions.UnEquip:
                    InventoryEvent.Trigger(Inventory.Events.UnEquip, null, inventoryName, item, 0, slotIndex, playerID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}