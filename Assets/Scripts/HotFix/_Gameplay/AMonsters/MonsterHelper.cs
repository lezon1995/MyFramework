using System.Collections.Generic;

namespace MoreMountains
{
    public class MonsterHelper
    {
        // static UIStrings uiStrings = CardCrawlGame.languagePack.getUIString("RunHistoryMonsterNames");
        // public static string[] MIXED_COMBAT_NAMES = uiStrings.TEXT;
        public static string BLUE_SLAVER_ENC = "Blue Slaver";
        public static string CULTIST_ENC = "Cultist";
        public static string JAW_WORM_ENC = "Jaw Worm";
        public static string LOOTER_ENC = "Looter";
        public static string TWO_LOUSE_ENC = "2 Louse";
        public static string SMALL_SLIMES_ENC = "Small Slimes";
        public static string GREMLIN_GANG_ENC = "Gremlin Gang";
        public static string RED_SLAVER_ENC = "Red Slaver";
        public static string LARGE_SLIME_ENC = "Large Slime";
        public static string LVL_1_THUGS_ENC = "Exordium Thugs";
        public static string LVL_1_WILDLIFE_ENC = "Exordium Wildlife";
        public static string THREE_LOUSE_ENC = "3 Louse";
        public static string TWO_FUNGI_ENC = "2 Fungi Beasts";
        public static string LOTS_OF_SLIMES_ENC = "Lots of Slimes";
        public static string GREMLIN_NOB_ENC = "Gremlin Nob";
        public static string LAGAVULIN_ENC = "Lagavulin";
        public static string THREE_SENTRY_ENC = "3 Sentries";
        public static string LAGAVULIN_EVENT_ENC = "Lagavulin Event";
        public static string MUSHROOMS_EVENT_ENC = "The Mushroom Lair";
        public static string GUARDIAN_ENC = "The Guardian";
        public static string HEXAGHOST_ENC = "Hexaghost";
        public static string SLIME_BOSS_ENC = "Slime Boss";
        public static string TWO_THIEVES_ENC = "2 Thieves";
        public static string THREE_BYRDS_ENC = "3 Byrds";
        public static string CHOSEN_ENC = "Chosen";
        public static string SHELL_PARASITE_ENC = "Shell Parasite";
        public static string SPHERE_GUARDIAN_ENC = "Spheric Guardian";
        public static string CULTIST_CHOSEN_ENC = "Cultist and Chosen";
        public static string THREE_CULTISTS_ENC = "3 Cultists";
        public static string FOUR_BYRDS_ENC = "4 Byrds";
        public static string CHOSEN_FLOCK_ENC = "Chosen and Byrds";
        public static string SENTRY_SPHERE_ENC = "Sentry and Sphere";
        public static string SNAKE_PLANT_ENC = "Snake Plant";
        public static string SNECKO_ENC = "Snecko";
        public static string TANK_HEALER_ENC = "Centurion and Healer";
        public static string PARASITE_AND_FUNGUS = "Shelled Parasite and Fungi";
        public static string STAB_BOOK_ENC = "Book of Stabbing";
        public static string GREMLIN_LEADER_ENC = "Gremlin Leader";
        public static string SLAVERS_ENC = "Slavers";
        public static string MASKED_BANDITS_ENC = "Masked Bandits";
        public static string COLOSSEUM_SLAVER_ENC = "Colosseum Slavers";
        public static string COLOSSEUM_NOB_ENC = "Colosseum Nobs";
        public static string AUTOMATON_ENC = "Automaton";
        public static string CHAMP_ENC = "Champ";
        public static string COLLECTOR_ENC = "Collector";
        public static string THREE_DARKLINGS_ENC = "3 Darklings";
        public static string THREE_SHAPES_ENC = "3 Shapes";
        public static string ORB_WALKER_ENC = "Orb Walker";
        public static string TRANSIENT_ENC = "Transient";
        public static string REPTOMANCER_ENC = "Reptomancer";
        public static string SPIRE_GROWTH_ENC = "Spire Growth";
        public static string MAW_ENC = "Maw";
        public static string FOUR_SHAPES_ENC = "4 Shapes";
        public static string SPHERE_TWO_SHAPES_ENC = "Sphere and 2 Shapes";
        public static string JAW_WORMS_HORDE = "Jaw Worm Horde";
        public static string SNECKO_WITH_MYSTICS = "Snecko and Mystics";
        public static string WRITHING_MASS_ENC = "Writhing Mass";
        public static string TWO_ORB_WALKER_ENC = "2 Orb Walkers";
        public static string NEMESIS_ENC = "Nemesis";
        public static string GIANT_HEAD_ENC = "Giant Head";
        public static string MYSTERIOUS_SPHERE_ENC = "Mysterious Sphere";
        public static string MIND_BLOOM_BOSS = "Mind Bloom Boss Battle";
        public static string TIME_EATER_ENC = "Time Eater";
        public static string AWAKENED_ENC = "Awakened One";
        public static string DONU_DECA_ENC = "Donu and Deca";
        public static string THE_HEART_ENC = "The Heart";
        public static string SHIELD_SPEAR_ENC = "Shield and Spear";
        public static string EYES_ENC = "The Eyes";
        public static string APOLOGY_SLIME_ENC = "Apologetic Slime";
        public static string OLD_REPTO_ONE_ENC = "Flame Bruiser 1 Orb";
        public static string OLD_REPTO_TWO_ENC = "Flame Bruiser 2 Orb";
        public static string OLD_SLAVER_PARASITE = "Slaver and Parasite";
        public static string OLD_SNECKO_MYSTICS = "Snecko and Mystics";

