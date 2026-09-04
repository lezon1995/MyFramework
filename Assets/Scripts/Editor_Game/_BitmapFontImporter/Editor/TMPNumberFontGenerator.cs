using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

[Serializable]
public class GlyphData
{
    public char character;
    public uint glyphIndex;
    public int x, y, width, height;
    public float bearingX, bearingY, advance;

    public GlyphData()
    {
    }

    public GlyphData(uint glyphIndex, float bearingX, float bearingY, float advance)
    {
        this.glyphIndex = glyphIndex;
        this.bearingX = bearingX;
        this.bearingY = bearingY;
        this.advance = advance;
    }
}

public class TMPNumberFontGenerator : EditorWindow
{
    TMPNumberFontSettings _settings;
    string inputFolder = "Assets/GameResources/Font/Numbers/01";
    string outputFolder = "Assets/GameResources/Font/Numbers/01";

    int atlasPadding = 2;
    int cellPadding = 0;

    [Serializable]
    class CharMap
    {
        public string fileSuffix = "";
        public char unicode = ' ';
    }

    List<CharMap> charMaps = new()
    {
        new CharMap { fileSuffix = "0", unicode = '0' },
        new CharMap { fileSuffix = "1", unicode = '1' },
        new CharMap { fileSuffix = "2", unicode = '2' },
        new CharMap { fileSuffix = "3", unicode = '3' },
        new CharMap { fileSuffix = "4", unicode = '4' },
        new CharMap { fileSuffix = "5", unicode = '5' },
        new CharMap { fileSuffix = "6", unicode = '6' },
        new CharMap { fileSuffix = "7", unicode = '7' },
        new CharMap { fileSuffix = "8", unicode = '8' },
        new CharMap { fileSuffix = "9", unicode = '9' },
        new CharMap { fileSuffix = "Add", unicode = '+' },
        new CharMap { fileSuffix = "Sub", unicode = '-' },
    };

    Vector2 scroll;

    [MenuItem("Tools/TMP Number Font/Create Number Font From PNG")]
    static void Open()
    {
        var window = GetWindow<TMPNumberFontGenerator>("TMP Number Font");
        window._settings = Resources.Load<TMPNumberFontSettings>("TMPNumberFontSettings");
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("TMP Number Font Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        _settings = (TMPNumberFontSettings)EditorGUILayout.ObjectField("TMPNumberFont Settings", _settings, typeof(TMPNumberFontSettings), false);
        inputFolder = EditorGUILayout.TextField("Input Folder", inputFolder);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        GUILayout.Space(10);
        atlasPadding = Mathf.Max(0, EditorGUILayout.IntField("Atlas Padding", atlasPadding));
        cellPadding = EditorGUILayout.IntField("Cell Padding", cellPadding);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Character Mapping", EditorStyles.boldLabel);

        for (int i = 0; i < charMaps.Count; i++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"#{i}", GUILayout.Width(30));
                charMaps[i].fileSuffix = EditorGUILayout.TextField("File Suffix", charMaps[i].fileSuffix);
                string ch = EditorGUILayout.TextField("Unicode", charMaps[i].unicode.ToString());
                if (ch.Length > 0) charMaps[i].unicode = ch[0];
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add")) charMaps.Add(new CharMap());
            if (GUILayout.Button("Remove Last") && charMaps.Count > 1) charMaps.RemoveAt(charMaps.Count - 1);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Create Number Font", GUILayout.Height(40)))
        {
            CreateFont();
        }

        EditorGUILayout.EndScrollView();
    }

    // ============================================================
    // 主流程
    // ============================================================

    void CreateFont()
    {
        if (charMaps == null || charMaps.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "至少需要一个字符映射。", "OK");
            return;
        }

        var paths = new List<string>();
        foreach (var m in charMaps)
        {
            if (string.IsNullOrEmpty(m.fileSuffix))
            {
                EditorUtility.DisplayDialog("Error", "存在空的 File Suffix。", "OK");
                return;
            }

            string p = Path.Combine(inputFolder, $"Numbers-{m.fileSuffix}.png").Replace('\\', '/');
            if (!File.Exists(p))
            {
                EditorUtility.DisplayDialog("Error", $"Missing file:\n{p}", "OK");
                return;
            }

            paths.Add(p);
        }

        var sourceTextures = new List<Texture2D>();
        try
        {
            foreach (var p in paths)
            {
                Texture2D tex = LoadTexture(p);
                if (tex == null)
                {
                    EditorUtility.DisplayDialog("Error", $"Failed to load:\n{p}", "OK");
                    return;
                }

                sourceTextures.Add(tex);
            }

            BuildFont(sourceTextures);
        }
        finally
        {
            foreach (var t in sourceTextures)
                if (t != null)
                    DestroyImmediate(t);
        }
    }

