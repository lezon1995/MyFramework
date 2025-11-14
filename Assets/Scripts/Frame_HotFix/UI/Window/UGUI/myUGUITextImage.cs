using static UnityUtility;

// 可实现图文混排的文本
// <quad width=xxx sprite=xxx/>
public class myUGUITextImage : myUGUIObject
{
    protected WindowPool<myUGUIImage> imagePool; // 图片节点对象池
    protected TextImage textImage; // 处理图文混排的组件,继承自Text
    protected myUGUIImage image; // 当前文本节点下的Image节点,用于获得图集信息,以及克隆图片

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out textImage))
        {
            if (!isNewObject)
            {
                logError("需要添加一个TextImage组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            textImage = go.AddComponent<TextImage>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }

        // 自动获取该节点下的名为Image的子节点
        layout.getScript().newObject(out image, this, "Image", false);
        if (image == null)
        {
            logError("可图文混排的文本下必须有一个名为Image的子节点");
        }

        // 初始化图片模板信息相关
        imagePool = new(layout.getScript());
        imagePool.assignTemplate(image);
        imagePool.init(false);
        textImage.setCreateImage(() => imagePool.newWindow());
        textImage.setDestroyImage(img =>
        {
            imagePool.unuseWindow(img);
        });
    }

    public TextImage getTextImage()
    {
        return textImage;
    }
}