        public static string getEncounterName(string key)
        {
            if (key == null)
                return "";

            return "";

            /*return key switch
            {
                "Flame Bruiser 1 Orb" or "Flame Bruiser 2 Orb" => MIXED_COMBAT_NAMES[25],
                "Slaver and Parasite" => MIXED_COMBAT_NAMES[26],
                "Snecko and Mystics" => MIXED_COMBAT_NAMES[27],
                _ => key switch
                {
                    "Blue Slaver" => SlaverBlue.NAME,
                    "Cultist" => Cultist.NAME,
                    "Jaw Worm" => JawWorm.NAME,
                    "Looter" => Looter.NAME,
                    "Gremlin Gang" => MIXED_COMBAT_NAMES[0],
                    "Red Slaver" => SlaverRed.NAME,
                    "Large Slime" => MIXED_COMBAT_NAMES[1],
                    "Exordium Thugs" => MIXED_COMBAT_NAMES[2],
                    "Exordium Wildlife" => MIXED_COMBAT_NAMES[3],
                    "3 Louse" => LouseNormal.NAME,
                    "2 Louse" => LouseNormal.NAME,
                    "2 Fungi Beasts" => FungiBeast.NAME,
                    "Lots of Slimes" => MIXED_COMBAT_NAMES[4],
                    "Small Slimes" => MIXED_COMBAT_NAMES[5],
                    "Gremlin Nob" => GremlinNob.NAME,
                    "Lagavulin" => Lagavulin.NAME,
                    "3 Sentries" => MIXED_COMBAT_NAMES[23],
                    "Lagavulin Event" => Lagavulin.NAME,
                    "The Mushroom Lair" => FungiBeast.NAME,
                    "The Guardian" => TheGuardian.NAME,
                    "Hexaghost" => Hexaghost.NAME,
                    "Slime Boss" => SlimeBoss.NAME,
                    _ => key switch
                    {
                        "2 Thieves" => MIXED_COMBAT_NAMES[6],
                        "3 Byrds" => MIXED_COMBAT_NAMES[7],
                        "4 Byrds" => MIXED_COMBAT_NAMES[8],
                        "Chosen" => Chosen.NAME,
                        "Shell Parasite" => ShelledParasite.NAME,
                        "Spheric Guardian" => SphericGuardian.NAME,
                        "Cultist and Chosen" => MIXED_COMBAT_NAMES[24],
                        "3 Cultists" => MIXED_COMBAT_NAMES[9],
                        "Chosen and Byrds" => MIXED_COMBAT_NAMES[10],
                        "Sentry and Sphere" => MIXED_COMBAT_NAMES[11],
                        "Snake Plant" => SnakePlant.NAME,
                        "Snecko" => Snecko.NAME,
                        "Centurion and Healer" => MIXED_COMBAT_NAMES[12],
                        "Shelled Parasite and Fungi" => MIXED_COMBAT_NAMES[13],
                        "Book of Stabbing" => BookOfStabbing.NAME,
                        "Gremlin Leader" => GremlinLeader.NAME,
                        "Slavers" => Taskmaster.NAME,
                        "Masked Bandits" => MIXED_COMBAT_NAMES[14],
                        "Colosseum Nobs" => MIXED_COMBAT_NAMES[15],
                        "Colosseum Slavers" => MIXED_COMBAT_NAMES[16],
                        "Automaton" => BronzeAutomaton.NAME,
                        "Champ" => Champ.NAME,
                        "Collector" => TheCollector.NAME,
                        _ => key switch
                        {
                            "Reptomancer" => Reptomancer.NAME,
                            "Transient" => Transient.NAME,
                            "3 Darklings" => Darkling.NAME,
                            "3 Shapes" => MIXED_COMBAT_NAMES[17],
                            "Jaw Worm Horde" => MIXED_COMBAT_NAMES[18],
                            "Orb Walker" => OrbWalker.NAME,
                            "Spire Growth" => SpireGrowth.NAME,
                            "Maw" => Maw.NAME,
                            "4 Shapes" => MIXED_COMBAT_NAMES[19],
                            "Sphere and 2 Shapes" => MIXED_COMBAT_NAMES[20],
                            "2 Orb Walkers" => MIXED_COMBAT_NAMES[21],
                            "Nemesis" => Nemesis.NAME,
                            "Writhing Mass" => WrithingMass.NAME,
                            "Giant Head" => GiantHead.NAME,
                            "Mysterious Sphere" => MysteriousSphere.NAME,
                            "Time Eater" => TimeEater.NAME,
                            "Awakened One" => AwakenedOne.NAME,
                            "Donu and Deca" => MIXED_COMBAT_NAMES[22],
                            _ => key switch
                            {
                                "The Heart" => CorruptHeart.NAME,
                                "Shield and Spear" => MIXED_COMBAT_NAMES[28],
                                _ => ""
                            }
                        }
                    }
                }
            };*/
        }

