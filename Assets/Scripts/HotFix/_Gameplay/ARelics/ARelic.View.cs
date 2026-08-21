
namespace MoreMountains
{
    public partial class ARelic
    {
        // static TutorialStrings tutorialStrings = Game.languagePack.getTutorialString("Relic Tip");
        // public static string[] MSG = tutorialStrings.TEXT;
        // public static string[] LABEL = tutorialStrings.LABEL;
        // public static string USED_UP_MSG = MSG[2];
        RelicStrings relicStrings;

        public string[] DESCRIPTIONS;
        // public List<PowerTip> tips = new();
        // FloatyEffect f_effect = new FloatyEffect(10.0F, 0.2F);

        public string imgUrl;

        public void update(float dt)
        {
        }

        public void playLandingSFX()
        {
            switch (landingSFX)
            {
                case LandingSound.CLINK:
                    sound.play("RELIC_DROP_CLINK");
                    return;
                case LandingSound.FLAT:
                    sound.play("RELIC_DROP_FLAT");
                    return;
                case LandingSound.SOLID:
                    sound.play("RELIC_DROP_ROCKY");
                    return;
                case LandingSound.HEAVY:
                    sound.play("RELIC_DROP_HEAVY");
                    return;
                case LandingSound.MAGICAL:
                    sound.play("RELIC_DROP_MAGICAL");
                    return;
                default:
                    sound.play("RELIC_DROP_CLINK");
                    return;
            }
            
        }

        public void flash()
        {
        }
    }
}