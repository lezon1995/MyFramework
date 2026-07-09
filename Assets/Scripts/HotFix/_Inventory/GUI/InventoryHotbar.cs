using MoreMountains.Tools;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// Special kind of inventory display, with a dedicated key associated to it, to allow for shortcuts for use and equip
    /// </summary>
    public class InventoryHotbar : InventoryDisplay
    {
        /// the possible actions that can be done on objects in the hotbar
        public enum HotbarPossibleAction
        {
            Use,
            Equip
        }

        [Header("Hotbar")]
        [MMInformation("Here you can define the keys your hotbar will listen to to activate the hotbar's action.")]
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		public InputActionProperty HotbarInputAction = new InputActionProperty(
			new InputAction(
				name: "IM_Demo_LeftKey",
				type: InputActionType.Button, 
				binding: "", 
				interactions: "Press(behavior=2)"));
#else
        /// the key associated to the hotbar, that will trigger the action when pressed
        public string HotbarKey;

        /// the alt key associated to the hotbar
        public string HotbarAltKey;
#endif
        /// the action associated to the key or alt key press
        public HotbarPossibleAction ActionOnKey;

        /// <summary>
        /// Executed when the key or alt key gets pressed, triggers the specified action
        /// </summary>
        public virtual void Action()
        {
            for (int i = Inventory.Size - 1; i >= 0; i--)
            {
                if (!Inventory[i].IsNull())
                {
                    var slot = SlotContainer[i];
                    switch (ActionOnKey)
                    {
                        case HotbarPossibleAction.Equip when slot:
                            slot.Equip();
                            break;
                        case HotbarPossibleAction.Use when slot:
                            slot.Use();
                            break;
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// On Enable, we start listening for MMInventoryEvents
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			HotbarInputAction.action.Enable();
#endif
        }

        /// <summary>
        /// On Disable, we stop listening for MMInventoryEvents
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			HotbarInputAction.action.Disable();
#endif
        }
    }
}