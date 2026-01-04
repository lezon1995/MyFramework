using System;
using System.Collections.Generic;

namespace MarbleHero
{
    public class EventHelper
    {
        static float BASE_ELITE_CHANCE = 0.1F;
        static float BASE_MONSTER_CHANCE = 0.1F;
        static float BASE_SHOP_CHANCE = 0.03F;
        static float BASE_TREASURE_CHANCE = 0.02F;

        static float RAMP_ELITE_CHANCE = 0.1F;
        static float RAMP_MONSTER_CHANCE = 0.1F;
        static float RAMP_SHOP_CHANCE = 0.03F;
        static float RAMP_TREASURE_CHANCE = 0.02F;

        static float RESET_ELITE_CHANCE = 0.0F;
        static float RESET_MONSTER_CHANCE = 0.1F;
        static float RESET_SHOP_CHANCE = 0.03F;
        static float RESET_TREASURE_CHANCE = 0.02F;

        static float ELITE_CHANCE = 0.1F;
        static float MONSTER_CHANCE = 0.1F;
        static float SHOP_CHANCE = 0.03F;
        public static float TREASURE_CHANCE = 0.02F;
        static List<float> saveFilePreviousChances;
        static string saveFileLastEventChoice;

        public enum RoomResult
        {
            EVENT,
            ELITE,
            TREASURE,
            SHOP,
            MONSTER
        }

        public static RoomResult roll()
        {
            return roll(ADungeon.eventRng);
        }

        public static RoomResult roll(Rand eventRng)
        {
            saveFilePreviousChances = getChances();
            float roll = eventRng.random();
            log("Rolling for room type... EVENT_RNG_COUNTER: " + ADungeon.eventRng.counter);
            var forceChest = false;
            if (player.hasRelic("Tiny Chest"))
            {
                var r = player.getRelic("Tiny Chest");
                r.counter++;
                if (r.counter == 4)
                {
                    r.counter = 0;
                    r.flash();
                    forceChest = true;
                }
            }

            log("ROLL: " + roll);
            log("Elite: " + ELITE_CHANCE);
            log("Monster: " + MONSTER_CHANCE);
            log("Shop: " + SHOP_CHANCE);
            log("Treasure: " + TREASURE_CHANCE);
            int eliteSize = 0;
            if (ModHelper.isModEnabled("DeadlyEvents"))
                eliteSize = (int)(ELITE_CHANCE * 100.0F);
            
            if (ADungeon.floorNum < 6)
                eliteSize = 0;
            
            int monsterSize = (int)(MONSTER_CHANCE * 100.0F);
            int shopSize = (int)(SHOP_CHANCE * 100.0F);
            
            if (room is ShopRoom)
                shopSize = 0;
            
            int treasureSize = (int)(TREASURE_CHANCE * 100.0F);
            int fillIndex = 0;
            RoomResult[] possibleResults = new RoomResult[100];
            Array.Fill(possibleResults, RoomResult.EVENT);
            if (ModHelper.isModEnabled("DeadlyEvents"))
            {
                Array.Fill(possibleResults, RoomResult.ELITE, Math.Min(99, fillIndex), Math.Min(100, fillIndex + eliteSize));
                fillIndex += eliteSize;
                Array.Fill(possibleResults, RoomResult.ELITE, Math.Min(99, fillIndex), Math.Min(100, fillIndex + eliteSize));
                fillIndex += eliteSize;
            }

            Array.Fill(possibleResults, RoomResult.MONSTER, Math.Min(99, fillIndex), Math.Min(100, fillIndex + monsterSize));
            fillIndex += monsterSize;
            Array.Fill(possibleResults, RoomResult.SHOP, Math.Min(99, fillIndex), Math.Min(100, fillIndex + shopSize));
            fillIndex += shopSize;
            Array.Fill(possibleResults, RoomResult.TREASURE, Math.Min(99, fillIndex), Math.Min(100, fillIndex + treasureSize));
            RoomResult choice = possibleResults[(int)(roll * 100.0F)];
            if (forceChest)
                choice = RoomResult.TREASURE;
            
            if (choice == RoomResult.ELITE)
            {
                ELITE_CHANCE = 0;
                if (ModHelper.isModEnabled("DeadlyEvents"))
                    ELITE_CHANCE = BASE_ELITE_CHANCE;
            }
            else
            {
                ELITE_CHANCE += RAMP_ELITE_CHANCE;
            }

            if (choice == RoomResult.MONSTER)
            {
                if (player.hasRelic("Juzu Bracelet"))
                {
                    player.getRelic("Juzu Bracelet").flash();
                    choice = RoomResult.EVENT;
                }

                MONSTER_CHANCE = BASE_MONSTER_CHANCE;
            }
            else
            {
                MONSTER_CHANCE += RAMP_MONSTER_CHANCE;
            }

            if (choice == RoomResult.SHOP)
                SHOP_CHANCE = BASE_SHOP_CHANCE;
            else
                SHOP_CHANCE += RAMP_SHOP_CHANCE;

            /*if (Settings.isEndless && player.hasBlight("MimicInfestation"))
            {
                if (choice == RoomResult.TREASURE)
                {
                    if (player.hasRelic("Juzu Bracelet"))
                    {
                        player.getRelic("Juzu Bracelet").flash();
                        choice = RoomResult.EVENT;
                    }
                    else
                    {
                        choice = RoomResult.ELITE;
                    }

                    TREASURE_CHANCE = BASE_TREASURE_CHANCE;
                    if (ModHelper.isModEnabled("DeadlyEvents"))
                        TREASURE_CHANCE += RAMP_TREASURE_CHANCE;
                }
            }
            else*/ if (choice == RoomResult.TREASURE)
            {
                TREASURE_CHANCE = BASE_TREASURE_CHANCE;
            }
            else
            {
                TREASURE_CHANCE += RAMP_TREASURE_CHANCE;
                if (ModHelper.isModEnabled("DeadlyEvents"))
                    TREASURE_CHANCE += RAMP_TREASURE_CHANCE;
            }

            return choice;
        }

