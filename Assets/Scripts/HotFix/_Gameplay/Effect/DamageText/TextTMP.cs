using TMPro;
using UnityEngine;

namespace MoreMountains;

public class TextTMP : ClassObject, IText, IArgs<TextMeshProUGUI, TextMeshProUGUI>
{
    TextMeshProUGUI _text;
    TextMeshProUGUI _textOutline;

    public override void resetProperty()
    {
        base.resetProperty();
        _text = null;
        _textOutline = null;
    }

    public void onCreate(TextMeshProUGUI t, TextMeshProUGUI tOutline)
    {
        _text = t;
        _textOutline = tOutline;
    }

    public string text
    {
        get => _text.text;
        set
        {
            _text.SetText(value);;
            _textOutline.SetText(value);
        }
    }

    public Color color
    {
        get => _text.color;
        set => _text.color = value;
    }

    public float fontSize
    {
        get => _text.fontSize;
        set
        {
            _text.fontSize = value;
            _textOutline.fontSize = value;
        }
    }

    public float outlineSize
    {
        get => _text.outlineWidth;
        set
        {
        }
    }

    public Color outlineColor
    {
        get => _text.outlineColor;
        set
        {
        }
    }
}