using System;
using System.Collections.Generic;

namespace MarbleHero
{
    public enum CardType
    {
        Weapon = 0,
        Passive = 10,
        Relic = 20,
        Power = 30,
        Status = 40,
        Curse = 50,
    }
    
    public enum CardRarity
    {
        Basic, //基础
        Special, //特殊
        Common, //普通
        Uncommon, //罕见
        Rare, //稀有
        Curse
    }

    public enum CardColor
    {
        Red,
        Green,
        Blue,
        Purple,
        Colorless,
        Curse
    }

    public class CardLibrary
    {
        public static int totalCardCount;
        public static Dictionary<string, ACard> cards = new();
        static Dictionary<string, ACard> curses = new();

        public static int redCards;
        public static int greenCards;
        public static int blueCards;
        public static int purpleCards;
        public static int colorlessCards;
        public static int curseCards;
        public static int seenRedCards;
        public static int seenGreenCards;
        public static int seenBlueCards;
        public static int seenPurpleCards;
        public static int seenColorlessCards;
        public static int seenCurseCards;

        public enum LibraryType
        {
            RED,
            GREEN,
            BLUE,
            PURPLE,
            CURSE,
            COLORLESS
        }

        public static void initialize()
        {
            long startTime = TimeUtility.getNowTimeStampMS();
            addRedCards();
            addGreenCards();
            addBlueCards();
            addPurpleCards();
            addColorlessCards();
            addCurseCards();

            log("Card load time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms with " + cards.Count + " cards");
            if (Settings.isDev)
            {
                log("[INFO] Red Cards: \t" + redCards);
                log("[INFO] Green Cards: \t" + greenCards);
                log("[INFO] Blue Cards: \t" + blueCards);
                log("[INFO] Purple Cards: \t" + purpleCards);
                log("[INFO] Colorless Cards: \t" + colorlessCards);
                log("[INFO] Curse Cards: \t" + curseCards);
                log("[INFO] Total Cards: \t" + (redCards + greenCards + blueCards + purpleCards + colorlessCards + curseCards));
            }
        }

        public static void resetForReload()
        {
            cards.Clear();
            curses.Clear();
            totalCardCount = 0;
            redCards = 0;
            greenCards = 0;
            blueCards = 0;
            purpleCards = 0;
            colorlessCards = 0;
            curseCards = 0;
            seenRedCards = 0;
            seenGreenCards = 0;
            seenBlueCards = 0;
            seenPurpleCards = 0;
            seenColorlessCards = 0;
            seenCurseCards = 0;
        }

        static void addRedCards()
        {
            // var datas = CardData.GetAll()
            //     .Where(d => d.CardColor == CardColor.Red)
            //     .ToArray();
            //
            // foreach (var data in datas)
            //     add(new ACard(data));

            // add(new Anger());
            // add(new Armaments());
            // add(new Barricade());
            // add(new Bash());
            // add(new BattleTrance());
            // add(new Berserk());
            // add(new BloodForBlood());
            // add(new Bloodletting());
            // add(new Bludgeon());
            // add(new BodySlam());
            // add(new Brutality());
            // add(new BurningPact());
            // add(new Carnage());
            // add(new Clash());
            // add(new Cleave());
            // add(new Clothesline());
            // add(new Combust());
            // add(new Corruption());
            // add(new DarkEmbrace());
            // add(new Defend_Red());
            // add(new DemonForm());
            // add(new Disarm());
            // add(new DoubleTap());
            // add(new Dropkick());
            // add(new DualWield());
            // add(new Entrench());
            // add(new Evolve());
            // add(new Exhume());
            // add(new Feed());
            // add(new FeelNoPain());
            // add(new FiendFire());
            // add(new FireBreathing());
            // add(new FlameBarrier());
            // add(new Flex());
            // add(new GhostlyArmor());
            // add(new Havoc());
            // add(new Headbutt());
            // add(new HeavyBlade());
            // add(new Hemokinesis());
            // add(new Immolate());
            // add(new Impervious());
            // add(new InfernalBlade());
            // add(new Inflame());
            // add(new Intimidate());
            // add(new IronWave());
            // add(new Juggernaut());
            // add(new LimitBreak());
            // add(new Metallicize());
            // add(new Offering());
            // add(new PerfectedStrike());
            // add(new PommelStrike());
            // add(new PowerThrough());
            // add(new Pummel());
            // add(new Rage());
            // add(new Rampage());
            // add(new Reaper());
            // add(new RecklessCharge());
            // add(new Rupture());
            // add(new SearingBlow());
            // add(new SecondWind());
            // add(new SeeingRed());
            // add(new Sentinel());
            // add(new SeverSoul());
            // add(new Shockwave());
            // add(new ShrugItOff());
            // add(new SpotWeakness());
            // add(new Strike_Red());
            // add(new SwordBoomerang());
            // add(new ThunderClap());
            // add(new TrueGrit());
            // add(new TwinStrike());
            // add(new Uppercut());
            // add(new Warcry());
            // add(new Whirlwind());
            // add(new WildStrike());
        }

