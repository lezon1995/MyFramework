namespace MoreMountains
{
    public class RestRoom : ARoom
    {
        public override RoomType Type => RoomType.REST;
        
        public int fireSoundId;
        public static int lastFireSoundId;
        // public CampfireUI campfireUI;

        public override void onPlayerEntry(APlayer p)
        {
            base.onPlayerEntry(p);
            // if (ADungeon.id != ("TheEnding"))
            // music.silenceBGM();
            // fireSoundId = Game.sound.playAndLoop("REST_FIRE_WET");
            lastFireSoundId = fireSoundId;
            // campfireUI = new CampfireUI();
            // foreach (var r in player.relics)
                // r.onEnterRestRoom();
        }

        public override CardRarity getCardRarity(int roll)
        {
            return getCardRarity(roll, false);
        }

        public override void update(float dt)
        {
            base.update(dt);
            // campfireUI?.update();
        }

        public void fadeIn()
        {
            // if (ADungeon.id != ("TheEnding"))
            // music.unsilenceBGM();
        }

        public void cutFireSound()
        {
            //Game.sound.fadeOut("REST_FIRE_WET", ((RestRoom)room).fireSoundId);
        }

        public void updateAmbience()
        {
            //Game.sound.adjustVolume("REST_FIRE_WET", fireSoundId);
        }

        // public override void render(SpriteBatch sb)
        // {
        //     if (campfireUI != null)
        //         campfireUI.render(sb);
        //     base.render(sb);
        // }
    }
}