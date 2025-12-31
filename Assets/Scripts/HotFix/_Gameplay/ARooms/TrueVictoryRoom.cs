namespace MarbleHero
{
    public class TrueVictoryRoom : ARoom
    {
        public override RoomType Type => RoomType.TRUE_VICTORY;
        
        // public Cutscene cutscene;

        public TrueVictoryRoom()
        {
            phase = RoomPhase.INCOMPLETE;
            // cutscene = new Cutscene(player.chosenClass);
            // ADungeon.overlayMenu.proceedButton.hideInstantly();
        }

        public override void onPlayerEntry()
        {
            ADungeon.isScreenUp = true;
            // ADungeon.overlayMenu.proceedButton.hide();
            GameCursor.hidden = true;
            ADungeon.screen = CurrentScreen.NO_INTERACT;
        }

        public override void update(float dt)
        {
            base.update(dt);
            // cutscene.update();
        }

        // public override void render(SpriteBatch sb)
        // {
        //     base.render(sb);
        //     cutscene.render(sb);
        // }
        //
        // public override void renderAboveTopPanel(SpriteBatch sb)
        // {
        //     base.renderAboveTopPanel(sb);
        //     cutscene.renderAbove(sb);
        // }

        public override void Dispose()
        {
            base.Dispose();
            // cutscene.dispose();
        }
    }
}