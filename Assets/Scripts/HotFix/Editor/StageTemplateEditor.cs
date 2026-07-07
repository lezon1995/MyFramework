#if UNITY_EDITOR
using MarbleHero;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageTemplate))]
public class StageTemplateEditor : Editor
{
    const float aspect = 16F / 9F;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var template = (StageTemplate)target;
        if (template.bricks == null || template.bricks.Length == 0)
        {
            EditorGUILayout.HelpBox("No bricks defined.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Brick Preview", EditorStyles.boldLabel);

        // ---- 16:9 锁定预览区 ----
        var availableWidth = EditorGUIUtility.currentViewWidth - 28;
        var previewScale = availableWidth / 19.2F;
        var previewWidth = availableWidth;
        var previewHeight = previewWidth / aspect;
        var previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
        previewRect.height = previewRect.width / aspect;

        // ---- 以 (0,0) 为中心的世界坐标范围 ----
        var halfWorldW = previewRect.width / (2f * previewScale);
        var halfWorldH = previewRect.height / (2f * previewScale);
        var worldBounds = new Rect(-halfWorldW, -halfWorldH, halfWorldW * 2, halfWorldH * 2);

        // ---- 绘制 ----
        Handles.BeginGUI();

        // 背景
        EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f, 1f));

        // 中心十字线
        var centerScreen = WorldToScreen(Vector2.zero, worldBounds, in previewRect);
        Handles.color = new Color(1f, 1f, 1f, 0.25f);
        Handles.DrawLine(new(centerScreen.x, previewRect.yMin), new(centerScreen.x, previewRect.yMax));
        Handles.DrawLine(new(previewRect.xMin, centerScreen.y), new(previewRect.xMax, centerScreen.y));

        // 边框
        Handles.color = Color.white;
        Handles.DrawLine(new(previewRect.xMin, previewRect.yMin), new(previewRect.xMax, previewRect.yMin));
        Handles.DrawLine(new(previewRect.xMax, previewRect.yMin), new(previewRect.xMax, previewRect.yMax));
        Handles.DrawLine(new(previewRect.xMax, previewRect.yMax), new(previewRect.xMin, previewRect.yMax));
        Handles.DrawLine(new(previewRect.xMin, previewRect.yMax), new(previewRect.xMin, previewRect.yMin));

        // 鼠标悬浮 Tooltip
        var e = Event.current;

        if (template.bricks is { Length: > 0 })
        {
            foreach (var b in template.bricks)
            {
                var size = b.rect.size;
                if (size.x <= 0 || size.y <= 0)
                    continue;

                // var brickMinWorld = new Vector2(b.position.x - size.x * 0.5f, b.position.y - size.y * 0.5f);
                // var brickMaxWorld = new Vector2(b.position.x + size.x * 0.5f, b.position.y + size.y * 0.5f);
                var brickMinWorld = b.rect.min;
                var brickMaxWorld = b.rect.max;

                var sMin = WorldToScreen(brickMinWorld, worldBounds, in previewRect);
                var sMax = WorldToScreen(brickMaxWorld, worldBounds, in previewRect);
                var rect = new Rect(sMin, sMax - sMin);
                Color color;
                if (rect.Contains(e.mousePosition))
                    color = Color.gray;
                else
                    color = GetColorByHealth(b.health);

                Handles.DrawSolidRectangleWithOutline(rect, color, Color.black);
            }
        }

        if (previewRect.Contains(e.mousePosition))
        {
            Vector2 mouseWorld = ScreenToWorld(e.mousePosition, in previewRect, worldBounds);
            foreach (var b in template.bricks)
            {
                var size = b.rect.size;
                if (size.x <= 0 || size.y <= 0)
                    continue;

                var brickRect = new Rect(b.position.x - size.x * 0.5f, b.position.y - size.y * 0.5f, size.x, size.y);
                if (brickRect.Contains(mouseWorld))
                {
                    var tooltip = $"pos: {b.position}\nsize: {size}\nhealth: {b.health}";
                    var labelPos = e.mousePosition + new Vector2(14, 14);
                    // 防止 tooltip 超出预览区右边界
                    if (labelPos.x + 160 > previewRect.xMax)
                        labelPos.x = e.mousePosition.x - 170;
                    EditorGUI.LabelField(new Rect(labelPos, new(170, 60)), tooltip);
                    break;
                }
            }
        }

        Handles.EndGUI();
    }

    // 屏幕坐标 -> 世界坐标
    static Vector2 ScreenToWorld(Vector2 s, in Rect previewRect, Rect worldBounds)
    {
        float u = (s.x - previewRect.xMin) / previewRect.width;
        float v = 1f - (s.y - previewRect.yMin) / previewRect.height;
        return new(worldBounds.xMin + u * worldBounds.width, worldBounds.yMin + v * worldBounds.height);
    }

    // 世界坐标 -> 屏幕坐标  (Y 轴翻转，原点在预览区中心)
    static Vector2 WorldToScreen(Vector2 w, Rect worldBounds, in Rect previewRect)
    {
        float u = (w.x - worldBounds.xMin) / worldBounds.width;
        float v = 1f - (w.y - worldBounds.yMin) / worldBounds.height;
        return new(previewRect.xMin + u * previewRect.width, previewRect.yMin + v * previewRect.height);
    }

    static Color GetColorByHealth(int health = 1)
    {
        // health 越高颜色越红，低 health 偏绿
        float t = Mathf.Clamp01(health / 50f);
        return Color.Lerp(Color.green, Color.red, t);
    }
}
#endif