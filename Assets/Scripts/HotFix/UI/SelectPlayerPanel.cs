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
        
        /*btnConfirm.registeCollider(vec3 =>
        {
            Debug.Log($"click at pos={vec3}");
        });
        if (btnConfirm.getOrAddComponent<COMWindowDrag>(out var drag))
        {
            drag.setDragStartCallback((ComponentOwner obj, TouchPoint point, ref bool allowDrag) =>
            {
                Debug.Log($"Drag Start obj={obj.getName()} point={point.getCurPosition()} allowDrag={allowDrag}");
            });
            drag.setDraggingCallback((obj, pos) =>
            {
                Debug.Log($"Dragging obj={obj.getName()} pos={pos}");
            });
            drag.setDragEndCallback((obj, pos, cancel) =>
            {
                Debug.Log($"Drag End obj={obj.getName()} pos={pos} cancel={cancel}");
            });
            drag.setDragEndTotallyCallback((obj, pos, cancel) =>
            {
                Debug.Log($"Drag End Totally obj={obj.getName()} pos={pos} cancel={cancel}");
            });
        }
        btnConfirm.setOnDragHover((e, pos, hover) =>
        {
            Debug.Log($"OnDragHover {e.getName()} pos={pos} hover={hover}");
        });
        // btnConfirm.setPassDragEvent();
        btnConfirm.setOnReceiveDrag((IMouseEventCollect e, Vector3 pos, ref bool flag) =>
        {
            Debug.Log($"OnReceiveDrag {e.getName()} pos={pos} flag={flag}");
        });*/
        
        
        if (btnConfirm.tryGetUnityComponent<UIEventListener>(out var listener))
        {
            listener.SetOnDropped(data =>
            {
                Debug.Log($"Dropped point={data.position}");
            });
            listener.SetOnPotentialDragInitialized(data =>
            {
                Debug.Log($"OnPotentialDragInitialized point={data.position}");
            });
            listener.SetOnDragStarted(data =>
            {
                Debug.Log($"Drag Start point={data.position}");
            });
            listener.SetOnDragging(data =>
            {
                Debug.Log($"Dragging point={data.position}");
            });
            listener.SetOnDragEnded(data =>
            {
                Debug.Log($"Drag End point={data.position}");
            });
            listener.SetOnDragReleasedOverUI(data =>
            {
                Debug.Log($"Drag ReleasedOverUI obj={data.TopmostGameObject?.name}");
            });
        }
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