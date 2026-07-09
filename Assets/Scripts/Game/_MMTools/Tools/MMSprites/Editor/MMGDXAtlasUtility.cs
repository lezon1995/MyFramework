using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class ModifyByAtlas
    {
        /// <summary>
        /// 子图 信息
        /// </summary>
        struct GDXAtlasElement
        {
            public string Name;
            public Rect Rect;

            public GDXAtlasElement(string name, Rect rect)
            {
                Name = name;
                Rect = rect;
            }
        }

        /// <summary>
        /// 图集 信息
        /// </summary>
        class GDXAtlas
        {
            public string Name;
            public List<GDXAtlasElement> Elements;

            public GDXAtlas(string name, List<GDXAtlasElement> elements)
            {
                Name = name;
                Elements = elements;
            }
        }

        /// <summary>
        /// 解析图集配置表工具类
        /// </summary>
        static class GDXAtlasUtility
        {
            static string _atlasTxtFile;
            static GDXAtlas[] _atlas;

            internal static bool ParseAtlas(string atlasTxtFile, out GDXAtlas[] atlas)
            {
                if (_atlasTxtFile != null && _atlas != null && _atlasTxtFile == atlasTxtFile)
                {
                    atlas = _atlas;
                    return true;
                }

                _atlasTxtFile = null;
                atlas = _atlas = null;
                if (!File.Exists(atlasTxtFile))
                {
                    Debug.Log($"Doesnt exist atlasTxtFile:{atlasTxtFile}");
                    return false;
                }

                var allLines = File.ReadAllLines(atlasTxtFile, Encoding.UTF8);
                var count = allLines.Count(s => s.Contains(".png"));
                var groups = new List<string>[count];
                _atlasTxtFile = atlasTxtFile;
                atlas = _atlas = new GDXAtlas[count];

                for (int i = 0; i < count; i++)
                {
                    groups[i] = new();
                }


                int index = -1;
                foreach (var line in allLines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.EndsWith(".png"))
                        index++;

                    groups[index].Add(line);
                }

                for (var idx = 0; idx < groups.Length; idx++)
                {
                    var lines = groups[idx];
                    if (lines.Count <= 0)
                    {
                        Debug.Log($"lines.Count <= 0 lines:{lines.Count}");
                        return false;
                    }

                    var atlasName = lines[0].Trim();
                    var elements = new List<GDXAtlasElement>();
                    for (int i = 5; i < lines.Count;)
                    {
                        if (i + 6 > lines.Count)
                            break;

                        string line = lines[i];
                        Debug.Log($"eName:{line} idx:{i}");
                        string[] positions = lines[i + 2].Trim().Split(':')[1].Trim().Split(',');
                        string[] sizes = lines[i + 3].Trim().Split(':')[1].Trim().Split(',');

                        var name = line.Trim();
                        var pos = new Vector2(float.Parse(positions[0].Trim()), float.Parse(positions[1].Trim()));
                        var size = new Vector2(float.Parse(sizes[0].Trim()), float.Parse(sizes[1].Trim()));
                        var rect = new Rect(pos, size);

                        elements.Add(new GDXAtlasElement(name, rect));
                        i += 7;
                    }

                    atlas[idx] = new GDXAtlas(atlasName, elements);
                }

                return true;
            }
        }

        [MenuItem("Assets/ModifyByAltas")]
        static void AutoSliceAtlas()
        {
            string selectObjDir = GetDirPath(AssetDatabase.GetAssetPath(Selection.activeInstanceID));

            GDXAtlas[] gdxAtlasArray;
            string atlasTxtFile = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (!GDXAtlasUtility.ParseAtlas(atlasTxtFile, out gdxAtlasArray))
            {
                Debug.Log("false");
                return;
            }

            string[] dirs = { selectObjDir };
            var assetIds = AssetDatabase.FindAssets("t:Texture", dirs);
            for (int i = 0; i < assetIds.Length; i++)
            {
                string spFileName = AssetDatabase.GUIDToAssetPath(assetIds[i]);
                var spTex = AssetDatabase.LoadAssetAtPath<Texture>(spFileName);

                GDXAtlas gdxAtlas = null;
                foreach (var atlas in gdxAtlasArray)
                {
                    if (spFileName.EndsWith(atlas.Name))
                    {
                        gdxAtlas = atlas;
                        break;
                    }
                }

                if (gdxAtlas == null)
                    continue;

                var spSheet = new SpriteMetaData[gdxAtlas.Elements.Count];
                for (int elemIndex = 0; elemIndex < gdxAtlas.Elements.Count; elemIndex++)
                {
                    var eDt = gdxAtlas.Elements[elemIndex];
                    var spDt = new SpriteMetaData();
                    var fixRect = eDt.Rect;
                    //libgdx图集坐标系原点是左上角, 这里需要转换到Unity坐标系(左下角)
                    fixRect.y = spTex.height - fixRect.y - fixRect.size.y;
                    spDt.name = eDt.Name;
                    spDt.rect = fixRect;
                    spDt.pivot = Vector2.one * 0.5f;
                    spDt.border = Vector4.zero;
                    spDt.alignment = (int)SpriteAlignment.Custom;

                    spSheet[elemIndex] = spDt;
                }

                var texImporter = (TextureImporter)AssetImporter.GetAtPath(spFileName);
                texImporter.spritesheet = spSheet;
                texImporter.spriteImportMode = SpriteImportMode.Multiple;
                texImporter.isReadable = true;
                Undo.RecordObject(texImporter, "Test Undo");
                texImporter.SaveAndReimport();
            }
        }

        public static string GetDirPath(string tpath)
        {
            if (string.IsNullOrEmpty(tpath))
                return "";

            int tidx = tpath.LastIndexOf('/');
            string path = tpath.Substring(0, tidx);
            return path;
        }
    }
}