using System.Collections.Generic;

namespace MoreMountains;

public partial class Exordium
{
    protected override void generateMonsters()
    {
        generateWeakEnemies(3);
        generateStrongEnemies(12);
        generateElites(10);
    }

    protected override void generateWeakEnemies(int count)
    {
        List<MonsterInfo> monsters = new();
        monsters.add(new("Cultist", 2.0F));
        monsters.add(new("Jaw Worm", 2.0F));
        monsters.add(new("2 Louse", 2.0F));
        monsters.add(new("Small Slimes", 2.0F));
        MonsterInfo.normalizeWeights(monsters);
        populateMonsterList(monsters, count);
    }

    protected override void generateStrongEnemies(int count)
    {
        List<MonsterInfo> monsters = new();
        monsters.add(new("Blue Slaver", 2.0F));
        monsters.add(new("Gremlin Gang", 1.0F));
        monsters.add(new("Looter", 2.0F));
        monsters.add(new("Large Slime", 2.0F));
        monsters.add(new("Lots of Slimes", 1.0F));
        monsters.add(new("Exordium Thugs", 1.5F));
        monsters.add(new("Exordium Wildlife", 1.5F));
        monsters.add(new("Red Slaver", 1.0F));
        monsters.add(new("3 Louse", 2.0F));
        monsters.add(new("2 Fungi Beasts", 2.0F));
        
        MonsterInfo.normalizeWeights(monsters);
        // populateFirstStrongEnemy(monsters, generateExclusions());
        populateMonsterList(monsters, count);
    }

    protected override void generateElites(int count)
    {
        List<MonsterInfo> monsters = new();
        monsters.add(new("Gremlin Nob", 1.0F));
        monsters.add(new("Lagavulin", 1.0F));
        monsters.add(new("3 Sentries", 1.0F));
        MonsterInfo.normalizeWeights(monsters);
        populateEliteMonsterList(monsters, count);
    }

    protected override void initializeBoss()
    {
    }
}