        static void addGreenCards()
        {
            // add(new Accuracy());
            // add(new Acrobatics());
            // add(new Adrenaline());
            // add(new AfterImage());
            // add(new Alchemize());
            // add(new AllOutAttack());
            // add(new AThousandCuts());
            // add(new Backflip());
            // add(new Backstab());
            // add(new Bane());
            // add(new BladeDance());
            // add(new Blur());
            // add(new BouncingFlask());
            // add(new BulletTime());
            // add(new Burst());
            // add(new CalculatedGamble());
            // add(new Caltrops());
            // add(new Catalyst());
            // add(new Choke());
            // add(new CloakAndDagger());
            // add(new Concentrate());
            // add(new CorpseExplosion());
            // add(new CripplingPoison());
            // add(new DaggerSpray());
            // add(new DaggerThrow());
            // add(new Dash());
            // add(new DeadlyPoison());
            // add(new Defend_Green());
            // add(new Deflect());
            // add(new DieDieDie());
            // add(new Distraction());
            // add(new DodgeAndRoll());
            // add(new Doppelganger());
            // add(new EndlessAgony());
            // add(new Envenom());
            // add(new EscapePlan());
            // add(new Eviscerate());
            // add(new Expertise());
            // add(new Finisher());
            // add(new Flechettes());
            // add(new FlyingKnee());
            // add(new Footwork());
            // add(new GlassKnife());
            // add(new GrandFinale());
            // add(new HeelHook());
            // add(new InfiniteBlades());
            // add(new LegSweep());
            // add(new Malaise());
            // add(new MasterfulStab());
            // add(new Neutralize());
            // add(new Nightmare());
            // add(new NoxiousFumes());
            // add(new Outmaneuver());
            // add(new PhantasmalKiller());
            // add(new PiercingWail());
            // add(new PoisonedStab());
            // add(new Predator());
            // add(new Prepared());
            // add(new QuickSlash());
            // add(new Reflex());
            // add(new RiddleWithHoles());
            // add(new Setup());
            // add(new Skewer());
            // add(new Slice());
            // add(new StormOfSteel());
            // add(new Strike_Green());
            // add(new SuckerPunch());
            // add(new Survivor());
            // add(new Tactician());
            // add(new Terror());
            // add(new ToolsOfTheTrade());
            // add(new SneakyStrike());
            // add(new Unload());
            // add(new WellLaidPlans());
            // add(new WraithForm());
        }

        static void addBlueCards()
        {
            // add(new Aggregate());
            // add(new AllForOne());
            // add(new Amplify());
            // add(new AutoShields());
            // add(new BallLightning());
            // add(new Barrage());
            // add(new BeamCell());
            // add(new BiasedCognition());
            // add(new Blizzard());
            // add(new BootSequence());
            // add(new Buffer());
            // add(new Capacitor());
            // add(new Chaos());
            // add(new Chill());
            // add(new Claw());
            // add(new ColdSnap());
            // add(new CompileDriver());
            // add(new ConserveBattery());
            // add(new Consume());
            // add(new Coolheaded());
            // add(new CoreSurge());
            // add(new CreativeAI());
            // add(new Darkness());
            // add(new Defend_Blue());
            // add(new Defragment());
            // add(new DoomAndGloom());
            // add(new DoubleEnergy());
            // add(new Dualcast());
            // add(new EchoForm());
            // add(new Electrodynamics());
            // add(new Fission());
            // add(new ForceField());
            // add(new FTL());
            // add(new Fusion());
            // add(new GeneticAlgorithm());
            // add(new Glacier());
            // add(new GoForTheEyes());
            // add(new Heatsinks());
            // add(new HelloWorld());
            // add(new Hologram());
            // add(new Hyperbeam());
            // add(new Leap());
            // add(new LockOn());
            // add(new Loop());
            // add(new MachineLearning());
            // add(new Melter());
            // add(new MeteorStrike());
            // add(new MultiCast());
            // add(new Overclock());
            // add(new Rainbow());
            // add(new Reboot());
            // add(new Rebound());
            // add(new Recursion());
            // add(new Recycle());
            // add(new ReinforcedBody());
            // add(new Reprogram());
            // add(new RipAndTear());
            // add(new Scrape());
            // add(new Seek());
            // add(new SelfRepair());
            // add(new Skim());
            // add(new Stack());
            // add(new StaticDischarge());
            // add(new SteamBarrier());
            // add(new Storm());
            // add(new Streamline());
            // add(new Strike_Blue());
            // add(new Sunder());
            // add(new SweepingBeam());
            // add(new Tempest());
            // add(new ThunderStrike());
            // add(new Turbo());
            // add(new Equilibrium());
            // add(new WhiteNoise());
            // add(new Zap());
        }

