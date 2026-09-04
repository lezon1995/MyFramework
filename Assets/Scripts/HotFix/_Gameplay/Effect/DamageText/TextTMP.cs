using TMPro;
using UnityEngine;

namespace MoreMountains
{
    public class TextTMP
    {
        protected TextMeshProUGUI _text;
        TextMeshProUGUI _textOutline;

        public TextTMP(TextMeshProUGUI t, TextMeshProUGUI tOutline)
        {
            _text = t;
            _textOutline = tOutline;
        }

        public virtual string text
        {
            get => _text.text;
            set
            {
                _text.SetText(value);;
                _textOutline.SetText(value);
            }
        }

        public virtual Color color
        {
            get => _text.color;
            set => _text.color = value;
        }

        public virtual float fontSize
        {
            get => _text.fontSize;
            set
            {
                _text.fontSize = value;
                _textOutline.fontSize = value;
            }
        }

        public virtual float outlineSize
        {
            get => _text.outlineWidth;
            set
            {
            }
        }

        public virtual Color outlineColor
        {
            get => _text.outlineColor;
            set
            {
            }
        }
    }
}