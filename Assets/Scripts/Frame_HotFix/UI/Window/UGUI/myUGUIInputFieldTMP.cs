// #if USE_TMP

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static StringUtility;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseUtility;

// 对TextMeshPro的InputField的封装
public class myUGUIInputFieldTMP : myUGUIObject, IInputField
{
    protected UnityAction<string> _onEditEnd; // 避免GC的委托
    protected UnityAction<string> _onEditing; // 避免GC的委托
    protected TMP_InputField mInputField; // TextMeshPro的InputField组件
    protected StringCallback OnEndEdit; // 输入结束时的回调
    protected StringCallback OnEditing; // 输入结束时的回调
    protected bool endNeedEnter; // 是否需要按下回车键才会认为是输入结束,false则是只要输入框失去焦点就认为输入结束而调用mEditorEndCallback

    public myUGUIInputFieldTMP()
    {
        _onEditEnd = onEndEdit;
        _onEditing = onEditing;
    }

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out mInputField))
        {
            if (!isNewObject)
            {
                logError("需要添加一个TMP_InputField组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            mInputField = go.AddComponent<TMP_InputField>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }
    }

    public override void setAlpha(float alpha, bool fadeChild)
    {
        base.setAlpha(alpha, fadeChild);
        Color color = mInputField.textComponent.color;
        color.a = alpha;
        mInputField.textComponent.color = color;
    }

    public void setOnEndEdit(StringCallback action, bool needEnter = true)
    {
        OnEndEdit = action;
        endNeedEnter = needEnter;
        mInputField.onEndEdit.AddListener(_onEditEnd);
    }

    public void setOnEditing(StringCallback action)
    {
        OnEditing = action;
        mInputField.onValueChanged.AddListener(_onEditing);
    }

    public void cleanUp()
    {
        setText(EMPTY);
    }

    public void setText(string value)
    {
        mInputField.text = value;
    }

    public void setText(int value)
    {
        setText(IToS(value));
    }

    public void setText(float value)
    {
        setText(FToS(value, 2));
    }

    public string getText()
    {
        return mInputField.text;
    }

    public bool isFocused()
    {
        return mInputField.isFocused;
    }

    public bool isVisible()
    {
        return isActive();
    }

    public void focus(bool active = true)
    {
        if (active)
        {
            mInputField.ActivateInputField();
        }
        else
        {
            mInputField.DeactivateInputField();
        }
    }

    public void clear(bool removeFocus = true)
    {
        setText(EMPTY);
        if (removeFocus)
        {
            focus(false);
        }
    }

    public void setCharacterLimit(int limit)
    {
        mInputField.characterLimit = limit;
    }

    public void setCaretPosition(int pos)
    {
        mInputField.caretPosition = pos;
    }

    public int getCaretPosition()
    {
        return mInputField.caretPosition;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void onEndEdit(string value)
    {
        // 只处理由回车触发的输入结束,移动端不处理
        if (!isRealMobile() && endNeedEnter && !isKeyDown(KeyCode.Return, FOCUS_MASK.UI) && !isKeyDown(KeyCode.KeypadEnter, FOCUS_MASK.UI))
        {
            return;
        }

        OnEndEdit?.Invoke(value);
    }

    protected void onEditing(string value)
    {
        OnEditing?.Invoke(value);
    }
}
// #endif