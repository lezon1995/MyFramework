using System;
using TMPro;
using UnityEngine;
using UIEventSystem = UnityEngine.EventSystems.EventSystem;
using UnityEngine.UI;

namespace MoreMountains;

/// <summary>主菜单子页面共用的框架生命周期、返回和焦点处理。</summary>
public abstract class MainMenuPage : LayoutScript
{
    protected TMP_Text title;
    protected TMP_Text status;
    protected Button back;
    Action onClosed;
    GameObject previousSelection;

    protected T Find<T>(string path) where T : Component => mTransform.Find(path).GetComponent<T>();
    protected static string L(string zh, string en) => MenuPreferences.IsChinese ? zh : en;
    protected void Label(string path, string text) => Find<TMP_Text>(path).text = text;
    protected void Click(string path, UnityEngine.Events.UnityAction action) => Find<Button>(path).onClick.AddListener(action);

    public override void assignWindow()
    {
        title = Find<TMP_Text>("Panel/Title");
        status = Find<TMP_Text>("Panel/Status");
        back = Find<Button>("Panel/Back");
    }

    public override void init()
    {
        base.init();
        back.onClick.AddListener(close);
    }

    public void Open(Action closed)
    {
        onClosed = closed;
        previousSelection = UIEventSystem.current?.currentSelectedGameObject;
        Label("Panel/Back/Text", L("返回", "Back"));
        Refresh();
        back.Select();
    }

    protected abstract void Refresh();

    public override bool onESCDown()
    {
        close();
        return true;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        var size = mTransform.rect.size;
        var scale = Mathf.Min(size.x / 1920f, size.y / 1080f);
        mTransform.Find("Panel").localScale = Vector3.one * scale;
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
            close();
    }

    public override void onHide()
    {
        base.onHide();
        var callback = onClosed;
        onClosed = null;
        callback?.Invoke();
        if (previousSelection != null && previousSelection.activeInHierarchy)
            UIEventSystem.current?.SetSelectedGameObject(previousSelection);
        previousSelection = null;
    }

    public override void destroy()
    {
        onClosed = null;
        previousSelection = null;
        base.destroy();
    }
}
