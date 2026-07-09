using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// A component that will handle the playing of songs when paired with an InventoryDisplay
    /// </summary>
    [RequireComponent(typeof(InventoryDisplay))]
    public class InventorySoundPlayer : MonoBehaviour, IEvent<InventoryEvent>
    {
        public enum Modes
        {
            Direct,
            Event
        }

        [Header("Settings")]
        /// the mode to choose to play sounds. Direct will play an audiosource, event will call a MMSfxEvent,
        /// meant to be caught by a MMSoundManager 
        public Modes Mode = Modes.Direct;

        [Header("Sounds")]
        [MMInformation("Here you can define the default sounds that will get played when interacting with this inventory.")]
        /// the audioclip to play when the inventory opens
        public AudioClip OpenFx;

        /// the audioclip to play when the inventory closes
        public AudioClip CloseFx;

        /// the audioclip to play when moving from one slot to another
        public AudioClip SelectionChangeFx;

        /// the audioclip to play when moving from one slot to another
        public AudioClip ClickFX;

        /// the audioclip to play when moving an object successfully
        public AudioClip MoveFX;

        /// the audioclip to play when an error occurs (selecting an empty slot, etc)
        public AudioClip ErrorFx;

        /// the audioclip to play when an item is used, if no other sound has been defined for it
        public AudioClip UseFx;

        /// the audioclip to play when an item is dropped, if no other sound has been defined for it
        public AudioClip DropFx;

        /// the audioclip to play when an item is equipped, if no other sound has been defined for it
        public AudioClip EquipFx;

        protected string _targetInventoryName;
        protected string _targetPlayerID;
        protected AudioSource _audioSource;

        /// <summary>
        /// On Start we setup our player and grab a few references for future use.
        /// </summary>
        protected virtual void Start()
        {
            SetupInventorySoundPlayer();
            _audioSource = GetComponent<AudioSource>();
            _targetInventoryName = GetComponent<InventoryDisplay>().TargetInventoryName;
            _targetPlayerID = GetComponent<InventoryDisplay>().PlayerID;
        }

        /// <summary>
        /// Setups the inventory sound player.
        /// </summary>
        public virtual void SetupInventorySoundPlayer()
        {
            AddAudioSource();
        }

        /// <summary>
        /// Adds an audio source component if needed.
        /// </summary>
        protected virtual void AddAudioSource()
        {
            if (GetComponent<AudioSource>() == null)
            {
                gameObject.AddComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Plays the sound specified in the parameter string
        /// </summary>
        /// <param name="soundFx">Sound fx.</param>
        public virtual void PlaySound(string soundFx)
        {
            if (string.IsNullOrEmpty(soundFx))
                return;

            AudioClip soundToPlay = null;
            float volume = 1f;

            switch (soundFx)
            {
                case "error":
                    soundToPlay = ErrorFx;
                    volume = 1f;
                    break;
                case "select":
                    soundToPlay = SelectionChangeFx;
                    volume = 0.5f;
                    break;
                case "click":
                    soundToPlay = ClickFX;
                    volume = 0.5f;
                    break;
                case "open":
                    soundToPlay = OpenFx;
                    volume = 1f;
                    break;
                case "close":
                    soundToPlay = CloseFx;
                    volume = 1f;
                    break;
                case "move":
                    soundToPlay = MoveFX;
                    volume = 1f;
                    break;
                case "use":
                    soundToPlay = UseFx;
                    volume = 1f;
                    break;
                case "drop":
                    soundToPlay = DropFx;
                    volume = 1f;
                    break;
                case "equip":
                    soundToPlay = EquipFx;
                    volume = 1f;
                    break;
            }

            if (soundToPlay)
            {
                if (Mode == Modes.Direct)
                {
                    _audioSource.PlayOneShot(soundToPlay, volume);
                }
                else
                {
                    MMSfxEvent.Trigger(soundToPlay, volume: volume, pitch: 1);
                }
            }
        }

        /// <summary>
        /// Plays the sound fx specified in parameters at the desired volume
        /// </summary>
        /// <param name="soundFx">Sound fx.</param>
        /// <param name="volume">Volume.</param>
        public virtual void PlaySound(AudioClip soundFx, float volume)
        {
            if (soundFx)
            {
                if (Mode == Modes.Direct)
                {
                    _audioSource.PlayOneShot(soundFx, volume);
                }
                else
                {
                    MMSfxEvent.Trigger(soundFx, volume: volume, pitch: 1);
                }
            }
        }

        /// <summary>
        /// Catches MMInventoryEvents and acts on them, playing the corresponding sounds
        /// </summary>
        /// <param name="e">Inventory event.</param>
        public virtual void onEvent(InventoryEvent e)
        {
            // if this event doesn't concern our inventory display, we do nothing and exit
            if (e.InventoryName != _targetInventoryName)
                return;

            if (e.PlayerID != _targetPlayerID)
                return;

            switch (e.Events)
            {
                case Inventory.Events.Select:
                    PlaySound("select");
                    break;
                case Inventory.Events.Click:
                    PlaySound("click");
                    break;
                case Inventory.Events.InventoryOpens:
                    PlaySound("open");
                    break;
                case Inventory.Events.InventoryCloses:
                    PlaySound("close");
                    break;
                case Inventory.Events.Error:
                    PlaySound("error");
                    break;
                case Inventory.Events.Move:
                    if (e.Item.MovedSound == null)
                    {
                        if (e.Item.UseDefaultSoundsIfNull)
                        {
                            PlaySound("move");
                        }
                    }
                    else
                    {
                        PlaySound(e.Item.MovedSound, 1f);
                    }

                    break;
                case Inventory.Events.ItemEquipped:
                    if (e.Item.EquippedSound == null)
                    {
                        if (e.Item.UseDefaultSoundsIfNull)
                        {
                            PlaySound("equip");
                        }
                    }
                    else
                    {
                        PlaySound(e.Item.EquippedSound, 1f);
                    }

                    break;
                case Inventory.Events.ItemUsed:
                    if (e.Item.UsedSound == null)
                    {
                        if (e.Item.UseDefaultSoundsIfNull)
                        {
                            PlaySound("use");
                        }
                    }
                    else
                    {
                        PlaySound(e.Item.UsedSound, 1f);
                    }

                    break;
                case Inventory.Events.Drop:
                    if (e.Item.DroppedSound == null)
                    {
                        if (e.Item.UseDefaultSoundsIfNull)
                        {
                            PlaySound("drop");
                        }
                    }
                    else
                    {
                        PlaySound(e.Item.DroppedSound, 1f);
                    }

                    break;
            }
        }

        /// <summary>
        /// OnEnable, we start listening to MMInventoryEvents.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.addListener<InventoryEvent>();
        }

        /// <summary>
        /// OnDisable, we stop listening to MMInventoryEvents.
        /// </summary>
        protected virtual void OnDisable()
        {
            this.removeListener<InventoryEvent>();
        }
    }
}