        static void addPurpleCards()
        {
            // add(new Alpha());
            // add(new BattleHymn());
            // add(new Blasphemy());
            // add(new BowlingBash());
            // add(new Brilliance());
            // add(new CarveReality());
            // add(new Collect());
            // add(new Conclude());
            // add(new ConjureBlade());
            // add(new Consecrate());
            // add(new Crescendo());
            // add(new CrushJoints());
            // add(new CutThroughFate());
            // add(new DeceiveReality());
            // add(new Defend_Watcher());
            // add(new DeusExMachina());
            // add(new DevaForm());
            // add(new Devotion());
            // add(new EmptyBody());
            // add(new EmptyFist());
            // add(new EmptyMind());
            // add(new Eruption());
            // add(new Establishment());
            // add(new Evaluate());
            // add(new Fasting());
            // add(new FearNoEvil());
            // add(new FlurryOfBlows());
            // add(new FlyingSleeves());
            // add(new FollowUp());
            // add(new ForeignInfluence());
            // add(new Foresight());
            // add(new Halt());
            // add(new Indignation());
            // add(new InnerPeace());
            // add(new Judgement());
            // add(new JustLucky());
            // add(new LessonLearned());
            // add(new LikeWater());
            // add(new MasterReality());
            // add(new Meditate());
            // add(new MentalFortress());
            // add(new Nirvana());
            // add(new Omniscience());
            // add(new Perseverance());
            // add(new Pray());
            // add(new PressurePoints());
            // add(new Prostrate());
            // add(new Protect());
            // add(new Ragnarok());
            // add(new ReachHeaven());
            // add(new Rushdown());
            // add(new Sanctity());
            // add(new SandsOfTime());
            // add(new SashWhip());
            // add(new Scrawl());
            // add(new SignatureMove());
            // add(new SimmeringFury());
            // add(new SpiritShield());
            // add(new Strike_Purple());
            // add(new Study());
            // add(new Swivel());
            // add(new TalkToTheHand());
            // add(new Tantrum());
            // add(new ThirdEye());
            // add(new Tranquility());
            // add(new Vault());
            // add(new Vigilance());
            // add(new Wallop());
            // add(new WaveOfTheHand());
            // add(new Weave());
            // add(new WheelKick());
            // add(new WindmillStrike());
            // add(new Wish());
            // add(new Worship());
            // add(new WreathOfFlame());
        }

        /*
        static void printMissingPortraitInfo()
        {
            foreach (var (id, card) in cards)
            {
                if (card.jokePortrait == null)
                    logger.Debug(card.name + ";" + card.color + ";" + card.type);
            }

            foreach (var (id, card) in cards)
            {
                if (ImageMaster.loadImage("images/1024PortraitsBeta/" + card.assetUrl + ".png") == null)
                    logger.Debug("[INFO] " + card.name + " missing LARGE beta portrait.");
            }
        }
        */

        /*
        static void printBlueCards(CardColor color)
        {
            foreach (var (id, card) in cards)
            {
                if (card.color == color)
                {
                    logger.Debug(card.originalName + "; " + card.type + "; " + card.rarity + "; " + card.cost + "; " + card.rawDescription);
                }
            }
        }
        */

