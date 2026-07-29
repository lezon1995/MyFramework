namespace MoreMountains
{
    public class TreasureRoomBoss : ARoom
    {
        public override RoomType Type => RoomType.TREASURE;

        // static UIStrings uiStrings = Game.languagePack.getUIString("TreasureRoomBoss");
        // public static String[] TEXT = uiStrings.TEXT;
        // public AbstractChest chest;
        float shinyTimer;
        static float SHINY_INTERVAL = 0.02F;
        public bool choseRelic = false;

        public TreasureRoomBoss()
        {
            Game.nextDungeon = getNextDungeonName();
            if (ADungeon.actNum < 4 || !APlayer.customMods.Contains("Blight Chests"))
                phase = RoomPhase.COMPLETE;
            else
                phase = RoomPhase.INCOMPLETE;

            // mapImg = ImageMaster.MAP_NODE_TREASURE;
            // mapImgOutline = ImageMaster.MAP_NODE_TREASURE_OUTLINE;
        }

        string getNextDungeonName()
        {
            switch (ADungeon.id)
            {
                case "Exordium":
                    return "TheCity";
                case "TheCity":
                    return "TheBeyond";
                case "TheBeyond":
                    if (Settings.isEndless)
                        return "Exordium";
                    return null;
            }

            return null;
        }

        public override void onPlayerEntry(APlayer p)
        {
            // music.silenceBGM();
            // if (ADungeon.actNum < 4 || !APlayer.customMods.Contains("Blight Chests"))
                // ADungeon.overlayMenu.proceedButton.setLabel(TEXT[0]);
            playBGM("SHRINE");
            // chest = new BossChest();
        }

        public override void update(float dt)
        {
            base.update(dt);
            // chest.update();
            // updateShiny();
        }

        // void updateShiny(float dt)
        // {
        //     if (!chest.isOpen)
        //     {
        //         shinyTimer -= dt;
        //         if (shinyTimer < 0.0F && !Settings.DISABLE_EFFECTS)
        //         {
        //             shinyTimer = 0.02F;
        //             ADungeon.effectList.add(new SpookierChestEffect());
        //             ADungeon.effectList.add(new SpookierChestEffect());
        //         }
        //     }
        // }

        // public override void renderAboveTopPanel(SpriteBatch sb)
        // {
        //     base.renderAboveTopPanel(sb);
        // }
        //
        // public override void render(SpriteBatch sb)
        // {
        //     chest.render(sb);
        //     base.render(sb);
        // }
    }
}