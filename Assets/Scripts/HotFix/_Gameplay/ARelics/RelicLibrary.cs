using System;
using System.Collections.Generic;

namespace MoreMountains
{
    public enum RelicTier
    {
        DEPRECATED,
        STARTER,
        COMMON,
        UNCOMMON,
        RARE,
        SPECIAL,
        BOSS,
        SHOP
    }

    public class RelicLibrary
    {
        public static int totalRelicCount;
        public static int seenRelics;
        static Dictionary<string, ARelic> sharedRelics = new();
        static Dictionary<string, ARelic> redRelics = new();
        static Dictionary<string, ARelic> greenRelics = new();
        static Dictionary<string, ARelic> blueRelics = new();
        static Dictionary<string, ARelic> purpleRelics = new();
        public static List<ARelic> starterList = new();
        public static List<ARelic> commonList = new();
        public static List<ARelic> uncommonList = new();
        public static List<ARelic> rareList = new();
        public static List<ARelic> bossList = new();
        public static List<ARelic> specialList = new();
        public static List<ARelic> shopList = new();
        public static List<ARelic> redList = new();
        public static List<ARelic> greenList = new();
        public static List<ARelic> blueList = new();
        public static List<ARelic> whiteList = new();

        public static void initialize()
        {
            long startTime = TimeUtility.getNowTimeStampMS();
            add(new AmmoSupply());
            add(new Blender());
            add(new BrokenTripod());
            add(new BurlapBag());
            add(new FishingNets());
            add(new FreeBall());
            add(new ImpactHammer());
            add(new LakeMirror());
            add(new MilkShake());
            add(new Origami());
            add(new Rattle());
            add(new RhombicDarts());
            add(new RoughCelling());
            add(new RoughWall());
            add(new SideBorderPortal());
            add(new TacticalShield());
            // add(new Abacus());
            // add(new Akabeko());
            // add(new Anchor());
            // add(new AncientTeaSet());
            // add(new ArtOfWar());
            // add(new Astrolabe());
            // add(new BagOfMarbles());
            // add(new BagOfPreparation());
            // add(new BirdFacedUrn());
            // add(new BlackStar());
            // add(new BloodVial());
            // add(new BloodyIdol());
            // add(new BlueCandle());
            // add(new Boot());
            // add(new BottledFlame());
            // add(new BottledLightning());
            // add(new BottledTornado());
            // add(new BronzeScales());
            // add(new BustedCrown());
            // add(new Calipers());
            // add(new CallingBell());
            // add(new CaptainsWheel());
            // add(new Cauldron());
            // add(new CentennialPuzzle());
            // add(new CeramicFish());
            // add(new ChemicalX());
            // add(new ClockworkSouvenir());
            // add(new CoffeeDripper());
            // add(new Courier());
            // add(new CultistMask());
            // add(new CursedKey());
            // add(new DarkstonePeriapt());
            // add(new DeadBranch());
            // add(new DollysMirror());
            // add(new DreamCatcher());
            // add(new DuVuDoll());
            // add(new Ectoplasm());
            // add(new EmptyCage());
            // add(new Enchiridion());
            // add(new EternalFeather());
            // add(new FaceOfCleric());
            // add(new FossilizedHelix());
            // add(new FrozenEgg2());
            // add(new FrozenEye());
            // add(new FusionHammer());
            // add(new GamblingChip());
            // add(new Ginger());
            // add(new Girya());
            // add(new GoldenIdol());
            // add(new GremlinHorn());
            // add(new GremlinMask());
            // add(new HandDrill());
            // add(new HappyFlower());
            // add(new HornCleat());
            // add(new IceCream());
            // add(new IncenseBurner());
            // add(new InkBottle());
            // add(new JuzuBracelet());
            // add(new Kunai());
            // add(new Lantern());
            // add(new LetterOpener());
            // add(new LizardTail());
            // add(new Mango());
            // add(new MarkOfTheBloom());
            // add(new Matryoshka());
            // add(new MawBank());
            // add(new MealTicket());
            // add(new MeatOnTheBone());
            // add(new MedicalKit());
            // add(new MembershipCard());
            // add(new MercuryHourglass());
            // add(new MoltenEgg2());
            // add(new MummifiedHand());
            // add(new MutagenicStrength());
            // add(new Necronomicon());
            // add(new NeowsLament());
            // add(new NilrysCodex());
            // add(new NlothsGift());
            // add(new NlothsMask());
            // add(new Nunchaku());
            // add(new OddlySmoothStone());
            // add(new OddMushroom());
            // add(new OldCoin());
            // add(new Omamori());
            // add(new OrangePellets());
            // add(new Orichalcum());
            // add(new OrnamentalFan());
            // add(new Orrery());
            // add(new PandorasBox());
            // add(new Pantograph());
            // add(new PeacePipe());
            // add(new Pear());
            // add(new PenNib());
            // add(new PhilosopherStone());
            // add(new Pocketwatch());
            // add(new PotionBelt());
            // add(new PrayerWheel());
            // add(new PreservedInsect());
            // add(new PrismaticShard());
            // add(new QuestionCard());
            // add(new RedMask());
            // add(new RegalPillow());
            // add(new RunicDome());
            // add(new RunicPyramid());
            // add(new SacredBark());
            // add(new Shovel());
            // add(new Shuriken());
            // add(new SingingBowl());
            // add(new SlaversCollar());
            // add(new Sling());
            // add(new SmilingMask());
            // add(new SneckoEye());
            // add(new Sozu());
            // add(new SpiritPoop());
            // add(new SsserpentHead());
            // add(new StoneCalendar());
            // add(new StrangeSpoon());
            // add(new Strawberry());
            // add(new StrikeDummy());
            // add(new Sundial());
            // add(new ThreadAndNeedle());
            // add(new TinyChest());
            // add(new TinyHouse());
            // add(new Toolbox());
            // add(new Torii());
            // add(new ToxicEgg2());
            // add(new ToyOrnithopter());
            // add(new TungstenRod());
            // add(new Turnip());
            // add(new UnceasingTop());
            // add(new Vajra());
            // add(new VelvetChoker());
            // add(new Waffle());
            // add(new WarPaint());
            // add(new WarpedTongs());
            // add(new Whetstone());
            // add(new WhiteBeast());
            // add(new WingBoots());
            // addGreen(new HoveringKite());
            // addGreen(new NinjaScroll());
            // addGreen(new PaperCrane());
            // addGreen(new RingOfTheSerpent());
            // addGreen(new SnakeRing());
            // addGreen(new SneckoSkull());
            // addGreen(new TheSpecimen());
            // addGreen(new Tingsha());
            // addGreen(new ToughBandages());
            // addGreen(new TwistedFunnel());
            // addGreen(new WristBlade());
            
            addRed(new RoundBattery());
            addRed(new UnstableBattery());
            addRed(new ExtremelyUnstableBattery());
            addRed(new BaseMagazine());

            // addRed(new BlackBlood());
            // addRed(new Brimstone());
            // addRed(new BurningBlood());
            // addRed(new ChampionsBelt());
            // addRed(new CharonsAshes());
            // addRed(new MagicFlower());
            // addRed(new MarkOfPain());
            // addRed(new PaperFrog());
            // addRed(new RedSkull());
            // addRed(new RunicCube());
            // addRed(new SelfFormingClay());
            // addBlue(new CrackedCore());
            // addBlue(new DataDisk());
            // addBlue(new EmotionChip());
            // addBlue(new FrozenCore());
            // addBlue(new GoldPlatedCables());
            // addBlue(new Inserter());
            // addBlue(new NuclearBattery());
            // addBlue(new RunicCapacitor());
            // addBlue(new SymbioticVirus());
            // addPurple(new CloakClasp());
            // addPurple(new Damaru());
            // addPurple(new GoldenEye());
            // addPurple(new HolyWater());
            // addPurple(new Melange());
            // addPurple(new PureWater());
            // addPurple(new VioletLotus());
            // addPurple(new TeardropLocket());
            // addPurple(new Duality());
            if (Settings.isBeta)
                log("Relic load time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
            sortLists();
        }

        public static void resetForReload()
        {
            totalRelicCount = 0;
            seenRelics = 0;
            sharedRelics.Clear();
            redRelics.Clear();
            greenRelics.Clear();
            blueRelics.Clear();
            purpleRelics.Clear();
            starterList.Clear();
            commonList.Clear();
            uncommonList.Clear();
            rareList.Clear();
            bossList.Clear();
            specialList.Clear();
            shopList.Clear();
            redList.Clear();
            greenList.Clear();
            blueList.Clear();
            whiteList.Clear();
        }

        static void sortLists()
        {
            starterList.Sort();
            commonList.Sort();
            uncommonList.Sort();
            rareList.Sort();
            bossList.Sort();
            specialList.Sort();
            shopList.Sort();
            if (Settings.isDev)
            {
                // log(starterList);
                // log(commonList);
                // log(uncommonList);
                // log(rareList);
                // log(bossList);
            }
        }

        static void printRelicsMissingLargeArt()
        {
            var common = 0;
            var uncommon = 0;
            var rare = 0;
            var boss = 0;
            var shop = 0;
            var other = 0;
            log("[ART] START DISPLAYING RELICS WITH MISSING HIGH RES ART");
            foreach (var (id, relic) in sharedRelics)
            {
                // if (ImageMaster.loadImage("images/largeRelics/" + relic.imgUrl) == null)
                log(relic.name);
            }
        }

        static void printRelicCount()
        {
            int common = 0, uncommon = 0, rare = 0, boss = 0, shop = 0, other = 0;
            foreach (var (id, relic) in sharedRelics)
            {
                switch (relic.tier)
                {
                    case RelicTier.COMMON:
                        common++;
                        continue;
                    case RelicTier.UNCOMMON:
                        uncommon++;
                        continue;
                    case RelicTier.RARE:
                        rare++;
                        continue;
                    case RelicTier.BOSS:
                        boss++;
                        continue;
                    case RelicTier.SHOP:
                        shop++;
                        continue;
                }

                other++;
            }

            if (Settings.isDev)
            {
                log("RELIC COUNTS");
                log("Common: " + common);
                log("Uncommon: " + uncommon);
                log("Rare: " + rare);
                log("Boss: " + boss);
                log("Shop: " + shop);
                log("Other: " + other);
                log("Red: " + redRelics.Count);
                log("Green: " + greenRelics.Count);
                log("Blue: " + blueRelics.Count);
                log("Purple: " + purpleRelics.Count);
            }
        }

        public static void add(ARelic relic)
        {
            if (UnlockTracker.isRelicSeen(relic.relicId))
                seenRelics++;
            relic.isSeen = UnlockTracker.isRelicSeen(relic.relicId);
            sharedRelics.Add(relic.relicId, relic);
            addToTierList(relic);
            totalRelicCount++;
        }

        public static void addRed(ARelic relic)
        {
            if (UnlockTracker.isRelicSeen(relic.relicId))
                seenRelics++;
            relic.isSeen = UnlockTracker.isRelicSeen(relic.relicId);
            redRelics.Add(relic.relicId, relic);
            addToTierList(relic);
            redList.Add(relic);
            totalRelicCount++;
        }

        public static void addGreen(ARelic relic)
        {
            if (UnlockTracker.isRelicSeen(relic.relicId))
                seenRelics++;
            relic.isSeen = UnlockTracker.isRelicSeen(relic.relicId);
            greenRelics.Add(relic.relicId, relic);
            addToTierList(relic);
            greenList.Add(relic);
            totalRelicCount++;
        }

        public static void addBlue(ARelic relic)
        {
            if (UnlockTracker.isRelicSeen(relic.relicId))
                seenRelics++;
            relic.isSeen = UnlockTracker.isRelicSeen(relic.relicId);
            blueRelics.Add(relic.relicId, relic);
            addToTierList(relic);
            blueList.Add(relic);
            totalRelicCount++;
        }

        public static void addPurple(ARelic relic)
        {
            if (UnlockTracker.isRelicSeen(relic.relicId))
                seenRelics++;
            relic.isSeen = UnlockTracker.isRelicSeen(relic.relicId);
            purpleRelics.Add(relic.relicId, relic);
            addToTierList(relic);
            whiteList.Add(relic);
            totalRelicCount++;
        }

        public static void addToTierList(ARelic relic)
        {
            switch (relic.tier)
            {
                case RelicTier.STARTER:
                    starterList.Add(relic);
                    return;
                case RelicTier.COMMON:
                    commonList.Add(relic);
                    return;
                case RelicTier.UNCOMMON:
                    uncommonList.Add(relic);
                    return;
                case RelicTier.RARE:
                    rareList.Add(relic);
                    return;
                case RelicTier.SHOP:
                    shopList.Add(relic);
                    return;
                case RelicTier.SPECIAL:
                    specialList.Add(relic);
                    return;
                case RelicTier.BOSS:
                    bossList.Add(relic);
                    return;
                case RelicTier.DEPRECATED:
                    log(relic.relicId + " is deprecated.");
                    return;
            }

            log(relic.relicId + " is undefined tier.");
        }

        public static ARelic getRelic(string key)
        {
            ARelic relic;
            if (sharedRelics.TryGetValue(key, out relic))
                return relic;
            if (redRelics.TryGetValue(key, out relic))
                return relic;
            if (greenRelics.TryGetValue(key, out relic))
                return relic;
            if (blueRelics.TryGetValue(key, out relic))
                return relic;
            if (purpleRelics.TryGetValue(key, out relic))
                return relic;

            // return new Circlet();
            return null;
        }

        public static bool isARelic(string key)
        {
            return (sharedRelics.ContainsKey(key) || redRelics.ContainsKey(key) || greenRelics.ContainsKey(key) || blueRelics.ContainsKey(key) || purpleRelics.ContainsKey(key));
        }

        public static void populateRelicPool(ref List<string> pool, RelicTier tier, APlayer.PlayerClass c)
        {
            foreach (var (id, relic) in sharedRelics)
            {
                if (relic.tier == tier && (!UnlockTracker.isRelicLocked(id) || Settings.treatEverythingAsUnlocked()))
                    pool.Add(id);
            }

            switch (c)
            {
                case APlayer.PlayerClass.IRONCLAD:
                    foreach (var (id, relic) in redRelics)
                    {
                        if (relic.tier == tier && (!UnlockTracker.isRelicLocked(id) || Settings.treatEverythingAsUnlocked()))
                            pool.Add(id);
                    }

                    break;
                case APlayer.PlayerClass.THE_SILENT:
                    foreach (var (id, relic) in greenRelics)
                    {
                        if (relic.tier == tier && (!UnlockTracker.isRelicLocked(id) || Settings.treatEverythingAsUnlocked()))
                            pool.Add(id);
                    }

                    break;
                case APlayer.PlayerClass.DEFECT:
                    foreach (var (id, relic) in blueRelics)
                    {
                        if (relic.tier == tier && (!UnlockTracker.isRelicLocked(id) || Settings.treatEverythingAsUnlocked()))
                            pool.Add(id);
                    }

                    break;
                case APlayer.PlayerClass.WATCHER:
                    foreach (var (id, relic) in purpleRelics)
                    {
                        if (relic.tier == tier && (!UnlockTracker.isRelicLocked(id) || Settings.treatEverythingAsUnlocked()))
                            pool.Add(id);
                    }

                    break;
            }
        }

        public static void addSharedRelics(List<ARelic> relicPool)
        {
            if (Settings.isDev)
                log("[RELIC] Adding " + sharedRelics.Count + " shared relics...");
            foreach (var (id, relic) in sharedRelics)
                relicPool.Add(relic);
        }

        public static void addClassSpecificRelics(List<ARelic> relicPool)
        {
            switch (player.chosenClass)
            {
                case APlayer.PlayerClass.IRONCLAD:
                    if (Settings.isDev)
                        log("[RELIC] Adding " + redRelics.Count + " red relics...");
                    foreach (var (id, relic) in redRelics)
                        relicPool.Add(relic);
                    break;
                case APlayer.PlayerClass.THE_SILENT:
                    if (Settings.isDev)
                        log("[RELIC] Adding " + greenRelics.Count + " green relics...");
                    foreach (var (id, relic) in greenRelics)
                        relicPool.Add(relic);
                    break;
                case APlayer.PlayerClass.DEFECT:
                    if (Settings.isDev)
                        log("[RELIC] Adding " + blueRelics.Count + " blue relics...");
                    foreach (var (id, relic) in blueRelics)
                        relicPool.Add(relic);
                    break;
                case APlayer.PlayerClass.WATCHER:
                    if (Settings.isDev)
                        log("[RELIC] Adding " + purpleRelics.Count + " purple relics...");
                    foreach (var (id, relic) in purpleRelics)
                        relicPool.Add(relic);
                    break;
            }
        }

        /*
        public static void uploadRelicData()
        {
            List<string> data = new();
            foreach (var (id, relic) in sharedRelics)
                data.Add(relic.gameDataUploadData("All"));
            foreach (var (id, relic) in redRelics)
                data.Add(relic.gameDataUploadData("Red"));
            foreach (var (id, relic) in greenRelics)
                data.Add(relic.gameDataUploadData("Green"));
            foreach (var (id, relic) in blueRelics)
                data.Add(relic.gameDataUploadData("Blue"));
            foreach (var (id, relic) in purpleRelics)
                data.Add(relic.gameDataUploadData("Purple"));
            BotDataUploader.uploadDataAsync(BotDataUploader.GameDataType.RELIC_DATA, ARelic.gameDataUploadHeader(), data);
        }
        */

        public static List<ARelic> sortByName(List<ARelic> group, bool ascending)
        {
            List<ARelic> tmp = new();
            foreach (ARelic r in group)
            {
                int addIndex = 0;
                foreach (ARelic r2 in tmp)
                {
                    if (!ascending ? (string.Compare(r.name, r2.name, StringComparison.Ordinal) < 0) : (string.Compare(r.name, r2.name, StringComparison.Ordinal) > 0))
                        break;
                    addIndex++;
                }

                tmp.Insert(addIndex, r);
            }

            return tmp;
        }

        public static List<ARelic> sortByStatus(List<ARelic> group, bool ascending)
        {
            List<ARelic> tmp = new();
            foreach (ARelic r in group)
            {
                int addIndex = 0;
                foreach (ARelic r2 in tmp)
                {
                    if (!ascending)
                    {
                        string a;
                        string b;
                        if (UnlockTracker.isRelicLocked(r.relicId))
                        {
                            a = "LOCKED";
                        }
                        else if (UnlockTracker.isRelicSeen(r.relicId))
                        {
                            a = "UNSEEN";
                        }
                        else
                        {
                            a = "SEEN";
                        }

                        if (UnlockTracker.isRelicLocked(r2.relicId))
                        {
                            b = "LOCKED";
                        }
                        else if (UnlockTracker.isRelicSeen(r2.relicId))
                        {
                            b = "UNSEEN";
                        }
                        else
                        {
                            b = "SEEN";
                        }

                        if (string.Compare(a, b, StringComparison.Ordinal) > 0)
                            break;
                    }
                    else
                    {
                        string a;
                        string b;
                        if (UnlockTracker.isRelicLocked(r.relicId))
                        {
                            a = "LOCKED";
                        }
                        else if (UnlockTracker.isRelicSeen(r.relicId))
                        {
                            a = "UNSEEN";
                        }
                        else
                        {
                            a = "SEEN";
                        }

                        if (UnlockTracker.isRelicLocked(r2.relicId))
                        {
                            b = "LOCKED";
                        }
                        else if (UnlockTracker.isRelicSeen(r2.relicId))
                        {
                            b = "UNSEEN";
                        }
                        else
                        {
                            b = "SEEN";
                        }

                        if (string.Compare(a, b, StringComparison.Ordinal) < 0)
                            break;
                    }

                    addIndex++;
                }

                tmp.Insert(addIndex, r);
            }

            return tmp;
        }

        public static void unlockAndSeeAllRelics()
        {
            foreach (string s in UnlockTracker.lockedRelics)
                UnlockTracker.hardUnlockOverride(s);

            foreach (var (id, relic) in sharedRelics)
                UnlockTracker.markRelicAsSeen(id);

            foreach (var (id, relic) in redRelics)
                UnlockTracker.markRelicAsSeen(id);

            foreach (var (id, relic) in greenRelics)
                UnlockTracker.markRelicAsSeen(id);

            foreach (var (id, relic) in blueRelics)
                UnlockTracker.markRelicAsSeen(id);

            foreach (var (id, relic) in purpleRelics)
                UnlockTracker.markRelicAsSeen(id);
        }
    }
}