        static void addColorlessCards()
        {
            // var datas = CardData.GetAll()
            //     .Where(d => d.CardColor == CardColor.Colorless)
            //     .ToArray();
            //
            // foreach (var data in datas)
            //     add(new ACard(data));

            // add(new Apotheosis());
            // add(new BandageUp());
            // add(new Blind());
            // add(new Chrysalis());
            // add(new DarkShackles());
            // add(new DeepBreath());
            // add(new Discovery());
            // add(new DramaticEntrance());
            // add(new Enlightenment());
            // add(new Finesse());
            // add(new FlashOfSteel());
            // add(new Forethought());
            // add(new GoodInstincts());
            // add(new HandOfGreed());
            // add(new Impatience());
            // add(new JackOfAllTrades());
            // add(new Madness());
            // add(new Magnetism());
            // add(new MasterOfStrategy());
            // add(new Mayhem());
            // add(new Metamorphosis());
            // add(new MindBlast());
            // add(new Panacea());
            // add(new Panache());
            // add(new PanicButton());
            // add(new Purity());
            // add(new SadisticNature());
            // add(new SecretTechnique());
            // add(new SecretWeapon());
            // add(new SwiftStrike());
            // add(new TheBomb());
            // add(new ThinkingAhead());
            // add(new Transmutation());
            // add(new Trip());
            // add(new Violence());
            // add(new Burn());
            // add(new Dazed());
            // add(new Slimed());
            // add(new VoidCard());
            // add(new Wound());
            // add(new Apparition());
            // add(new Beta());
            // add(new Bite());
            // add(new JAX());
            // add(new Insight());
            // add(new Miracle());
            // add(new Omega());
            // add(new RitualDagger());
            // add(new Safety());
            // add(new Shiv());
            // add(new Smite());
            // add(new ThroughViolence());
            // add(new BecomeAlmighty());
            // add(new FameAndFortune());
            // add(new LiveForever());
            // add(new Expunger());
        }

        static void addCurseCards()
        {
            // add(new AscendersBane());
            // add(new CurseOfTheBell());// var datas = CardData.GetAll()
            //     .Where(d => d.CardColor == CardColor.Curse)
            //     .ToArray();
            //
            // foreach (var data in datas)
            //     add(new ACard(data));
            // add(new Clumsy());
            // add(new Decay());
            // add(new Doubt());
            // add(new Injury());
            // add(new Necronomicurse());
            // add(new Normality());
            // add(new Pain());
            // add(new Parasite());
            // add(new Pride());
            // add(new Regret());
            // add(new Shame());
            // add(new Writhe());
        }

        static void removeNonFinalizedCards()
        {
            List<string> toRemove = new();
            foreach (var (id, card) in cards)
            {
                if (card.assetUrl == null)
                    toRemove.Add(id);
            }

            foreach (string s in toRemove)
            {
                log("Removing Card " + s + " for trailer build.");
                cards.Remove(s);
            }

            toRemove.Clear();
            foreach (var (id, card) in curses)
            {
                if (card.assetUrl == null)
                    toRemove.Add(id);
            }

            foreach (string s in toRemove)
            {
                log("Removing Curse " + s + " for trailer build.");
                curses.Remove(s);
            }
        }

        public static void unlockAndSeeAllCards()
        {
            foreach (string s in UnlockTracker.lockedCards)
                UnlockTracker.hardUnlockOverride(s);

            foreach (var (id, card) in cards)
            {
                if (card.rarity != CardRarity.Basic && !UnlockTracker.isCardSeen(id))
                    UnlockTracker.markCardAsSeen(id);
            }

            foreach (var (id, card) in curses)
            {
                if (!UnlockTracker.isCardSeen(id))
                    UnlockTracker.markCardAsSeen(id);
            }
        }

        public static void add(ACard card)
        {
            switch (card.color)
            {
                case CardColor.Red:
                    redCards++;
                    if (UnlockTracker.isCardSeen(card.cardID))
                        seenRedCards++;
                    break;
                case CardColor.Green:
                    greenCards++;
                    if (UnlockTracker.isCardSeen(card.cardID))
                        seenGreenCards++;
                    break;
                case CardColor.Purple:
                    purpleCards++;
                    if (UnlockTracker.isCardSeen(card.cardID))
                        seenPurpleCards++;
                    break;
                case CardColor.Blue:
                    blueCards++;
                    if (UnlockTracker.isCardSeen(card.cardID))
                        seenBlueCards++;
                    break;
                case CardColor.Colorless:
                    colorlessCards++;
                    if (UnlockTracker.isCardSeen(card.cardID))
                        seenColorlessCards++;
                    break;
                case CardColor.Curse:
                    curseCards++;
                    if (UnlockTracker.isCardSeen(card.cardID))
                        seenCurseCards++;
                    curses.Add(card.cardID, card);
                    break;
            }

            if (!UnlockTracker.isCardSeen(card.cardID))
                card.isSeen = false;

            cards.Add(card.cardID, card);
            totalCardCount++;
        }