        public static void resetProbabilities()
        {
            saveFilePreviousChances = null;
            ELITE_CHANCE = RESET_ELITE_CHANCE;
            MONSTER_CHANCE = RESET_MONSTER_CHANCE;
            SHOP_CHANCE = RESET_SHOP_CHANCE;
            TREASURE_CHANCE = RESET_TREASURE_CHANCE;
        }

        public static void setChances(List<float> chances)
        {
            ELITE_CHANCE = chances[0];
            MONSTER_CHANCE = chances[1];
            SHOP_CHANCE = chances[2];
            TREASURE_CHANCE = chances[3];
        }

        public static List<float> getChances()
        {
            List<float> chances = new()
            {
                ELITE_CHANCE,
                MONSTER_CHANCE,
                SHOP_CHANCE,
                TREASURE_CHANCE
            };
            return chances;
        }

        public static List<float> getChancesPreRoll()
        {
            if (saveFilePreviousChances != null)
                return saveFilePreviousChances;
            return getChances();
        }

        public static string getMostRecentEventID()
        {
            return saveFileLastEventChoice;
        }

        public static AEvent getEvent(string key)
        {
            // if (Settings.isDev)
            // ;

            // saveFileLastEventChoice = key;
            // switch (key)
            // {
            //     case "Accursed Blacksmith":
            //         return new AccursedBlacksmith();
            //     case "Bonfire Elementals":
            //         return new Bonfire();
            //     case "Fountain of Cleansing":
            //         return new FountainOfCurseRemoval();
            //     case "Designer":
            //         return new Designer();
            //     case "Duplicator":
            //         return new Duplicator();
            //     case "Lab":
            //         return new Lab();
            //     case "Match and Keep!":
            //         return new GremlinMatchGame();
            //     case "Golden Shrine":
            //         return new GoldShrine();
            //     case "Purifier":
            //         return new PurificationShrine();
            //     case "Transmorgrifier":
            //         return new Transmogrifier();
            //     case "Wheel of Change":
            //         return new GremlinWheelGame();
            //     case "Upgrade Shrine":
            //         return new UpgradeShrine();
            //     case "FaceTrader":
            //         return new FaceTrader();
            //     case "NoteForYourself":
            //         return new NoteForYourself();
            //     case "WeMeetAgain":
            //         return new WeMeetAgain();
            //     case "The Woman in Blue":
            //         return new WomanInBlue();
            //     case "Big Fish":
            //         return new BigFish();
            //     case "The Cleric":
            //         return new Cleric();
            //     case "Dead Adventurer":
            //         return new DeadAdventurer();
            //     case "Golden Wing":
            //         return new GoldenWing();
            //     case "Golden Idol":
            //         return new GoldenIdolEvent();
            //     case "World of Goop":
            //         return new GoopPuddle();
            //     case "Forgotten Altar":
            //         return new ForgottenAltar();
            //     case "Scrap Ooze":
            //         return new ScrapOoze();
            //     case "Liars Game":
            //         return new Sssserpent();
            //     case "Living Wall":
            //         return new LivingWall();
            //     case "Mushrooms":
            //         return new Mushrooms();
            //     case "N'loth":
            //         return new Nloth();
            //     case "Shining Light":
            //         return new ShiningLight();
            //     case "Vampires":
            //         return new Vampires();
            //     case "Ghosts":
            //         return new Ghosts();
            //     case "Addict":
            //         return new Addict();
            //     case "Back to Basics":
            //         return new BackToBasics();
            //     case "Beggar":
            //         return new Beggar();
            //     case "Cursed Tome":
            //         return new CursedTome();
            //     case "Drug Dealer":
            //         return new DrugDealer();
            //     case "Knowing Skull":
            //         return new KnowingSkull();
            //     case "Masked Bandits":
            //         return new MaskedBandits();
            //     case "Nest":
            //         return new Nest();
            //     case "The Library":
            //         return new TheLibrary();
            //     case "The Mausoleum":
            //         return new TheMausoleum();
            //     case "The Joust":
            //         return new TheJoust();
            //     case "Colosseum":
            //         return new Colosseum();
            //     case "Mysterious Sphere":
            //         return new MysteriousSphere();
            //     case "SecretPortal":
            //         return new SecretPortal();
            //     case "Tomb of Lord Red Mask":
            //         return new TombRedMask();
            //     case "Falling":
            //         return new Falling();
            //     case "Winding Halls":
            //         return new WindingHalls();
            //     case "The Moai Head":
            //         return new MoaiHead();
            //     case "SensoryStone":
            //         return new SensoryStone();
            //     case "MindBloom":
            //         return new MindBloom();
            // }

            log("---------------------------\nERROR: Unspecified key: " + key + " in EventHelper.\n---------------------------");
            return null;
        }

