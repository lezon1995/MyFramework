using System;
using System.Collections.Generic;
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
	protected CharacterDetailView characterDetailView;
	protected CharacterListView characterListView;
	protected myUGUIButton btnConfirm;
    // auto generate member end

    public Action<PlayerDef> onPlayerSelected;
    public Action onNextStep;

    public SelectPlayerPanel()
    {
        // auto generate constructor start
		characterDetailView = new(this);
		characterListView = new(this);
        // auto generate constructor end
        mNeedUpdate = false;
    }

    public override void assignWindow()
    {
        // auto generate assignWindow start
		newObject(out textTitle, "Content/V/TextTitle");
		characterDetailView.assignWindow(mRoot, "Content/V/CharacterDetailView");
		characterListView.assignWindow(mRoot, "Content/V/CharacterListView");
		newObject(out btnConfirm, "Content/BtnConfirm");
        // auto generate assignWindow end
    }

    public override void init()
    {
        base.init();
        // auto generate init start
        // auto generate init end

        characterListView.setCharacterDetailView(characterDetailView);
        characterListView.setSelectPlayerPanel(this);
        characterListView.initPlayerItems();
        initNextStepButton();
        updateNextStepButton();
    }

    void initNextStepButton()
    {
        btnConfirm.setUGUIButtonClick(onNextStepClicked);
    }

    public void updateNextStepButton()
    {
        bool canClick = characterListView.isCharacterSelected();
        btnConfirm.setInteractable(canClick);
    }

    void onNextStepClicked()
    {
        if (!characterListView.isCharacterSelected())
            return;

        onPlayerSelected?.Invoke(characterListView.selectedPlayer);
        onNextStep?.Invoke();
    }

    public override void onGameState()
    {
        base.onGameState();
    }

    public override void destroy()
    {
        base.destroy();
        onPlayerSelected = null;
        onNextStep = null;
    }

    public void resetSelection()
    {
        characterListView.RefreshPlayerItems();
        updateNextStepButton();
    }
}