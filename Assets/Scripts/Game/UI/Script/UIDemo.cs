using UnityEngine;
using UnityEngine.UI;

public class UIDemo : GameLayout
{
    protected Transform background;
    protected Text label;

    public override void assignWindow()
    {
        getUIComponent(out background, "Background");
        getUIComponent(out label, "Label");
    }

    public override void init()
    {
    }

    public void setText(string text)
    {
        label.text = text;
    }
}