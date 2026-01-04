using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero
{
    public class Legend
    {
        public static float X = 1670.0F * Settings.xScale;
        public static float Y = 600.0F * Settings.yScale;
        static int LW = 512;
        static int LH = 800;

        public Color c = new Color(1.0F, 1.0F, 1.0F, 0.0F);

        // static UIStrings uiStrings = Game.languagePack.getUIString("Legend");
        // public static string[] TEXT = uiStrings.TEXT;
        public List<LegendItem> items = new();
        public bool isLegendHighlighted;
        static Texture img;

        public Legend()
        {
            // items.Add(new LegendItem(TEXT[0], ImageMaster.MAP_NODE_EVENT, TEXT[1], TEXT[2], 0));
            // items.Add(new LegendItem(TEXT[3], ImageMaster.MAP_NODE_MERCHANT, TEXT[4], TEXT[5], 1));
            // items.Add(new LegendItem(TEXT[6], ImageMaster.MAP_NODE_TREASURE, TEXT[7], TEXT[8], 2));
            // items.Add(new LegendItem(TEXT[9], ImageMaster.MAP_NODE_REST, TEXT[10], TEXT[11], 3));
            // items.Add(new LegendItem(TEXT[12], ImageMaster.MAP_NODE_ENEMY, TEXT[13], TEXT[14], 4));
            // items.Add(new LegendItem(TEXT[15], ImageMaster.MAP_NODE_ELITE, TEXT[16], TEXT[17], 5));
            // if (img == null)
            // img = ImageMaster.loadImage("images/ui/map/selectBox.png");
        }

        // public bool isIconHovered(string nodeHovered)
        // {
        //     return nodeHovered switch
        //     {
        //         "?" => items[0].hb.hovered,
        //         "$" => items[1].hb.hovered,
        //         "T" => items[2].hb.hovered,
        //         "R" => items[3].hb.hovered,
        //         "M" => items[4].hb.hovered,
        //         "E" => items[5].hb.hovered,
        //         _ => false
        //     };
        // }

        public void update(float mapAlpha, bool isMapScreen, float dt)
        {
            if (mapAlpha >= 0.8F && isMapScreen)
            {
                // updateControllerInput();
                c.a = MathHelper.fadeLerpSnap(c.a, 1.0F, dt);
                foreach (LegendItem i in items)
                    i.update();
            }
            else
            {
                c.a = MathHelper.fadeLerpSnap(c.a, 0.0F, dt);
            }
        }

        // void updateControllerInput()
        // {
        //     if (!Settings.isControllerMode)
        //         return;
        //
        //     if (isLegendHighlighted)
        //     {
        //         if (CInputActionSet.proceed.isJustPressed() || CInputActionSet.cancel.isJustPressed() || CInputActionSet.left.isJustPressed() || CInputActionSet.altLeft.isJustPressed())
        //         {
        //             CInputActionSet.cancel.unpress();
        //             isLegendHighlighted = false;
        //             return;
        //         }
        //     }
        //     else if (CInputActionSet.proceed.isJustPressed())
        //     {
        //         isLegendHighlighted = true;
        //         return;
        //     }
        //
        //     if (!isLegendHighlighted)
        //         return;
        //     bool anyHovered = false;
        //     int index = 0;
        //     foreach (LegendItem i in items)
        //     {
        //         if (i.hb.hovered)
        //         {
        //             anyHovered = true;
        //             break;
        //         }
        //
        //         index++;
        //     }
        //
        //     if (!anyHovered)
        //     {
        //         Gdx.input.setCursorPosition((int)items[0].hb.cX, Settings.HEIGHT - (int)items[0].hb.cY);
        //     }
        //     else if (CInputActionSet.down.isJustPressed() || CInputActionSet.altDown.isJustPressed())
        //     {
        //         index++;
        //         if (index > items.Count - 1)
        //             index = 0;
        //         Gdx.input.setCursorPosition((int)items[index].hb.cX, Settings.HEIGHT - (int)items[index].hb.cY);
        //     }
        //     else if (CInputActionSet.up.isJustPressed() || CInputActionSet.altUp.isJustPressed())
        //     {
        //         index--;
        //         if (index < 0)
        //             index = items.Count - 1;
        //         Gdx.input.setCursorPosition((int)items[index].hb.cX, Settings.HEIGHT - (int)items[index].hb.cY);
        //     }
        // }

        // public void render(SpriteBatch sb)
        // {
        //     sb.setColor(c);
        //     if (!Settings.isMobile)
        //         sb.draw(ImageMaster.MAP_LEGEND, X - 256.0F, Y - 400.0F, 256.0F, 400.0F, 512.0F, 800.0F, Settings.scale, Settings.yScale, 0.0F, 0, 0, 512, 800, false, false);
        //     else
        //         sb.draw(ImageMaster.MAP_LEGEND, X - 256.0F, Y - 400.0F, 256.0F, 400.0F, 512.0F, 800.0F, Settings.scale * 1.1F, Settings.yScale * 1.1F, 0.0F, 0, 0, 512, 800, false, false);
        //
        //     Color c2 = new Color(MapRoomNode.AVAILABLE_COLOR.r, MapRoomNode.AVAILABLE_COLOR.g, MapRoomNode.AVAILABLE_COLOR.b, c.a);
        //     if (Settings.isMobile)
        //         FontHelper.renderFontCentered(sb, FontHelper.menuBannerFont, TEXT[18], X, Y + 190.0F * Settings.yScale, c2, 1.4F);
        //     else
        //         FontHelper.renderFontCentered(sb, FontHelper.menuBannerFont, TEXT[18], X, Y + 170.0F * Settings.yScale, c2);
        //
        //     sb.setColor(c2);
        //     foreach (LegendItem i in items)
        //         i.render(sb, c2);
        //
        //     if (Settings.isControllerMode)
        //     {
        //         sb.setColor(new Color(1.0F, 1.0F, 1.0F, c2.a));
        //         sb.draw(CInputActionSet.proceed.getKeyImg(), 1570.0F * Settings.xScale - 32.0F, Y + 170.0F * Settings.yScale - 32.0F, 32.0F, 32.0F, 64.0F, 64.0F, Settings.scale, Settings.scale, 0.0F, 0, 0, 64, 64, false, false);
        //         if (isLegendHighlighted)
        //         {
        //             sb.setColor(new Color(1.0F, 0.9F, 0.5F, 0.6F + MathUtils.cosDeg((float)(TimeHelper.currentTimeMillis() / 2L % 360L)) / 5.0F));
        //             float doop = 1.0F + (1.0F + MathUtils.cosDeg((float)(TimeHelper.currentTimeMillis() / 2L % 360L))) / 50.0F;
        //             sb.draw(img, 1670.0F * Settings.scale - 160.0F, (Settings.HEIGHT - Gdx.input.getY()) - 52.0F + 4.0F * Settings.scale, 160.0F, 52.0F, 320.0F, 104.0F, Settings.scale * doop, Settings.scale * doop, 0.0F, 0, 0, 320, 104, false, false);
        //         }
        //     }
        // }
    }
}