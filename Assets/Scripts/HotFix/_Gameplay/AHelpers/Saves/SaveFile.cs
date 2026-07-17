using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains
{
    [Serializable]
    public class CardSave
    {
        public int upgrades;
        public int misc;
        public string id;

        public CardSave(string cardID, int timesUpgraded, int _misc)
        {
            id = cardID;
            upgrades = timesUpgraded;
            misc = _misc;
        }
    }

    [Serializable]
    public class RewardSave
    {
        public string type;
        public string id;
        public int amount;
        public int bonusGold;

        public RewardSave(string _type, string _id, int _amount = 0, int _bonusGold = 0)
        {
            type = _type;
            id = _id;
            amount = _amount;
        }
    }

    [Serializable]
    public class PathPoint
    {
        public int x;
        public int y;

        public PathPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public enum SaveType
    {
        ENTER_ROOM,
        POST_NEOW,
        POST_COMBAT,
        AFTER_BOSS_RELIC,
        ENDLESS_NEOW
    }

    [Serializable]
    public class SaveFile
    {
        public string name;
        public string loadout;
        public int current_health;
        public int max_health;
        // public int max_orbs;
        public int gold;
        public int hand_size;
        // public int potion_slots;
        public int red;
        public int green;
        public int blue;
        public List<CardSave> cards;
        public Dictionary<string, int> obtained_cards;
        public List<string> relics;
        public List<int> relic_counters;

        // public List<string> blights;
        // public List<int> blight_counters;

        // public List<string> potions;

        public bool is_ascension_mode;
        public int ascension_level;
        public bool chose_neow_reward;
        public string level_name;
        public long play_time;
        public long save_date;
        public long daily_date;
        public int floor_num;
        public int act_num;

        public long seed;
        public long special_seed;
        public bool seed_set;

        public bool is_trial;
        public bool is_daily;
        public bool is_final_act_on;

        public bool has_ruby_key;
        public bool has_emerald_key;
        public bool has_sapphire_key;

        public List<string> custom_mods;
        public List<string> daily_mods;

        public int monster_seed_count;
        public int event_seed_count;
        public int merchant_seed_count;
        public int card_seed_count;
        public int treasure_seed_count;
        public int relic_seed_count;
        public int potion_seed_count;
        public int monster_hp_seed_count;
        public int ai_seed_count;
        public int shuffle_seed_count;
        public int card_random_seed_count;
        public int card_random_seed_randomizer;

        public int potion_chance;
        public int purgeCost;
        public List<string> monster_list;
        public List<string> elite_monster_list;
        public List<string> boss_list;
        public List<string> event_list;
        public List<string> one_time_event_list;
        public List<float> event_chances;
        public List<Vector2Int> path;
        public int room_x;
        public int room_y;
        public int spirit_count;

        public string boss;
        public string current_room;

        public List<string> common_relics;
        public List<string> uncommon_relics;
        public List<string> rare_relics;
        public List<string> shop_relics;
        public List<string> boss_relics;

        public string bottled_flame;
        public string bottled_lightning;
        public string bottled_tornado;
        public int bottled_flame_upgrade;
        public int bottled_lightning_upgrade;
        public int bottled_tornado_upgrade;
        public int bottled_flame_misc;
        public int bottled_lightning_misc;
        public int bottled_tornado_misc;

        public bool is_endless_mode;
        public List<int> endless_increments;

        public bool post_combat;
        public bool mugged;
        public bool smoked;
        public List<RewardSave> combat_rewards;
        public int monsters_killed;
        public int elites1_killed;
        public int elites2_killed;
        public int elites3_killed;
        public int champions;
        public int perfect;
        public bool overkill;
        public bool combo;
        public bool cheater;
        public int gold_gained;
        public int mystery_machine;

        public int metric_campfire_rested;
        public int metric_campfire_upgraded;
        public int metric_campfire_rituals;
        public int metric_campfire_meditates;
        public int metric_purchased_purges;
        public List<int> metric_potions_floor_spawned;
        public List<int> metric_potions_floor_usage;
        public List<int> metric_current_hp_per_floor;
        public List<int> metric_max_hp_per_floor;
        public List<int> metric_gold_per_floor;
        public List<string> metric_path_per_floor;
        public List<string> metric_path_taken;
        public List<string> metric_items_purchased;
        public List<int> metric_item_purchase_floors;
        public List<string> metric_items_purged;
        public List<int> metric_items_purged_floors;
        public List<Dictionary<string, object>> metric_card_choices;
        public List<Dictionary<string, object>> metric_event_choices;
        public List<Dictionary<string, object>> metric_boss_relics;
        public List<Dictionary<string, object>> metric_damage_taken;
        public List<Dictionary<string, object>> metric_potions_obtained;
        public List<Dictionary<string, object>> metric_relics_obtained;
        public List<Dictionary<string, object>> metric_campfire_choices;
        public string metric_build_version;
        public string metric_seed_played;
        public int metric_floor_reached;
        public long metric_playtime;

        public string neow_bonus;
        public string neow_cost;

        public SaveFile(SaveType type)
        {
            APlayer p = player;
            name = p.name;
            current_health = p.currentHealth;
            max_health = p.maxHealth;
            // max_orbs = p.masterMaxOrbs;
            gold = p.gold;
            // hand_size = p.masterHandSize;
            // red = p.energy.energyMaster;
            green = 0;
            blue = 0;
            monsters_killed = Game.monstersSlain;
            elites1_killed = Game.elites1Slain;
            elites2_killed = Game.elites2Slain;
            elites3_killed = Game.elites3Slain;
            champions = Game.champion;
            perfect = Game.perfect;
            overkill = Game.overkill;
            combo = Game.combo;
            cheater = Game.cheater;
            gold_gained = Game.goldGained;
            mystery_machine = Game.mysteryMachine;
            play_time = (long)Game.playtime;
            // cards = p.masterDeck.getCardDeck();
            obtained_cards = CardHelper.obtainedCards;
            relics = new();
            relic_counters = new();
            foreach (var r in p.relics)
            {
                relics.Add(r.relicId);
                relic_counters.Add(r.counter);
            }

            is_endless_mode = Settings.isEndless;

            // blights = new();
            // blight_counters = new();
            // foreach (AbstractBlight b in p.blights)
            // {
            //     blights.Add(b.blightID);
            //     blight_counters.Add(b.counter);
            // }

            // endless_increments = new();
            // foreach (AbstractBlight b in p.blights)
            // endless_increments.Add(b.increment);

            // potion_slots = player.potionSlots;
            // potions = new();
            // foreach (AbstractPotion pot in player.potions)
            // potions.Add(pot.ID);

            is_ascension_mode = ADungeon.isAscensionMode;
            ascension_level = ADungeon.ascensionLevel;
            chose_neow_reward = false;
            level_name = ADungeon.id;
            floor_num = ADungeon.floorNum;
            act_num = ADungeon.actNum;
            monster_list = ADungeon.monsterList;
            elite_monster_list = ADungeon.eliteMonsterList;
            boss_list = ADungeon.bossList;
            event_list = ADungeon.eventList;
            one_time_event_list = ADungeon.specialOneTimeEventList;
            potion_chance = ARoom.blizzardPotionMod;
            event_chances = type == SaveType.POST_COMBAT ? EventHelper.getChancesPreRoll() : EventHelper.getChances();
            save_date = TimeUtility.getNowTimeStampMS();

            if (Settings.seed != 0)
                seed = Settings.seed;

            if (Settings.specialSeed != 0)
                special_seed = Settings.specialSeed;

            seed_set = Settings.seedSet;
            is_daily = Settings.isDailyRun;
            is_final_act_on = Settings.isFinalActAvailable;
            has_ruby_key = Settings.hasRubyKey;
            has_emerald_key = Settings.hasEmeraldKey;
            has_sapphire_key = Settings.hasSapphireKey;
            daily_date = Settings.dailyDate;
            is_trial = Settings.isTrial;
            daily_mods = ModHelper.getEnabledModIDs();
            if (APlayer.customMods == null)
            {
                // if (Game.trial != null)
                //     APlayer.customMods = Game.trial.dailyModIDs();
                // else
                    APlayer.customMods = new();
            }

            custom_mods = APlayer.customMods;
            boss = ADungeon.bossKey;
            // purgeCost = ShopScreen.purgeCost;
            monster_seed_count = ADungeon.monsterRng.counter;
            event_seed_count = ADungeon.eventRng.counter;
            merchant_seed_count = ADungeon.merchantRng.counter;
            card_seed_count = ADungeon.cardRng.counter;
            // card_random_seed_randomizer = ADungeon.cardBlizzRandomizer;
            treasure_seed_count = ADungeon.treasureRng.counter;
            relic_seed_count = ADungeon.relicRng.counter;
            potion_seed_count = ADungeon.potionRng.counter;
            path = ADungeon.path.Select(t => new Vector2Int(t.x, t.y)).ToList();
            if (ADungeon.nextRoom == null || type == SaveType.ENDLESS_NEOW)
            {
                room_x = mapNode.x;
                room_y = mapNode.y;
                current_room = room.GetType().Name;
            }
            else
            {
                room_x = ADungeon.nextRoom.x;
                room_y = ADungeon.nextRoom.y;
                current_room = ADungeon.nextRoom.room.GetType().Name;
            }

            // spirit_count = ADungeon.bossCount;
            log("Next Room: " + current_room);
            common_relics = ADungeon.commonRelicPool;
            uncommon_relics = ADungeon.uncommonRelicPool;
            rare_relics = ADungeon.rareRelicPool;
            shop_relics = ADungeon.shopRelicPool;
            boss_relics = ADungeon.bossRelicPool;
            post_combat = false;
            mugged = false;
            smoked = false;
            switch (type)
            {
                case SaveType.POST_COMBAT:
                    post_combat = true;
                    mugged = room.mugged;
                    smoked = room.smoked;
                    combat_rewards = new();
                    // foreach (var i in room.rewards)
                    // {
                    //     switch (i.type)
                    //     {
                    //         case RewardType.CARD:
                    //         case RewardType.EMERALD_KEY:
                    //         case RewardType.SAPPHIRE_KEY:
                    //             combat_rewards.Add(new RewardSave(i.type.ToString(), null));
                    //             break;
                    //         case RewardType.GOLD:
                    //             combat_rewards.Add(new RewardSave(i.type.ToString(), null, i.goldAmt, i.bonusGold));
                    //             break;
                    //         // case RewardType.POTION:
                    //         // combat_rewards.Add(new RewardSave(i.type.ToString(), i.potion.ID));
                    //         // break;
                    //         case RewardType.RELIC:
                    //             combat_rewards.Add(new RewardSave(i.type.ToString(), i.relic.relicId));
                    //             break;
                    //         case RewardType.STOLEN_GOLD:
                    //             combat_rewards.Add(new RewardSave(i.type.ToString(), null, i.goldAmt, 0));
                    //             break;
                    //     }
                    // }

                    break;
                case SaveType.POST_NEOW:
                    chose_neow_reward = true;
                    break;
            }

            // if (player.hasRelic("Bottled Flame"))
            // {
            //     if (((BottledFlame)player.getRelic("Bottled Flame")).card != null)
            //         bottled_flame = ((BottledFlame)player.getRelic("Bottled Flame")).card.cardID;
            //     else
            //         bottled_flame = null;
            // }
            // else
            // {
            //     bottled_flame = null;
            // }

            // if (player.hasRelic("Bottled Lightning"))
            // {
            //     if (((BottledLightning)player.getRelic("Bottled Lightning")).card != null)
            //         bottled_lightning = ((BottledLightning)player.getRelic("Bottled Lightning")).card.cardID;
            //     else
            //         bottled_lightning = null;
            // }
            // else
            // {
            //     bottled_lightning = null;
            // }

            // if (player.hasRelic("Bottled Tornado"))
            // {
            //     if (((BottledTornado)player.getRelic("Bottled Tornado")).card != null)
            //         bottled_tornado = ((BottledTornado)player.getRelic("Bottled Tornado")).card.cardID;
            //     else
            //         bottled_tornado = null;
            // }
            // else
            // {
            //     bottled_tornado = null;
            // }

            MetricData metric = metricData;
            metric_campfire_rested = metric.campfire_rested;
            metric_campfire_upgraded = metric.campfire_upgraded;
            metric_purchased_purges = metric.purchased_purges;
            metric_potions_floor_spawned = metric.potions_floor_spawned;
            metric_potions_floor_usage = metric.potions_floor_usage;
            metric_current_hp_per_floor = metric.current_hp_per_floor;
            metric_max_hp_per_floor = metric.max_hp_per_floor;
            metric_gold_per_floor = metric.gold_per_floor;
            metric_path_per_floor = metric.path_per_floor;
            metric_path_taken = metric.path_taken;
            metric_items_purchased = metric.items_purchased;
            metric_item_purchase_floors = metric.item_purchase_floors;
            metric_items_purged = metric.items_purged;
            metric_items_purged_floors = metric.items_purged_floors;
            metric_card_choices = metric.card_choices;
            metric_event_choices = metric.event_choices;
            metric_boss_relics = metric.boss_relics;
            metric_potions_obtained = metric.potions_obtained;
            metric_relics_obtained = metric.relics_obtained;
            metric_campfire_choices = metric.campfire_choices;
            metric_damage_taken = metric.damage_taken;
            metric_build_version = Game.TRUE_VERSION_NUM;
            metric_seed_played = Settings.seed.ToString();
            metric_floor_reached = ADungeon.floorNum;
            metric_playtime = (long)Game.playtime;
            neow_bonus = metric.neowBonus;
            neow_cost = metric.neowCost;
        }
    }
}