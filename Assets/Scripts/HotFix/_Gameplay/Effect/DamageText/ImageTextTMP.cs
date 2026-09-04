using TMPro;
using UnityEngine;

namespace MoreMountains
{
    public class ImageTextTMP : TextTMP
    {
        public ImageTextTMP(TextMeshProUGUI t) : base(t, null)
        {
        }

        public override string text
        {
            get => _text.text;
            set
            {
                
                
                _text.SetText(value);
            }
        }

        public override Color color
        {
            get => _text.color;
            set { }
        }

        public override float fontSize
        {
            get => _text.fontSize;
            set { }
        }

        public override float outlineSize
        {
            get => _text.outlineWidth;
            set { }
        }

        public override Color outlineColor
        {
            get => _text.outlineColor;
            set { }
        }
    }
}