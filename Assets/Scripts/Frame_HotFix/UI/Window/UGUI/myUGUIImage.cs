using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseDefine;

// 对UGUI的Image的封装,普通版本,提供替换图片的功能,UGUI的静态图片不支持递归变化透明度
public class myUGUIImage : myUGUIImageSimple, IUGUIImage
{
    protected UGUIAtlasPtr originAtlasPtr; // 图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
    protected UGUIAtlasPtr atlasPtr; // 当前正在使用的图集
    protected Sprite originSprite; // 备份加载物体时原始的精灵图片
    protected string originSpriteName; // 初始图片的名字,用于外部根据初始名字设置其他效果的图片

    public override void init()
    {
        base.init();
        originSprite = image.sprite;
        // 获取初始的精灵所在图集
        if (originSprite != null)
        {
            if (!go.TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
            {
                logError("找不到图集,请添加ImageAtlasPath组件, GameObject:" + getGameObjectPath(go));
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
                    logWarning("无法加载初始化的图集:" + atlasPath + ", GameObject:" + getGameObjectPath(go) +
                               ",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
                }
            }
            else
            {
                logError("需要切换图片的节点上不要使用引擎内置的图片, GameObject:" + getGameObjectPath(go));
            }

            atlasPtr = originAtlasPtr;
        }

        originSpriteName = getSpriteName();
    }

    public override void destroy()
    {
        // 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
        // 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
        image.sprite = originSprite;
        if (layout.isInResources())
        {
            mAtlasManager.unloadAtlasInResourcecs(ref originAtlasPtr);
        }
        else
        {
            mAtlasManager.unloadAtlas(ref originAtlasPtr);
        }

        base.destroy();
    }

    public UGUIAtlasPtr getAtlas()
    {
        return atlasPtr;
    }

    public virtual void setAtlas(UGUIAtlasPtr atlas, bool clearSprite = false, bool force = false)
    {
        if (image == null)
            return;

        atlasPtr = atlas;
        setSprite(clearSprite ? null : atlas?.getSprite(getSpriteName()));
    }

    public void setSpriteName(string spriteName)
    {
        setSpriteNamePro(spriteName, false, 1.0f);
    }

    public void setSpriteNamePro(string spriteName, bool useSpriteSize, float sizeScale)
    {
        if (image == null)
            return;

        if (spriteName.isEmpty())
        {
            image.sprite = null;
            return;
        }

        setSprite(getSpriteInAtlas(spriteName), useSpriteSize, sizeScale);
    }

    // 设置图片,需要确保图片在当前图集内
    public void setSprite(Sprite sprite, bool useSpriteSize = false, float sizeScale = 1.0f)
    {
        if (image == null || image.sprite == sprite)
            return;

        if (sprite != null && !atlasPtr.hasSprite(sprite))
        {
            logError("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly");
        }

        setSpriteOnly(sprite, useSpriteSize, sizeScale);
    }

    // 设置可自动本地化的文本内容,collection是myUGUIText对象所属的布局对象或者布局结构体对象,如LayoutScript或WindowObjectUGUI
    public void setLocalizationImage(string chineseSpriteName, ILocalizationCollection collection)
    {
        mLocalizationManager.registeLocalization(this, chineseSpriteName);
        collection.addLocalizationObject(this);
    }

    public string getOriginSpriteName()
    {
        return originSpriteName;
    }

    public void setOriginSpriteName(string textureName)
    {
        originSpriteName = textureName;
    }

    // 自动计算图片的原始名称,也就是不带后缀的名称,后缀默认以_分隔
    public void generateOriginSpriteName(char key = '_')
    {
        if (!originSpriteName.Contains(key))
        {
            logError("texture name is not valid!can not generate origin texture name, texture name : " + originSpriteName);
            return;
        }

        originSpriteName = originSpriteName.rangeToLastInclude(key);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected Sprite getSpriteInAtlas(string spriteName)
    {
        Sprite sprite = atlasPtr?.getSprite(spriteName);
        if (sprite)
            return sprite;

        return null;
    }
}