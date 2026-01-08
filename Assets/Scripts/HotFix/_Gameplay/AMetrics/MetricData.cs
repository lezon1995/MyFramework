using System.Collections.Generic;

namespace MarbleHero
{
    public class MetricData
    {
        public int campfire_rested;
        public int campfire_upgraded;
        public int purchased_purges;
        public float win_rate = 0.5F;
        public List<int> potions_floor_spawned = new();
        public List<int> potions_floor_usage = new();
        public List<int> current_hp_per_floor = new();
        public List<int> max_hp_per_floor = new();
        public List<int> gold_per_floor = new();
        public List<string> path_per_floor = new();
        public List<string> path_taken = new();
        public List<string> items_purchased = new();
        public List<int> item_purchase_floors = new();
        public List<string> items_purged = new();
        public List<int> items_purged_floors = new();
        public List<Dictionary<string, object>> card_choices = new();
        public List<Dictionary<string, object>> event_choices = new();
        public List<Dictionary<string, object>> boss_relics = new();
        public List<Dictionary<string, object>> damage_taken = new();
        public List<Dictionary<string, object>> potions_obtained = new();
        public List<Dictionary<string, object>> relics_obtained = new();
        public List<Dictionary<string, object>> campfire_choices = new();
        public string neowBonus = "";
        public string neowCost = "";

        public void clearData()
        {
            campfire_rested = 0;
            campfire_upgraded = 0;
            purchased_purges = 0;
            potions_floor_spawned.Clear();
            potions_floor_usage.Clear();
            current_hp_per_floor.Clear();
            max_hp_per_floor.Clear();
            gold_per_floor.Clear();
            path_per_floor.Clear();
            path_taken.Clear();
            items_purchased.Clear();
            item_purchase_floors.Clear();
            items_purged.Clear();
            items_purged_floors.Clear();
            card_choices.Clear();
            event_choices.Clear();
            damage_taken.Clear();
            potions_obtained.Clear();
            relics_obtained.Clear();
            campfire_choices.Clear();
            boss_relics.Clear();
            neowBonus = "";
            neowCost = "";
        }

        public void addEncounterData()
        {
            Dictionary<string, object> combat = new()
            {
                ["floor"] = ADungeon.floorNum,
                ["enemies"] = ADungeon.lastCombatMetricKey,
                ["damage"] = GameActionManager.damageReceivedThisCombat,
                ["turns"] = GameActionManager.turn.value
            };
            damage_taken.Add(combat);
        }

        // public void addPotionObtainData(AbstractPotion potion)
        // {
        //     Dictionary<string, object> obtainInfo = new()
        //     {
        //         ["key"] = potion.ID,
        //         // ["floor"] = ADungeon.floorNum
        //     };
        //     potions_obtained.Add(obtainInfo);
        // }
        //
        public void addRelicObtainData(ARelic relic)
        {
            Dictionary<string, object> obtainInfo = new()
            {
                ["key"] = relic.relicId,
                ["floor"] = ADungeon.floorNum
            };
            relics_obtained.Add(obtainInfo);
        }

        public void addCampfireChoiceData(string choiceKey)
        {
            addCampfireChoiceData(choiceKey, null);
        }

        public void addCampfireChoiceData(string choiceKey, string data)
        {
            Dictionary<string, object> choice = new()
            {
                ["floor"] = ADungeon.floorNum,
                ["key"] = choiceKey
            };
            if (data != null)
                choice["data"] = data;
            campfire_choices.Add(choice);
        }

        public void addShopPurchaseData(string key)
        {
            if (items_purchased.Count == item_purchase_floors.Count)
                item_purchase_floors.Add(ADungeon.floorNum);
            items_purchased.Add(key);
        }

        public void addPurgedItem(string key)
        {
            if (items_purged.Count == items_purged_floors.Count)
                items_purged_floors.Add(ADungeon.floorNum);

            items_purged.Add(key);
            purchased_purges++;
        }

        public void addNeowData(string bonus, string cost)
        {
            neowBonus = bonus;
            neowCost = cost;
        }
    }
}