        public static MonsterGroup getEncounter(string key)
        {
            // var path = $"{GAMEPLAY_PATH}/Characters/OpCharacter.prefab";
            // var o = prefabPool.createObject(path);
            // o.TryGetComponent(out Opponent newPlayer);
            return new MonsterGroup();

            /*
            switch (key)
            {
                case "Blue Slaver":
                    return new MonsterGroup(new SlaverBlue(0.0F, 0.0F));
                case "Cultist":
                    return new MonsterGroup(new Cultist(0.0F, -10.0F));
                case "Jaw Worm":
                    return new MonsterGroup(new JawWorm(0.0F, 25.0F));
                case "Looter":
                    return new MonsterGroup(new Looter(0.0F, 0.0F));
                case "Gremlin Gang":
                    return spawnGremlins();
                case "Red Slaver":
                    return new MonsterGroup(new SlaverRed(0.0F, 0.0F));
                case "Large Slime":
                    if (ADungeon.miscRng.randomBool())
                        return new MonsterGroup(new AcidSlime_L(0.0F, 0.0F));
                    return new MonsterGroup(new SpikeSlime_L(0.0F, 0.0F));
                case "Exordium Thugs":
                    return bottomHumanoid();
                case "Exordium Wildlife":
                    return bottomWildlife();
                case "3 Louse":
                    return new MonsterGroup(new AMonster[] { getLouse(-350.0F, 25.0F), getLouse(-125.0F, 10.0F), getLouse(80.0F, 30.0F) });
                case "2 Louse":
                    return new MonsterGroup(new AMonster[] { getLouse(-200.0F, 10.0F), getLouse(80.0F, 30.0F) });
                case "2 Fungi Beasts":
                    return new MonsterGroup(new AMonster[] { new FungiBeast(-400.0F, 30.0F), new FungiBeast(-40.0F, 20.0F) });
                case "Lots of Slimes":
                    return spawnManySmallSlimes();
                case "Small Slimes":
                    return spawnSmallSlimes();
                case "Gremlin Nob":
                    return new MonsterGroup(new GremlinNob(0.0F, 0.0F));
                case "Lagavulin":
                    return new MonsterGroup(new Lagavulin(true));
                case "3 Sentries":
                    return new MonsterGroup(new AMonster[] { new Sentry(-330.0F, 25.0F), new Sentry(-85.0F, 10.0F), new Sentry(140.0F, 30.0F) });
                case "Lagavulin Event":
                    return new MonsterGroup(new Lagavulin(false));
                case "The Mushroom Lair":
                    return new MonsterGroup(new AMonster[] { new FungiBeast(-450.0F, 30.0F), new FungiBeast(-145.0F, 20.0F), new FungiBeast(180.0F, 15.0F) });
                case "The Guardian":
                    return new MonsterGroup(new TheGuardian());
                case "Hexaghost":
                    return new MonsterGroup(new Hexaghost());
                case "Slime Boss":
                    return new MonsterGroup(new SlimeBoss());
            }

            switch (key)
            {
                case "2 Thieves":
                    return new MonsterGroup(new AMonster[] { new Looter(-200.0F, 15.0F), new Mugger(80.0F, 0.0F) });
                case "3 Byrds":
                    return new MonsterGroup(new AMonster[]
                    {
                        new Byrd(-360.0F,
                            MathUtils.random(25.0F, 70.0F)),
                        new Byrd(-80.0F,
                            MathUtils.random(25.0F, 70.0F)),
                        new Byrd(200.0F, MathUtils.random(25.0F, 70.0F))
                    });
                case "4 Byrds":
                    return new MonsterGroup(new AMonster[]
                    {
                        new Byrd(-470.0F,
                            MathUtils.random(25.0F, 70.0F)),
                        new Byrd(-210.0F,
                            MathUtils.random(25.0F, 70.0F)),
                        new Byrd(50.0F, MathUtils.random(25.0F, 70.0F)), new Byrd(310.0F,
                            MathUtils.random(25.0F, 70.0F))
                    });
                case "Chosen":
                    return new MonsterGroup(new Chosen());
                case "Shell Parasite":
                    return new MonsterGroup(new ShelledParasite());
                case "Spheric Guardian":
                    return new MonsterGroup(new SphericGuardian());
                case "Cultist and Chosen":
                    return new MonsterGroup(new AMonster[] { new Cultist(-230.0F, 15.0F, false), new Chosen(100.0F, 25.0F) });
                case "3 Cultists":
                    return new MonsterGroup(new AMonster[] { new Cultist(-465.0F, -20.0F, false), new Cultist(-130.0F, 15.0F, false), new Cultist(200.0F, -5.0F) });
                case "Chosen and Byrds":
                    return new MonsterGroup(new AMonster[]
                    {
                        new Byrd(-170.0F, MathUtils.random(25.0F, 70.0F)),
                        new Chosen(80.0F, 0.0F)
                    });
                case "Sentry and Sphere":
                    return new MonsterGroup(new AMonster[] { new Sentry(-305.0F, 30.0F), new SphericGuardian() });
                case "Snake Plant":
                    return new MonsterGroup(new SnakePlant(-30.0F, -30.0F));
                case "Snecko":
                    return new MonsterGroup(new Snecko());
                case "Centurion and Healer":
                    return new MonsterGroup(new AMonster[] { new Centurion(-200.0F, 15.0F), new Healer(120.0F, 0.0F) });
                case "Shelled Parasite and Fungi":
                    return new MonsterGroup(new AMonster[] { new ShelledParasite(-260.0F, 15.0F), new FungiBeast(120.0F, 0.0F) });
                case "Book of Stabbing":
                    return new MonsterGroup(new BookOfStabbing());
                case "Gremlin Leader":
                    return new MonsterGroup(new AMonster[] { spawnGremlin(GremlinLeader.POSX[0], GremlinLeader.POSY[0]), spawnGremlin(GremlinLeader.POSX[1], GremlinLeader.POSY[1]), new GremlinLeader() });
                case "Slavers":
                    return new MonsterGroup(new AMonster[] { new SlaverBlue(-385.0F, -15.0F), new Taskmaster(-133.0F, 0.0F), new SlaverRed(125.0F, -30.0F) });
                case "Masked Bandits":
                    return new MonsterGroup(new AMonster[] { new BanditPointy(-320.0F, 0.0F), new BanditLeader(-75.0F, -6.0F), new BanditBear(150.0F, -6.0F) });
                case "Colosseum Nobs":
                    return new MonsterGroup(new AMonster[] { new Taskmaster(-270.0F, 15.0F), new GremlinNob(130.0F, 0.0F) });
                case "Colosseum Slavers":
                    return new MonsterGroup(new AMonster[] { new SlaverBlue(-270.0F, 15.0F), new SlaverRed(130.0F, 0.0F) });
                case "Automaton":
                    return new MonsterGroup(new BronzeAutomaton());
                case "Champ":
                    return new MonsterGroup(new Champ());
                case "Collector":
                    return new MonsterGroup(new TheCollector());
            }

            switch (key)
            {
                case "Flame Bruiser 1 Orb":
                    return new MonsterGroup(new AMonster[] { new Reptomancer(), new SnakeDagger(Reptomancer.POSX[0], Reptomancer.POSY[0]) });
                case "Flame Bruiser 2 Orb":
                case "Reptomancer":
                    return new MonsterGroup(new AMonster[] { new SnakeDagger(Reptomancer.POSX[1], Reptomancer.POSY[1]), new Reptomancer(), new SnakeDagger(Reptomancer.POSX[0], Reptomancer.POSY[0]) });
                case "Transient":
                    return new MonsterGroup(new Transient());
                case "3 Darklings":
                    return new MonsterGroup(new AMonster[] { new Darkling(-440.0F, 10.0F), new Darkling(-140.0F, 30.0F), new Darkling(180.0F, -5.0F) });
                case "3 Shapes":
                    return spawnShapes(true);
                case "Jaw Worm Horde":
                    return new MonsterGroup(new AMonster[] { new JawWorm(-490.0F, -5.0F, true), new JawWorm(-150.0F, 20.0F, true), new JawWorm(175.0F, 5.0F, true) });
                case "Snecko and Mystics":
                    return new MonsterGroup(new AMonster[] { new Healer(-475.0F, -10.0F), new Snecko(-130.0F, -13.0F), new Healer(175.0F, -10.0F) });
                case "Orb Walker":
                    return new MonsterGroup(new OrbWalker(-30.0F, 20.0F));
                case "Spire Growth":
                    return new MonsterGroup(new SpireGrowth());
                case "Maw":
                    return new MonsterGroup(new Maw(-70.0F, 20.0F));
                case "4 Shapes":
                    return spawnShapes(false);
                case "Sphere and 2 Shapes":
                    return new MonsterGroup(new AMonster[] { getAncientShape(-435.0F, 10.0F), getAncientShape(-210.0F, 0.0F), new SphericGuardian(110.0F, 10.0F) });
                case "2 Orb Walkers":
                    return new MonsterGroup(new AMonster[] { new OrbWalker(-250.0F, 32.0F), new OrbWalker(150.0F, 26.0F) });
                case "Nemesis":
                    return new MonsterGroup(new Nemesis());
                case "Writhing Mass":
                    return new MonsterGroup(new WrithingMass());
                case "Giant Head":
                    return new MonsterGroup(new GiantHead());
                case "Mysterious Sphere":
                    return new MonsterGroup(new AMonster[] { getAncientShape(-475.0F, 10.0F), getAncientShape(-250.0F, 0.0F), new OrbWalker(150.0F, 30.0F) });
                case "Time Eater":
                    return new MonsterGroup(new TimeEater());
                case "Awakened One":
                    return new MonsterGroup(new AMonster[] { new Cultist(-590.0F, 10.0F, false), new Cultist(-298.0F, -10.0F, false), new AwakenedOne(100.0F, 15.0F) });
                case "Donu and Deca":
                    return new MonsterGroup(new AMonster[] { new Deca(), new Donu() });
            }

            switch (key)
            {
                case "The Heart":
                    return new MonsterGroup(new CorruptHeart());
                case "Shield and Spear":
                    return new MonsterGroup(new AMonster[] { new SpireShield(), new SpireSpear() });
            }
            */

            return null;
            // return new MonsterGroup(new ApologySlime());
        }

