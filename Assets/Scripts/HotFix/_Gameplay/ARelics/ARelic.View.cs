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
        // public Hitbox hb = new Hitbox(PAD_X, PAD_X);

        public string imgUrl;

        /*
        protected void initializeTips()
        {
            Scanner desc = new Scanner(description);
            while (desc.hasNext())
            {
                string s = desc.next();
                if (s.charAt(0) == '#')
                    s = s.substring(2);
                s = s.replace(',', ' ');
                s = s.replace('.', ' ');
                s = s.trim();
                s = s.toLowerCase();
                bool alreadyExists = false;
                if (GameDictionary.keywords.containsKey(s))
                {
                    s = GameDictionary.parentWord.get(s);
                    foreach (PowerTip t in tips)
                    {
                        if (t.header.toLowerCase().equals(s))
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (!alreadyExists)
                        tips.Add(new PowerTip(TipHelper.capitalize(s), GameDictionary.keywords.get(s)));
                }
            }

            desc.close();
        }
        */

        public void update(float dt)
        {
        }

        public void playLandingSFX()
        {
            // switch (landingSFX)
            // {
            //     case LandingSound.CLINK:
            //         Game.sound.play("RELIC_DROP_CLINK");
            //         return;
            //     case LandingSound.FLAT:
            //         Game.sound.play("RELIC_DROP_FLAT");
            //         return;
            //     case LandingSound.SOLID:
            //         Game.sound.play("RELIC_DROP_ROCKY");
            //         return;
            //     case LandingSound.HEAVY:
            //         Game.sound.play("RELIC_DROP_HEAVY");
            //         return;
            //     case LandingSound.MAGICAL:
            //         Game.sound.play("RELIC_DROP_MAGICAL");
            //         return;
            // }
            //
            // Game.sound.play("RELIC_DROP_CLINK");
        }

        public void flash()
        {
        }
    }
}