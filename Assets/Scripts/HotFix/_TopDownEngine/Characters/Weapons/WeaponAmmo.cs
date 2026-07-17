using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Weapon))]
    [AddComponentMenu("TopDown Engine/Weapons/WeaponAmmo")]
    public class WeaponAmmo : TopDownMonoBehaviour,
        IEvent<MMStateChangeEvent<Weapon.States>>,
        IEvent<InventoryEvent>,
        IEvent<MMGameEvent>
    {
        [Header("Ammo")]
        [Tooltip("the ID of this ammo, to be matched on the ammo display if you use one")]
        public string AmmoID;

        [Tooltip("the name of the inventory where the system should look for ammo")]
        public string AmmoInventoryName = "MainInventory";

        [Tooltip("the theoretical maximum of ammo")]
        public int MaxAmmo = 100;

        [Tooltip("if this is true, everytime you equip this weapon, it'll auto fill with ammo")]
        public bool ShouldLoadOnStart = true;

        [Tooltip("if this is true, everytime you equip this weapon, it'll auto fill with ammo")]
        public bool ShouldEmptyOnSave = true;

        [ShowInInspector, ReadOnly]
        [Tooltip("the current amount of ammo available in the inventory")]
        public int CurrentAmmoAvailable { get; set; }

        /// the inventory where ammo for this weapon is stored
        public virtual Inventory AmmoInventory { get; set; }

        protected Weapon _weapon;
        protected InventoryItem _ammoItem;
        protected bool _emptied;
        protected List<int> _ammoList = new();

        protected virtual void Start()
        {
            TryGetComponent(out _weapon);
            foreach (var inventory in FindObjectsByType<Inventory>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (inventory.PlayerID == _weapon.Owner.PlayerID)
                {
                    if (AmmoInventory == null && inventory.name == AmmoInventoryName)
                    {
                        AmmoInventory = inventory;
                    }
                }
            }

            if (ShouldLoadOnStart)
            {
                LoadOnStart();
            }
        }

        /// <summary>
        /// Loads our weapon with ammo
        /// </summary>
        protected virtual void LoadOnStart()
        {
            FillWeaponWithAmmo();
        }

        /// <summary>
        /// Updates the CurrentAmmoAvailable counter
        /// </summary>
        protected virtual void RefreshCurrentAmmoAvailable()
        {
            CurrentAmmoAvailable = AmmoInventory.GetQuantity(AmmoID);
        }

        /// <summary>
        /// Returns true if this weapon has enough ammo to fire, false otherwise
        /// </summary>
        /// <returns></returns>
        public virtual bool EnoughAmmoToFire()
        {
            if (AmmoInventory == null)
            {
                Debug.LogWarning(name + " couldn't find the associated inventory. Is there one present in the scene? It should be named '" + AmmoInventoryName + "'.");
                return false;
            }

            RefreshCurrentAmmoAvailable();

            if (_weapon.MagazineBased)
            {
                return _weapon.CurrentAmmoLoaded >= _weapon.AmmoConsumedPerShot;
            }

            return CurrentAmmoAvailable >= _weapon.AmmoConsumedPerShot;
        }

        /// <summary>
        /// Consumes ammo based on the amount of ammo to consume per shot
        /// </summary>
        protected virtual void ConsumeAmmo()
        {
            if (_weapon.MagazineBased)
            {
                _weapon.CurrentAmmoLoaded -= _weapon.AmmoConsumedPerShot;
            }
            else
            {
                for (int i = 0; i < _weapon.AmmoConsumedPerShot; i++)
                {
                    AmmoInventory.UseItem(AmmoID);
                    CurrentAmmoAvailable--;
                }
            }

            if (CurrentAmmoAvailable < _weapon.AmmoConsumedPerShot)
            {
                if (_weapon.AutoDestroyWhenEmpty)
                {
                    Timing.RunCoroutine(_weapon.WeaponDestruction());
                }
            }
        }

        /// <summary>
        /// Fills the weapon with ammo
        /// </summary>
        public virtual void FillWeaponWithAmmo()
        {
            if (AmmoInventory)
                RefreshCurrentAmmoAvailable();

            if (_ammoItem == null)
            {
                AmmoInventory.Search(AmmoID, ref _ammoList);
                if (_ammoList.Count > 0)
                {
                    _ammoItem = AmmoInventory[_ammoList[^1]].Copy();
                }
            }

            if (_weapon.MagazineBased)
            {
                var counter = 0;
                var stock = CurrentAmmoAvailable;
                for (var i = _weapon.CurrentAmmoLoaded; i < _weapon.MagazineSize; i++)
                {
                    if (stock > 0)
                    {
                        stock--;
                        counter++;

                        AmmoInventory.UseItem(AmmoID);
                    }
                }

                _weapon.CurrentAmmoLoaded += counter;
            }

            RefreshCurrentAmmoAvailable();
        }

        /// <summary>
        /// Empties the weapon's magazine and puts the ammo back in the inventory
        /// </summary>
        public virtual void EmptyMagazine()
        {
            if (AmmoInventory)
                RefreshCurrentAmmoAvailable();

            if (_ammoItem == null || AmmoInventory == null)
                return;

            if (_emptied)
                return;

            if (_weapon.MagazineBased)
            {
                int stock = _weapon.CurrentAmmoLoaded;
                int counter = 0;

                for (int i = 0; i < stock; i++)
                {
                    AmmoInventory.AddItem(_ammoItem);
                    counter++;
                }

                _weapon.CurrentAmmoLoaded -= counter;

                if (AmmoInventory.Persistent)
                    AmmoInventory.SaveInventory();
            }

            RefreshCurrentAmmoAvailable();
            _emptied = true;
        }

        /// <summary>
        /// When getting weapon events, we either consume ammo or refill it
        /// </summary>
        /// <param name="e"></param>
        public virtual void onEvent(MMStateChangeEvent<Weapon.States> e)
        {
            // if this event doesn't concern us, we do nothing and exit
            if (e.Target == gameObject)
            {
                switch (e.NewState)
                {
                    case Weapon.States.Use:
                        ConsumeAmmo();
                        break;

                    case Weapon.States.ReloadStop:
                        FillWeaponWithAmmo();
                        break;
                }
            }
        }

        /// <summary>
        /// Grabs inventory events and refreshes ammo if needed
        /// </summary>
        /// <param name="e"></param>
        public virtual void onEvent(InventoryEvent e)
        {
            switch (e.Events)
            {
                case Inventory.Events.Pick:
                    if (e.Item.ItemClass == ItemClasses.Ammo)
                    {
                        Timing.RunCoroutine(DelayedRefreshCurrentAmmoAvailable());
                    }

                    break;
            }
        }

        protected IEnumerator<float> DelayedRefreshCurrentAmmoAvailable()
        {
            yield return Timing.WaitForOneFrame;
            RefreshCurrentAmmoAvailable();
        }

        /// <summary>
        /// Grabs inventory events and refreshes ammo if needed
        /// </summary>
        /// <param name="inventoryEvent"></param>
        public virtual void onEvent(MMGameEvent e)
        {
            switch (e.EventName)
            {
                case "Save":
                    if (ShouldEmptyOnSave)
                    {
                        EmptyMagazine();
                    }

                    break;
            }
        }

        protected void OnDestroy()
        {
            // on destroy we put our ammo back in the inventory
            EmptyMagazine();
        }

        /// <summary>
        /// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.addListener<MMStateChangeEvent<Weapon.States>>();
            this.addListener<InventoryEvent>();
            this.addListener<MMGameEvent>();
        }

        /// <summary>
        /// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
        /// </summary>
        protected virtual void OnDisable()
        {
            this.removeListener<MMStateChangeEvent<Weapon.States>>();
            this.removeListener<InventoryEvent>();
            this.removeListener<MMGameEvent>();
        }
    }
}