        static float randomYOffset(float y)
        {
            return y + MathUtils.random(-20.0F, 20.0F);
        }

        static float randomXOffset(float x)
        {
            return x + MathUtils.random(-20.0F, 20.0F);
        }

        public static AMonster getGremlin(string key, float xPos, float yPos)
        {
            /*switch (key)
            {
                case "GremlinWarrior":
                    return new GremlinWarrior(xPos, yPos);
                case "GremlinThief":
                    return new GremlinThief(xPos, yPos);
                case "GremlinFat":
                    return new GremlinFat(xPos, yPos);
                case "GremlinTsundere":
                    return new GremlinTsundere(xPos, yPos);
                case "GremlinWizard":
                    return new GremlinWizard(xPos, yPos);
            }*/

            log("UNKNOWN GREMLIN: " + key);
            return null;
        }

        public static AMonster getAncientShape(float x, float y)
        {
            // return ADungeon.miscRng.random(2) switch
            // {
            //     0 => new Spiker(x, y),
            //     1 => new Repulsor(x, y),
            //     _ => new Exploder(x, y)
            // };

            return null;
        }

        public static AMonster getShape(string key, float xPos, float yPos)
        {
            /*switch (key)
            {
                case "Repulsor":
                    return new Repulsor(xPos, yPos);
                case "Spiker":
                    return new Spiker(xPos, yPos);
                case "Exploder":
                    return new Exploder(xPos, yPos);
            }*/

            log("UNKNOWN SHAPE: " + key);
            return null;
        }

