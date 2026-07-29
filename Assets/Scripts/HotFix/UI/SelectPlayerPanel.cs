using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using Obfuz;
using PrimeTween;
using UnityEngine;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/SelectPlayerPanel.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class SelectPlayerPanel : LayoutScript
// auto generate classname end
{
    // auto generate member start
    protected myUGUITextTMP textTitle;
    protected SelectedCharacterDetailView selectedCharacterDetailView;
    protected SelectCharacterListView selectCharacterListView;
    protected SelectBallListView selectBallListView;
    protected SelectRelicListView selectRelicListView;
    protected myUGUIButton btnNext;
    protected myUGUIButton btnPrevious;
    // auto generate member end

    public Action<CharSelectInfo> onSubmitCharacterSelectInfo;
    public Action onPrevStep;
    public Action onNextStep;
    MMStateMachine<int> stepState = new();

    public SelectPlayerPanel()
    {
        // auto generate constructor start
        selectedCharacterDetailView = new(this);
        selectCharacterListView = new(this);
        selectBallListView = new(this);
        selectRelicListView = new(this);
        // auto generate constructor end
        mNeedUpdate = false;
        stepState.OnStateChange = OnStepStateChange;
    }

    public override void assignWindow()
    {
        // auto generate assignWindow start
        newObject(out textTitle, "Content/V/TextTitle");
        selectedCharacterDetailView.assignWindow(mRoot, "Content/V/CentralArea/SelectedCharacterDetailView");
        selectCharacterListView.assignWindow(mRoot, "Content/V/SelectionArea/SelectCharacterListView");
        selectBallListView.assignWindow(mRoot, "Content/V/SelectionArea/SelectBallListView");
        selectRelicListView.assignWindow(mRoot, "Content/V/SelectionArea/SelectRelicListView");
        newObject(out btnNext, "Content/BtnNext");
        newObject(out btnPrevious, "Content/BtnPrevious");
        // auto generate assignWindow end
    }

    public override void init()
    {
        base.init();
        // auto generate init start
        // auto generate init end

        selectCharacterListView.setCharacterDetailView(selectedCharacterDetailView);
        selectCharacterListView.setSelectPlayerPanel(this);
        selectCharacterListView.initPlayerItems();

        selectBallListView.setCharacterDetailView(selectedCharacterDetailView);
        selectBallListView.initBallItems();

        selectRelicListView.setCharacterDetailView(selectedCharacterDetailView);
        selectRelicListView.initRelicItems();
        
        initButtons();
        updateNextStepButton();

        stepState.ChangeState(1);
    }

    void initButtons()
    {
        btnNext.setUGUIButtonClick(onNextStepClicked);
        btnPrevious.setUGUIButtonClick(onPreviousStepClicked);
    }

    public void updateNextStepButton()
    {
        bool canClick = selectCharacterListView.isCharacterSelected();
        btnNext.setInteractable(canClick);
    }

    void onNextStepClicked()
    {
        if (stepState.CurrentState == 3)
        {
            onSubmitCharacterSelectInfo?.Invoke(_charSelectInfo);
            return;
        }

        if (!selectCharacterListView.isCharacterSelected())
            return;

        onNextStep?.Invoke();

        stepState.ChangeState(stepState.CurrentState + 1);
    }

    void onPreviousStepClicked()
    {
        onPrevStep?.Invoke();

        stepState.ChangeState(stepState.CurrentState - 1);
    }

    public override void onGameState()
    {
        base.onGameState();
    }

    public override void destroy()
    {
        base.destroy();
        onSubmitCharacterSelectInfo = null;
        onNextStep = null;
    }

    public void resetSelection()
    {
        selectCharacterListView.RefreshPlayerItems();
        updateNextStepButton();
    }

    public void setOnNextStepClick(Action a)
    {
        onNextStep = a;
    }

    public void setOnSubmitCharacterSelectInfo(Action<CharSelectInfo> a)
    {
        onSubmitCharacterSelectInfo = a;
    }

    void OnStepStateChange(int pre, int cur)
    {
        switch (cur)
        {
            case 1:
                btnPrevious.setActive(false);
                selectCharacterListView.setActive(true);
                selectBallListView.setActive(false);
                selectRelicListView.setActive(false);
                break;
            case 2:
                btnPrevious.setActive(true);
                selectCharacterListView.setActive(false);
                selectBallListView.setActive(true);
                selectRelicListView.setActive(false);
                break;
            case 3:
                btnPrevious.setActive(true);
                selectCharacterListView.setActive(false);
                selectBallListView.setActive(false);
                selectRelicListView.setActive(true);
                break;
        }
    }
}