        public static ACard getCopy(string key, int misc)
        {
            var tKey = key;
            if (!getCard(key, out _))
                tKey = "Madness";

            if (!getCard(tKey, out var card))
                return null;

            var retVal = card.makeCopy();
            retVal.misc = misc;
            if (misc != 0)
            {
                switch (retVal.cardID)
                {
                    case "Genetic Algorithm":
                        // retVal.block = misc;
                        // retVal.baseBlock = misc;
                        // retVal.initializeDescription();
                        break;
                    case "RitualDagger":
                        // retVal.damage = misc;
                        // retVal.baseDamage = misc;
                        // retVal.initializeDescription();
                        break;
                }
            }

            return retVal;
        }

        public static bool getCopy(string key, out ACard card)
        {
            if (getCard(key, out var c))
            {
                card = c.makeCopy();
                return true;
            }

            card = null;
            return false;
        }

        public static bool getCard(APlayer.PlayerClass plyrClass, string key, out ACard card)
        {
            if (cards.TryGetValue(key, out card))
                return true;

            logWarning($"CardLibrary中不存在 Id={key}的卡牌");
            return false;
        }

        public static bool getCard(string key, out ACard card)
        {
            if (cards.TryGetValue(key, out card))
                return true;

            logWarning($"CardLibrary中不存在 Id={key}的卡牌");
            return false;
        }

