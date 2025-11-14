using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;

// 对UGUI的Image的封装,简化版,只有Image组件
public class myUGUIImageSimple : myUGUIObject
{
    protected CanvasGroup canvasGroup; // 用于是否显示
    protected Image image; // 图片组件
    protected bool canvasGroupValid; // 当前CanvasGroup是否有效,在测试中发现判断mCanvasGroup是否为空的写法会比较耗时,所以替换为bool判断

    public override void init()
    {
        base.init();
        // 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
        if (!go.TryGetComponent(out image))
        {
            if (!isNewObject)
            {
                logError("需要添加一个Image组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            image = go.AddComponent<Image>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }
    }

    public override void destroy()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup = null;
        }

        canvasGroupValid = false;
        base.destroy();
    }

    // 是否剔除渲染
    public void cull(bool isCull)
    {
        if (canvasGroup == null)
        {
            canvasGroup = getOrAddUnityComponent<CanvasGroup>();
        }

        canvasGroup.alpha = isCull ? 0.0f : 1.0f;
        canvasGroupValid = true;
    }

    public override bool isCulled()
    {
        return canvasGroupValid && isFloatZero(canvasGroup.alpha);
    }

    public override bool canUpdate()
    {
        return !isCulled() && base.canUpdate();
    }

    public override bool canGenerateDepth()
    {
        return !isCulled();
    }

    public void setRenderQueue(int renderQueue)
    {
        if (image == null || image.material == null)
        {
            return;
        }

        image.material.renderQueue = renderQueue;
    }

    public int getRenderQueue()
    {
        if (image == null || image.material == null)
        {
            return 0;
        }

        return image.material.renderQueue;
    }

    // 只设置图片,不关心所在图集,一般不会用到此函数,只有当确认要设置的图片与当前图片不在同一图集时才会使用
    // 并且需要自己保证设置不同图集的图片以后不会有什么问题
    public void setSpriteOnly(Sprite sprite, bool useSpriteSize = false, float sizeScale = 1.0f)
    {
        if (image == null || image.sprite == sprite)
        {
            return;
        }

        image.sprite = sprite;
        if (useSpriteSize)
        {
            setWindowSize(getSpriteSize() * sizeScale);
        }
    }

    public Vector2 getSpriteSize()
    {
        if (image == null)
        {
            return Vector2.zero;
        }

        if (image.sprite != null)
        {
            return image.sprite.rect.size;
        }

        return getWindowSize();
    }

    public Image getImage()
    {
        return image;
    }

    public Sprite getSprite()
    {
        return image.sprite;
    }

    public void setMaterial(Material material)
    {
        image.material = material;
    }

    public void setShader(Shader shader, bool force)
    {
        if (image == null || image.material == null)
        {
            return;
        }

        if (force)
        {
            image.material.shader = null;
            image.material.shader = shader;
        }
    }

    public string getSpriteName()
    {
        if (image == null || image.sprite == null)
        {
            return null;
        }

        return image.sprite.name;
    }

    public Material getMaterial()
    {
        if (image == null)
        {
            return null;
        }

        return image.material;
    }

    public string getMaterialName()
    {
        if (image == null || image.material == null)
        {
            return null;
        }

        return image.material.name;
    }

    public string getShaderName()
    {
        if (image.material == null || image.material.shader == null)
        {
            return null;
        }

        return image.material.shader.name;
    }

    public override void setAlpha(float alpha, bool fadeChild)
    {
        base.setAlpha(alpha, fadeChild);
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    public override float getAlpha()
    {
        return image.color.a;
    }

    public override void setFillPercent(float percent)
    {
        if (image == null)
        {
            return;
        }

        image.fillAmount = percent;
    }

    public override float getFillPercent()
    {
        return image.fillAmount;
    }

    public override void setColor(Color color)
    {
        if (image == null)
        {
            return;
        }

        image.color = color;
    }

    public void setColor(Vector3 color)
    {
        if (image == null)
        {
            return;
        }

        image.color = new(color.x, color.y, color.z);
    }

    public override Color getColor()
    {
        return image.color;
    }

    public void setUGUIRaycastTarget(bool enable)
    {
        if (image == null)
        {
            return;
        }

        image.raycastTarget = enable;
    }
}