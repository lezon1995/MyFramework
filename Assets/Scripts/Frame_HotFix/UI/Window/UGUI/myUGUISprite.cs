using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static StringUtility;
using static MathUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 对SpriteRenderer的封装
public class myUGUISprite : myUGUIObject, IShaderWindow
{
    protected SpriteRenderer spriteRenderer; // 图片组件
    protected WindowShader windowShader; // 图片所使用的shader类,用于动态设置shader参数
    protected UGUIAtlasPtr originAtlasPtr; // 图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
    protected UGUIAtlasPtr atlasPtr; // 图片图集
    protected Material originMaterial; // 初始的材质,用于重置时恢复材质
    protected Sprite originSprite; // 备份加载物体时原始的精灵图片
    protected string originMaterialPath; // 原始材质的文件路径
    protected string originSpriteName; // 初始图片的名字,用于外部根据初始名字设置其他效果的图片
    protected bool isNewMaterial; // 当前的材质是否是新建的材质对象

    public override void init()
    {
        base.init();
        // 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
        spriteRenderer = getOrAddUnityComponent<SpriteRenderer>();
        originSprite = spriteRenderer.sprite;
        originMaterial = spriteRenderer.sharedMaterial;
        originSpriteName = getSpriteName();
        // 获取初始的精灵所在图集
        if (originSprite != null)
        {
            if (!go.TryGetComponent<ImageAtlasPath>(out var comImageAtlasPath))
            {
                logError("需要切换图片的SpriteRenderer组件上找不到ImageAtlasPath组件, GameObject:" + getGameObjectPath(go));
                return;
            }

            string atlasPath;
            if (layout.isInResources())
            {
                atlasPath = comImageAtlasPath.mAtlasPath.removeStartString(P_RESOURCES_PATH);
                originAtlasPtr = mAtlasManager.getAtlasInResources(atlasPath, false);
            }
            else
            {
                atlasPath = comImageAtlasPath.mAtlasPath.removeStartString(P_GAME_RESOURCES_PATH);
                originAtlasPtr = mAtlasManager.getAtlas(atlasPath, false);
            }

            if (originAtlasPtr == null || !originAtlasPtr.isValid())
            {
                logError("无法加载初始化的图集:" + atlasPath + ",GameObject:" + getGameObjectPath(go) +
                         ",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (comImageAtlasPath != null ? comImageAtlasPath.mAtlasPath : EMPTY));
            }

            atlasPtr = originAtlasPtr;
        }

        string materialName = getMaterialName().removeAll(" (Instance)");
        // 不再将默认材质替换为自定义的默认材质,只判断其他材质
        if (!materialName.isEmpty() && materialName != DEFAULT_MATERIAL && materialName != SPRITE_DEFAULT_MATERIAL)
        {
            if (originMaterial != null && go.TryGetComponent<MaterialPath>(out var comMaterialPath))
            {
                originMaterialPath = comMaterialPath.mMaterialPath;
            }

            if (originMaterialPath.isEmpty())
            {
                logError("没有找到MaterialPath组件,name:" + getName());
            }

            originMaterialPath = originMaterialPath.removeStartString(P_GAME_RESOURCES_PATH);
            if (!originMaterialPath.endWith("/unity_builtin_extra"))
            {
                if (!originMaterialPath.Contains('.'))
                {
                    logError("材质文件需要带后缀:" + originMaterialPath + ",GameObject:" + getName() + ",parent:" + getParent()?.getName());
                }

                setMaterialName(originMaterialPath, !mShaderManager.isSingleShader(originMaterial.shader.name));
            }
        }
    }

    public override void destroy()
    {
        // 卸载创建出的材质
        if (isNewMaterial)
        {
            if (!isEditor())
            {
                destroyUnityObject(spriteRenderer.sharedMaterial);
            }
        }

        // 为了尽量确保ImageAtlasPath中记录的图集路径与图集完全一致,在销毁窗口时还原初始的图片
        // 这样在重复使用当前物体时在校验图集路径时不会出错,但是如果在当前物体使用过程中销毁了原始的图片,则可能会报错
        spriteRenderer.sprite = originSprite;
        setMaterial(originMaterial);
        setAlpha(1.0f);
        if (layout.isInResources())
        {
            mAtlasManager.unloadAtlasInResourcecs(ref originAtlasPtr);
        }
        else
        {
            mAtlasManager.unloadAtlas(ref originAtlasPtr);
        }

        atlasPtr = null;
        base.destroy();
    }

    // 是否剔除渲染
    public void cull(bool isCull)
    {
        setAlpha(isCull ? 0.0f : 1.0f);
    }

    public override bool isCulled()
    {
        return isFloatZero(getAlpha());
    }

    public override bool canUpdate()
    {
        return !isCulled() && base.canUpdate();
    }

    public override bool canGenerateDepth()
    {
        return !isCulled();
    }

    public void setWindowShader(WindowShader shader)
    {
        windowShader = shader;
        // 因为shader参数的需要在update中更新,所以需要启用窗口的更新
        needUpdate = true;
    }

    public WindowShader getWindowShader()
    {
        return windowShader;
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (windowShader != null && !isCulled() && spriteRenderer.sharedMaterial != null)
        {
            windowShader.applyShader(spriteRenderer.sharedMaterial);
        }
    }

    // 谨慎使用设置RendererQueue,尤其是操作material而非sharedMaterial
    // 操作material会复制出一个材质实例,从而导致drawcall增加
    public void setRenderQueue(int renderQueue, bool shareMaterial = false)
    {
        if (spriteRenderer == null)
            return;

        if (shareMaterial)
        {
            if (spriteRenderer.sharedMaterial == null)
                return;

            spriteRenderer.sharedMaterial.renderQueue = renderQueue;
        }
        else
        {
            if (spriteRenderer.material == null)
                return;

            spriteRenderer.material.renderQueue = renderQueue;
        }
    }

    public int getRenderQueue()
    {
        if (spriteRenderer == null || spriteRenderer.sharedMaterial == null)
            return 0;

        return spriteRenderer.sharedMaterial.renderQueue;
    }

    public override Vector2 getWindowSize(bool transformed = false)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return Vector2.zero;

        if (transformed)
            return getSpriteSize() * getScale();

        return getSpriteSize();
    }