        static MonsterGroup spawnShapes(bool weak)
        {
            List<string> shapePool = new()
            {
                "Repulsor",
                "Repulsor",
                "Exploder",
                "Exploder",
                "Spiker",
                "Spiker"
            };

            AMonster[] retVal;
            if (weak)
                retVal = new AMonster[3];
            else
                retVal = new AMonster[4];

            int index = ADungeon.miscRng.random(shapePool.Count - 1);
            string key = shapePool[index];
            shapePool.RemoveAt(index);
            retVal[0] = getShape(key, -480.0F, 6.0F);
            index = ADungeon.miscRng.random(shapePool.Count - 1);
            key = shapePool[index];
            shapePool.RemoveAt(index);
            retVal[1] = getShape(key, -240.0F, -6.0F);
            index = ADungeon.miscRng.random(shapePool.Count - 1);
            key = shapePool[index];
            shapePool.RemoveAt(index);
            retVal[2] = getShape(key, 0.0F, -12.0F);
            if (!weak)
            {
                index = ADungeon.miscRng.random(shapePool.Count - 1);
                key = shapePool[index];
                shapePool.RemoveAt(index);
                retVal[3] = getShape(key, 240.0F, 12.0F);
            }

            return new MonsterGroup(retVal);
        }

        static MonsterGroup spawnSmallSlimes()
        {
            AMonster[] retVal = new AMonster[2];
            /*if (ADungeon.miscRng.randomBool())
            {
                retVal[0] = new SpikeSlime_S(-230.0F, 32.0F, 0);
                retVal[1] = new AcidSlime_M(35.0F, 8.0F);
            }
            else
            {
                retVal[0] = new AcidSlime_S(-230.0F, 32.0F, 0);
                retVal[1] = new SpikeSlime_M(35.0F, 8.0F);
            }*/

            return new MonsterGroup(retVal);
        }

