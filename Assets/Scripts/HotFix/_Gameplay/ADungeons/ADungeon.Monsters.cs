using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    public partial class ADungeon
    {
        public static string bossKey;
        public static List<string> monsterList = new();
        public static List<string> eliteMonsterList = new();
        public static List<string> bossList = new();
        
        protected abstract void generateMonsters();
        protected abstract void generateWeakEnemies(int paramInt);
        protected abstract void generateStrongEnemies(int paramInt);
        protected abstract void generateElites(int paramInt);
        protected abstract void initializeBoss();
        
        protected virtual void loadVolumeManager()
        {
            var manager = Object.FindFirstObjectByType<VolumeManager>();
            if (manager)
            {
                volumeManager = manager;
                return;
            }
            string path = $"{GAMEPLAY_PATH}/Characters/VolumeManager.prefab";
            var res = resource.loadGameResource<VolumeManager>(path);
            volumeManager = Object.Instantiate(res.getResource());
        }

        protected virtual void loadGridManager()
        {
            var manager = Object.FindFirstObjectByType<GridManager>();
            if (manager)
            {
                gridManager = manager;
                return;
            }
            string path = $"{GAMEPLAY_PATH}/Grids/GridManager.prefab";
            var res = resource.loadGameResource<GridManager>(path);
            gridManager = Object.Instantiate(res.getResource());
        }

        public static MonsterGroup getMonsters() => room.monsters;

        public MonsterGroup getMonsterForRoomCreation()
        {
            if (monsterList.Count == 0)
                generateStrongEnemies(12);

            log("Monster: " + monsterList[0]);
            lastCombatMetricKey = monsterList[0];
            return MonsterHelper.getEncounter(monsterList[0]);
        }

        public MonsterGroup getEliteMonsterForRoomCreation()
        {
            if (eliteMonsterList.Count == 0)
                generateElites(10);

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
                string monster = MonsterInfo.roll(monsters, monsterRng.random());
                if (!exclusions.Contains(monster))
                {
                    monsterList.Add(monster);
                    return;
                }
            }
        }

        public static void populateMonsterList(List<MonsterInfo> monsters, int numMonsters)
        {
            for (int i = 0; i < numMonsters; i++)
            {
                var monster = MonsterInfo.roll(monsters, monsterRng.random());
                if (monsterList.Count == 0)
                {
                    monsterList.Add(monster);
                }
                else
                {
                    if (monster != monsterList[^1])
                    {
                        if (monsterList.Count > 1 && monster == monsterList[^2])
                            i--;
                        else
                            monsterList.Add(monster);
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
                var monster = MonsterInfo.roll(monsters, monsterRng.random());
                if (eliteMonsterList.Count == 0)
                {
                    eliteMonsterList.Add(monster);
                }
                else
                {
                    if (monster != eliteMonsterList[^1])
                        eliteMonsterList.Add(monster);
                    else
                        i--;
                }
            }
        }
    }
}