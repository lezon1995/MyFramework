using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public class LevelEditorManager : MonoBehaviour
    {
        BrickGridLayout layout;
        public int rows = 10;
        public int cols = 6;
        public Vector2 size = new(6, 8);
        public Vector2 spacing = new(0.05F, 0.05F);
        public Vector2 padding = new(0.05F, 0.05F);
        public Vector2 levelSize = new(19.2F, 10.8F);

        Camera mainCamera;

        void Awake()
        {
            mainCamera = Camera.main;
            layout = new(size, cols, rows, spacing, padding);
        }

        [ContextMenu("RefreshLayout")]
        void RefreshLayout(Vector2 offset)
        {
            layout.setRows(rows);
            layout.setCols(cols);
            layout.setSize(size.x, size.y);
            layout.setSpacing(spacing);
            layout.setPadding(padding);
            layout.setOffset(offset);
            layout.getGrids();
        }

        void Update()
        {
            var size = layout.getSize();
            var screenSize = new Vector2(Screen.width, Screen.height) / 100F;
            var topY = size.y / 2F;
            var offset = new Vector2(0, levelSize.y / 2F - topY);

            var (top1, top2) = (new Vector2(-screenSize.x / 2F, topY) + offset, new Vector2(screenSize.x / 2F, topY) + offset);
            var (bot1, bot2) = (new Vector2(-screenSize.x / 2F, -size.y / 2F) + offset, new Vector2(screenSize.x / 2F, -size.y / 2F) + offset);
            var (left1, left2) = (new Vector2(-size.x / 2F, screenSize.y / 2F) + offset, new Vector2(-size.x / 2F, -screenSize.y / 2F) + offset);
            var (right1, right2) = (new Vector2(size.x / 2F, screenSize.y / 2F) + offset, new Vector2(size.x / 2F, -screenSize.y / 2F) + offset);
            RefreshLayout(offset);

            Draw.ingame.xy.Line(top1, top2);
            Draw.ingame.xy.Line(bot1, bot2);
            Draw.ingame.xy.Line(left1, left2);
            Draw.ingame.xy.Line(right1, right2);

            Vector2 mousePos = screenToWorld(Input.mousePosition, mainCamera, screenCenterAsZero: false);
            var add = Input.GetKey(KeyCode.LeftShift);
            var remove = Input.GetKey(KeyCode.LeftControl);

            var grids = layout.getAllGrids();
            foreach (var grid in grids)
            {
                Color color = Color.gray6;
                if (grid.Contains(mousePos))
                {
                    color = Color.green;
                }

                Draw.ingame.xy.WireRectangle(grid, color);
            }
        }
    }
}