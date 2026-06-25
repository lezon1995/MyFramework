using System.Linq;
using Drawing;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MarbleHero
{
    public class LevelEditorManager : MonoBehaviour
    {
        public StageTemplate Template;
        
        BrickGridLayout layout;
        public int rows = 10;
        public int cols = 6;
        public Vector2 size = new(6.0F, 9.4F);
        public Vector2 spacing = new(0.05F, 0.05F);
        public Vector2 padding = new(0.05F, 0.05F);
        public Vector2 levelSize = new(19.2F, 10.8F);

        Camera mainCamera;

        SafeHashSet<BrickTemplate> brickTemplates = new();

        void Awake()
        {
            mainCamera = Camera.main;
            layout = new(size, cols, rows, spacing, padding);
        }

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

            Vector2 mousePos = screenToWorldKeepZ(Input.mousePosition, mainCamera, screenCenterAsZero: false);
            var add = Input.GetKey(KeyCode.LeftShift);
            var remove = Input.GetKey(KeyCode.LeftControl);

            var grids = layout.getAllGrids();
            foreach (var grid in grids)
            {
                Color color = Color.gray6;
                if (grid.Contains(mousePos))
                {
                    color = Color.green;

                    if (add)
                    {
                        brickTemplates.add(new(grid, 1));
                    }
                    else if (remove)
                    {
                        brickTemplates.remove(new(grid, 1));
                    }
                }

                Draw.ingame.xy.WireRectangle(grid, color);
            }

            using var _ = new SafeHashSetReader<BrickTemplate>(brickTemplates, out var reader);
            foreach (var template in reader)
            {
                Color color = Color.gray6;
                Draw.ingame.xy.SolidRectangle(template.rect, color);
                var selectedColor = Color.red;
                if (template.rect.Contains(mousePos))
                {
                    selectedColor = Color.green;
                }

                Draw.ingame.xy.WireRectangle(template.rect, selectedColor);
                Draw.ingame.xy.Label2D(template.position, $"{template.health}", 20, LabelAlignment.Center, Color.red);
                if (template.rect.Contains(mousePos))
                {
                    if (remove)
                    {
                        brickTemplates.remove(template);
                    }
                }
            }
        }

        [Button]
        void SaveToTemplate()
        {
            if (Template == null)
                return;

            Template.bricks = brickTemplates.getMainList().ToArray();
        }
        
        [Button]
        void LoadFromTemplate()
        {
            if (Template == null)
                return;

            brickTemplates.clear();
            foreach (var b in Template.bricks)
                brickTemplates.add(b);
        }
    }
}