        public static string getEventName(string key)
        {
            return key switch
            {
                // "Accursed Blacksmith" => AccursedBlacksmith.NAME,
                // "Bonfire Elementals" => Bonfire.NAME,
                // "Fountain of Cleansing" => FountainOfCurseRemoval.NAME,
                // "Designer" => Designer.NAME,
                // "Duplicator" => Duplicator.NAME,
                // "Lab" => Lab.NAME,
                // "Match and Keep!" => GremlinMatchGame.NAME,
                // "Golden Shrine" => GoldShrine.NAME,
                // "Purifier" => PurificationShrine.NAME,
                // "Transmorgrifier" => Transmogrifier.NAME,
                // "Wheel of Change" => GremlinWheelGame.NAME,
                // "Upgrade Shrine" => UpgradeShrine.NAME,
                // "FaceTrader" => FaceTrader.NAME,
                // "NoteForYourself" => NoteForYourself.NAME,
                // "WeMeetAgain" => WeMeetAgain.NAME,
                // "The Woman in Blue" => WomanInBlue.NAME,
                // "Big Fish" => BigFish.NAME,
                // "The Cleric" => Cleric.NAME,
                // "Dead Adventurer" => DeadAdventurer.NAME,
                // "Golden Wing" => GoldenWing.NAME,
                // "Golden Idol" => GoldenIdolEvent.NAME,
                // "World of Goop" => GoopPuddle.NAME,
                // "Forgotten Altar" => ForgottenAltar.NAME,
                // "Scrap Ooze" => ScrapOoze.NAME,
                // "Liars Game" => Sssserpent.NAME,
                // "Living Wall" => LivingWall.NAME,
                // "Mushrooms" => Mushrooms.NAME,
                // "N'loth" => Nloth.NAME,
                // "Shining Light" => ShiningLight.NAME,
                // "Vampires" => Vampires.NAME,
                // "Ghosts" => Ghosts.NAME,
                // "Addict" => Addict.NAME,
                // "Back to Basics" => BackToBasics.NAME,
                // "Beggar" => Beggar.NAME,
                // "Cursed Tome" => CursedTome.NAME,
                // "Drug Dealer" => DrugDealer.NAME,
                // "Knowing Skull" => KnowingSkull.NAME,
                // "Masked Bandits" => MaskedBandits.NAME,
                // "Nest" => Nest.NAME,
                // "The Library" => TheLibrary.NAME,
                // "The Mausoleum" => TheMausoleum.NAME,
                // "The Joust" => TheJoust.NAME,
                // "Colosseum" => Colosseum.NAME,
                // "Mysterious Sphere" => MysteriousSphere.NAME,
                // "SecretPortal" => SecretPortal.NAME,
                // "Tomb of Lord Red Mask" => TombRedMask.NAME,
                // "Falling" => Falling.NAME,
                // "Winding Halls" => WindingHalls.NAME,
                // "The Moai Head" => MoaiHead.NAME,
                // "SensoryStone" => SensoryStone.NAME,
                // "MindBloom" => MindBloom.NAME,
                _ => ""
            };
        }
    }
}