using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Add this component to a Character, and it'll persist with its exact current state when transitioning to a new scene.
    /// It'll be automatically passed to the new scene's LevelManager to be used as this scene's main character.
    /// It'll keep the exact state all its components are in at the moment they finish the level.
    /// Its health, enabled abilities, component values, equipped weapons, new components you may have added, etc, will all remain once in the new scene. 
    /// Animator parameters : None
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/CharacterPersistence")]
    public class CharacterPersistence : CharacterAbility,
        IEvent<MMGameEvent>,
        IEvent<TopDownEngineEvent>
    {
        public virtual bool Initialized { get; set; }

        /// <summary>
        /// On Start(), we prevent our character from being destroyed if needed
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();

            if (AbilityAuthorized)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            Initialized = true;
        }

        public override void onEvent(OnDeath e)
        {
            base.onEvent(e);
            Initialized = false;
        }

        /// <summary>
        /// When we get a save request, we store our character in the game manager for future use
        /// </summary>
        /// <param name="e"></param>
        public virtual void onEvent(MMGameEvent e)
        {
            if (e.EventName == "Save")
            {
                SaveCharacter();
            }
        }

        /// <summary>
        /// When we get a TopDown Engine event, we act on it
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void onEvent(TopDownEngineEvent e)
        {
            if (AbilityUnauthorized)
                return;

            switch (e.EventType)
            {
                case TopDownEngineEventTypes.LoadNextScene:
                    gameObject.SetActive(false);
                    break;
                case TopDownEngineEventTypes.SpawnCharacterStarts:
                    // transform.position = LevelManager.Instance.InitialSpawnPoint.transform.position;
                    gameObject.SetActive(true);
                    Character character = GetComponentInParent<Character>();
                    character.enabled = true;
                    character.conditionState.ChangeState(Character.Conditions.Normal);
                    character.motionState.ChangeState(Character.Motions.Idle);
                    character.SetInputManager();
                    break;
                case TopDownEngineEventTypes.LevelStart:
                    break;
                case TopDownEngineEventTypes.RespawnComplete:
                    Initialized = true;
                    break;
            }
        }

        /// <summary>
        /// Saves to the game manager a reference to our character
        /// </summary>
        protected virtual void SaveCharacter()
        {
            if (AbilityUnauthorized)
                return;

            GameManager.Instance.PersistentCharacter = _character;
        }

        /// <summary>
        /// Clears any saved character that may have been stored in the GameManager
        /// </summary>
        public virtual void ClearSavedCharacter()
        {
            if (AbilityUnauthorized)
                return;

            GameManager.Instance.PersistentCharacter = null;
        }

        /// <summary>
        /// On enable we start listening for events
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            this.addListener<MMGameEvent>();
            this.addListener<TopDownEngineEvent>();
        }

        /// <summary>
        /// On disable we stop listening for events
        /// </summary>
        protected virtual void OnDestroy()
        {
            this.removeListener<MMGameEvent>();
            this.removeListener<TopDownEngineEvent>();
        }
    }
}