        static MonsterGroup spawnManySmallSlimes()
        {
            List<string> slimePool = new()
            {
                "SpikeSlime_S",
                "SpikeSlime_S",
                "SpikeSlime_S",
                "AcidSlime_S",
                "AcidSlime_S"
            };

            AMonster[] retVal = new AMonster[5];

            /*int index = ADungeon.miscRng.random(slimePool.Count - 1);
            string key = slimePool[index];
            slimePool.RemoveAt(index);
            if (key == ("SpikeSlime_S"))
                retVal[0] = new SpikeSlime_S(-480.0F, 30.0F, 0);
            else
                retVal[0] = new AcidSlime_S(-480.0F, 30.0F, 0);

            index = ADungeon.miscRng.random(slimePool.Count - 1);
            key = slimePool[index];
            slimePool.RemoveAt(index);
            if (key == ("SpikeSlime_S"))
                retVal[1] = new SpikeSlime_S(-320.0F, 2.0F, 0);
            else
                retVal[1] = new AcidSlime_S(-320.0F, 2.0F, 0);

            index = ADungeon.miscRng.random(slimePool.Count - 1);
            key = slimePool[index];
            slimePool.RemoveAt(index);
            if (key == ("SpikeSlime_S"))
                retVal[2] = new SpikeSlime_S(-160.0F, 32.0F, 0);
            else
                retVal[2] = new AcidSlime_S(-160.0F, 32.0F, 0);

            index = ADungeon.miscRng.random(slimePool.Count - 1);
            key = slimePool[index];
            slimePool.RemoveAt(index);
            if (key == ("SpikeSlime_S"))
                retVal[3] = new SpikeSlime_S(10.0F, -12.0F, 0);
            else
                retVal[3] = new AcidSlime_S(10.0F, -12.0F, 0);

            index = ADungeon.miscRng.random(slimePool.Count - 1);
            key = slimePool[index];
            slimePool.RemoveAt(index);
            if (key == ("SpikeSlime_S"))
                retVal[4] = new SpikeSlime_S(200.0F, 9.0F, 0);
            else
                retVal[4] = new AcidSlime_S(200.0F, 9.0F, 0);
                */

            return new MonsterGroup(retVal);
        }

        static MonsterGroup spawnGremlins()
        {
            List<string> gremlinPool = new()
            {
                "GremlinWarrior",
                "GremlinWarrior",
                "GremlinThief",
                "GremlinThief",
                "GremlinFat",
                "GremlinFat",
                "GremlinTsundere",
                "GremlinWizard"
            };
            AMonster[] retVal = new AMonster[4];
            int index = ADungeon.miscRng.random(gremlinPool.Count - 1);
            string key = gremlinPool[index];
            gremlinPool.RemoveAt(index);
            retVal[0] = getGremlin(key, -320.0F, 25.0F);
            index = ADungeon.miscRng.random(gremlinPool.Count - 1);
            key = gremlinPool[index];
            gremlinPool.RemoveAt(index);
            retVal[1] = getGremlin(key, -160.0F, -12.0F);
            index = ADungeon.miscRng.random(gremlinPool.Count - 1);
            key = gremlinPool[index];
            gremlinPool.RemoveAt(index);
            retVal[2] = getGremlin(key, 25.0F, -35.0F);
            index = ADungeon.miscRng.random(gremlinPool.Count - 1);
            key = gremlinPool[index];
            gremlinPool.RemoveAt(index);
            retVal[3] = getGremlin(key, 205.0F, 40.0F);
            return new MonsterGroup(retVal);
        }

        static AMonster spawnGremlin(float x, float y)
        {
            List<string> gremlinPool = new()
            {
                "GremlinWarrior",
                "GremlinWarrior",
                "GremlinThief",
                "GremlinThief",
                "GremlinFat",
                "GremlinFat",
                "GremlinTsundere",
                "GremlinWizard"
            };
            return getGremlin(gremlinPool[ADungeon.miscRng.random(0, gremlinPool.Count - 1)], x, y);
        }

