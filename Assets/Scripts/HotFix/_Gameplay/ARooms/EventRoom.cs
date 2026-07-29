namespace MoreMountains
{
    public class EventRoom : ARoom
    {
        public override RoomType Type => RoomType.EVENT;

        public override void onPlayerEntry(APlayer p)
        {
            // ADungeon.overlayMenu.proceedButton.hide();
            // var eventRngDuplicate = new Rand(Settings.seed, ADungeon.eventRng.counter);
            // evt = ADungeon.generateEvent(eventRngDuplicate);
            // evt.onEnterRoom();
        }

        public override void update(float dt)
        {
            base.update(dt);
            if (!ADungeon.isScreenUp)
                evt.update(dt);

            if (evt.waitTimer == 0.0F && !evt.hasFocus && phase != RoomPhase.COMBAT)
            {
                phase = RoomPhase.COMPLETE;
                evt.reopen();
            }
        }

        // public override void render(SpriteBatch sb)
        // {
        //     if (evt != null)
        //         evt.render(sb);
        //     
        //     base.render(sb);
        // }
        //
        // public override void renderAboveTopPanel(SpriteBatch sb)
        // {
        //     base.renderAboveTopPanel(sb);
        //     if (evt != null)
        //         evt.renderAboveTopPanel(sb);
        // }

        protected override int onPlayerCompletedGetPotionChance()
        {
            return 40 + blizzardPotionMod;
        }
    }
}