    Texture2D LoadTexture(string path)
    {
        byte[] data = File.ReadAllBytes(path);

        // 先用 LoadImage 解析（让 Unity 自动得到正确尺寸），
        // 再销毁重建为 RGBA32 可读格式，保证 GetPixels32 返回准确的 alpha 数据。
        var temp = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!temp.LoadImage(data, false))
        {
            DestroyImmediate(temp);
            return null;
        }

        int w = temp.width;
        int h = temp.height;
        var pixels = temp.GetPixels32();
        DestroyImmediate(temp);

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        tex.Apply(false, false);
        return tex;
    }

    // ============================================================
    // 构建图集
    // ============================================================


    void BuildFont(List<Texture2D> textures)
    {
        Directory.CreateDirectory(outputFolder);

        int totalWidth = 0;
        int maxHeight = 0;
        foreach (var t in textures)
        {
            totalWidth += t.width;
            maxHeight = Mathf.Max(maxHeight, t.height);
        }

        int paddedW = totalWidth + atlasPadding * (textures.Count + 1);
        int paddedH = maxHeight + atlasPadding * 2;

        int atlasW = Mathf.NextPowerOfTwo(Mathf.Max(paddedW, 256));
        int atlasH = Mathf.NextPowerOfTwo(Mathf.Max(paddedH, 128));
        atlasW = Mathf.Clamp(atlasW, 256, 4096);
        atlasH = Mathf.Clamp(atlasH, 128, 4096);

        var atlas = new Texture2D(atlasW, atlasH, TextureFormat.RGBA32, false);
        var clear = new Color32[atlasW * atlasH];
        atlas.SetPixels32(clear);

        var glyphDataList = new List<GlyphData>();
        int cursorX = atlasPadding;

        for (int i = 0; i < textures.Count; i++)
        {
            var tex = textures[i];
            var g = _settings.glyphsData[i];
            // Unity Texture2D 和 TMP GlyphRect 都用 Y-up 坐标系（左下角为原点）。
            // SetPixels32(x, y, ...) 的 y 也是从底部数，所以 glyphRect.y 直接等于 topY 即可。
            int topY = atlasPadding;

            // 用 GetPixels32 + SetPixels32 避免任何 RGBA32 -> Color -> RGBA32 的来回转换
            var pixels = tex.GetPixels32();
            atlas.SetPixels32(cursorX, topY, tex.width, tex.height, pixels);

            var d = new GlyphData
            {
                character = charMaps[i].unicode,
                glyphIndex = (uint)i,
                x = cursorX,
                y = topY,
                width = tex.width,
                height = tex.height,
                bearingX = g.bearingX,
                bearingY = g.bearingY,
                advance = g.advance,
            };
            glyphDataList.Add(d);

            cursorX += tex.width + atlasPadding;
        }

        atlas.Apply(false, false);

        string atlasPath = $"{outputFolder}/NumberFont Atlas.png";
        File.WriteAllBytes(atlasPath, atlas.EncodeToPNG());
        AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled = false;
            // TextMeshPro/Sprite shader + GlyphRenderMode.RASTER 用 Point (硬边) 避免采到相邻 glyph 像素。
            // TMP 官方在 FontAssetCreatorWindow.cs:1363-1364 也是这么做的。
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();
        }

        var atlasAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
        CreateFontAsset(atlasAsset, glyphDataList);
        DestroyImmediate(atlas);
    }

    // ============================================================
    // 创建 TMP FontAsset —— 全部走反射
    // ============================================================

    void CreateFontAsset(Texture2D atlas, List<GlyphData> glyphDataList)
    {
        string fontPath = $"{outputFolder}/NumberFont.asset";
        string materialPath = $"{outputFolder}/NumberFont Material.mat";

        Shader shader = Shader.Find("TextMeshPro/Sprite");
        if (shader == null)
        {
            Debug.LogError("Cannot find TextMeshPro/Sprite shader. 请确认 TMP Essential Resources 已导入。");
            return;
        }

        var material = new Material(shader) { name = "NumberFont Material" };
        material.mainTexture = atlas; // 写到 _MainTex (TextMeshPro/Sprite 渲染时用的纹理)
        // 先 CreateAsset 把 material 写盘，然后再 SetFloat 并 SaveAssetIfDirty 单个保存。
        // 这样能确保两个 float 真的写进 .mat 文件里。
        AssetDatabase.CreateAsset(material, materialPath);

        var fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();
        fontAsset.name = "NumberFont";
        AssetDatabase.CreateAsset(fontAsset, fontPath);
        AssetDatabase.ImportAsset(fontPath, ImportAssetOptions.ForceUpdate);

        var loaded = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (loaded == null)
        {
            Debug.LogError($"[TMPNumberFontGenerator] 无法加载刚创建的 FontAsset: {fontPath}");
            return;
        }

        // 用 LoadAssetAtPath 重新加载确保拿到磁盘实例（不是内存里那个"克隆"）。
        var savedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (savedMaterial == null)
        {
            Debug.LogError($"[TMPNumberFontGenerator] 无法加载 material: {materialPath}");
            return;
        }

        // 替换后面所有用到的 material 变量引用
        material = savedMaterial;
        material.SetFloat("_TextureWidth", atlas.width);
        material.SetFloat("_TextureHeight", atlas.height);
        material.SetTexture("_MainTex", atlas); // TextMeshPro/Sprite shader 真正用的纹理
        EditorUtility.SetDirty(material);
#if UNITY_2020_3_OR_NEWER
        AssetDatabase.SaveAssetIfDirty(material);
#else
        AssetDatabase.SaveAssets();
#endif

        // ---- 构建 Glyph ----
        var glyphListType = typeof(List<>).MakeGenericType(typeof(Glyph));
        var glyphs = (IList)Activator.CreateInstance(glyphListType);

        foreach (var d in glyphDataList)
        {
            object g = Activator.CreateInstance(typeof(Glyph));

            // 字段名跨版本: index / m_Index
            TrySetField(g, "m_Index", (uint)d.glyphIndex);
            TrySetField(g, "index", (uint)d.glyphIndex);

            // GlyphRect 是 struct，跨版本签名差异大，
            // 绝对不调用构造函数 —— 用 default + 反射设字段。
            // Unity 6 / TMP 5 可能把字段改名了，所以同时尝试多种命名。
            object rect = default(GlyphRect);
            TrySetField(rect, "m_X", d.x);
            TrySetField(rect, "x", d.x);
            TrySetSetProp(rect, "x", d.x);
            TrySetField(rect, "m_Y", d.y);
            TrySetField(rect, "y", d.y);
            TrySetSetProp(rect, "y", d.y);
            TrySetField(rect, "m_Width", d.width);
            TrySetField(rect, "width", d.width);
            TrySetSetProp(rect, "width", d.width);
            TrySetField(rect, "m_Height", d.height);
            TrySetField(rect, "height", d.height);
            TrySetSetProp(rect, "height", d.height);
            GlyphRect rectValue = (rect == null) ? default : (GlyphRect)rect;
            TrySetField(g, "m_GlyphRect", rectValue);
            TrySetField(g, "glyphRect", rectValue);
            TrySetSetProp(g, "glyphRect", rectValue);

            TrySetField(g, "m_Scale", 1f);
            TrySetField(g, "scale", 1f);
            TrySetSetProp(g, "scale", 1f);

            TrySetField(g, "m_AtlasIndex", 0);
            TrySetField(g, "atlasIndex", 0);
            TrySetSetProp(g, "atlasIndex", 0);

            // GlyphMetrics 也是 struct，构造签名跨版本差异大，
            // 用 default + 反射设字段，避免编译期依赖具体构造函数
            object metricsObj = default(GlyphMetrics);
            TrySetField(metricsObj, "m_Width", d.width);
            TrySetField(metricsObj, "width", d.width);
            TrySetSetProp(metricsObj, "width", d.width);
            TrySetField(metricsObj, "m_Height", d.height);
            TrySetField(metricsObj, "height", d.height);
            TrySetSetProp(metricsObj, "height", d.height);
            TrySetField(metricsObj, "m_HorizontalBearingX", d.bearingX);
            TrySetField(metricsObj, "horizontalBearingX", d.bearingX);
            TrySetSetProp(metricsObj, "horizontalBearingX", d.bearingX);
            TrySetField(metricsObj, "m_HorizontalBearingY", d.bearingY);
            TrySetField(metricsObj, "horizontalBearingY", d.bearingY);
            TrySetSetProp(metricsObj, "horizontalBearingY", d.bearingY);
            TrySetField(metricsObj, "m_HorizontalAdvance", d.advance);
            TrySetField(metricsObj, "horizontalAdvance", d.advance);
            TrySetSetProp(metricsObj, "horizontalAdvance", d.advance);
            GlyphMetrics metricsValue = (metricsObj == null) ? default : (GlyphMetrics)metricsObj;
            TrySetField(g, "m_Metrics", metricsValue);
            TrySetField(g, "metrics", metricsValue);
            TrySetSetProp(g, "metrics", metricsValue);

            glyphs.Add(g);
        }

        // ---- 构建 TMP_Character ----
        var charListType = typeof(List<>).MakeGenericType(typeof(TMP_Character));
        var characters = (IList)Activator.CreateInstance(charListType);

        foreach (var d in glyphDataList)
        {
            // 不能用三参 (uint, TMP_FontAsset, Glyph) 构造：构造函数内部会立刻
            // 访问 glyph.index，传 null 必崩空指针。
            // 用两参 (uint, TMP_FontAsset) 或无参构造，构造完再反射设字段。
            object character;
            var ctors = typeof(TMP_Character).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // 优先 (uint, TMP_FontAsset) 两参构造
            ConstructorInfo picked = null;
            foreach (var c in ctors)
            {
                var ps = c.GetParameters();
                if (ps.Length == 2 && ps[0].ParameterType == typeof(uint) && ps[1].ParameterType == typeof(TMP_FontAsset))
                {
                    picked = c;
                    break;
                }
            }

            if (picked != null)
            {
                character = picked.Invoke(new object[] { (uint)d.character, loaded });
            }
            else
            {
                // 退到 (uint) 单参构造
                foreach (var c in ctors)
                {
                    var ps = c.GetParameters();
                    if (ps.Length == 1 && ps[0].ParameterType == typeof(uint))
                    {
                        picked = c;
                        break;
                    }
                }

                if (picked != null)
                    character = picked.Invoke(new object[] { (uint)d.character });
                else
                    character = Activator.CreateInstance(typeof(TMP_Character));
            }

            // 绑定 glyph 引用和 index
            var glyphObj = glyphs[(int)d.glyphIndex];
            TrySetField(character, "m_Glyph", glyphObj);
            TrySetField(character, "m_GlyphIndex", (uint)d.glyphIndex);
            TrySetField(character, "m_FontAsset", loaded);
            TrySetField(character, "m_Unicode", (uint)d.character);

            characters.Add(character);
        }

        // ---- 反射写入 FontAsset 字段 ----
        TrySetField(loaded, "m_GlyphTable", glyphs);
        TrySetField(loaded, "m_CharacterTable", characters);

        var atlasArr = new Texture2D[] { atlas };
        TrySetField(loaded, "m_AtlasTextures", atlasArr);
        TrySetField(loaded, "m_AtlasTextureIndex", 0);

        TrySetField(loaded, "m_Material", material);
        TrySetField(loaded, "m_MaterialHashCode", material.name.GetHashCode());

        // ---- 关键：设 atlas 尺寸 ----
        // TMP shader 需要 atlasWidth / atlasHeight 计算 UV，
        // 这两个值如果不设，会出现"显示透明"问题（TMP 内 shader 用 0 计算 UV 得到 NaN）。
        // material 的 _TextureWidth/_TextureHeight 已经在 CreateAsset 之后单独保存过了，
        // 这里只更新 fontasset 内存里的引用关系。
        TrySetField(loaded, "m_AtlasWidth", atlas.width);
        TrySetField(loaded, "m_AtlasHeight", atlas.height);
        // padding 也设上
        TrySetField(loaded, "m_AtlasPadding", atlasPadding);
        // TextMeshPro/Sprite shader 配合 RASTER render mode（彩色 bitmask atlas）。
        // 设成 RASTER 才会让 glyph 当作 RGB color bitmap 采样（保留原图颜色），而不是 SDF/alpha mask。
        // 用 (int) cast 直接绕过 GlyphRenderMode enum 的 namespace 问题（它实际定义在 UnityEngine.TextCore）。
        // 反射写入时字段类型是 GlyphRenderMode enum，runtime 会自动转换回 enum。
        TrySetField(loaded, "m_AtlasRenderMode", (int)UnityEngine.TextCore.LowLevel.GlyphRenderMode.RASTER);
        // 调试: 确认 shader 真的支持 _TextureWidth
        if (!material.HasProperty("_TextureWidth"))
        {
            Debug.LogWarning($"[TMPNumberFontGenerator] shader '{material.shader.name}' 不支持 _TextureWidth，UV 计算可能异常。请确认 TMP Essential Resources 已导入。");
        }

        // ---- FaceInfo ----
        const float fontSize = 100f;
        var faceInfo = new FaceInfo
        {
            familyName = "NumberFont",
            styleName = "Regular",
            pointSize = fontSize,
            scale = _settings.globalScale,
            lineHeight = fontSize,
            ascentLine = fontSize,
            capLine = fontSize,
            meanLine = fontSize * 0.7f,
            baseline = 0f,
            descentLine = -20f,
            superscriptOffset = fontSize * 0.5f,
            superscriptSize = 0.7f,
            subscriptOffset = -fontSize * 0.1f,
            subscriptSize = 0.7f,
            underlineOffset = -10f,
            underlineThickness = 5f,
            strikethroughOffset = fontSize * 0.3f,
            strikethroughThickness = 5f,
            tabWidth = fontSize,
        };
        TrySetField(loaded, "m_FaceInfo", faceInfo);

        // ---- 关键：设版本号 + 初始化遗留字段 ----
        // 否则 TMP 的 Awake() 会因为 m_Version 为空触发 UpgradeFontAsset()，
        // 而 UpgradeFontAsset() 直接遍历 m_glyphInfoList（未初始化为 null）→ NRE。
        TrySetField(loaded, "m_Version", "1.1.0");

        // 同时把遗留字段初始化为空列表，作为双重保险
        // （即便某些 TMP 路径仍会读它，也至少不会 NRE）
        TrySetField(loaded, "m_glyphInfoList", new List<TMP_Glyph>());

        // ---- 重建查找表 ----
        CallInternalIfExists(loaded, "InitializeGlyphDictionary");
        CallInternalIfExists(loaded, "InitializeCharacterLookupDictionary");
        CallInternalIfExists(loaded, "InitializeLigatureSubstitutionLookupDictionary");

        EditorUtility.SetDirty(loaded);
        EditorUtility.SetDirty(material);
#if UNITY_2020_3_OR_NEWER
        AssetDatabase.SaveAssetIfDirty(loaded);
        AssetDatabase.SaveAssetIfDirty(material);
#else
        AssetDatabase.SaveAssets();
#endif
        AssetDatabase.ImportAsset(fontPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        var reloaded = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (reloaded != null) Selection.activeObject = reloaded;

        EditorUtility.DisplayDialog(
            "Success",
            $"Number font created:\n{fontPath}\n\n字符: {string.Join(" ", charMaps.ConvertAll(m => m.unicode.ToString()))}",
            "OK"
        );
    }

    // ============================================================
    // 反射工具
    // ============================================================

    static void TrySetField(object obj, string name, object value)
    {
        if (obj == null) return;
        var f = obj.GetType().GetField(name,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (f != null) f.SetValue(obj, value);
    }

    // GlyphRect / GlyphMetrics 在 Unity 6 + TMP 5 改用 properties 而非字段。
    // FieldInfo 找不到时退路：尝试 property。
    static void TrySetSetProp(object obj, string name, object value)
    {
        if (obj == null) return;
        var p = obj.GetType().GetProperty(name,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (p != null && p.CanWrite)
        {
            try
            {
                p.SetValue(obj, value, null);
            }
            catch
            {
                /* ignore type mismatch */
            }
        }
    }

    static void CallInternalIfExists(object obj, string methodName)
    {
        try
        {
            var m = obj.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (m != null) m.Invoke(obj, null);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[TMPNumberFontGenerator] {methodName} 调用失败: {e.Message}");
        }
    }

    static GlyphMetrics MakeGlyphMetrics(int width, int height, float bearingX, float bearingY, float advance)
    {
        var t = typeof(GlyphMetrics);

        var ctor = t.GetConstructor(new[] { typeof(int), typeof(int), typeof(float), typeof(float), typeof(float) });
        if (ctor != null)
            return (GlyphMetrics)ctor.Invoke(new object[] { width, height, bearingX, bearingY, advance });

        ctor = t.GetConstructor(new[] { typeof(int), typeof(int), typeof(float), typeof(float) });
        if (ctor != null)
        {
            var gm = (GlyphMetrics)ctor.Invoke(new object[] { width, height, bearingX, bearingY });
            var prop = t.GetProperty("horizontalAdvance") ?? t.GetProperty("HorizontalAdvance");
            if (prop != null && prop.CanWrite) prop.SetValue(gm, advance);
            return gm;
        }

        var inst = Activator.CreateInstance(t);
        TrySetField(inst, "m_Width", width);
        TrySetField(inst, "m_Height", height);
        TrySetField(inst, "m_HorizontalBearingX", bearingX);
        TrySetField(inst, "m_HorizontalBearingY", bearingY);
        TrySetField(inst, "m_HorizontalAdvance", advance);
        return (GlyphMetrics)inst;
    }
}