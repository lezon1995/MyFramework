using UnityEngine;

#if UNITY_EDITOR

namespace MoreMountains
{
    public partial class Buff
    {
        Camera _camera;
        int FontSize = 24;
        float FontGap = 40F;

        void Awake()
        {
            _camera = Camera.main;
        }

        void OnGUI()
        {
            if (gameObject.activeInHierarchy && Owner)
            {
                var start = transform.position;

                // 定义要显示的文字内容
                string strDuration;
                if (_isInfinite)
                    strDuration = string.Empty;
                else
                    strDuration = $"({DurationLeft:F1}/{Duration:F1})";

                string str;
                if (_isStackable)
                    str = $"{BuffType.main.Name}:({Stack}/{MaxStack}):{strDuration}";
                else
                    str = $"{BuffType.main.Name}:{strDuration}";

                // 定义文字的位置和大小
                Vector2 screenPoint = _camera.WorldToScreenPoint(start);
                screenPoint.y = Screen.height - screenPoint.y;
                var count = Owner.Buffs.IndexOf(this) + 1;
                screenPoint.y += FontGap * count;
                Rect forwardRect = new Rect(screenPoint, new Vector2(100, 20));
                // 在屏幕上绘制文字
                var guiStyle = new GUIStyle
                {
                    fontSize = FontSize,
                    normal = { textColor = Color.green }
                };
                GUI.Label(forwardRect, str, guiStyle);
            }
        }
    }
}
#endif