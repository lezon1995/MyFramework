using System.Collections.Generic;

namespace MoreMountains
{
    public static class TrialHelper
    {
        static Dictionary<string, TRIAL> trialKeysMap;

        enum TRIAL
        {
            RANDOM_MODS,
            NO_CARD_DROPS,
            UNCEASING_TOP,
            LOSE_MAX_HP,
            SNECKO,
            SLOW,
            FORMS,
            DRAFT,
            MEGA_DRAFT,
            ONE_HP,
            MORE_CARDS,
            CURSED
        }

        static void initialize()
        {
            if (trialKeysMap != null)
                return;
            trialKeysMap = new()
            {
                { formatKey("RandomMods"), TRIAL.RANDOM_MODS },
                { formatKey("DailyMods"), TRIAL.RANDOM_MODS },
                { formatKey("StarterDeck"), TRIAL.NO_CARD_DROPS },
                { formatKey("Inception"), TRIAL.UNCEASING_TOP },
                { formatKey("FadeAway"), TRIAL.LOSE_MAX_HP },
                { formatKey("PraiseSnecko"), TRIAL.SNECKO },
                { formatKey("YoureTooSlow"), TRIAL.SLOW },
                { formatKey("MyTrueForm"), TRIAL.FORMS },
                { formatKey("Draft"), TRIAL.DRAFT },
                { formatKey("MegaDraft"), TRIAL.MEGA_DRAFT },
                { formatKey("1HitWonder"), TRIAL.ONE_HP },
                { formatKey("MoreCards"), TRIAL.MORE_CARDS },
                { formatKey("Cursed"), TRIAL.CURSED }
            };
        }

        static string formatKey(string key)
        {
            return SeedHelper.sterilizeString(key);
        }

        public static bool isTrialSeed(string seed)
        {
            initialize();
            return trialKeysMap.ContainsKey(seed);
        }

        public static ATrial getTrialForSeed(string seed)
        {
            initialize();
            if (seed == null)
                return null;

            if (!trialKeysMap.TryGetValue(seed, out var picked))
                return null;

            return picked switch
            {
                TRIAL.RANDOM_MODS => new RandomModsTrial(),
                TRIAL.NO_CARD_DROPS => new StarterDeckTrial(),
                TRIAL.UNCEASING_TOP => new InceptionTrial(),
                TRIAL.LOSE_MAX_HP => new LoseMaxHpTrial(),
                TRIAL.SNECKO => new SneckoTrial(),
                TRIAL.SLOW => new SlowpokeTrial(),
                TRIAL.FORMS => new MyTrueFormTrial(),
                TRIAL.DRAFT => new DraftTrial(),
                TRIAL.MEGA_DRAFT => new AnyColorDraftTrial(),
                TRIAL.ONE_HP => new OneHpTrial(),
                TRIAL.MORE_CARDS => new HoarderTrial(),
                TRIAL.CURSED => new CursedTrial(),
                _ => null
            };
        }
    }
}