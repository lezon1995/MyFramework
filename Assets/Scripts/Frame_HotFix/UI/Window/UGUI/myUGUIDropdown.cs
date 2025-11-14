using System.Collections.Generic;
using UnityEngine.UI;
using static UnityUtility;

// 封装的UGUI的Dropdown下拉列表
public class myUGUIDropdown : myUGUIObject
{
    protected Dropdown dropdown; // UGUI的Dropdown组件

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out dropdown))
        {
            if (!isNewObject)
            {
                logError("需要添加一个Dropdown组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            dropdown = go.AddComponent<Dropdown>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }
    }

    public Dropdown getDropdown()
    {
        return dropdown;
    }

    public void clearOptions()
    {
        dropdown.ClearOptions();
    }

    public void addOptions(List<string> opstions)
    {
        dropdown.AddOptions(opstions);
    }

    public void setSelect(int value)
    {
        dropdown.value = value;
    }

    public int getSelect()
    {
        return dropdown.value;
    }

    public string getText()
    {
        return dropdown.options[dropdown.value].text;
    }
}