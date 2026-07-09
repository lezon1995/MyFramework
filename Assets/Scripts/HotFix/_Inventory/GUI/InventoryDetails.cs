using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// A class used to display an item's details in GUI
    /// </summary>
    public class InventoryDetails : MonoBehaviour, IEvent<InventoryEvent>
    {
        /// the reference inventory from which we'll display item details
        [MMInformation("Specify here the name of the inventory whose content's details you want to display in this Details panel. You can also decide to make it global. If you do so, it'll display the details of all items, regardless of their inventory.")]
        public string TargetInventoryName;

        public string PlayerID = "Player1";

        /// if you make this panel global, it'll ignore 
        public bool Global;

        /// whether the details are currently hidden or not 
        public virtual bool Hidden { get; protected set; }

        [Header("Default")]
        [MMInformation("By checking HideOnEmptySlot, the Details panel won't be displayed if you select an empty slot.")]
        // whether or not the details panel should be hidden when the currently selected slot is empty
        public bool HideOnEmptySlot = true;

        [MMInformation("Here you can set default values for all fields of the details panel. These values will be displayed when no item is selected (and if you've chosen not to hide the panel in that case).")]
        // the title to display when none is provided
        public string DefaultTitle;

        /// the short description to display when none is provided
        public string DefaultShortDescription;

        /// the description to display when none is provided
        public string DefaultDescription;

        /// the quantity to display when none is provided
        public string DefaultQuantity;

        /// the icon to display when none is provided
        public Sprite DefaultIcon;

        [Header("Behaviour")]
        [MMInformation("Here you can decide whether or not to hide the details panel on start.")]
        // whether or not to hide the details panel at start
        public bool HideOnStart = true;

        [Header("Components")]
        [MMInformation("Here you need to bind the panel components.")]
        // the icon container object
        public Image Icon;

        /// the title container object
        public Text Title;

        /// the short description container object
        public Text ShortDescription;

        /// the description container object
        public Text Description;

        /// the quantity container object
        public Text Quantity;

        protected float _fadeDelay = 0.2f;
        protected CanvasGroup _canvasGroup;

        /// <summary>
        /// On Start, we grab and store the canvas group and determine our current Hidden status
        /// </summary>
        protected virtual void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (HideOnStart)
                _canvasGroup.alpha = 0;

            if (_canvasGroup.alpha == 0)
                Hidden = true;
            else
                Hidden = false;
        }

        /// <summary>
        /// Starts the display coroutine or the panel's fade depending on whether or not the current slot is empty
        /// </summary>
        /// <param name="item">Item.</param>
        public virtual void DisplayDetails(InventoryItem item)
        {
            if (item.IsNull())
            {
                if (HideOnEmptySlot && !Hidden)
                {
                    Timing.RunCoroutine(MMFade.FadeCanvasGroup(_canvasGroup, _fadeDelay, 0f));
                    Hidden = true;
                }

                if (!HideOnEmptySlot)
                {
                    Timing.RunCoroutine(FillDetailFieldsWithDefaults(0));
                }
            }
            else
            {
                Timing.RunCoroutine(FillDetailFields(item, 0f));

                if (HideOnEmptySlot && Hidden)
                {
                    Timing.RunCoroutine(MMFade.FadeCanvasGroup(_canvasGroup, _fadeDelay, 1f));
                    Hidden = false;
                }
            }
        }

        /// <summary>
        /// Fills the various detail fields with the item's metadata
        /// </summary>
        /// <returns>The detail fields.</returns>
        /// <param name="item">Item.</param>
        /// <param name="initialDelay">Initial delay.</param>
        protected virtual IEnumerator<float> FillDetailFields(InventoryItem item, float initialDelay)
        {
            yield return Timing.WaitForSeconds(initialDelay);
            if (Title)
                Title.text = item.ItemName;

            if (ShortDescription)
                ShortDescription.text = item.ShortDescription;

            if (Description)
                Description.text = item.Description;

            if (Quantity)
                Quantity.text = item.Quantity.ToString();

            if (Icon)
                Icon.sprite = item.Icon;

            if (HideOnEmptySlot && !Hidden && (item.Quantity == 0))
            {
                Timing.RunCoroutine(MMFade.FadeCanvasGroup(_canvasGroup, _fadeDelay, 0f));
                Hidden = true;
            }
        }

        /// <summary>
        /// Fills the detail fields with default values.
        /// </summary>
        /// <returns>The detail fields with defaults.</returns>
        /// <param name="initialDelay">Initial delay.</param>
        protected virtual IEnumerator<float> FillDetailFieldsWithDefaults(float initialDelay)
        {
            yield return Timing.WaitForSeconds(initialDelay);
            if (Title)
                Title.text = DefaultTitle;

            if (ShortDescription)
                ShortDescription.text = DefaultShortDescription;

            if (Description)
                Description.text = DefaultDescription;

            if (Quantity)
                Quantity.text = DefaultQuantity;

            if (Icon)
                Icon.sprite = DefaultIcon;
        }

        /// <summary>
        /// Catches MMInventoryEvents and displays details if needed
        /// </summary>
        /// <param name="e">Inventory event.</param>
        public virtual void onEvent(InventoryEvent e)
        {
            // if this event doesn't concern our inventory display, we do nothing and exit
            if (!Global && (e.InventoryName != this.TargetInventoryName))
                return;

            if (e.PlayerID != PlayerID)
                return;

            switch (e.Events)
            {
                case Inventory.Events.Select:
                    DisplayDetails(e.Item);
                    break;
                case Inventory.Events.Use:
                    DisplayDetails(e.Item);
                    break;
                case Inventory.Events.InventoryOpens:
                    DisplayDetails(e.Item);
                    break;
                case Inventory.Events.Drop:
                    DisplayDetails(null);
                    break;
                case Inventory.Events.Equip:
                    DisplayDetails(null);
                    break;
            }
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