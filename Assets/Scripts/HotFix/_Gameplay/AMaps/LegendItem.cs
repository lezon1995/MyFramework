namespace MarbleHero
{
    public class LegendItem
    {
        static float ICON_X = 1575.0F * Settings.xScale;
        static float TEXT_X = 1670.0F * Settings.xScale;
        static float SPACE_Y = Settings.isMobile ? (64.0F * Settings.yScale) : (58.0F * Settings.yScale);

        static float OFFSET_Y = Settings.isMobile ? (110.0F * Settings.yScale) : (100.0F * Settings.yScale);

        // Texture img;
        static int W = 128;
        int index;
        string label;
        string header;
        string body;
        // public Hitbox hb = new Hitbox(230.0F * Settings.xScale, SPACE_Y - 2.0F);

        public LegendItem(string label, /*Texture img,*/ string tipHeader, string tipBody, int index)
        {
            this.label = label;
            // this.img = img;
            header = tipHeader;
            body = tipBody;
            this.index = index;
        }

        public void update()
        {
            // hb.update();
            // if (hb.hovered)
            // TipHelper.renderGenericTip(1500.0F * Settings.xScale, 270.0F * Settings.scale, header, body);
        }

        // public void render(SpriteBatch sb, Color c)
        // {
        //     sb.setColor(c);
        //     if (!Settings.isMobile)
        //     {
        //         if (hb.hovered)
        //         {
        //             sb.draw(img, ICON_X - 64.0F, Legend.Y - SPACE_Y * index + OFFSET_Y - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, Settings.scale / 1.2F, Settings.scale / 1.2F, 0.0F, 0, 0, 128, 128, false, false);
        //         }
        //         else
        //         {
        //             sb.draw(img, ICON_X - 64.0F, Legend.Y - SPACE_Y * index + OFFSET_Y - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, Settings.scale / 1.65F, Settings.scale / 1.65F, 0.0F, 0, 0, 128, 128, false, false);
        //         }
        //     }
        //     else if (hb.hovered)
        //     {
        //         sb.draw(img, ICON_X - 64.0F, Legend.Y - SPACE_Y * index + OFFSET_Y - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, Settings.scale, Settings.scale, 0.0F, 0, 0, 128, 128, false, false);
        //     }
        //     else
        //     {
        //         sb.draw(img, ICON_X - 64.0F, Legend.Y - SPACE_Y * index + OFFSET_Y - 64.0F, 64.0F, 64.0F, 128.0F, 128.0F, Settings.scale / 1.3F, Settings.scale / 1.3F, 0.0F, 0, 0, 128, 128, false, false);
        //     }
        //
        //     if (Settings.isMobile)
        //         FontHelper.panelNameFont.getData().setScale(1.2F);
        //     FontHelper.renderFontLeftTopAligned(sb, FontHelper.panelNameFont, label, TEXT_X - 50.0F * Settings.scale, Legend.Y - SPACE_Y * index + OFFSET_Y + 13.0F * Settings.yScale, c);
        //     if (Settings.isMobile)
        //         FontHelper.panelNameFont.getData().setScale(1.0F);
        //     hb.move(TEXT_X, Legend.Y - SPACE_Y * index + OFFSET_Y);
        //     if (c.a != 0.0F)
        //         hb.render(sb);
        // }
    }
}