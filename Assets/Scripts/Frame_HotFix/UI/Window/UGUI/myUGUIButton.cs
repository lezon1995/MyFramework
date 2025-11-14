using UnityEngine.Events;
using UnityEngine.UI;
using static UnityUtility;

// 因为button组件一般都是跟Image组件一起的,所以继承myUGUIImage
public class myUGUIButton : myUGUIImageSimple
{
    protected Button button; // UGUI的Button组件

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out button))
        {
            if (!isNewObject)
            {
                logError("需要添加一个button组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            button = go.AddComponent<Button>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }
    }

    public void setUGUIButtonClick(UnityAction callback)
    {
        button.onClick.AddListener(callback);
    }
}