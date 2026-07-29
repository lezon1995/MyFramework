namespace MoreMountains
{
    public class VictoryRoom : ARoom
    {
        public override RoomType Type => RoomType.VICTORY;

        public EventType eType;

        public enum EventType
        {
            HEART,
            NONE
        }

        public VictoryRoom(EventType type)
        {
            phase = RoomPhase.EVENT;
            eType = type;
        }

        public override void onPlayerEntry(APlayer p)
        {
            // ADungeon.overlayMenu.proceedButton.hide();
            // switch (eType)
            // {
            //     case HEART:
            //         evt = new SpireHeart();
            //         evt.onEnterRoom();
            //         break;
            // }
        }

        public override void update(float dt)
        {
            base.update(dt);
            // if (!ADungeon.isScreenUp)
                // evt.update();
        }

        // public override void render(SpriteBatch sb)
        // {
        //     if (evt != null)
        //     {
        //         evt.renderRoomEventPanel(sb);
        //         evt.render(sb);
        //     }
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
    }
}