    public UGUIAtlasPtr getAtlas()
    {
        return atlasPtr;
    }

    public virtual void setAtlas(UGUIAtlasPtr atlas, bool clearSprite = false, bool force = false)
    {
        if (spriteRenderer == null)
            return;

        atlasPtr = atlas;
        setSprite(clearSprite ? null : atlasPtr?.getSprite(getSpriteName()));
    }

    public void setSpriteName(string spriteName)
    {
        if (spriteRenderer == null || atlasPtr == null || !atlasPtr.isValid())
            return;

        if (spriteName.isEmpty())
        {
            spriteRenderer.sprite = null;
            return;
        }

        setSprite(atlasPtr.getSprite(spriteName));
    }

    // 设置图片,需要确保图片在当前图集内
    public void setSprite(Sprite sprite)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == sprite)
            return;

        if (sprite != null && atlasPtr != null && !atlasPtr.hasSprite(sprite))
        {
            logWarning("设置不同图集的图片可能会引起问题,如果需要设置其他图集的图片,请使用setSpriteOnly");
        }

        setSpriteOnly(sprite);
    }

    // 只设置图片,不关心所在图集,一般不会用到此函数,只有当确认要设置的图片与当前图片不在同一图集时才会使用
    // 并且需要自己保证设置不同图集的图片以后不会有什么问题
    public void setSpriteOnly(Sprite sprite)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == sprite)
            return;

        if (sprite != null && !isFloatEqual(sprite.pixelsPerUnit, 1.0f))
        {
            logError("sprite的pixelsPerUnit需要为1, name:" + sprite.name);
        }

        spriteRenderer.sprite = sprite;
    }

    public Vector2 getSpriteSize()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return Vector2.zero;

        return spriteRenderer.sprite.rect.size;
    }

    public SpriteRenderer getSpriteRenderer()
    {
        return spriteRenderer;
    }

    public Sprite getSprite()
    {
        return spriteRenderer.sprite;
    }

    public void setOrderInLayer(int order)
    {
        spriteRenderer.sortingOrder = order;
    }

    public int getOrderInLayer()
    {
        return spriteRenderer.sortingOrder;
    }

    public void setRendererPriority(int priority)
    {
        spriteRenderer.rendererPriority = priority;
    }

    public int getRendererPriority()
    {
        return spriteRenderer.rendererPriority;
    }

    public string getOriginMaterialPath()
    {
        return originMaterialPath;
    }

    // materialPath是GameResources下的相对路径,带后缀
    public void setMaterialName(string materialPath, bool newMaterial, bool loadAsync = false)
    {
        if (spriteRenderer == null)
            return;

        isNewMaterial = newMaterial;
        // 异步加载
        if (loadAsync)
        {
            res.loadGameResourceAsync(materialPath, (Material mat) =>
            {
                if (spriteRenderer == null)
                    return;

                if (isNewMaterial)
                {
                    // 当需要复制一个新的材质时,刚加载出来的材质实际上就不会再用到了
                    // 只有当下次还加载相同的材质时才会直接返回已加载的材质
                    // 如果要卸载最开始加载出来的材质,只能通过卸载整个文件夹的资源来卸载
                    Material newMat = new(mat);
                    newMat.name = getFileNameNoSuffixNoDir(materialPath) + "_" + IToS(uid);
                    setMaterial(newMat);
                }
                else
                {
                    setMaterial(mat);
                }
            });
        }
        // 同步加载
        else
        {
            var loadedMaterial = res.loadGameResource<Material>(materialPath);
            if (isNewMaterial)
            {
                Material mat = new(loadedMaterial);
                mat.name = getFileNameNoSuffixNoDir(materialPath) + "_" + IToS(uid);
                setMaterial(mat);
            }
            else
            {
                setMaterial(loadedMaterial);
            }
        }
    }

    public void setMaterial(Material mat)
    {
        spriteRenderer.material = mat;
    }

    public void setShader(Shader shader, bool force)
    {
        if (spriteRenderer == null || spriteRenderer.sharedMaterial == null)
            return;

        if (force)
        {
            spriteRenderer.sharedMaterial.shader = null;
            spriteRenderer.sharedMaterial.shader = shader;
        }
    }

    public string getSpriteName()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return null;

        return spriteRenderer.sprite.name;
    }

    public Material getMaterial()
    {
        if (spriteRenderer == null)
            return null;

        return spriteRenderer.sharedMaterial;
    }

    public string getMaterialName()
    {
        if (spriteRenderer == null || spriteRenderer.sharedMaterial == null)
            return null;

        return spriteRenderer.sharedMaterial.name;
    }

    public string getShaderName()
    {
        if (spriteRenderer.sharedMaterial == null || spriteRenderer.sharedMaterial.shader == null)
            return null;

        return spriteRenderer.sharedMaterial.shader.name;
    }

    public override void setAlpha(float alpha, bool fadeChild)
    {
        base.setAlpha(alpha, fadeChild);
        if (spriteRenderer == null)
            return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    public override float getAlpha()
    {
        return spriteRenderer.color.a;
    }

    public override void setColor(Color color)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = color;
    }

    public void setColor(Vector3 color)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = new(color.x, color.y, color.z);
    }

    public override Color getColor()
    {
        return spriteRenderer.color;
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
    protected override void ensureColliderSize()
    {
        // 确保RectTransform和BoxCollider一样大
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        collider?.setColliderSize(spriteRenderer.sprite.rect.size);
    }
}