        static MonsterGroup bottomHumanoid()
        {
            AMonster[] monsters = new AMonster[2];
            monsters[0] = bottomGetWeakWildlife(randomXOffset(-160.0F), randomYOffset(20.0F));
            monsters[1] = bottomGetStrongHumanoid(randomXOffset(130.0F), randomYOffset(20.0F));
            return new MonsterGroup(monsters);
        }

        static MonsterGroup bottomWildlife()
        {
            int numMonster = 2;
            AMonster[] monsters = new AMonster[numMonster];
            switch (numMonster)
            {
                case 2:
                    monsters[0] = bottomGetStrongWildlife(randomXOffset(-150.0F), randomYOffset(20.0F));
                    monsters[1] = bottomGetWeakWildlife(randomXOffset(150.0F), randomYOffset(20.0F));
                    break;
                case 3:
                    monsters[0] = bottomGetWeakWildlife(randomXOffset(-200.0F), randomYOffset(20.0F));
                    monsters[1] = bottomGetWeakWildlife(randomXOffset(0.0F), randomYOffset(20.0F));
                    monsters[2] = bottomGetWeakWildlife(randomXOffset(200.0F), randomYOffset(20.0F));
                    break;
            }

            return new MonsterGroup(monsters);
        }

        static AMonster bottomGetStrongHumanoid(float x, float y)
        {
            List<AMonster> monsters = new();
            // monsters.Add(new Cultist(x, y));
            // monsters.Add(getSlaver(x, y));
            // monsters.Add(new Looter(x, y));
            AMonster output = monsters[ADungeon.miscRng.random(0, monsters.Count - 1)];
            return output;
        }

        static AMonster bottomGetStrongWildlife(float x, float y)
        {
            List<AMonster> monsters = new();
            // monsters.Add(new FungiBeast(x, y));
            // monsters.Add(new JawWorm(x, y));
            AMonster output = monsters[ADungeon.miscRng.random(0, monsters.Count - 1)];
            return output;
        }

        static AMonster bottomGetWeakWildlife(float x, float y)
        {
            List<AMonster> monsters = new();
            monsters.Add(getLouse(x, y));
            // monsters.Add(new SpikeSlime_M(x, y));
            // monsters.Add(new AcidSlime_M(x, y));
            return monsters[ADungeon.miscRng.random(0, monsters.Count - 1)];
        }

        static AMonster getSlaver(float x, float y)
        {
            // if (ADungeon.miscRng.randomBool())
            //     return new SlaverRed(x, y);
            // return new SlaverBlue(x, y);
            return null;
        }

        static AMonster getLouse(float x, float y)
        {
            // if (ADungeon.miscRng.randomBool())
            //     return new LouseNormal(x, y);
            // return new LouseDefensive(x, y);
            return null;
        }

