using TMPro;
using UnityEngine;

namespace MarbleHero;

public class TextTMP : IText
{
    static readonly int UnderlayColor = Shader.PropertyToID("_UnderlayColor");
    TextMeshProUGUI _text;
    bool _useUnderlay;
    float _outlineDivider = 6;

    public TextTMP(TextMeshProUGUI t, bool useUnderlay = true)
    {
        _text = t;
        _useUnderlay = useUnderlay;
    }

    public string text
    {
        get => _text.text;
        set => _text.text = value;
    }

    public Color color
    {
        get => _text.color;
        set => _text.color = value;
    }

    public float fontSize
    {
        get => _text.fontSize;
        set => _text.fontSize = value;
    }

    public float outlineSize
    {
        get => _text.outlineWidth;
        set
        {
            if (!_useUnderlay)
                _text.outlineWidth = value / _outlineDivider;
        }
    }

    public Color outlineColor
    {
        get => _text.outlineColor;
        set
        {
            if (!_useUnderlay)
                _text.outlineColor = value;
            else
                _text.fontMaterial.SetColor(UnderlayColor, value);
        }
    }
}