        public static string getCardNameFromMetricID(string metricID)
        {
            string[] components = metricID.Split("\\+");
            string baseId = components[0];
            cards.TryGetValue(baseId, out var card);
            if (card == null)
                return metricID;

            try
            {
                if (components.Length > 1)
                {
                    card = card.makeCopy();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return card.name;
        }

        public static bool isACard(string metricID)
        {
            string[] components = metricID.Split("\\+");
            string baseId = components[0];
            cards.TryGetValue(baseId, out var card);
            return card != null;
        }

        public static ACard getCurse()
        {
            List<string> tmp = new();
            foreach (var (id, card) in curses)
            {
                switch (card.cardID)
                {
                    case "Necronomicurse":
                    case "AscendersBane":
                    case "CurseOfTheBell":
                    case "Pride":
                        continue;
                }

                tmp.Add(id);
            }

            var index = ADungeon.cardRng.random(0, tmp.Count - 1);
            return cards[tmp[index]];
        }

        public static ACard getCurse(ACard prohibitedCard, Rand rng)
        {
            List<string> tmp = new();
            foreach (var (id, card) in curses)
            {
                if (card.cardID == prohibitedCard.cardID)
                    continue;

                switch (card.cardID)
                {
                    case "Necronomicurse":
                    case "AscendersBane":
                    case "CurseOfTheBell":
                    case "Pride":
                        continue;
                }

                tmp.Add(id);
            }

            var index = rng.random(0, tmp.Count - 1);
            return cards[tmp[index]];
        }

        public static ACard getCurse(ACard prohibitedCard)
        {
            return getCurse(prohibitedCard, new Rand());
        }

        /*
        public static void uploadCardData()
        {
            List<string> data = new();
            foreach (var (id, card) in cards)
            {
                data.Add(card.gameDataUploadData());
                ACard c2 = card.makeCopy();
                if (c2.canUpgrade())
                {
                    c2.upgrade();
                    data.Add(c2.gameDataUploadData());
                }
            }

            BotDataUploader.uploadDataAsync(BotDataUploader.GameDataType.CARD_DATA, ACard.gameDataUploadHeader(), data);
        }
        */

        public static List<ACard> getAllCards()
        {
            List<ACard> retVal = new();
            foreach (var (id, card) in cards)
                retVal.Add(card);
            return retVal;
        }

        public static ACard getAnyColorCard(CardType type, CardRarity rarity)
        {
            var anyCard = new TempCards();
            foreach (var (id, card) in cards)
            {
                // if (card.rarity == rarity && !card.hasTag(CardTags.HEALING) && card.type != CardType.Curse && card.type != CardType.Status && card.type == type && (!UnlockTracker.isCardLocked(id) || Settings.treatEverythingAsUnlocked()))
                    anyCard.addToBottom(card);
            }

            anyCard.shuffle(ADungeon.cardRandomRng);
            return anyCard.getRandomCard(true, rarity);
        }

        public static ACard getAnyColorCard(CardRarity rarity)
        {
            var anyCard = new TempCards();
            foreach (var (id, card) in cards)
            {
                if (card.rarity == rarity && card.type != CardType.Curse && card.type != CardType.Status && (!UnlockTracker.isCardLocked(id) || Settings.treatEverythingAsUnlocked()))
                    anyCard.addToBottom(card);
            }

            anyCard.shuffle(ADungeon.cardRng);
            return anyCard.getRandomCard(true, rarity).makeCopy();
        }

        public static CardGroup getEachRare(APlayer p)
        {
            var everyRareCard = new TempCards();
            foreach (var (id, card) in cards)
            {
                // if (card.color == p.getCardColor() && card.rarity == CardRarity.Rare)
                    everyRareCard.addToBottom(card.makeCopy());
            }

            return everyRareCard;
        }

        public static List<ACard> getCardList(LibraryType type)
        {
            List<ACard> retVal = new();
            switch (type)
            {
                case LibraryType.COLORLESS:
                    foreach (var (id, card) in cards)
                    {
                        if (card.color == CardColor.Colorless)
                            retVal.Add(card);
                    }

                    break;
                case LibraryType.CURSE:
                    foreach (var (id, card) in cards)
                    {
                        if (card.color == CardColor.Curse)
                            retVal.Add(card);
                    }

                    break;
                case LibraryType.RED:
                    foreach (var (id, card) in cards)
                    {
                        if (card.color == CardColor.Red)
                            retVal.Add(card);
                    }

                    break;
                case LibraryType.GREEN:
                    foreach (var (id, card) in cards)
                    {
                        if (card.color == CardColor.Green)
                            retVal.Add(card);
                    }

                    break;
                case LibraryType.BLUE:
                    foreach (var (id, card) in cards)
                    {
                        if (card.color == CardColor.Blue)
                            retVal.Add(card);
                    }

                    break;
                case LibraryType.PURPLE:
                    foreach (var (id, card) in cards)
                    {
                        if (card.color == CardColor.Purple)
                            retVal.Add(card);
                    }

                    break;
            }

            return retVal;
        }

        public static void addCardsIntoPool(List<ACard> tmpPool, CardColor color)
        {
            log("[INFO] Adding " + color + " cards into card pool.");
            foreach (var (id, card) in cards)
            {
                if (card.color == color && card.rarity != CardRarity.Basic && card.type != CardType.Status && (!UnlockTracker.isCardLocked(id) || Settings.treatEverythingAsUnlocked()))
                    tmpPool.Add(card);
            }
        }

        public static void addRedCards(List<ACard> tmpPool)
        {
            log("[INFO] Adding red cards into card pool.");
            foreach (var (id, card) in cards)
            {
                if (card.color == CardColor.Red && card.rarity != CardRarity.Basic && (!UnlockTracker.isCardLocked(id) || Settings.treatEverythingAsUnlocked()))
                    tmpPool.Add(card);
            }
        }

        public static void addGreenCards(List<ACard> tmpPool)
        {
            log("[INFO] Adding green cards into card pool.");
            foreach (var (id, card) in cards)
            {
                if (card.color == CardColor.Green && card.rarity != CardRarity.Basic && (!UnlockTracker.isCardLocked(id) || Settings.treatEverythingAsUnlocked()))
                    tmpPool.Add(card);
            }
        }

        public static void addBlueCards(List<ACard> tmpPool)
        {
            log("[INFO] Adding blue cards into card pool.");
            foreach (var (id, card) in cards)
            {
                if (card.color == CardColor.Blue && card.rarity != CardRarity.Basic && (!UnlockTracker.isCardLocked(id) || Settings.treatEverythingAsUnlocked()))
                    tmpPool.Add(card);
            }
        }

        public static void addPurpleCards(List<ACard> tmpPool)
        {
            log("[INFO] Adding purple cards into card pool.");
            foreach (var (id, card) in cards)
            {
                if (card.color == CardColor.Purple && card.rarity != CardRarity.Basic && (!UnlockTracker.isCardLocked(id) || Settings.treatEverythingAsUnlocked()))
                    tmpPool.Add(card);
            }
        }

        public static void addColorlessCards(List<ACard> tmpPool)
        {
            log("[INFO] Adding colorless cards into card pool.");
            foreach (var (id, card) in cards)
            {
                if (card.color == CardColor.Colorless && card.type != CardType.Status && (!UnlockTracker.isCardLocked(id) || Settings.treatEverythingAsUnlocked()))
                    tmpPool.Add(card);
            }
        }
    }
}