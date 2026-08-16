namespace MoreMountains
{
    public class EmptyRoom : ARoom
    {
        public override RoomType Type => RoomType.EMPTY;
        
        public override void onPlayerEntry(APlayer p)
        {
            base.onPlayerEntry(p);
        }
    }
}