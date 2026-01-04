using System;
using System.Collections.Generic;

namespace MarbleHero
{
    public class RunData
    {
        public string character_chosen;
        public string loadout;
        public string build_version;
        public string seed_played;
        public bool chose_seed;
        public string timestamp;
        public string local_time;
        public bool victory;
        public bool is_daily;
        public bool is_trial;
        public bool is_endless;
        public bool is_ascension_mode;
        public bool is_special_run;
        public long special_seed;
        public bool isUploaded;
        public int score;
        public int ascension_level;
        public int floor_reached;
        public int gold;
        public int playtime;
        public int purchased_purges;
        public string killed_by;
        public string neow_bonus;
        public string neow_cost;
        public int rested;
        public int rituals;
        public int upgraded;
        public int meditates;
        public List<string> master_deck;
        public List<string> relics;
        public int circlet_count;
        public List<string> path_taken;
        public List<string> path_per_floor;
        public List<int> current_hp_per_floor;
        public List<int> max_hp_per_floor;
        public List<string> items_purchased;
        public List<int> item_purchase_floors;
        public List<string> items_purged;
        public List<int> items_purged_floors;
        public List<int> gold_per_floor;
        public List<string> daily_mods;
        public List<BattleStats> damage_taken;
        public List<EventStats> event_choices;
        public List<CardChoiceStats> card_choices;
        public List<ObtainStats> relics_obtained;
        public List<ObtainStats> potions_obtained;
        public List<BossRelicChoiceStats> boss_relics;
        public List<CampfireChoice> campfire_choices;
        public static Comparison<RunData> orderByTimestampDesc = (o1, o2) => string.Compare(o2.timestamp, o1.timestamp, StringComparison.Ordinal);
    }

    public class ObtainStats
    {
        public string key;
        public int floor;
    }

    public class EventStats
    {
        public string event_name;
        public string player_choice;
        public int floor;
        public List<string> cards_obtained;
        public List<string> cards_removed;
        public List<string> cards_transformed;
        public List<string> cards_upgraded;
        public List<string> relics_obtained;
        public List<string> relics_lost;
        public List<string> potions_obtained;
        public int damage_taken;
        public int damage_healed;
        public int max_hp_loss;
        public int max_hp_gain;
        public int gold_gain;
        public int gold_loss;
    }

    public class CardChoiceStats
    {
        public List<string> not_picked;
        public string picked;
        public int floor;
    }

    public class CampfireChoice
    {
        public int floor;
        public string key;
        public string data;
    }

    public class BossRelicChoiceStats
    {
        public List<string> not_picked;
        public string picked;
    }

    public class BattleStats
    {
        public int floor;
        public string enemies;
        public int damage;
        public int turns;
    }
}