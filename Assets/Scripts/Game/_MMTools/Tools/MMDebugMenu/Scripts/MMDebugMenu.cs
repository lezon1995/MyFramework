using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// A debug menu helper, meant to help create quick mobile friendly debug menus
    /// </summary>
    public class MMDebugMenu : MonoBehaviour
    {
        /// the possible directions for the menu to appear
        public enum ToggleDirections
        {
            TopToBottom,
            LeftToRight,
            RightToLeft,
            BottomToTop
        }

        [Header("Data")]
        /// the scriptable object containing the menu's data
        public MMDebugMenuData Data;

        [Header("Bindings")]
        /// the container of the whole menu
        public CanvasGroup MenuContainer;

        /// the scrolling contents
        public RectTransform Contents;

        /// the menu's background image
        public Image MenuBackground;

        /// the icon used to close the menu
        public Image CloseIcon;

        /// the tab bar (where the tab buttons go)
        public RectTransform TabBar;

        /// the tab contents container (where the contents of the page will go)
        public RectTransform TabContainer;

        /// the tab manager
        public MMDebugMenuTabManager TabManager;

        /// the MoreMountains logo
        public Image MMLogo;

        [Header("Events")]
        /// an event to call when the menu opens
        public UnityEvent OnOpenEvent;

        /// an event to call when the menu closes
        public UnityEvent OnCloseEvent;

        [Header("Test")]
        /// whether or not this menu is active at this moment
        [MMReadOnly]
        public bool Active;

        /// a test button to toggle the menu
        [MMInspectorButton("ToggleMenu")]
        public bool ToggleButton;

        protected RectTransform _containerRect;
        protected Vector3 _initialContainerPosition;
        protected Vector3 _offPosition;
        protected Vector3 _newPosition;
        protected bool _toggling;

        /// <summary>
        /// On Start we init our menu
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Prepares transitions and grabs components
        /// </summary>
        protected virtual void Initialization()
        {
            if (Data != null)
            {
                FillMenu();
            }

            CloseIcon.color = Data.TextColor;
            _containerRect = MenuContainer.GetComponent<RectTransform>();
            _initialContainerPosition = _containerRect.localPosition;
            MenuBackground.color = Data.BackgroundColor;
            switch (Data.ToggleDirection)
            {
                case ToggleDirections.RightToLeft:
                    _offPosition = _initialContainerPosition + Vector3.right * _containerRect.rect.width;
                    break;
                case ToggleDirections.LeftToRight:
                    _offPosition = _initialContainerPosition + Vector3.left * _containerRect.rect.width;
                    break;
                case ToggleDirections.TopToBottom:
                    _offPosition = _initialContainerPosition + Vector3.up * _containerRect.rect.height;
                    break;
                case ToggleDirections.BottomToTop:
                    _offPosition = _initialContainerPosition + Vector3.down * _containerRect.rect.height;
                    break;
            }

            _containerRect.localPosition = _offPosition;
        }

        /// <summary>
        /// Fills the menu based on the data's contents
        /// </summary>
        public virtual void FillMenu(bool triggerEvents = false)
        {
            int tabCounter = 0;
            if (MMLogo != null)
            {
                MMLogo.color = Data.TextColor;
            }

            foreach (Transform child in Contents.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in TabBar.transform)
                Destroy(child.gameObject);

            TabManager.Tabs.Clear();
            TabManager.TabsContents.Clear();

            foreach (MMDebugMenuTabData tab in Data.Tabs)
            {
                if (!tab.Active)
                {
                    continue;
                }

                // create tab in the menu
                MMDebugMenuTab tabBarTab = Instantiate(Data.TabPrefab, TabBar, true);
                tabBarTab.SelectedBackgroundColor = Data.TextColor;
                tabBarTab.SelectedTextColor = Data.BackgroundColor;
                tabBarTab.DeselectedBackgroundColor = Data.BackgroundColor;
                tabBarTab.DeselectedTextColor = Data.TextColor;
                tabBarTab.TabText.text = tab.Name;
                tabBarTab.TabText.font = Data.RegularFont;
                tabBarTab.Index = tabCounter;
                tabBarTab.Manager = TabManager;
                TabManager.Tabs.Add(tabBarTab);

                // create tab contents
                MMDebugMenuTabContents contents = Instantiate(Data.TabContentsPrefab, TabContainer, true);
                RectTransform rectTransform = contents.GetComponent<RectTransform>();
                rectTransform.MMSetLeft(0f);
                rectTransform.MMSetRight(0f);
                rectTransform.MMSetTop(0f);
                rectTransform.MMSetBottom(0f);
                contents.Index = tabCounter;
                FillTab(contents, tabCounter, triggerEvents);
                if (tabCounter == Data.InitialActiveTabIndex)
                {
                    contents.gameObject.SetActive(true);
                    tabBarTab.Select();
                }
                else
                {
                    contents.gameObject.SetActive(false);
                    tabBarTab.Deselect();
                }

                TabManager.TabsContents.Add(contents);

                tabCounter++;
            }

            // debug tab
            if (Data.DisplayDebugTab)
            {
                MMDebugMenuTab tabBarTab = Instantiate(Data.TabPrefab, TabBar, true);
                tabBarTab.SelectedBackgroundColor = Data.TextColor;
                tabBarTab.SelectedTextColor = Data.BackgroundColor;
                tabBarTab.DeselectedBackgroundColor = Data.BackgroundColor;
                tabBarTab.DeselectedTextColor = Data.TextColor;
                tabBarTab.TabText.text = Data.DebugTabName;
                tabBarTab.TabText.font = Data.RegularFont;
                tabBarTab.Index = tabCounter;
                tabBarTab.Manager = TabManager;
                TabManager.Tabs.Add(tabBarTab);

                MMDebugMenuDebugTab debugTab = Instantiate(Data.DebugTabPrefab, TabContainer, true);
                debugTab.DebugText.color = Data.TextColor;
                debugTab.DebugText.font = Data.RegularFont;

                debugTab.CommandPrompt.textComponent.font = Data.RegularFont;
                debugTab.CommandPrompt.textComponent.color = Data.TextColor;

                debugTab.CommandPromptCharacter.font = Data.RegularFont;
                debugTab.CommandPromptCharacter.color = Data.TextColor;

                MMDebugMenuTabContents debugTabContents = debugTab.GetComponent<MMDebugMenuTabContents>();
                debugTabContents.Index = tabCounter;
                TabManager.TabsContents.Add(debugTabContents);
                RectTransform rectTransform = debugTabContents.GetComponent<RectTransform>();
                rectTransform.MMSetLeft(0f);
                rectTransform.MMSetRight(0f);
                rectTransform.MMSetTop(0f);
                rectTransform.MMSetBottom(0f);
                if (tabCounter == Data.InitialActiveTabIndex)
                {
                    debugTab.gameObject.SetActive(true);
                    TabManager.Tabs[tabCounter].Select();
                }
                else
                {
                    debugTab.gameObject.SetActive(false);
                    TabManager.Tabs[tabCounter].Deselect();
                }

                tabCounter++;
            }

            // fill with spacers 
            int spacerCount = Data.MaxTabs - tabCounter;
            for (int i = 0; i < spacerCount; i++)
            {
                Instantiate(Data.TabSpacerPrefab, TabBar, true);
            }
        }

        protected virtual void FillTab(MMDebugMenuTabContents tab, int index, bool triggerEvents = false)
        {
            Transform parent = tab.Parent;

            foreach (MMDebugMenuItem item in Data.Tabs[index].MenuItems)
            {
                if (!item.Active)
                {
                    continue;
                }

                switch (item.Type)
                {
                    case MMDebugMenuItem.MMDebugMenuItemTypes.Button:
                        MMDebugMenuItemButton button;
                        button = (item.ButtonType == MMDebugMenuItem.MMDebugMenuItemButtonTypes.Border) ? Instantiate(Data.ButtonBorderPrefab) : Instantiate(Data.ButtonPrefab);
                        button.name = "MMDebugMenuItemButton_" + item.Name;
                        button.ButtonText.text = item.ButtonText;
                        button.ButtonEventName = item.ButtonEventName;
                        if (item.ButtonType == MMDebugMenuItem.MMDebugMenuItemButtonTypes.Border)
                        {
                            button.ButtonText.color = Data.AccentColor;
                            button.ButtonBg.color = Data.TextColor;
                        }
                        else
                        {
                            button.ButtonText.color = Data.BackgroundColor;
                            button.ButtonBg.color = Data.AccentColor;
                        }

                        button.ButtonText.font = Data.RegularFont;
                        button.transform.SetParent(parent);
                        break;

                    case MMDebugMenuItem.MMDebugMenuItemTypes.Checkbox:
                        MMDebugMenuItemCheckbox checkbox = Instantiate(Data.CheckboxPrefab, parent, true);
                        checkbox.name = "MMDebugMenuItemCheckbox_" + item.Name;
                        checkbox.SwitchText.text = item.CheckboxText;
                        if (item.CheckboxInitialState)
                        {
                            checkbox.Switch.SetTrue();
                        }
                        else
                        {
                            checkbox.Switch.SetFalse();
                        }

                        checkbox.CheckboxEventName = item.CheckboxEventName;
                        checkbox.Switch.GetComponent<Image>().color = Data.AccentColor;
                        checkbox.SwitchText.color = Data.TextColor;
                        checkbox.SwitchText.font = Data.RegularFont;
                        break;

                    case MMDebugMenuItem.MMDebugMenuItemTypes.Slider:
                        MMDebugMenuItemSlider slider = Instantiate(Data.SliderPrefab, parent, true);
                        slider.name = "MMDebugMenuItemSlider_" + item.Name;
                        slider.Mode = item.SliderMode;
                        slider.RemapZero = item.SliderRemapZero;
                        slider.RemapOne = item.SliderRemapOne;
                        slider.TargetSlider.value = MMMaths.Remap(item.SliderInitialValue, item.SliderRemapZero, item.SliderRemapOne, 0f, 1f);

                        slider.SliderText.text = item.SliderText;
                        slider.SliderText.color = Data.TextColor;
                        slider.SliderText.font = Data.RegularFont;

                        slider.SliderValueText.text = (item.SliderMode == MMDebugMenuItemSlider.Modes.Int) ? item.SliderInitialValue.ToString() : item.SliderInitialValue.ToString("F3");
                        slider.SliderValueText.color = Data.AccentColor;
                        slider.SliderValueText.font = Data.BoldFont;

                        slider.SliderKnob.color = Data.AccentColor;
                        slider.SliderLine.color = Data.TextColor;

                        slider.SliderEventName = item.SliderEventName;
                        break;

                    case MMDebugMenuItem.MMDebugMenuItemTypes.Spacer:
                        GameObject spacerPrefab = (item.SpacerType == MMDebugMenuItem.MMDebugMenuItemSpacerTypes.Small) ? Data.SpacerSmallPrefab : Data.SpacerBigPrefab;
                        GameObject spacer = Instantiate(spacerPrefab, parent, true);
                        spacer.name = "MMDebugMenuItemSpacer_" + item.Name;
                        break;

                    case MMDebugMenuItem.MMDebugMenuItemTypes.Title:
                        MMDebugMenuItemTitle title = Instantiate(Data.TitlePrefab, parent, true);
                        title.name = "MMDebugMenuItemSlider_" + item.Name;
                        title.TitleText.text = item.TitleText;
                        title.TitleText.color = Data.TextColor;
                        title.TitleText.font = Data.BoldFont;
                        title.TitleLine.color = Data.AccentColor;
                        break;

                    case MMDebugMenuItem.MMDebugMenuItemTypes.Choices:
                        MMDebugMenuItemChoices choicesPrefab;
                        if (item.ChoicesType == MMDebugMenuItem.MMDebugMenuItemChoicesTypes.TwoChoices)
                        {
                            choicesPrefab = Data.TwoChoicesPrefab;
                        }
                        else
                        {
                            choicesPrefab = Data.ThreeChoicesPrefab;
                        }

                        MMDebugMenuItemChoices choices = Instantiate(choicesPrefab, parent, true);
                        choices.name = "MMDebugMenuItemChoices_" + item.Name;

                        choices.Choices[0].ButtonText.text = item.ChoiceOneText;
                        choices.Choices[1].ButtonText.text = item.ChoiceTwoText;

                        choices.Choices[0].ButtonEventName = item.ChoiceOneEventName;
                        choices.Choices[1].ButtonEventName = item.ChoiceTwoEventName;

                        if (item.ChoicesType == MMDebugMenuItem.MMDebugMenuItemChoicesTypes.ThreeChoices)
                        {
                            choices.Choices[2].ButtonEventName = item.ChoiceThreeEventName;
                            choices.Choices[2].ButtonText.text = item.ChoiceThreeText;
                        }

                        choices.OffColor = Data.BackgroundColor;
                        choices.OnColor = Data.TextColor;
                        choices.AccentColor = Data.AccentColor;

                        foreach (MMDebugMenuChoiceEntry entry in choices.Choices)
                        {
                            if (entry != null)
                            {
                                entry.ButtonText.font = Data.RegularFont;
                            }
                        }

                        choices.Select(item.SelectedChoice);
                        if (triggerEvents)
                            choices.TriggerButtonEvent(item.SelectedChoice);

                        break;

                    case MMDebugMenuItem.MMDebugMenuItemTypes.Value:
                        MMDebugMenuItemValue value = Instantiate(Data.ValuePrefab, parent, true);
                        value.name = "MMDebugMenuItemValue_" + item.Name;
                        value.LabelText.text = item.ValueLabel;
                        value.LabelText.color = Data.TextColor;
                        value.LabelText.font = Data.RegularFont;
                        value.ValueText.text = item.ValueInitialValue;
                        value.ValueText.color = Data.AccentColor;
                        value.ValueText.font = Data.BoldFont;
                        value.RadioReceiver.Channel = item.ValueMMRadioReceiverChannel;
                        break;

                    case MMDebugMenuItem.MMDebugMenuItemTypes.Text:

                        MMDebugMenuItemText textPrefab;
                        switch (item.TextType)
                        {
                            case MMDebugMenuItem.MMDebugMenuItemTextTypes.Tiny:
                                textPrefab = Data.TextTinyPrefab;
                                break;
                            case MMDebugMenuItem.MMDebugMenuItemTextTypes.Small:
                                textPrefab = Data.TextSmallPrefab;
                                break;
                            case MMDebugMenuItem.MMDebugMenuItemTextTypes.Long:
                                textPrefab = Data.TextLongPrefab;
                                break;
                            default:
                                textPrefab = Data.TextTinyPrefab;
                                break;
                        }

                        MMDebugMenuItemText text = Instantiate(textPrefab, parent, true);
                        text.name = "MMDebugMenuItemText_" + item.Name;
                        text.ContentText.text = item.TextContents;
                        text.ContentText.color = Data.TextColor;
                        text.ContentText.font = Data.RegularFont;
                        break;
                }
            }

            // we always add a spacer at the end because scrollviews are terrible
            GameObject finalSpacer = Instantiate(Data.SpacerBigPrefab, parent, true);
            finalSpacer.name = "MMDebugMenuItemSpacer_FinalSpacer";
        }

        /// <summary>
        /// Makes the menu appear
        /// </summary>
        public virtual void OpenMenu()
        {
            OnOpenEvent?.Invoke();
            Timing.RunCoroutine(ToggleCo(false));
        }

        /// <summary>
        /// Makes the menu disappear
        /// </summary>
        public virtual void CloseMenu()
        {
            Timing.RunCoroutine(ToggleCo(true));
        }

        /// <summary>
        /// Closes or opens the menu depending on its current state
        /// </summary>
        public virtual void ToggleMenu()
        {
            Timing.RunCoroutine(ToggleCo(Active));
        }

        /// <summary>
        /// A coroutine used to toggle the menu
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        protected virtual IEnumerator<float> ToggleCo(bool active)
        {
            if (_toggling)
            {
                yield break;
            }

            if (!active)
            {
                OnOpenEvent?.Invoke();
                _containerRect.gameObject.SetActive(true);
            }

            _toggling = true;
            Active = active;
            _newPosition = active ? _offPosition : _initialContainerPosition;
            MMTween.MoveRectTransform(_containerRect, _containerRect.localPosition, _newPosition, 0f, 0f, Data.ToggleDuration, Data.ToggleCurve, ignoreTimescale: true);
            yield return Timing.WaitUntilDone(MMCoroutine.WaitForUnscaled(Data.ToggleDuration));
            if (active)
            {
                OnCloseEvent?.Invoke();
                _containerRect.gameObject.SetActive(false);
            }

            Active = !active;
            _toggling = false;
        }

        /// <summary>
        /// On update we handle our input
        /// </summary>
        protected virtual void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// Looks for shortcut input
        /// </summary>
        protected virtual void HandleInput()
        {
            bool input;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				input = Keyboard.current[Data.ToggleKey].wasPressedThisFrame;
#else
            input = Input.GetKeyDown(Data.ToggleShortcut);
#endif

            if (input)
            {
                ToggleMenu();
            }
        }

        /// <summary>
        /// Routes console logs to the MMDebugConsole 
        /// </summary>
        /// <param name="logString"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        protected virtual void CaptureConsoleLog(string logString, string stackTrace, LogType type)
        {
            MMDebug.LogDebugToConsole(logString + " (" + type + ")", "#00FFFF", 3, false);
        }

        /// <summary>
        /// On Enable, we start listening for log messages
        /// </summary>   
        protected virtual void OnEnable()
        {
            Application.logMessageReceived += CaptureConsoleLog;
        }

        /// <summary>
        /// On Disable, we stop listening for log messages
        /// </summary>
        protected virtual void OnDisable()
        {
            Application.logMessageReceived -= CaptureConsoleLog;
        }
    }
}