using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Add this component to an object containing multiple weapons and it'll turn it into a ComboWeapon, allowing you to chain attacks from all the different weapons
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/ComboWeapon")]
    public class ComboWeapon : TopDownMonoBehaviour
    {
        public enum InputModes
        {
            SemiAuto,
            Auto
        }

        [Header("Combo")]
        [Tooltip("whether or not the combo can be dropped if enough time passes between two consecutive attacks")]
        public bool DroppableCombo = true;

        [Tooltip("the delay after which the combo drops")]
        public float DropComboDelay = 0.5f;

        [Tooltip("the input mode for this combo weapon. In Auto mode, you'll want to make sure you've set ContinuousPress:true on your CharacterHandleWeapon ability")]
        public InputModes InputMode = InputModes.SemiAuto;

        [Header("Animation")]
        [Tooltip("the name of the animation parameter to update when a combo is in progress.")]
        public string ComboInProgressAnimationParameter = "ComboInProgress";

        [Sirenix.OdinInspector.Title("Debug")]
        [Sirenix.OdinInspector.ShowInInspector]
        [Tooltip("the list of weapons, set automatically by the class")]
        public Weapon[] Weapons  { get; set; }

        [Sirenix.OdinInspector.ShowInInspector]
        [Tooltip("the reference to the weapon's Owner")]
        public CharacterHandleWeapon OwnerCharacterHandleWeapon  { get; set; }

        [Sirenix.OdinInspector.ShowInInspector]
        [Tooltip("the time spent since the last weapon stopped")]
        public float TimeSinceLastWeaponStopped  { get; set; }

        /// <summary>
        /// True if a combo is in progress, false otherwise
        /// </summary>
        /// <returns></returns>
        public bool ComboInProgress
        {
            get
            {
                bool comboInProgress = false;
                foreach (Weapon weapon in Weapons)
                {
                    if (weapon.State.Not(Weapon.States.Idle))
                    {
                        comboInProgress = true;
                    }
                }

                return comboInProgress;
            }
        }

        protected int _currentWeaponIndex;
        protected WeaponAutoShoot _weaponAutoShoot;
        protected bool _countdownActive;

        /// <summary>
        /// On start we initialize our Combo Weapon
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Grabs all Weapon components and initializes them
        /// </summary>
        public virtual void Initialization()
        {
            Weapons = GetComponents<Weapon>();
            _weaponAutoShoot = GetComponent<WeaponAutoShoot>();
            InitializeUnusedWeapons();
        }

        /// <summary>
        /// On Update we reset our combo if needed
        /// </summary>
        protected virtual void Update()
        {
            ResetCombo();
        }

        /// <summary>
        /// Resets the combo if enough time has passed since the last attack
        /// </summary>
        public virtual void ResetCombo()
        {
            if (Weapons.Length > 1)
            {
                if (_countdownActive && DroppableCombo)
                {
                    TimeSinceLastWeaponStopped += Time.deltaTime;
                    if (TimeSinceLastWeaponStopped > DropComboDelay)
                    {
                        _countdownActive = false;

                        _currentWeaponIndex = 0;
                        OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[_currentWeaponIndex];
                        OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[_currentWeaponIndex], Weapons[_currentWeaponIndex].WeaponName, true);
                        if (_weaponAutoShoot)
                        {
                            _weaponAutoShoot.SetCurrentWeapon(Weapons[_currentWeaponIndex]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When one of the weapons get used we turn our countdown off
        /// </summary>
        /// <param name="weaponThatStarted"></param>
        public virtual void WeaponStarted(Weapon weaponThatStarted)
        {
            _countdownActive = false;
        }

        /// <summary>
        /// When one of the weapons has ended its attack, we start our countdown and switch to the next weapon
        /// </summary>
        /// <param name="weaponThatStopped"></param>
        public virtual void WeaponStopped(Weapon weaponThatStopped)
        {
            ProceedToNextWeapon();
        }

        public virtual void ProceedToNextWeapon()
        {
            OwnerCharacterHandleWeapon = Weapons[_currentWeaponIndex].HandleWeapon;

            if (OwnerCharacterHandleWeapon)
            {
                if (Weapons.Length > 1)
                {
                    int newIndex;
                    if (_currentWeaponIndex < Weapons.Length - 1)
                    {
                        newIndex = _currentWeaponIndex + 1;
                    }
                    else
                    {
                        newIndex = 0;
                    }

                    _countdownActive = true;
                    TimeSinceLastWeaponStopped = 0f;

                    _currentWeaponIndex = newIndex;
                    OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[newIndex];
                    OwnerCharacterHandleWeapon.CurrentWeapon.WeaponCurrentlyActive = false;
                    OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[newIndex], Weapons[newIndex].WeaponName, true);
                    OwnerCharacterHandleWeapon.CurrentWeapon.WeaponCurrentlyActive = true;

                    if (_weaponAutoShoot)
                    {
                        _weaponAutoShoot.SetCurrentWeapon(Weapons[newIndex]);
                    }
                }
            }
        }

        /// <summary>
        /// Flips all unused weapons so they remain properly oriented
        /// </summary>
        public virtual void FlipUnusedWeapons()
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (i != _currentWeaponIndex)
                {
                    Weapons[i].Flipped = !Weapons[i].Flipped;
                }
            }
        }

        /// <summary>
        /// Initializes all unused weapons
        /// </summary>
        protected virtual void InitializeUnusedWeapons()
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (i != _currentWeaponIndex)
                {
                    Weapons[i].SetOwner(Weapons[_currentWeaponIndex].Owner, Weapons[_currentWeaponIndex].HandleWeapon);
                    Weapons[i].Initialization();
                    Weapons[i].WeaponCurrentlyActive = false;
                }
            }
        }
    }
}