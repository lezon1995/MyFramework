using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace MoreMountains
{
    public class SaveAndContinue
    {
        public static string SAVE_PATH = Application.persistentDataPath + "/" + "saves" + "/";
        static StringBuilder sb = new StringBuilder();

        public static string getPlayerSavePath(string name)
        {
            sb.Length = 0;
            sb.Append(SAVE_PATH);
            if (Game.saveSlot != 0)
                sb.Append(Game.saveSlot).Append("_");
            sb.Append(name).Append(".json");
            return sb.ToString();
        }

        public static bool saveExistsAndNotCorrupted(string name)
        {
            string filepath = getPlayerSavePath(name);
            bool fileExists = File.Exists(filepath);
            if (fileExists)
            {
                try
                {
                    loadSaveFile(filepath);
                }
                catch (Exception)
                {
                    deleteSave(filepath);
                    log(name + " save INVALID!");
                    return false;
                }

                log(name + " save exists and is valid.");
                return true;
            }

            log(name + " save does NOT exist!");
            return false;
        }

        public static string loadSaveString(APlayer.PlayerClass c)
        {
            return loadSaveString(getPlayerSavePath(c.ToString()));
        }

        static string loadSaveString(string filePath)
        {
            string data = File.ReadAllText(filePath);
            if (SaveFileObfuscator.isObfuscated(data))
                return SaveFileObfuscator.decode(data, "key");
            return data;
        }

        public static SaveFile loadSaveFile(APlayer.PlayerClass c)
        {
            string fileName = getPlayerSavePath(c.ToString());
            try
            {
                return loadSaveFile(fileName);
            }
            catch (Exception e)
            {
                log("Exception occurred while loading save!");
                logException(e);
                Application.Quit();
                return null;
            }
        }

        static SaveFile loadSaveFile(string filePath)
        {
            SaveFile saveFile = null;
            string savestr;
            Exception err = null;
            try
            {
                savestr = loadSaveString(filePath);
                saveFile = JsonConvert.DeserializeObject<SaveFile>(savestr);
            }
            catch (Exception e)
            {
                if (File.Exists(filePath))
                    SaveHelper.preserveCorruptFile(filePath);
                err = e;
                if (!filePath.EndsWith(".backUp"))
                {
                    log(filePath + " was corrupt, loading backup...");
                    return loadSaveFile(filePath + ".backUp");
                }
            }

            if (saveFile == null)
                throw new Exception("Unable to load save file: " + filePath, err);

            log(filePath + " save file was successfully loaded.");
            return saveFile;
        }

        public static void save(SaveFile save)
        {
            Game.loadingSave = false;
            Dictionary<string, object> dict = new()
            {
                { "name", save.name },
                { "loadout", save.loadout },
                { "current_health", save.current_health },
                { "max_health", save.max_health },
                // { "max_orbs", save.max_orbs },
                { "gold", save.gold },
                { "hand_size", save.hand_size },
                { "red", save.red },
                { "green", save.green },
                { "blue", save.blue },
                { "monsters_killed", save.monsters_killed },
                { "elites1_killed", save.elites1_killed },
                { "elites2_killed", save.elites2_killed },
                { "elites3_killed", save.elites3_killed },
                { "gold_gained", save.gold_gained },
                { "mystery_machine", save.mystery_machine },
                { "champions", save.champions },
                { "perfect", save.perfect },
                { "overkill", save.overkill },
                { "combo", save.combo },
                { "cards", save.cards },
                { "obtained_cards", save.obtained_cards },
                { "relics", save.relics },
                { "relic_counters", save.relic_counters },
                // { "potions", save.potions },
                // { "potion_slots", save.potion_slots },
                { "is_endless_mode", save.is_endless_mode },
                // { "blights", save.blights },
                // { "blight_counters", save.blight_counters },
                { "endless_increments", save.endless_increments },
                { "chose_neow_reward", save.chose_neow_reward },
                { "neow_bonus", save.neow_bonus },
                { "neow_cost", save.neow_cost },
                { "is_ascension_mode", save.is_ascension_mode },
                { "ascension_level", save.ascension_level },
                { "level_name", save.level_name },
                { "floor_num", save.floor_num },
                { "act_num", save.act_num },
                { "event_list", save.event_list },
                { "one_time_event_list", save.one_time_event_list },
                { "potion_chance", save.potion_chance },
                { "event_chances", save.event_chances },
                { "monster_list", save.monster_list },
                { "elite_monster_list", save.elite_monster_list },
                { "boss_list", save.boss_list },
                { "play_time", save.play_time },
                { "save_date", save.save_date },
                { "seed", save.seed },
                { "special_seed", save.special_seed },
                { "seed_set", save.seed_set },
                { "is_daily", save.is_daily },
                { "is_final_act_on", save.is_final_act_on },
                { "has_ruby_key", save.has_ruby_key },
                { "has_emerald_key", save.has_emerald_key },
                { "has_sapphire_key", save.has_sapphire_key },
                { "daily_date", save.daily_date },
                { "is_trial", save.is_trial },
                { "daily_mods", save.daily_mods },
                { "custom_mods", save.custom_mods },
                { "boss", save.boss },
                { "purgeCost", save.purgeCost },
                { "monster_seed_count", save.monster_seed_count },
                { "event_seed_count", save.event_seed_count },
                { "merchant_seed_count", save.merchant_seed_count },
                { "card_seed_count", save.card_seed_count },
                { "treasure_seed_count", save.treasure_seed_count },
                { "relic_seed_count", save.relic_seed_count },
                { "potion_seed_count", save.potion_seed_count },
                { "ai_seed_count", save.ai_seed_count },
                { "shuffle_seed_count", save.shuffle_seed_count },
                { "card_random_seed_count", save.card_random_seed_count },
                { "card_random_seed_randomizer", save.card_random_seed_randomizer },
                { "path", save.path },
                { "room_x", save.room_x },
                { "room_y", save.room_y },
                { "spirit_count", save.spirit_count },
                { "current_room", save.current_room },
                { "common_relics", save.common_relics },
                { "uncommon_relics", save.uncommon_relics },
                { "rare_relics", save.rare_relics },
                { "shop_relics", save.shop_relics },
                { "boss_relics", save.boss_relics },
                { "post_combat", save.post_combat },
                { "mugged", save.mugged },
                { "smoked", save.smoked },
                { "combat_rewards", save.combat_rewards },
                // if (player.hasRelic("Bottled Flame"))
                //     saveBottle(dict, "Bottled Flame", "bottled_flame", ((BottledFlame)player.getRelic("Bottled Flame")).card);
                // else
                //     dict.Add("bottled_flame", null);
                //
                // if (player.hasRelic("Bottled Lightning"))
                //     saveBottle(dict, "Bottled Lightning", "bottled_lightning", ((BottledLightning)player.getRelic("Bottled Lightning")).card);
                // else
                //     dict.Add("bottled_lightning", null);
                //
                // if (player.hasRelic("Bottled Tornado"))
                //     saveBottle(dict, "Bottled Tornado", "bottled_tornado", ((BottledTornado)player.getRelic("Bottled Tornado")).card);
                // else
                //     dict.Add("bottled_tornado", null);
                { "metric_campfire_rested", save.metric_campfire_rested },
                { "metric_campfire_upgraded", save.metric_campfire_upgraded },
                { "metric_campfire_rituals", save.metric_campfire_rituals },
                { "metric_campfire_meditates", save.metric_campfire_meditates },
                { "metric_purchased_purges", save.metric_purchased_purges },
                { "metric_potions_floor_spawned", save.metric_potions_floor_spawned },
                { "metric_potions_floor_usage", save.metric_potions_floor_usage },
                { "metric_current_hp_per_floor", save.metric_current_hp_per_floor },
                { "metric_max_hp_per_floor", save.metric_max_hp_per_floor },
                { "metric_gold_per_floor", save.metric_gold_per_floor },
                { "metric_path_per_floor", save.metric_path_per_floor },
                { "metric_path_taken", save.metric_path_taken },
                { "metric_items_purchased", save.metric_items_purchased },
                { "metric_item_purchase_floors", save.metric_item_purchase_floors },
                { "metric_items_purged", save.metric_items_purged },
                { "metric_items_purged_floors", save.metric_items_purged_floors },
                { "metric_card_choices", save.metric_card_choices },
                { "metric_event_choices", save.metric_event_choices },
                { "metric_boss_relics", save.metric_boss_relics },
                { "metric_damage_taken", save.metric_damage_taken },
                { "metric_potions_obtained", save.metric_potions_obtained },
                { "metric_relics_obtained", save.metric_relics_obtained },
                { "metric_campfire_choices", save.metric_campfire_choices },
                { "metric_build_version", save.metric_build_version },
                { "metric_seed_played", save.metric_seed_played },
                { "metric_floor_reached", save.metric_floor_reached },
                { "metric_playtime", save.metric_playtime }
            };

            string data = JsonConvert.SerializeObject(dict);
            string filepath = getPlayerSavePath(player.chosenClass.ToString());
            if (Settings.isBeta)
                AsyncSaver.save(filepath + "BETA", data);

            // AsyncSaver.save(filepath, SaveFileObfuscator.encode(data, "key"));
            AsyncSaver.save(filepath, data);
        }

        static void saveBottle(Dictionary<string, object> dict, string bottleId, string save_name, ACard card)
        {
            if (player.hasRelic(bottleId))
            {
                if (card != null)
                {
                    dict.Add(save_name, card.cardID);
                    // dict.Add(save_name + "_upgrade", (card.timesUpgraded));
                    // dict.Add(save_name + "_misc", (card.misc));
                }
                else
                {
                    dict.Add(save_name, null);
                }
            }
            else
            {
                dict.Add(save_name, null);
            }
        }

        public static void deleteSave(string savePath)
        {
            log("DELETING " + savePath + " SAVE");
            File.Delete(savePath);
            File.Delete(savePath + ".backUp");
        }
    }
}