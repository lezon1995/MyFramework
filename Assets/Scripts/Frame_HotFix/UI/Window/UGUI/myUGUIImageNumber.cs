using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameBaseHotFix;

// 可显示数字的窗口,只支持整数,且每个数字图片的大小必须一样,不能显示小数,负数
// 因为使用了自定义的组件,所以性能上比myUGUINumber更高,只是相比之下myUGUINumber更加灵活一点
public class myUGUIImageNumber : myUGUIObject
{
    protected ImageNumber imageNumber; // 渲染组件
    protected UGUIAtlasPtr originAtlasPtr = new(); // 初始的图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
    protected UGUIAtlasPtr atlasPtr = new(); // 当前正在使用的图片图集
    protected Sprite originSprite; // 备份加载物体时原始的精灵图片
    protected string numberStyle; // 数字图集名

    public override void init()
    {
        base.init();
        // 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
        if (!go.TryGetComponent(out imageNumber))
        {
            if (!isNewObject)
            {
                logError("需要添加一个ImageNumber组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            imageNumber = go.AddComponent<ImageNumber>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }

        originSprite = imageNumber.sprite;
        // 获取初始的精灵所在图集
        if (originSprite != null)
        {
            if (!go.TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
            {
                logError("找不到图集,请添加ImageAtlasPath组件, window:" + name + ", layout:" + layout.getName());
            }

            string atlasPath = imageAtlasPath.mAtlasPath;
            // unity_builtin_extra是unity内置的资源,不需要再次加载
            if (!atlasPath.endWith("/unity_builtin_extra"))
            {
                if (layout.isInResources())
                {
                    atlasPath = atlasPath.removeStartString(P_RESOURCES_PATH);
                    originAtlasPtr = mAtlasManager.getAtlasInResources(atlasPath, false);
                }
                else
                {
                    atlasPath = atlasPath.removeStartString(P_GAME_RESOURCES_PATH);
                    originAtlasPtr = mAtlasManager.getAtlas(atlasPath, false);
                }

                if (originAtlasPtr == null || !originAtlasPtr.isValid())
                {
                    logWarning("无法加载初始化的图集:" + atlasPath + ", window:" + name + ", layout:" + layout.getName() +
                               ",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
                }
            }

            atlasPtr = originAtlasPtr;
        }

        numberStyle = imageNumber.sprite.name.rangeToLast('_');
        refreshSpriteList();
    }

    public override void notifyAnchorApply()
    {
        base.notifyAnchorApply();
        // 此处默认数字窗口都是以ASPECT_BASE.AB_AUTO的方式等比放大
        imageNumber.setInterval((int)(imageNumber.getInterval() * getScreenScale(ASPECT_BASE.AUTO).x));
    }

    public override void cloneFrom(myUGUIObject obj)
    {
        base.cloneFrom(obj);
        var source = obj as myUGUIImageNumber;
        imageNumber.setInterval(source.imageNumber.getInterval());
        numberStyle = source.numberStyle;
        imageNumber.setNumber(source.getNumber());
        imageNumber.setSpriteList(source.imageNumber.getSpriteList());
        imageNumber.setDockingPosition(source.getDockingPosition());
    }

    public void setAtlas(UGUIAtlasPtr atlas)
    {
        atlasPtr = atlas;
        refreshSpriteList();
    }

    public void setNumberStyle(string style)
    {
        numberStyle = style;
        refreshSpriteList();
    }

    public void setInterval(int interval)
    {
        imageNumber.setInterval(interval);
    }

    public void setDockingPosition(DOCKING_POSITION dock)
    {
        imageNumber.setDockingPosition(dock);
    }

    public void setNumber(int num, int limitLen = 0)
    {
        imageNumber.setNumber(IToS(num, limitLen));
    }

    public void clearNumber()
    {
        imageNumber.clearNumber();
    }

    public int getContentWidth()
    {
        return imageNumber.getContentWidth();
    }

    public string getNumber()
    {
        return imageNumber.getNumber();
    }

    public int getInterval()
    {
        return imageNumber.getInterval();
    }

    public string getNumberStyle()
    {
        return numberStyle;
    }

    public DOCKING_POSITION getDockingPosition()
    {
        return imageNumber.getDockingPosition();
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void refreshSpriteList()
    {
        using var a = new DicScope<char, Sprite>(out var spriteList);
        for (int i = 0; i < 10; ++i)
        {
            spriteList.add((char)('0' + i), atlasPtr.getSprite(numberStyle + "_" + IToS(i)));
        }

        imageNumber.sprite = spriteList.firstValue();
        imageNumber.setSpriteList(spriteList);
    }
}