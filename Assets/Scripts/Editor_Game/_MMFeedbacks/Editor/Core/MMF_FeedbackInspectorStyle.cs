using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    internal static class MMF_FeedbackInspectorStyle
    {
        public static GUIStyle ContainerStyle;
        public static GUIStyle BoxChildStyle;
        public static GUIStyle GroupStyle;
        public static GUIStyle TextStyle;

        public static bool IsProSkin = EditorGUIUtility.isProSkin;
        public static Texture2D GroupClosedTriangle = Resources.Load<Texture2D>("IN foldout focus-6510");
        public static Texture2D GroupOpenTriangle = Resources.Load<Texture2D>("IN foldout focus on-5718");
        public static Texture2D NoTexture = new Texture2D(0, 0);

        static MMF_FeedbackInspectorStyle()
        {
            // TEXT STYLE --------------------------------------------------------------------------------------------------------------

            TextStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                richText = true,
                contentOffset = new Vector2(0, 25)
            };

            //TextStyle.font = Font.CreateDynamicFontFromOSFont(new[] { "Terminus (TTF) for Windows", "Calibri" }, 14);

            // GROUP STYLE --------------------------------------------------------------------------------------------------------------

            GroupStyle = new GUIStyle(EditorStyles.foldout)
            {
                active =
                {
                    background = GroupClosedTriangle
                },
                focused =
                {
                    background = GroupClosedTriangle
                },
                hover =
                {
                    background = GroupClosedTriangle
                },
                onActive =
                {
                    background = GroupOpenTriangle
                },
                onFocused =
                {
                    background = GroupOpenTriangle
                },
                onHover =
                {
                    background = GroupOpenTriangle
                },
                fontStyle = FontStyle.Bold,
                overflow = new RectOffset(100, 0, 0, 0),
                padding = new RectOffset(20, 0, 0, 0)
            };

            // CONTAINER STYLE --------------------------------------------------------------------------------------------------------------

            ContainerStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(20, 0, 0, 0)
            };

            // BOX CHILD STYLE --------------------------------------------------------------------------------------------------------------

            BoxChildStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                normal =
                {
                    background = NoTexture
                }
            };
        }

        static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}