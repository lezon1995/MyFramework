using System.Collections.Generic;

namespace MarbleHero
{
    public partial class ADungeon
    {
        public static string bossKey;
        public static List<string> monsterList = new();
        public static List<string> eliteMonsterList = new();
        public static List<string> bossList = new();

        public static MonsterGroup getMonsters() => room.monsters;

        public MonsterGroup getMonsterForRoomCreation()
        {
            if (monsterList.Count == 0)
                Data.generateStrongEnemies(12);

            log("Monster: " + monsterList[0]);
            lastCombatMetricKey = monsterList[0];
            return MonsterHelper.getEncounter(monsterList[0]);
        }

        public MonsterGroup getEliteMonsterForRoomCreation()
        {
            if (eliteMonsterList.Count == 0)
                Data.generateElites(10);

            log("Elite: " + eliteMonsterList[0]);
            lastCombatMetricKey = eliteMonsterList[0];
            return MonsterHelper.getEncounter(eliteMonsterList[0]);
        }

        public MonsterGroup getBoss()
        {
            lastCombatMetricKey = bossKey;
            // dungeonMapScreen.map.atBoss = true;
            return MonsterHelper.getEncounter(bossKey);
        }

        void setBoss(string key)
        {
            bossKey = key;
            if (DungeonMap.boss != null && DungeonMap.bossOutline != null)
            {
                //DungeonMap.boss.dispose();
                //DungeonMap.bossOutline.dispose();
            }

            switch (key)
            {
                case "The Guardian":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/guardian.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/guardian.png");
                    break;
                case "Hexaghost":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/hexaghost.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/hexaghost.png");
                    break;
                case "Slime Boss":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/slime.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/slime.png");
                    break;
                case "Collector":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/collector.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/collector.png");
                    break;
                case "Automaton":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/automaton.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/automaton.png");
                    break;
                case "Champ":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/champ.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/champ.png");
                    break;
                case "Awakened One":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/awakened.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/awakened.png");
                    break;
                case "Time Eater":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/timeeater.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/timeeater.png");
                    break;
                case "Donu and Deca":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/donu.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/donu.png");
                    break;
                case "The Heart":
                    //DungeonMap.boss = ImageMaster.loadImage("images/ui/map/boss/heart.png");
                    //DungeonMap.bossOutline = ImageMaster.loadImage("images/ui/map/bossOutline/heart.png");
                    break;
                default:
                    log("WARNING: UNKNOWN BOSS ICON: " + key);
                    //DungeonMap.boss = null;
                    break;
            }

            log("[BOSS] " + key);
        }

        public static void populateFirstStrongEnemy(List<MonsterInfo> monsters, List<string> exclusions)
        {
            while (true)
            {
                string m = MonsterInfo.roll(monsters, monsterRng.random());
                if (!exclusions.Contains(m))
                {
                    monsterList.Add(m);
                    return;
                }
            }
        }

        public static void populateMonsterList(List<MonsterInfo> monsters, int numMonsters)
        {
            for (int i = 0; i < numMonsters; i++)
            {
                if (monsterList.Count == 0)
                {
                    monsterList.Add(MonsterInfo.roll(monsters, monsterRng.random()));
                }
                else
                {
                    string toAdd = MonsterInfo.roll(monsters, monsterRng.random());
                    if (toAdd != monsterList[^1])
                    {
                        if (monsterList.Count > 1 && toAdd == monsterList[^2])
                            i--;
                        else
                            monsterList.Add(toAdd);
                    }
                    else
                    {
                        i--;
                    }
                }
            }
        }

        public static void populateEliteMonsterList(List<MonsterInfo> monsters, int numMonsters)
        {
            for (int i = 0; i < numMonsters; i++)
            {
                if (eliteMonsterList.Count == 0)
                {
                    eliteMonsterList.Add(MonsterInfo.roll(monsters, monsterRng.random()));
                }
                else
                {
                    string toAdd = MonsterInfo.roll(monsters, monsterRng.random());
                    if (toAdd != eliteMonsterList[^1])
                        eliteMonsterList.Add(toAdd);
                    else
                        i--;
                }
            }
        }
    }
}