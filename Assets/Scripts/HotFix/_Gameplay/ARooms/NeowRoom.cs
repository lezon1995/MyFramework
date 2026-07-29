namespace MoreMountains
{
    public class NeowRoom : ARoom
    {
        public override RoomType Type => RoomType.NEOW;

        public NeowRoom(bool isDone)
        {
            phase = RoomPhase.EVENT;
            evt = new NeowEvent(isDone);
        }

        public override void onPlayerEntry(APlayer p)
        {
            // ADungeon.overlayMenu.proceedButton.hide();
            evt.onEnterRoom();

            foreach (var ballDef in _charSelectInfo.balls)
            {
                p.BallManagement.EquipBallAtInitialization(BallItem.New(ballDef, 1));
            }
        }

        public override void onPlayerExit()
        {
            base.onPlayerExit();
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

        // public void render(SpriteBatch sb)
        // {
        //     super.render(sb);
        //     evt.render(sb);
        // }
        //
        // public void renderAboveTopPanel(SpriteBatch sb)
        // {
        //     super.renderAboveTopPanel(sb);
        //     if (evt != null)
        //         evt.renderAboveTopPanel(sb);
        // }
    }
}