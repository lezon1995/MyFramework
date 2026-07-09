using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this component to an object, and it will show a health-bar above it.
    /// You can either use a prefab for it, or have the component draw one at the start
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/GUI/MMHealthBar")]
    public class MMHealthBar : MonoBehaviour
    {
        /// the possible health bar types
        public enum Types
        {
            Prefab,
            Drawn,
            Existing
        }

        /// the possible timescales the bar can work on
        public enum TimeScales
        {
            UnscaledTime,
            Time
        }

        [MMInformation("Add this component to an object and it'll add a healthbar next to it to reflect its health level in real time. You can decide here whether the health bar should be drawn automatically or use a prefab.")]
        [Tooltip("whether the health bar uses a prefab or is drawn automatically")]
        public Types HealthBarType = Types.Drawn;

        [Tooltip("defines whether the bar will work on scaled or unscaled time (whether or not it'll keep moving if time is slowed down for example)")]
        public TimeScales TimeScale = TimeScales.UnscaledTime;

        [Header("Select a Prefab")]
        [MMInformation("Select a prefab with a progress bar script on it. There is one example of such a prefab in Common/Prefabs/GUI.")]
        [Tooltip("the prefab to use as the health bar")]
        public MMProgressBar HealthBarPrefab;

        [Header("Existing MMProgressBar")]
        [Tooltip("the MMProgressBar this health bar should update")]
        public MMProgressBar TargetProgressBar;

        [Header("Drawn Healthbar Settings ")]
        [MMInformation("Set the size (in world units), padding, back and front colors of the healthbar.")]
        [Tooltip("if the healthbar is drawn, its size in world units")]
        public Vector2 Size = new(1f, 0.2f);

        [Tooltip("if the healthbar is drawn, the padding to apply to the foreground, in world units")]
        public Vector2 BackgroundPadding = new(0.01f, 0.01f);

        [Tooltip("the rotation to apply to the MMHealthBarContainer when drawing it")]
        public Vector3 InitialRotationAngles;

        [Tooltip("if the healthbar is drawn, the color of its foreground")]
        public Gradient ForegroundColor = new()
        {
            colorKeys = new GradientColorKey[]
            {
                new(MMColors.BestRed, 0),
                new(MMColors.BestRed, 1f)
            },
            alphaKeys = new GradientAlphaKey[]
            {
                new(1, 0),
                new(1, 1)
            }
        };

        [Tooltip("if the healthbar is drawn, the color of its delayed bar")]
        public Gradient DelayedColor = new()
        {
            colorKeys = new GradientColorKey[]
            {
                new(MMColors.Orange, 0),
                new(MMColors.Orange, 1f)
            },
            alphaKeys = new GradientAlphaKey[]
            {
                new(1, 0),
                new(1, 1)
            }
        };

        [Tooltip("if the healthbar is drawn, the color of its border")]
        public Gradient BorderColor = new()
        {
            colorKeys = new GradientColorKey[]
            {
                new(MMColors.AntiqueWhite, 0),
                new(MMColors.AntiqueWhite, 1f)
            },
            alphaKeys = new GradientAlphaKey[]
            {
                new(1, 0),
                new(1, 1)
            }
        };

        [Tooltip("if the healthbar is drawn, the color of its background")]
        public Gradient BackgroundColor = new()
        {
            colorKeys = new GradientColorKey[]
            {
                new(MMColors.Black, 0),
                new(MMColors.Black, 1f)
            },
            alphaKeys = new GradientAlphaKey[]
            {
                new(1, 0),
                new(1, 1)
            }
        };

        [Tooltip("the name of the sorting layer to put this health bar on")]
        public string SortingLayerName = "UI";

        [Tooltip("the delay to apply to the delayed bar if drawn")]
        public float Delay = 0.5f;

        [Tooltip("whether or not the front bar should lerp")]
        public bool LerpFrontBar = true;

        [Tooltip("the speed at which the front bar lerps")]
        public float LerpFrontBarSpeed = 15f;

        [Tooltip("whether or not the delayed bar should lerp")]
        public bool LerpDelayedBar = true;

        [Tooltip("the speed at which the delayed bar lerps")]
        public float LerpDelayedBarSpeed = 15f;

        [Tooltip("if this is true, bumps the scale of the healthbar when its value changes")]
        public bool BumpScaleOnChange = true;

        [Tooltip("the duration of the bump animation")]
        public float BumpDuration = 0.2f;

        [Tooltip("the animation curve to map the bump animation on")]
        public AnimationCurve BumpAnimationCurve = AnimationCurve.Constant(0, 1, 1);

        [Tooltip("the mode the bar should follow the target in")]
        public MMFollowTarget.UpdateModes FollowTargetMode = MMFollowTarget.UpdateModes.LateUpdate;

        [Tooltip("if this is true, the drawn health bar will adapt its rotation to match the one of its target")]
        public bool FollowRotation;

        [Tooltip("if this is true, the drawn health bar will adapt its scale to match the one of its target")]
        public bool FollowScale = true;

        [Tooltip("if this is true, the drawn health bar will be nested below the MMHealthBar")]
        public bool NestDrawnHealthBar;

        [Tooltip("if this is true, a MMBillboard component will be added to the progress bar to make sure it always looks towards the camera")]
        public bool Billboard;

        [Header("Death")]
        [Tooltip("a gameobject (usually a particle system) to instantiate when the healthbar reaches zero")]
        public GameObject InstantiatedOnDeath;

        [Header("Offset")]
        [MMInformation("Set the offset (in world units), relative to the object's center, to which the health bar will be displayed.")]
        [Tooltip("the offset to apply to the healthbar compared to the object's center")]
        public Vector3 HealthBarOffset = new Vector3(0f, 1f, 0f);

        [Header("Display")]
        [MMInformation("Here you can define whether or not the healthbar should always be visible. If not, you can set here how long after a hit it'll remain visible.")]
        [Tooltip("whether or not the bar should be permanently displayed")]
        public bool AlwaysVisible = true;

        [Tooltip("the duration (in seconds) during which to display the bar")]
        public float DisplayDurationOnHit = 1f;

        [Tooltip("if this is set to true the bar will hide itself when it reaches zero")]
        public bool HideBarAtZero = true;

        [Tooltip("the delay (in seconds) after which to hide the bar")]
        public float HideBarAtZeroDelay = 1f;

        [Header("Test")]
        [Tooltip("a test value to use when pressing the TestUpdateHealth button")]
        public float TestMinHealth;

        [Tooltip("a test value to use when pressing the TestUpdateHealth button")]
        public float TestMaxHealth = 100f;

        [Tooltip("a test value to use when pressing the TestUpdateHealth button")]
        public float TestCurrentHealth = 25f;

        [MMInspectorButton("TestUpdateHealth")]
        public bool TestUpdateHealthButton;


        protected MMProgressBar _progressBar;
        protected MMFollowTarget _followTransform;
        protected float _lastShowTimestamp;
        protected bool _showBar;
        protected Image _backgroundImage;
        protected Image _borderImage;
        protected Image _foregroundImage;
        protected Image _delayedImage;
        protected bool _finalHideStarted;

        /// <summary>
        /// On Start, creates or sets the health bar up
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
        }

        /// <summary>
        /// On enable, initializes the bar again
        /// </summary>
        protected void OnEnable()
        {
            _finalHideStarted = false;

            SetInitialActiveState();
        }

        /// <summary>
        /// Forces the bar into its initial active state (hiding it if AlwaysVisible is false)
        /// </summary>
        public virtual void SetInitialActiveState()
        {
            if (!AlwaysVisible && _progressBar)
            {
                ShowBar(false);
            }
        }

        /// <summary>
        /// Shows or hides the bar by changing its object's active state
        /// </summary>
        /// <param name="state"></param>
        public virtual void ShowBar(bool state)
        {
            _progressBar.gameObject.SetActive(state);
        }

        /// <summary>
        /// Whether the bar is currently active
        /// </summary>
        /// <returns></returns>
        public virtual bool BarIsShown()
        {
            return _progressBar.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Initializes the bar (handles visibility, parenting, initial value
        /// </summary>
        public virtual void Initialization()
        {
            _finalHideStarted = false;

            if (_progressBar)
            {
                ShowBar(AlwaysVisible);
                return;
            }

            switch (HealthBarType)
            {
                case Types.Prefab:
                    if (HealthBarPrefab == null)
                    {
                        Debug.LogWarning(name + " : the HealthBar has no prefab associated to it, nothing will be displayed.");
                        return;
                    }

                    _progressBar = Instantiate(HealthBarPrefab, transform.position + HealthBarOffset, transform.rotation);
                    SceneManager.MoveGameObjectToScene(_progressBar.gameObject, gameObject.scene);
                    _progressBar.transform.SetParent(transform);
                    _progressBar.gameObject.name = "HealthBar";
                    break;
                case Types.Drawn:
                    DrawHealthBar();
                    UpdateDrawnColors();
                    break;
                case Types.Existing:
                    _progressBar = TargetProgressBar;
                    break;
            }

            if (!AlwaysVisible)
            {
                ShowBar(false);
            }

            if (_progressBar)
            {
                _progressBar.SetBar(100f, 0f, 100f);
            }
        }


        /// <summary>
        /// Draws the health bar.
        /// </summary>
        protected virtual void DrawHealthBar()
        {
            var go = new GameObject { name = "HealthBar|" + gameObject.name };
            SceneManager.MoveGameObjectToScene(go, gameObject.scene);

            if (NestDrawnHealthBar)
            {
                go.transform.SetParent(transform);
            }

            _progressBar = go.AddComponent<MMProgressBar>();

            _followTransform = go.AddComponent<MMFollowTarget>();
            _followTransform.Offset = HealthBarOffset;
            _followTransform.Target = transform;
            _followTransform.FollowRotation = FollowRotation;
            _followTransform.FollowScale = FollowScale;
            _followTransform.InterpolatePosition = false;
            _followTransform.InterpolateRotation = false;
            _followTransform.UpdateMode = FollowTargetMode;

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = Vector3.one;
            canvas.GetComponent<RectTransform>().sizeDelta = Size;
            if (!string.IsNullOrEmpty(SortingLayerName))
            {
                canvas.sortingLayerName = SortingLayerName;
            }

            var container = new GameObject();
            container.transform.SetParent(go.transform);
            container.name = "MMProgressBarContainer";
            container.transform.localScale = Vector3.one;

            var border = new GameObject();
            border.transform.SetParent(container.transform);
            border.name = "HealthBar Border";
            _borderImage = border.AddComponent<Image>();
            _borderImage.transform.position = Vector3.zero;
            _borderImage.transform.localScale = Vector3.one;
            _borderImage.GetComponent<RectTransform>().sizeDelta = Size;
            _borderImage.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

            var bg = new GameObject();
            bg.transform.SetParent(container.transform);
            bg.name = "HealthBar Background";
            _backgroundImage = bg.AddComponent<Image>();
            _backgroundImage.transform.position = Vector3.zero;
            _backgroundImage.transform.localScale = Vector3.one;
            _backgroundImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding * 2;
            _backgroundImage.GetComponent<RectTransform>().anchoredPosition = -_backgroundImage.GetComponent<RectTransform>().sizeDelta / 2;
            _backgroundImage.GetComponent<RectTransform>().pivot = Vector2.zero;

            var delayed = new GameObject();
            delayed.transform.SetParent(container.transform);
            delayed.name = "HealthBar Delayed Foreground";
            _delayedImage = delayed.AddComponent<Image>();
            _delayedImage.transform.position = Vector3.zero;
            _delayedImage.transform.localScale = Vector3.one;
            _delayedImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding * 2;
            _delayedImage.GetComponent<RectTransform>().anchoredPosition = -_delayedImage.GetComponent<RectTransform>().sizeDelta / 2;
            _delayedImage.GetComponent<RectTransform>().pivot = Vector2.zero;

            var front = new GameObject();
            front.transform.SetParent(container.transform);
            front.name = "HealthBar Foreground";
            _foregroundImage = front.AddComponent<Image>();
            _foregroundImage.transform.position = Vector3.zero;
            _foregroundImage.transform.localScale = Vector3.one;
            _foregroundImage.color = GetForegroundColor(1F);
            _foregroundImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding * 2;
            _foregroundImage.GetComponent<RectTransform>().anchoredPosition = -_foregroundImage.GetComponent<RectTransform>().sizeDelta / 2;
            _foregroundImage.GetComponent<RectTransform>().pivot = Vector2.zero;

            if (Billboard)
            {
                MMBillboard billboard = _progressBar.gameObject.AddComponent<MMBillboard>();
                billboard.NestObject = !NestDrawnHealthBar;
            }

            _progressBar.LerpDecreasingDelayedBar = LerpDelayedBar;
            _progressBar.LerpForegroundBar = LerpFrontBar;
            _progressBar.LerpDecreasingDelayedBarSpeed = LerpDelayedBarSpeed;
            _progressBar.LerpForegroundBarSpeedIncreasing = LerpFrontBarSpeed;
            _progressBar.ForegroundBar = _foregroundImage.transform;
            _progressBar.DelayedBarDecreasing = _delayedImage.transform;
            _progressBar.DecreasingDelay = Delay;
            _progressBar.BumpScaleOnChange = BumpScaleOnChange;
            _progressBar.BumpDuration = BumpDuration;
            _progressBar.BumpScaleAnimationCurve = BumpAnimationCurve;
            _progressBar.TimeScale = TimeScale == TimeScales.Time ? MMProgressBar.TimeScales.Time : MMProgressBar.TimeScales.UnscaledTime;
            container.transform.localEulerAngles = InitialRotationAngles;
            _progressBar.Initialization();
        }

        protected virtual Color GetForegroundColor(float f) => ForegroundColor.Evaluate(f);

        /// <summary>
        /// On Update, we hide or show our healthbar based on our current status
        /// </summary>
        protected virtual void Update()
        {
            if (_progressBar == null)
                return;

            if (_finalHideStarted)
                return;

            UpdateDrawnColors();

            if (AlwaysVisible)
                return;

            if (_showBar)
            {
                ShowBar(true);
                float currentTime = (TimeScale == TimeScales.UnscaledTime) ? Time.unscaledTime : Time.time;
                if (currentTime - _lastShowTimestamp > DisplayDurationOnHit)
                {
                    _showBar = false;
                }
            }
            else
            {
                if (BarIsShown())
                {
                    ShowBar(false);
                }
            }
        }

        /// <summary>
        /// Hides the bar when it reaches zero
        /// </summary>
        /// <returns>The hide bar.</returns>
        protected virtual IEnumerator<float> FinalHideBar()
        {
            _finalHideStarted = true;
            if (InstantiatedOnDeath)
            {
                GameObject instantiatedOnDeath = Instantiate(InstantiatedOnDeath, transform.position + HealthBarOffset, transform.rotation);
                SceneManager.MoveGameObjectToScene(instantiatedOnDeath.gameObject, gameObject.scene);
            }

            if (HideBarAtZeroDelay == 0)
            {
                _showBar = false;
                ShowBar(false);
                yield return Timing.WaitForOneFrame;
            }
            else
            {
                _progressBar.HideBar(HideBarAtZeroDelay);
            }
        }

        /// <summary>
        /// Updates the colors of the different bars
        /// </summary>
        protected virtual void UpdateDrawnColors()
        {
            if (HealthBarType != Types.Drawn)
                return;

            if (_progressBar.Bumping)
                return;

            if (_borderImage)
                _borderImage.color = BorderColor.Evaluate(_progressBar.BarProgress);

            if (_backgroundImage)
                _backgroundImage.color = BackgroundColor.Evaluate(_progressBar.BarProgress);

            if (_delayedImage)
                _delayedImage.color = DelayedColor.Evaluate(_progressBar.BarProgress);

            if (_foregroundImage)
                _foregroundImage.color = GetForegroundColor(_progressBar.BarProgress);
        }

        /// <summary>
        /// Updates the bar
        /// </summary>
        /// <param name="currentHealth">Current health.</param>
        /// <param name="minHealth">Minimum health.</param>
        /// <param name="maxHealth">Max health.</param>
        /// <param name="show">Whether we should show the bar.</param>
        public virtual void UpdateBar(float currentHealth, float minHealth, float maxHealth, bool show)
        {
            // if the healthbar isn't supposed to be always displayed, we turn it on for the specified duration
            if (!AlwaysVisible && show)
            {
                _showBar = true;
                _lastShowTimestamp = TimeScale == TimeScales.UnscaledTime ? Time.unscaledTime : Time.time;
            }

            if (_progressBar)
            {
                _progressBar.UpdateBar(currentHealth, minHealth, maxHealth);

                if (HideBarAtZero && _progressBar.BarTarget <= 0)
                {
                    Timing.RunCoroutine(FinalHideBar());
                }

                if (BumpScaleOnChange)
                {
                    _progressBar.Bump();
                }
            }
        }

        /// <summary>
        /// A test method used to update the bar when pressing the TestUpdateHealth button in the inspector
        /// </summary>
        protected virtual void TestUpdateHealth()
        {
            UpdateBar(TestCurrentHealth, TestMinHealth, TestMaxHealth, true);
        }
    }
}