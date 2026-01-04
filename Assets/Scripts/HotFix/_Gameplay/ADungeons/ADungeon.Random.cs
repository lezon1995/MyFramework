namespace MarbleHero
{
    public abstract partial class ADungeon
    {
        public static Rand monsterRng;
        public static Rand mapRng;
        public static Rand eventRng;
        public static Rand merchantRng;
        public static Rand cardRng;
        public static Rand treasureRng;
        public static Rand relicRng;
        public static Rand potionRng;
        public static Rand monsterHpRng;
        public static Rand aiRng;
        public static Rand shuffleRng;
        public static Rand cardRandomRng;
        public static Rand miscRng;

        public static void GenSeeds()
        {
            if (Settings.seed != null)
            {
                var seed = Settings.seed.Value;
                log("Generating seeds: " + seed);
                monsterRng = new(seed);
                eventRng = new(seed);
                merchantRng = new(seed);
                cardRng = new(seed);
                treasureRng = new(seed);
                relicRng = new(seed);
                monsterHpRng = new(seed);
                potionRng = new(seed);
                aiRng = new(seed);
                shuffleRng = new(seed);
                cardRandomRng = new(seed);
                miscRng = new(seed);
            }
        }

        public static void LoadSeeds(SaveFile save)
        {
            if (save.is_daily || save.is_trial)
            {
                Settings.isDailyRun = save.is_daily;
                Settings.isTrial = save.is_trial;
                Settings.specialSeed = save.special_seed;
                var seed = save.is_daily ? save.special_seed : save.seed;
                ModHelper.setTodaysMods(seed, player.chosenClass);
            }


            if (Settings.seed != null)
            {
                var seed = Settings.seed.Value;
                monsterRng = new(seed, save.monster_seed_count);
                eventRng = new(seed, save.event_seed_count);
                merchantRng = new(seed, save.merchant_seed_count);
                cardRng = new(seed, save.card_seed_count);
                // cardBlizzRandomizer = save.card_random_seed_randomizer;
                treasureRng = new(seed, save.treasure_seed_count);
                relicRng = new(seed, save.relic_seed_count);
                potionRng = new(seed, save.potion_seed_count);
                log("Loading seeds: " + seed);
                log("Monster seed:  " + monsterRng.counter);
                log("Event seed:    " + eventRng.counter);
                log("Merchant seed: " + merchantRng.counter);
                log("Card seed:     " + cardRng.counter);
                log("Treasure seed: " + treasureRng.counter);
                log("Relic seed:    " + relicRng.counter);
                log("Potion seed:   " + potionRng.counter);
            }
        }
    }
}