        public static void uploadEnemyData()
        {
            List<string> derp = new();
            List<EnemyData> data = new()
            {
                new("Blue Slaver", 1, EnemyData.MonsterType.WEAK),
                new("Cultist", 1, EnemyData.MonsterType.WEAK),
                new("Jaw Worm", 1, EnemyData.MonsterType.WEAK),
                new("2 Louse", 1, EnemyData.MonsterType.WEAK),
                new("Small Slimes", 1, EnemyData.MonsterType.WEAK),
                new("Gremlin Gang", 1, EnemyData.MonsterType.STRONG),
                new("Large Slime", 1, EnemyData.MonsterType.STRONG),
                new("Looter", 1, EnemyData.MonsterType.STRONG),
                new("Lots of Slimes", 1, EnemyData.MonsterType.STRONG),
                new("Exordium Thugs", 1, EnemyData.MonsterType.STRONG),
                new("Exordium Wildlife", 1, EnemyData.MonsterType.STRONG),
                new("Red Slaver", 1, EnemyData.MonsterType.STRONG),
                new("3 Louse", 1, EnemyData.MonsterType.STRONG),
                new("2 Fungi Beasts", 1, EnemyData.MonsterType.STRONG),
                new("Gremlin Nob", 1, EnemyData.MonsterType.ELITE),
                new("Lagavulin", 1, EnemyData.MonsterType.ELITE),
                new("3 Sentries", 1, EnemyData.MonsterType.ELITE),
                new("Lagavulin Event", 1, EnemyData.MonsterType.EVENT),
                new("The Mushroom Lair", 1, EnemyData.MonsterType.EVENT),
                new("The Guardian", 1, EnemyData.MonsterType.BOSS),
                new("Hexaghost", 1, EnemyData.MonsterType.BOSS),
                new("Slime Boss", 1, EnemyData.MonsterType.BOSS),
                new("Chosen", 2, EnemyData.MonsterType.WEAK),
                new("Shell Parasite", 2, EnemyData.MonsterType.WEAK),
                new("Spheric Guardian", 2, EnemyData.MonsterType.WEAK),
                new("3 Byrds", 2, EnemyData.MonsterType.WEAK),
                new("2 Thieves", 2, EnemyData.MonsterType.WEAK),
                new("Chosen and Byrds", 2, EnemyData.MonsterType.STRONG),
                new("Sentry and Sphere", 2, EnemyData.MonsterType.STRONG),
                new("Snake Plant", 2, EnemyData.MonsterType.STRONG),
                new("Snecko", 2, EnemyData.MonsterType.STRONG),
                new("Centurion and Healer", 2, EnemyData.MonsterType.STRONG),
                new("Cultist and Chosen", 2, EnemyData.MonsterType.STRONG),
                new("3 Cultists", 2, EnemyData.MonsterType.STRONG),
                new("Shelled Parasite and Fungi", 2, EnemyData.MonsterType.STRONG),
                new("Gremlin Leader", 2, EnemyData.MonsterType.ELITE),
                new("Slavers", 2, EnemyData.MonsterType.ELITE),
                new("Book of Stabbing", 2, EnemyData.MonsterType.ELITE),
                new("Masked Bandits", 2, EnemyData.MonsterType.EVENT),
                new("Colosseum Nobs", 2, EnemyData.MonsterType.EVENT),
                new("Colosseum Slavers", 2, EnemyData.MonsterType.EVENT),
                new("Automaton", 2, EnemyData.MonsterType.BOSS),
                new("Champ", 2, EnemyData.MonsterType.BOSS),
                new("Collector", 2, EnemyData.MonsterType.BOSS),
                new("Orb Walker", 3, EnemyData.MonsterType.WEAK),
                new("3 Darklings", 3, EnemyData.MonsterType.WEAK),
                new("3 Shapes", 3, EnemyData.MonsterType.WEAK),
                new("Transient", 3, EnemyData.MonsterType.STRONG),
                new("4 Shapes", 3, EnemyData.MonsterType.STRONG),
                new("Maw", 3, EnemyData.MonsterType.STRONG),
                new("Jaw Worm Horde", 3, EnemyData.MonsterType.STRONG),
                new("Sphere and 2 Shapes", 3, EnemyData.MonsterType.STRONG),
                new("Spire Growth", 3, EnemyData.MonsterType.STRONG),
                new("Writhing Mass", 3, EnemyData.MonsterType.STRONG),
                new("Giant Head", 3, EnemyData.MonsterType.ELITE),
                new("Nemesis", 3, EnemyData.MonsterType.ELITE),
                new("Reptomancer", 3, EnemyData.MonsterType.ELITE),
                new("Mysterious Sphere", 3, EnemyData.MonsterType.EVENT),
                new("Mind Bloom Boss Battle", 3, EnemyData.MonsterType.EVENT),
                new("2 Orb Walkers", 3, EnemyData.MonsterType.EVENT),
                new("Awakened One", 3, EnemyData.MonsterType.BOSS),
                new("Donu and Deca", 3, EnemyData.MonsterType.BOSS),
                new("Time Eater", 3, EnemyData.MonsterType.BOSS),
                new("Shield and Spear", 4, EnemyData.MonsterType.ELITE),
                new("The Heart", 4, EnemyData.MonsterType.BOSS)
            };

            foreach (EnemyData d in data)
                derp.Add(d.gameDataUploadData());

            // BotDataUploader.uploadDataAsync(BotDataUploader.GameDataType.ENEMY_DATA, EnemyData.gameDataUploadHeader(), derp);
        }
    }

    public class EnemyData
    {
        public string name;
        public int level;
        public MonsterType type;

        public enum MonsterType
        {
            WEAK,
            STRONG,
            ELITE,
            BOSS,
            EVENT
        }

        public EnemyData(string key, int level, MonsterType type)
        {
            this.name = key;
            this.level = level;
            this.type = type;
        }

        public static string gameDataUploadHeader()
        {
            GameDataStringBuilder builder = new GameDataStringBuilder();
            builder.addFieldData("name");
            builder.addFieldData("level");
            builder.addFieldData("type");
            return builder.toString();
        }

        public string gameDataUploadData()
        {
            GameDataStringBuilder builder = new GameDataStringBuilder();
            builder.addFieldData(name);
            builder.addFieldData(level);
            builder.addFieldData(type.ToString());
            return builder.toString();
        }
    }
}