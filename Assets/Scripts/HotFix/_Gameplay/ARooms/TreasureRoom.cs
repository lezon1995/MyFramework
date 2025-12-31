namespace MarbleHero
{
    public class TreasureRoom : ARoom
    {
        public override RoomType Type => RoomType.TREASURE;
        
        // static UIStrings uiStrings = Game.languagePack.getUIString("TreasureRoom");
        // public static String[] TEXT = uiStrings.TEXT;
        // public AbstractChest chest;
        float shinyTimer = 0.0F;
        static float SHINY_INTERVAL = 0.2F;

        public TreasureRoom()
        {
            phase = RoomPhase.COMPLETE;
            mapSymbol = "T";
            // mapImg = ImageMaster.MAP_NODE_TREASURE;
            // mapImgOutline = ImageMaster.MAP_NODE_TREASURE_OUTLINE;
        }

        public override void onPlayerEntry()
        {
            playBGM(null);
            // chest = ADungeon.getRandomChest();
            // ADungeon.overlayMenu.proceedButton.setLabel(TEXT[0]);
        }

        public override void update(float dt)
        {
            base.update(dt);
            // if (chest != null)
                // chest.update();
            // updateShiny();
        }

        // void updateShiny()
        // {
        //     if (!chest.isOpen)
        //     {
        //         shinyTimer -= dt;
        //         if (shinyTimer < 0.0F && !Settings.DISABLE_EFFECTS)
        //         {
        //             shinyTimer = 0.2F;
        //             ADungeon.topLevelEffects.add(new ChestShineEffect());
        //             ADungeon.effectList.add(new SpookyChestEffect());
        //             ADungeon.effectList.add(new SpookyChestEffect());
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
        //     if (chest != null)
        //         chest.render(sb);
        //     base.render(sb);
        // }
    }
}