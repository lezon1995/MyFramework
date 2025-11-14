using UnityEngine;
using UnityEngine.UI;
using static FrameBaseHotFix;
using static MathUtility;
using static StringUtility;
using static FrameDefine;
using static UnityUtility;
using static FrameBaseUtility;

// 对UGUI的RawImage的封装
public class myUGUIRawImage : myUGUIObject, IShaderWindow
{
    protected WindowShader windowShader; // shader对象
    protected RawImage rawImage; // UGUI的RawImage组件
    protected CanvasGroup canvasGroup; // 用于是否显示
    protected Material originMaterial; // 初始的材质,用于重置时恢复材质
    protected Texture originTexture; // 初始的图片
    protected string originMaterialPath; // 初始材质的文件路径
    protected bool isNewMaterial; // 当前的材质是否是新创建的材质对象

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out rawImage))
        {
            if (!isNewObject)
            {
                logError("需要添加一个RawImage组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            rawImage = go.AddComponent<RawImage>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }

        originMaterial = rawImage.material;
        originTexture = rawImage.texture;
        string materialName = getMaterialName().removeAll(" (Instance)");
        // 不再将默认材质替换为自定义的默认材质,只判断其他材质
        if (!materialName.isEmpty() &&
            materialName != BUILDIN_UI_MATERIAL)
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
                destroyUnityObject(rawImage.material);
            }
        }

        rawImage.material = originMaterial;
        rawImage.texture = originTexture;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup = null;
        }

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
    }

    public bool isCull()
    {
        return canvasGroup != null && isFloatZero(canvasGroup.alpha);
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
        if (rawImage.material != null)
        {
            windowShader?.applyShader(rawImage.material);
        }
    }

    public override void setAlpha(float alpha, bool fadeChild)
    {
        base.setAlpha(alpha, fadeChild);
        Color color = rawImage.color;
        color.a = alpha;
        rawImage.color = color;
    }

    public virtual void setTexture(Texture tex, bool useTextureSize = false)
    {
        if (rawImage == null)
        {
            return;
        }

        rawImage.texture = tex;
        if (useTextureSize && tex != null)
        {
            setWindowSize(getTextureSize());
        }
    }

    public Texture getTexture()
    {
        if (rawImage == null)
        {
            return null;
        }

        return rawImage.texture;
    }

    public Vector2 getTextureSize()
    {
        if (rawImage.texture == null)
        {
            return Vector2.zero;
        }

        return new(rawImage.texture.width, rawImage.texture.height);
    }

    public string getTextureName()
    {
        if (rawImage == null || rawImage.texture == null)
        {
            return null;
        }

        return rawImage.texture.name;
    }

    public void setTextureName(string name, bool useTextureSize = false, bool loadAsync = false)
    {
        if (name.isEmpty())
        {
            setTexture(null, useTextureSize);
            return;
        }

        // 同步加载
        if (!loadAsync)
        {
            setTexture(res.loadGameResource<Texture>(name), useTextureSize);
        }
        // 异步加载
        else
        {
            res.loadGameResourceAsync(name, (Texture tex) =>
            {
                setTexture(tex, useTextureSize);
            });
        }
    }

    public Material getMaterial()
    {
        if (rawImage == null)
        {
            return null;
        }

        return rawImage.material;
    }

    public string getMaterialName()
    {
        if (rawImage == null || rawImage.material == null)
        {
            return null;
        }

        return rawImage.material.name;
    }

    public void setMaterialName(string materialPath, bool newMaterial, bool loadAsync = false)
    {
        if (rawImage == null)
        {
            return;
        }

        isNewMaterial = newMaterial;
        // 异步加载
        if (loadAsync)
        {
            res.loadGameResourceAsync(materialPath, (Material mat) =>
            {
                if (rawImage == null)
                {
                    return;
                }

                if (isNewMaterial)
                {
                    Material newMat = new(mat);
                    newMat.name = getFileNameNoSuffixNoDir(materialPath) + "_" + IToS(uid);
                    rawImage.material = newMat;
                }
                else
                {
                    rawImage.material = mat;
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
                rawImage.material = mat;
            }
            else
            {
                rawImage.material = loadedMaterial;
            }
        }
    }
}