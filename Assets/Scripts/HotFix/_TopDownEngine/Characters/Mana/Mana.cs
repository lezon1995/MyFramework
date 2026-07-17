using System;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This class manages the mana of an object, pilots its potential mana bar, handles what happens when it costs mana,
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/Mana")]
    public class Mana : TopDownMonoBehaviour
    {
        [MMInspectorGroup("Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("the current mana of the character")]
        public float CurrentMana { get; set; }

        [MMInspectorGroup("Mana")]
        [MMInformation("Add this component to an object and it'll have mana")]
        [Tooltip("the initial amount of mana of the object")]
        public float InitialMana;

        public bool InitialManaDrivenByMaximumMana;

        [Tooltip("the maximum amount of mana of the object")]
        public float MaximumMana = 10;

        public ValueModifier MaximumManaModifier { get; set; }

        public float maximumMana
        {
            get
            {
                var maxMana = MaximumMana;
                return MaximumManaModifier.SafeInvoke(ref maxMana);
            }
        }

        [Tooltip("if this is true, mana values will be reset everytime this character is enabled (usually at the start of a scene)")]
        public bool ResetManaOnEnable = true;

        [MMInspectorGroup("Mana Regen")]
        [Tooltip("the base amount of mana regen (receive mana X per 1 second)")]
        public float BaseManaRegen = 1;

        public ValueModifier ManaRegenModifier { get; set; }

        public float manaRegen
        {
            get
            {
                var value = BaseManaRegen;
                return ManaRegenModifier.SafeInvoke(ref value);
            }
        }

        public float LastManaCost { get; set; }

        public event Action OnCostMana;

        protected Character _character;
        protected MMHealthBar _manaBar;

        float _timeElapsed;

        #region Initialization

        /// <summary>
        /// On Awake, we initialize our mana
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
            InitializeCurrentMana();
        }

        protected virtual void Start()
        {
            BindStats();
        }

        void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            _timeElapsed += dt;
            if (_timeElapsed >= 0.5F)
            {
                _timeElapsed = 0F;
                ReceiveMana(manaRegen, gameObject);
            }
        }

        /// <summary>
        /// Grabs useful components, enables damage and gets the initial color
        /// </summary>
        public virtual void Initialization()
        {
            _character = GetComponentInParent<Character>();
            _manaBar = GetComponentInParent<MMHealthBar>();
            _timeElapsed = 0F;
        }

        protected virtual void BindStats()
        {
            if (_character && _character.Stats)
            {
                var mp = _character.GetStat(Character.Stat.ManaMax);
                mp.Event.Add((pre, now) => UpdateManaBar(true));
                MaximumManaModifier = (ref float raw) =>
                {
                    raw = mp.Value;
                };

                var mpRegen = _character.GetStat(Character.Stat.ManaRegen);
                ManaRegenModifier = (ref float raw) =>
                {
                    raw = mpRegen.Value;
                };

                InitializeCurrentMana();
            }
        }

        /// <summary>
        /// Initializes mana to either initial or current values
        /// </summary>
        public virtual void InitializeCurrentMana()
        {
            var initialMana = InitialManaDrivenByMaximumMana ? maximumMana : InitialMana;
            SetMana(initialMana);
        }

        /// <summary>
        /// When the object is enabled (on respawn for example), we restore its initial mana levels
        /// </summary>
        protected virtual void OnEnable()
        {
            if (ResetManaOnEnable)
            {
                InitializeCurrentMana();
            }
        }

        /// <summary>
        /// On Disable, we prevent any delayed destruction from running
        /// </summary>
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }

        #endregion

        /// <summary>
        /// Returns true if this Mana component can be costed this frame, and false otherwise
        /// </summary>
        /// <returns></returns>
        public virtual bool CanCostManaThisFrame()
        {
            if (!enabled)
                return false;

            // if we're already below zero, we do nothing and exit
            if (CurrentMana <= 0 && InitialMana != 0)
                return false;

            return true;
        }

        /// <summary>
        /// Called when the object takes damage
        /// </summary>
        /// <param name="manaCost">The amount of mana points that will get lost.</param>
        /// <param name="instigator">The object that caused the damage.</param>
        public virtual void CostMana(float manaCost, GameObject instigator)
        {
            if (!CanCostManaThisFrame())
                return;

            SetMana(CurrentMana - manaCost);

            LastManaCost = manaCost;
            OnCostMana?.Invoke();

            // we update the mana bar
            UpdateManaBar(true);

            if (CurrentMana <= 0)
            {
                CurrentMana = 0;
            }
        }

        #region ManaManipulationAPIs

        /// <summary>
        /// Sets the current mana to the specified new value, and updates the mana bar
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetMana(float newValue)
        {
            CurrentMana = newValue;
            UpdateManaBar(false);
        }

        /// <summary>
        /// Called when the character gets mana (from a stimpack for example)
        /// </summary>
        /// <param name="delta">The mana the character gets.</param>
        /// <param name="instigator">The thing that gives the character mana.</param>
        public virtual void ReceiveMana(float delta, GameObject instigator)
        {
            if (delta <= 0F)
                return;

            // this function adds mana to the character's Mana and prevents it to go above MaxMana.
            var maxMana = maximumMana;
            var newMana = Mathf.Min(CurrentMana + delta, maxMana);
            if (newMana >= maxMana)
                return;

            SetMana(newMana);

            UpdateManaBar(true);
        }

        /// <summary>
        /// Resets the character's mana to its max value
        /// </summary>
        public virtual void ResetManaToMaxMana()
        {
            SetMana(maximumMana);
        }

        /// <summary>
        /// Forces a refresh of the character's mana bar
        /// </summary>
        public virtual void UpdateManaBar(bool show)
        {
            if (_manaBar)
                _manaBar.UpdateBar(CurrentMana, 0f, maximumMana, show);
        }

        #endregion
    }
}