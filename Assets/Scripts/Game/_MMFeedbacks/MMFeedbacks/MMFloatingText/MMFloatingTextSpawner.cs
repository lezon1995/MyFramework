using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
    #region Events

    /// <summary>
    /// An event used (usually by feedbacks) to trigger the spawn of a new floating text
    /// </summary>
    public struct MMFloatingTextSpawnEvent
    {
        static event Delegate OnEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialization()
        {
            OnEvent = null;
        }

        public static void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        public static void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        public delegate void Delegate(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
            bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false);

        public static void Trigger(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
            bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false)
        {
            OnEvent?.Invoke(channelData, spawnPosition, value, direction, intensity, forceLifetime, lifetime, forceColor, animateColorGradient, useUnscaledTime);
        }
    }

    #endregion

    /// <summary>
    /// This class will let you pool, recycle and spawn floating texts, usually to show damage info.
    /// It requires as input a MMFloatingText object.
    /// </summary>
    public class MMFloatingTextSpawner : MMMonoBehaviour
    {
        /// whether to spawn a single prefab or one at random 
        public enum PoolerModes
        {
            Simple,
            Multiple
        }

        /// whether the spawned text should have a fixed alignment, orient to match the initial spawn direction, or its movement curve
        public enum AlignmentModes
        {
            Fixed,
            MatchInitialDirection,
            MatchMovementDirection
        }

        [MMInspectorGroup("General Settings")]
        [Tooltip("whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what. " +
                 "MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable")]
        public MMChannelModes ChannelMode = MMChannelModes.Int;

        [Tooltip("the channel to listen to - has to match the one on the feedback")]
        [MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
        public int Channel;

        [Tooltip("the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel, " +
                 "right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name")]
        [MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
        public MMChannel MMChannelDefinition;

        [Tooltip("whether or not this spawner can spawn at this time")]
        public bool CanSpawn = true;

        [Tooltip("whether or not this spawner should spawn objects on unscaled time")]
        public bool UseUnscaledTime;

        [MMInspectorGroup("Pooler")]
        [Tooltip("the selected pooler mode (single prefab or multiple ones)")]
        public PoolerModes PoolerMode = PoolerModes.Simple;

        [Tooltip("the prefab to spawn (ignored if in multiple mode)")]
        public MMFloatingText PooledSimpleMMFloatingText;

        [Tooltip("the prefabs to spawn (ignored if in simple mode)")]
        public List<MMFloatingText> PooledMultipleMMFloatingText;

        [Tooltip("the amount of objects to pool to avoid having to instantiate them at runtime. Should be bigger than the max amount of texts you plan on having on screen at any given moment")]
        public int PoolSize = 20;

        [Tooltip("whether or not to nest the waiting pools")]
        public bool NestWaitingPool = true;

        [Tooltip("whether or not to mutualize the waiting pools")]
        public bool MutualizeWaitingPools = true;

        [Tooltip("whether or not the text pool can expand if the pool is empty")]
        public bool PoolCanExpand = true;

        [MMInspectorGroup("Spawn Settings")]
        [Tooltip("the random min and max lifetime duration for the spawned texts (in seconds)")]
        [MMVector("Min", "Max")]
        public Vector2 Lifetime = Vector2.one;

        [Header("Spawn Position Offset")]
        [Tooltip("the random min position at which to spawn the text, relative to its intended spawn position")]
        public Vector3 SpawnOffsetMin = Vector3.zero;

        [Tooltip("the random max position at which to spawn the text, relative to its intended spawn position")]
        public Vector3 SpawnOffsetMax = Vector3.zero;

        [MMInspectorGroup("Animate Position")]
        [Header("Movement")]
        [Tooltip("whether or not to animate the movement of spawned texts")]
        public bool AnimateMovement = true;

        [Tooltip("whether or not to animate the X movement of spawned texts")]
        public bool AnimateX;

        [Tooltip("the value to which the x movement curve's zero should be remapped to")]
        [MMCondition("AnimateX", true)]
        public Vector2 RemapXZero = Vector2.zero;

        [Tooltip("the value to which the x movement curve's one should be remapped to")]
        [MMCondition("AnimateX", true)]
        public Vector2 RemapXOne = Vector2.one;

        [Tooltip("the curve on which to animate the x movement")]
        [MMCondition("AnimateX", true)]
        public AnimationCurve AnimateXCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        [Tooltip("whether or not to animate the Y movement of spawned texts")]
        public bool AnimateY = true;

        [Tooltip("the value to which the y movement curve's zero should be remapped to")]
        [MMCondition("AnimateY", true)]
        public Vector2 RemapYZero = Vector2.zero;

        [Tooltip("the value to which the y movement curve's one should be remapped to")]
        [MMCondition("AnimateY", true)]
        public Vector2 RemapYOne = new Vector2(5f, 5f);

        [Tooltip("the curve on which to animate the y movement")]
        [MMCondition("AnimateY", true)]
        public AnimationCurve AnimateYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        [Tooltip("whether or not to animate the Z movement of spawned texts")]
        public bool AnimateZ;

        [Tooltip("the value to which the z movement curve's zero should be remapped to")]
        [MMCondition("AnimateZ", true)]
        public Vector2 RemapZZero = Vector2.zero;

        [Tooltip("the value to which the z movement curve's one should be remapped to")]
        [MMCondition("AnimateZ", true)]
        public Vector2 RemapZOne = Vector2.one;

        [Tooltip("the curve on which to animate the z movement")]
        [MMCondition("AnimateZ", true)]
        public AnimationCurve AnimateZCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        [MMInspectorGroup("Facing Directions")]
        [Header("Alignment")]
        [Tooltip("the selected alignment mode (whether the spawned text should have a fixed alignment, orient to match the initial spawn direction, or its movement curve)")]
        public AlignmentModes AlignmentMode = AlignmentModes.Fixed;

        [Tooltip("when in fixed mode, the direction in which to keep the spawned texts")]
        [MMEnumCondition("AlignmentMode", (int)AlignmentModes.Fixed)]
        public Vector3 FixedAlignment = Vector3.up;

        [Header("Billboard")]
        [Tooltip("whether or not spawned texts should always face the camera")]
        public bool AlwaysFaceCamera;

        [Tooltip("whether or not this spawner should automatically grab the main camera on start")]
        [MMCondition("AlwaysFaceCamera", true)]
        public bool AutoGrabMainCameraOnStart = true;

        [Tooltip("if not in auto grab mode, the camera to use for billboards")]
        [MMCondition("AlwaysFaceCamera", true)]
        public Camera TargetCamera;

        [MMInspectorGroup("Animate Scale")]
        [Tooltip("whether or not to animate the scale of spawned texts")]
        public bool AnimateScale = true;

        [Tooltip("the value to which the scale curve's zero should be remapped to")]
        [MMCondition("AnimateScale", true)]
        public Vector2 RemapScaleZero = Vector2.zero;

        [Tooltip("the value to which the scale curve's one should be remapped to")]
        [MMCondition("AnimateScale", true)]
        public Vector2 RemapScaleOne = Vector2.one;

        [Tooltip("the curve on which to animate the scale")]
        [MMCondition("AnimateScale", true)]
        public AnimationCurve AnimateScaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 1f), new Keyframe(0.85f, 1f), new Keyframe(1f, 0f));

        [MMInspectorGroup("Animate Color")]
        [Tooltip("whether or not to animate the spawned text's color over time")]
        public bool AnimateColor;

        [Tooltip("the gradient over which to animate the spawned text's color over time")]
        [GradientUsage(true)]
        public Gradient AnimateColorGradient = new Gradient();

        [MMInspectorGroup("Animate Opacity")]
        [Tooltip("whether or not to animate the opacity of the spawned texts")]
        public bool AnimateOpacity = true;

        [Tooltip("the value to which the opacity curve's zero should be remapped to")]
        [MMCondition("AnimateOpacity", true)]
        public Vector2 RemapOpacityZero = Vector2.zero;

        [Tooltip("the value to which the opacity curve's one should be remapped to")]
        [MMCondition("AnimateOpacity", true)]
        public Vector2 RemapOpacityOne = Vector2.one;

        [Tooltip("the curve on which to animate the opacity")]
        [MMCondition("AnimateOpacity", true)]
        public AnimationCurve AnimateOpacityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.2f, 1f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0f));

        [MMInspectorGroup("Intensity Multipliers")]
        [Tooltip("whether or not the intensity multiplier should impact lifetime")]
        public bool IntensityImpactsLifetime;

        [Tooltip("when getting an intensity multiplier, the value by which to multiply the lifetime")]
        [MMCondition("IntensityImpactsLifetime", true)]
        public float IntensityLifetimeMultiplier = 1f;

        [Tooltip("whether or not the intensity multiplier should impact movement")]
        public bool IntensityImpactsMovement;

        [Tooltip("when getting an intensity multiplier, the value by which to multiply the movement values")]
        [MMCondition("IntensityImpactsMovement", true)]
        public float IntensityMovementMultiplier = 1f;

        [Tooltip("whether or not the intensity multiplier should impact scale")]
        public bool IntensityImpactsScale;

        [Tooltip("when getting an intensity multiplier, the value by which to multiply the scale values")]
        [MMCondition("IntensityImpactsScale", true)]
        public float IntensityScaleMultiplier = 1f;

        [MMInspectorGroup("Debug")]
        [Tooltip("a random value to display when pressing the TestSpawnOne button")]
        public Vector2Int DebugRandomValue = new Vector2Int(100, 500);

        [Tooltip("the min and max bounds within which to pick a value to output when pressing the TestSpawnMany button")]
        [MMVector("Min", "Max")]
        public Vector2 DebugInterval = new Vector2(0.3f, 0.5f);

        [Tooltip("a button used to test the spawn of one text")]
        [MMInspectorButton("TestSpawnOne")]
        public bool TestSpawnOneBtn;

        [Tooltip("a button used to start/stop the spawn of texts at regular intervals")]
        [MMInspectorButton("TestSpawnMany")]
        public bool TestSpawnManyBtn;

        protected MMObjectPooler _pooler;
        protected MMFloatingText _floatingText;
        protected CoroutineHandle _testSpawnCoroutine;

        protected float _lifetime;
        protected float _speed;
        protected Vector3 _spawnOffset;
        protected Vector3 _direction;
        protected Gradient _colorGradient;
        protected bool _animateColor;

        #region Initialization

        /// <summary>
        /// On awake we initialize our spawner
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
        }

        /// <summary>
        /// On Start we grab our main camera if needed
        /// </summary>
        protected virtual void Start()
        {
            GrabMainCamera();
        }

        /// <summary>
        /// On init, we instantiate our object pool and grab the main camera
        /// </summary>
        protected virtual void Initialization()
        {
            InstantiateObjectPool();
        }

        /// <summary>
        /// Instantiates the specified type of object pool
        /// </summary>
        protected virtual void InstantiateObjectPool()
        {
            if (_pooler == null)
            {
                if (PoolerMode == PoolerModes.Simple)
                {
                    InstantiateSimplePool();
                }
                else
                {
                    InstantiateMultiplePool();
                }
            }
        }

        /// <summary>
        /// Instantiates a simple object pooler and sets it up
        /// </summary>
        protected virtual void InstantiateSimplePool()
        {
            if (PooledSimpleMMFloatingText == null)
            {
                Debug.LogError(name + " : no PooledSimpleMMFloatingText prefab has been set.");
                return;
            }

            GameObject newPooler = new GameObject();
            SceneManager.MoveGameObjectToScene(newPooler, gameObject.scene);
            newPooler.name = PooledSimpleMMFloatingText.name + "_Pooler";
            newPooler.transform.SetParent(transform);
            MMSimpleObjectPooler simplePooler = newPooler.AddComponent<MMSimpleObjectPooler>();
            simplePooler.PoolSize = PoolSize;
            simplePooler.GameObjectToPool = PooledSimpleMMFloatingText.gameObject;
            simplePooler.NestWaitingPool = NestWaitingPool;
            simplePooler.MutualizeWaitingPools = MutualizeWaitingPools;
            simplePooler.PoolCanExpand = PoolCanExpand;
            simplePooler.FillObjectPool();
            _pooler = simplePooler;
        }

        /// <summary>
        /// Instantiates a multiple object pooler and sets it up
        /// </summary>
        protected virtual void InstantiateMultiplePool()
        {
            GameObject newPooler = new GameObject();
            SceneManager.MoveGameObjectToScene(newPooler, gameObject.scene);
            newPooler.name = name + "_Pooler";
            newPooler.transform.SetParent(transform);
            MMMultipleObjectPooler multiplePooler = newPooler.AddComponent<MMMultipleObjectPooler>();
            multiplePooler.Pool = new List<MMMultipleObjectPoolerObject>();
            foreach (MMFloatingText obj in PooledMultipleMMFloatingText)
            {
                MMMultipleObjectPoolerObject item = new MMMultipleObjectPoolerObject();
                item.GameObjectToPool = obj.gameObject;
                item.PoolCanExpand = PoolCanExpand;
                item.PoolSize = PoolSize;
                item.Enabled = true;
                multiplePooler.Pool.Add(item);
            }

            multiplePooler.NestWaitingPool = NestWaitingPool;
            multiplePooler.MutualizeWaitingPools = MutualizeWaitingPools;
            multiplePooler.FillObjectPool();
            _pooler = multiplePooler;
        }

        /// <summary>
        /// Grabs the main camera if needed
        /// </summary>
        protected virtual void GrabMainCamera()
        {
            if (AutoGrabMainCameraOnStart)
            {
                TargetCamera = Camera.main;
            }
        }

        #endregion

        /// <summary>
        /// Spawns a new floating text
        /// </summary>
        /// <param name="value"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="intensity"></param>
        /// <param name="forceLifetime"></param>
        /// <param name="lifetime"></param>
        /// <param name="forceColor"></param>
        /// <param name="animateColorGradient"></param>
        protected virtual void Spawn(string value, Vector3 position, Vector3 direction, float intensity = 1f,
            bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null)
        {
            if (!CanSpawn)
                return;

            _direction = direction != Vector3.zero ? direction + transform.up : transform.up;

            transform.position = position;

            GameObject nextGameObject = _pooler.GetPooledGameObject();

            float lifetimeMultiplier = IntensityImpactsLifetime ? intensity * IntensityLifetimeMultiplier : 1f;
            float movementMultiplier = IntensityImpactsMovement ? intensity * IntensityMovementMultiplier : 1f;
            float scaleMultiplier = IntensityImpactsScale ? intensity * IntensityScaleMultiplier : 1f;

            _lifetime = Random.Range(Lifetime.x, Lifetime.y) * lifetimeMultiplier;
            _spawnOffset = MMMaths.RandomVector3(SpawnOffsetMin, SpawnOffsetMax);
            _animateColor = AnimateColor;
            _colorGradient = AnimateColorGradient;

            float remapXZero = Random.Range(RemapXZero.x, RemapXZero.y);
            float remapXOne = Random.Range(RemapXOne.x, RemapXOne.y) * movementMultiplier;
            float remapYZero = Random.Range(RemapYZero.x, RemapYZero.y);
            float remapYOne = Random.Range(RemapYOne.x, RemapYOne.y) * movementMultiplier;
            float remapZZero = Random.Range(RemapZZero.x, RemapZZero.y);
            float remapZOne = Random.Range(RemapZOne.x, RemapZOne.y) * movementMultiplier;
            float remapOpacityZero = Random.Range(RemapOpacityZero.x, RemapOpacityZero.y);
            float remapOpacityOne = Random.Range(RemapOpacityOne.x, RemapOpacityOne.y);
            float remapScaleZero = Random.Range(RemapScaleZero.x, RemapOpacityZero.y);
            float remapScaleOne = Random.Range(RemapScaleOne.x, RemapScaleOne.y) * scaleMultiplier;

            if (forceLifetime)
            {
                _lifetime = lifetime;
            }

            if (forceColor)
            {
                _animateColor = true;
                _colorGradient = animateColorGradient;
            }

            // mandatory checks
            if (nextGameObject == null)
            {
                return;
            }

            // we activate the object
            nextGameObject.gameObject.SetActive(true);
            nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

            // we position the object
            nextGameObject.transform.position = transform.position + _spawnOffset;

            _floatingText = nextGameObject.MMGetComponentNoAlloc<MMFloatingText>();
            _floatingText.SetUseUnscaledTime(UseUnscaledTime, true);
            _floatingText.ResetPosition();
            _floatingText.SetProperties(value, _lifetime, _direction, AnimateMovement,
                AlignmentMode, FixedAlignment, AlwaysFaceCamera, TargetCamera,
                AnimateX, AnimateXCurve, remapXZero, remapXOne,
                AnimateY, AnimateYCurve, remapYZero, remapYOne,
                AnimateZ, AnimateZCurve, remapZZero, remapZOne,
                AnimateOpacity, AnimateOpacityCurve, remapOpacityZero, remapOpacityOne,
                AnimateScale, AnimateScaleCurve, remapScaleZero, remapScaleOne,
                _animateColor, _colorGradient);
        }

        /// <summary>
        /// When we get a floating text event on this spawner's Channel, we spawn a new floating text
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <param name="intensity"></param>
        /// <param name="forceLifetime"></param>
        /// <param name="lifetime"></param>
        /// <param name="forceColor"></param>
        /// <param name="animateColorGradient"></param>
        public virtual void OnMMFloatingTextSpawnEvent(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
            bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false)
        {
            if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
            {
                return;
            }

            UseUnscaledTime = useUnscaledTime;
            Spawn(value, spawnPosition, direction, intensity, forceLifetime, lifetime, forceColor, animateColorGradient);
        }

        /// <summary>
        /// On enable we start listening for floating text events
        /// </summary>
        protected virtual void OnEnable()
        {
            MMFloatingTextSpawnEvent.Register(OnMMFloatingTextSpawnEvent);
        }

        /// <summary>
        /// On disable we stop listening for floating text events
        /// </summary>
        protected virtual void OnDisable()
        {
            MMFloatingTextSpawnEvent.Unregister(OnMMFloatingTextSpawnEvent);
        }

        // Test methods ----------------------------------------------------------------------------------------

        #region TestMethods

        /// <summary>
        /// A test method that spawns one floating text
        /// </summary>
        protected virtual void TestSpawnOne()
        {
            string test = Random.Range(DebugRandomValue.x, DebugRandomValue.y).ToString();
            Spawn(test, transform.position, Vector3.zero);
        }

        /// <summary>
        /// A method used to start/stop the regular spawning of debug floating texts 
        /// </summary>
        protected virtual void TestSpawnMany()
        {
            if (_testSpawnCoroutine == default)
            {
                _testSpawnCoroutine = Timing.RunCoroutine(TestSpawnManyCo());
            }
            else
            {
                Timing.KillCoroutines(_testSpawnCoroutine);
                _testSpawnCoroutine = default;
            }
        }

        /// <summary>
        /// A coroutine used to spawn debug floating texts until stopped 
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator<float> TestSpawnManyCo()
        {
            float lastSpawnAt = Time.time;
            float interval = Random.Range(DebugInterval.x, DebugInterval.y);
            while (true)
            {
                if (Time.time - lastSpawnAt > interval)
                {
                    TestSpawnOne();
                    lastSpawnAt = Time.time;
                    interval = Random.Range(DebugInterval.x, DebugInterval.y);
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        #endregion
    }
}