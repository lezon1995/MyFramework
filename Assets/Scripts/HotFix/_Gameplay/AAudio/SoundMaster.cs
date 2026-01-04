using System.Collections.Generic;

namespace MarbleHero
{
    public class SoundMaster
    {
        Dictionary<string, Sfx> map = new();
        List<SoundInfo> fadeOutList = new();
        const string SFX_DIR = "Audio/Sound/";

        public SoundMaster()
        {
            long startTime = TimeUtility.getNowTimeStampMS();
            Settings.SOUND_VOLUME = Settings.soundPref.getFloat("Sound Volume", 0.5F);
            map.Add("AMBIANCE_BOTTOM", load("SOTE_Level1_Ambience_v6"));
            map.Add("AMBIANCE_CITY", load("SOTE_SFX_CityAmb_v1"));
            map.Add("AMBIANCE_BEYOND", load("STS_SFX_BeyondAmb_v1"));
            map.Add("SCENE_TORCH_EXTINGUISH", load("STS_SFX_BGTorchExtinguish_v1"));
            map.Add("APPEAR", load("SOTE_SFX_Appear_v2"));
            map.Add("ATTACK_DAGGER_1", load("STS_SFX_DaggerThrow_1"));
            map.Add("ATTACK_DAGGER_2", load("STS_SFX_DaggerThrow_2"));
            map.Add("ATTACK_DAGGER_3", load("STS_SFX_DaggerThrow_3"));
            map.Add("ATTACK_DAGGER_4", load("STS_SFX_DaggerThrow_2_1"));
            map.Add("ATTACK_DAGGER_5", load("STS_SFX_DaggerThrow_2_2"));
            map.Add("ATTACK_DAGGER_6", load("STS_SFX_DaggerThrow_2_3"));
            map.Add("ATTACK_DEFECT_BEAM", load("STS_SFX_DefectBeam_v1"));
            map.Add("ATTACK_FAST", load("SOTE_SFX_FastAtk_v2"));
            map.Add("ATTACK_FIRE", load("SOTE_SFX_FireIgnite_2_v1"));
            map.Add("ATTACK_FLAME_BARRIER", load("STS_SFX_FlameBarrier_v2"));
            map.Add("ATTACK_HEAVY", load("SOTE_SFX_HeavyAtk_v2"));
            map.Add("ATTACK_IRON_1", load("SOTE_SFX_IronClad_Atk_RR1_v2"));
            map.Add("ATTACK_IRON_2", load("SOTE_SFX_IronClad_Atk_RR2_v2"));
            map.Add("ATTACK_IRON_3", load("SOTE_SFX_IronClad_Atk_RR3_v2"));
            map.Add("ATTACK_MAGIC_BEAM", load("SOTE_SFX_SlowMagic_Beam_v1"));
            map.Add("ATTACK_MAGIC_BEAM_SHORT", load("SOTE_SFX_SlowMagic_BeamShort_v1"));
            map.Add("ATTACK_MAGIC_FAST_1", load("SOTE_SFX_MagicFast_1_v1"));
            map.Add("ATTACK_MAGIC_FAST_2", load("SOTE_SFX_MagicFast_2_v1"));
            map.Add("ATTACK_MAGIC_FAST_3", load("SOTE_SFX_MagicFast_3_v1"));
            map.Add("ATTACK_MAGIC_SLOW_1", load("SOTE_SFX_SlowMagic_1_v1"));
            map.Add("ATTACK_MAGIC_SLOW_2", load("SOTE_SFX_SlowMagic_2_v1"));
            map.Add("ATTACK_PIERCING_WAIL", load("STS_SFX_PiercingWail_v2"));
            map.Add("ATTACK_POISON", load("SOTE_SFX_PoisonCard_1_v1"));
            map.Add("ATTACK_POISON2", load("SOTE_SFX_PoisonCard_2_v1"));
            map.Add("ATTACK_WHIFF_1", load("SOTE_SFX_SlowThrow_1_v1"));
            map.Add("ATTACK_WHIFF_2", load("SOTE_SFX_SlowThrow_2_v1"));
            map.Add("ATTACK_WHIRLWIND", load("STS_SFX_Whirlwind_v2"));
            map.Add("ATTACK_BOWLING", load("bowling"));
            map.Add("CARD_DRAW_8", load("STS_SFX_CardDeal8_v1"));
            map.Add("KEY_OBTAIN", load("SOTE_SFX_Key_v2"));
            map.Add("AUTOMATON_ORB_SPAWN", load("STS_SFX_AutomatonOrbSpawn_v1"));
            map.Add("BATTLE_START_BOSS", load("STS_SFX_BattleStart_Boss_v1"));
            map.Add("BATTLE_START_1", load("STS_SFX_BattleStart_1_v1"));
            map.Add("BATTLE_START_2", load("STS_SFX_BattleStart_2_v1"));
            map.Add("BELL", load("SOTE_SFX_Bell_v1"));
            map.Add("BLOCK_ATTACK", load("SOTE_SFX_BlockAtk_v2"));
            map.Add("BLOCK_BREAK", load("SOTE_SFX_DefenseBreak_v2"));
            map.Add("BLOCK_GAIN_1", load("SOTE_SFX_GainDefense_RR1_v3"));
            map.Add("BLOCK_GAIN_2", load("SOTE_SFX_GainDefense_RR3_v3"));
            map.Add("BLOCK_GAIN_3", load("SOTE_SFX_GainDefense_RR2_v3"));
            map.Add("BLOOD_SPLAT", load("SOTE_SFX_Blood_2_v2"));
            map.Add("BLOOD_SWISH", load("SOTE_SFX_Blood_1_v2"));
            map.Add("BLUNT_FAST", load("SOTE_SFX_FastBlunt_v2"));
            map.Add("BLUNT_HEAVY", load("SOTE_SFX_HeavyBlunt_v2"));
            map.Add("BOSS_VICTORY_STINGER", load("STS_BossVictoryStinger_1_v3_SFX"));
            map.Add("BUFF_1", load("SOTE_SFX_Buff_1_v1"));
            map.Add("BUFF_2", load("SOTE_SFX_Buff_2_v1"));
            map.Add("BUFF_3", load("SOTE_SFX_Buff_3_v1"));
            map.Add("BYRD_DEATH", load("STS_SFX_ByrdDefeat_v2"));
            map.Add("CARD_BURN", load("STS_SFX_BurnCard_v1"));
            map.Add("CARD_EXHAUST", load("SOTE_SFX_ExhaustCard"));
            map.Add("CARD_OBTAIN", load("SOTE_SFX_ObtainCard_v2"));
            map.Add("CARD_REJECT", load("SOTE_SFX_CardReject_v1"));
            map.Add("CARD_SELECT", load("SOTE_SFX_CardSelect_v2"));
            map.Add("CARD_UPGRADE", load("SOTE_SFX_UpgradeCard_v1"));
            map.Add("CEILING_BOOM_1", load("SOTE_SFX_CeilingDust1_Boom_v1"));
            map.Add("CEILING_BOOM_2", load("SOTE_SFX_CeilingDust2_Boom_v1"));
            map.Add("CEILING_BOOM_3", load("SOTE_SFX_CeilingDust3_Boom_v1"));
            map.Add("CEILING_DUST_1", load("SOTE_SFX_CeilingDust1_v1"));
            map.Add("CEILING_DUST_2", load("SOTE_SFX_CeilingDust2_v1"));
            map.Add("CEILING_DUST_3", load("SOTE_SFX_CeilingDust3_v1"));
            map.Add("CHEST_OPEN", load("SOTE_SFX_ChestOpen_v2"));
            map.Add("CHOSEN_DEATH", load("STS_SFX_ChosenDefeat_v2"));
            map.Add("DARKLING_REGROW_1", load("STS_SFX_DarklingRegrow_v2"));
            map.Add("DARKLING_REGROW_2", load("STS_SFX_DarklingRegrow_2_v2"));
            map.Add("DEATH_STINGER", load("STS_DeathStinger_v4_SFX"));
            map.Add("DEBUFF_1", load("SOTE_SFX_Debuff_1_v1"));
            map.Add("DEBUFF_2", load("SOTE_SFX_Debuff_2_v1"));
            map.Add("DEBUFF_3", load("SOTE_SFX_Debuff_3_v1"));
            map.Add("DECK_CLOSE", load("SOTE_SFX_UI_Parchment_2_v1"));
            map.Add("DECK_OPEN", load("SOTE_SFX_UI_Parchment_3_v1"));
            map.Add("DUNGEON_TRANSITION", load("SOTE_SFX_DungeonGate"));
            map.Add("END_TURN", load("SOTE_SFX_EndTurn_v2"));
            map.Add("ENEMY_TURN", load("SOTE_SFX_EnemyTurn_v3"));
            map.Add("EVENT_PURCHASE", load("SOTE_SFX_EventPurchase"));
            map.Add("EVENT_ANCIENT", load("STS_SFX_AncientWriting_v1"));
            map.Add("EVENT_FALLING", load("STS_SFX_Falling_v1"));
            map.Add("EVENT_FORGE", load("STS_SFX_OminousForge_v1"));
            map.Add("EVENT_FORGOTTEN", load("STS_SFX_ForgottenShrine_v1"));
            map.Add("EVENT_FOUNTAIN", load("STS_SFX_CursedTome_v1"));
            map.Add("EVENT_GHOSTS", load("STS_SFX_CouncilGhosts-Mausoleum_v1"));
            map.Add("EVENT_GOLDEN", load("STS_SFX_GoldenIdolBoulder_v1"));
            map.Add("EVENT_GOOP", load("STS_SFX_WorldOfGoop_v1"));
            map.Add("EVENT_LAB", load("STS_SFX_Lab_v1"));
            map.Add("EVENT_LIVING_WALL", load("STS_SFX_LivingWall_v1"));
            map.Add("EVENT_NLOTH", load("STS_SFX_NLoth_v1"));
            map.Add("EVENT_OOZE", load("STS_SFX_ScrapOoze_v1"));
            map.Add("EVENT_PORTAL", load("STS_SFX_SecretPortal_v1"));
            map.Add("EVENT_SENSORY", load("STS_SFX_SensoryStone_v1"));
            map.Add("EVENT_SERPENT", load("STS_SFX_Ssserpent_v1"));
            map.Add("EVENT_SHINING", load("STS_SFX_ShiningLight_v1"));
            map.Add("EVENT_SKULL", load("STS_SFX_KnowingSkull_v1"));
            map.Add("EVENT_SPIRITS", load("STS_SFX_BonfireSpirits_v1"));
            map.Add("EVENT_TOME", load("STS_SFX_CursedTome_v1"));
            map.Add("EVENT_WINDING", load("STS_SFX_WindingHalls_v1"));
            map.Add("EVENT_VAMP_BITE", load("STS_SFX_VampireBite_v2"));
            map.Add("GHOST_FLAMES", load("SOTE_SFX_GhostGuardianFlames_v1"));
            map.Add("GHOST_ORB_IGNITE_1", load("SOTE_SFX_BossOrbIgnite1_v2"));
            map.Add("GHOST_ORB_IGNITE_2", load("SOTE_SFX_BossOrbIgnite2_v2"));
            map.Add("GOLD_GAIN", load("SOTE_SFX_Gold_RR1_v3"));
            map.Add("GOLD_GAIN_2", load("SOTE_SFX_Gold_RR2_v3"));
            map.Add("GOLD_GAIN_3", load("SOTE_SFX_Gold_RR3_v3"));
            map.Add("GOLD_GAIN_4", load("SOTE_SFX_Gold_RR4_v3"));
            map.Add("GOLD_GAIN_5", load("SOTE_SFX_Gold_RR5_v3"));
            map.Add("GOLD_JINGLE", load("SOTE_SFX_Gold_v1"));
            map.Add("GUARDIAN_ROLL_UP", load("SOTE_SFX_BossBallTransform_v1"));
            map.Add("HEAL_1", load("SOTE_SFX_HealShort_1_v2"));
            map.Add("HEAL_2", load("SOTE_SFX_HealShort_2_v2"));
            map.Add("HEAL_3", load("SOTE_SFX_HealShort_3_v2"));
            map.Add("HEART_BEAT", load("SLS_SFX_HeartBeat_Resonant_v1"));
            map.Add("HEART_SIMPLE", load("SLS_SFX_HeartBeat_Simple_v1"));
            map.Add("HOVER_CHARACTER", load("SOTE_SFX_UI_Parchment_3_v1"));
            map.Add("INTIMIDATE", load("SOTE_SFX_IntimidateCard_v1"));
            map.Add("MAP_CLOSE", load("SOTE_SFX_UI_Parchment_1_v2"));
            map.Add("MAP_HOVER_1", load("SOTE_SFX_MapHover_1_v1"));
            map.Add("MAP_HOVER_2", load("SOTE_SFX_MapHover_2_v1"));
            map.Add("MAP_HOVER_3", load("SOTE_SFX_MapHover_3_v1"));
            map.Add("MAP_HOVER_4", load("SOTE_SFX_MapHover_4_v1"));
            map.Add("MAP_OPEN", load("SOTE_SFX_Map_1_v3"));
            map.Add("MAP_OPEN_2", load("SOTE_SFX_Map_2_v3"));
            map.Add("MAP_SELECT_1", load("SOTE_SFX_MapSelect_1_v1"));
            map.Add("MAP_SELECT_2", load("SOTE_SFX_MapSelect_2_v1"));
            map.Add("MAP_SELECT_3", load("SOTE_SFX_MapSelect_3_v1"));
            map.Add("MAP_SELECT_4", load("SOTE_SFX_MapSelect_4_v1"));
            map.Add("MAW_DEATH", load("STS_SFX_MawDefeat_v2"));
            map.Add("NECRONOMICON", load("SOTE_SFX_NecroLaugh_v2"));
            map.Add("NULLIFY_SFX", load("STS_SFX_Nullify_v1"));
            map.Add("POTION_1", load("SOTE_SFX_Potion_1_v2"));
            map.Add("POTION_2", load("SOTE_SFX_Potion_2_v2"));
            map.Add("POTION_3", load("SOTE_SFX_Potion_3_v2"));
            map.Add("POTION_DROP_1", load("SOTE_SFX_DropPotion_1_v1"));
            map.Add("POTION_DROP_2", load("SOTE_SFX_DropPotion_2_v1"));
            map.Add("JAW_WORM_DEATH", load("STS_SFX_JawWormDefeat_v2"));
            map.Add("MONSTER_AUTOMATON_SUMMON", load("STS_SFX_BronzeAutomatonSummon_v2"));
            map.Add("MONSTER_AWAKENED_ATTACK", load("STS_SFX_AwakenedOne3Atk_v1"));
            map.Add("MONSTER_AWAKENED_POUNCE", load("STS_SFX_AwakenedOnePounce_v2"));
            map.Add("MONSTER_BYRD_ATTACK_0", load("STS_SFX_ByrdAtk1_v2"));
            map.Add("MONSTER_BYRD_ATTACK_1", load("STS_SFX_ByrdAtk2_v2"));
            map.Add("MONSTER_BYRD_ATTACK_2", load("STS_SFX_ByrdAtk3_v2"));
            map.Add("MONSTER_BYRD_ATTACK_3", load("STS_SFX_ByrdAtk4_v2"));
            map.Add("MONSTER_BYRD_ATTACK_4", load("STS_SFX_ByrdAtk5_v2"));
            map.Add("MONSTER_BYRD_ATTACK_5", load("STS_SFX_ByrdAtk6_v2"));
            map.Add("MONSTER_CHAMP_CHARGE", load("STS_SFX_ChampChargeUp_v2"));
            map.Add("MONSTER_CHAMP_SLAP", load("STS_SFX_ChampSlap_v2"));
            map.Add("MONSTER_COLLECTOR_DEBUFF", load("STS_SFX_CollectorDebuff_v2"));
            map.Add("MONSTER_COLLECTOR_SUMMON", load("STS_SFX_CollectorSummon_v2"));
            map.Add("MONSTER_DONU_DEFENSE", load("STS_SFX_DonuDecaDefense_v2"));
            map.Add("MONSTER_GUARDIAN_DESTROY", load("STS_SFX_Guardian3Destroy_v2"));
            map.Add("MONSTER_JAW_WORM_BELLOW", load("STS_SFX_JawWormBellow_v1"));
            map.Add("MONSTER_SLIME_ATTACK", load("STS_SFX_SlimedAtk_v2"));
            map.Add("MONSTER_BOOK_STAB_0", load("STS_SFX_BookofStabbing1_v1"));
            map.Add("MONSTER_BOOK_STAB_1", load("STS_SFX_BookofStabbing2_v1"));
            map.Add("MONSTER_BOOK_STAB_2", load("STS_SFX_BookofStabbing3_v1"));
            map.Add("MONSTER_BOOK_STAB_3", load("STS_SFX_BookofStabbing4_v1"));
            map.Add("MONSTER_SNECKO_GLARE", load("STS_SFX_SneckoGlareWave_v1"));
            map.Add("POWER_CONFUSION", load("STS_SFX_Confused_v2"));
            map.Add("POWER_CONSTRICTED", load("STS_SFX_Constrict_v2"));
            map.Add("POWER_DEXTERITY", load("STS_SFX_Dexterity_v2"));
            map.Add("POWER_ENTANGLED", load("STS_SFX_Entangle_v2"));
            map.Add("POWER_FLIGHT", load("STS_SFX_Flight_v2"));
            map.Add("POWER_FOCUS", load("STS_SFX_Focus_v2"));
            map.Add("POWER_INTANGIBLE", load("STS_SFX_Intangible_v1"));
            map.Add("POWER_METALLICIZE", load("STS_SFX_Metallicize_v2"));
            map.Add("POWER_PLATED", load("STS_SFX_PlateArmor_v2"));
            map.Add("POWER_POISON", load("STS_SFX_PoisonApply_v1"));
            map.Add("POWER_SHACKLE", load("STS_SFX_Shackled_v1"));
            map.Add("POWER_STRENGTH", load("STS_SFX_Strength_v1"));
            map.Add("POWER_TIME_WARP", load("STS_SFX_TimeWarp_v2"));
            map.Add("RAGE", load("SOTE_SFX_RageCard_v1"));
            map.Add("RELIC_DROP_CLINK", load("SOTE_SFX_DropRelic_Clink"));
            map.Add("RELIC_DROP_FLAT", load("SOTE_SFX_DropRelic_Flat"));
            map.Add("RELIC_DROP_HEAVY", load("SOTE_SFX_DropRelic_Heavy"));
            map.Add("RELIC_DROP_MAGICAL", load("SOTE_SFX_DropRelic_Magical"));
            map.Add("RELIC_DROP_ROCKY", load("SOTE_SFX_DropRelic_Rocky"));
            map.Add("REST_FIRE_DRY", load("SOTE_SFX_RestFireDry_v2"));
            map.Add("REST_FIRE_WET", load("SOTE_SFX_RestFireWet_v2"));
            map.Add("SHOP_CLOSE", load("SOTE_SFX_ShopRugClose_v1"));
            map.Add("SHOP_OPEN", load("SOTE_SFX_ShopRugOpen_v1"));
            map.Add("SHOP_PURCHASE", load("SOTE_SFX_CashRegister"));
            map.Add("SHOVEL", load("sts_sfx_shovel_v1"));
            map.Add("SINGING_BOWL", load("SOTE_SFX_Relic_PrayerBowl_Soft"));
            map.Add("SLEEP_1-1", load("STS_SleepJingle_1a_NewMix_v1"));
            map.Add("SLEEP_1-2", load("STS_SleepJingle_1b_NewMix_v1"));
            map.Add("SLEEP_1-3", load("STS_SleepJingle_1c_NewMix_v1"));
            map.Add("SLEEP_2-1", load("STS_SleepJingle_2a_NewMix_v1"));
            map.Add("SLEEP_2-2", load("STS_SleepJingle_2b_NewMix_v1"));
            map.Add("SLEEP_2-3", load("STS_SleepJingle_2c_NewMix_v1"));
            map.Add("SLEEP_3-1", load("STS_SleepJingle_3a_NewMix_v1"));
            map.Add("SLEEP_3-2", load("STS_SleepJingle_3b_NewMix_v1"));
            map.Add("SLEEP_3-3", load("STS_SleepJingle_3c_NewMix_v1"));
            map.Add("SLEEP_BLANKET", load("SOTE_SFX_SleepBlanket_v1"));
            map.Add("SLIME_ATTACK", load("SOTE_SFX_SlimeAtk_1_v1"));
            map.Add("SLIME_ATTACK_2", load("SOTE_SFX_SlimeAtk_2_v1"));
            map.Add("SLIME_BLINK_1", load("SOTE_SFX_SlimeBlink_1_v2"));
            map.Add("SLIME_BLINK_2", load("SOTE_SFX_SlimeBlink_2_v1"));
            map.Add("SLIME_BLINK_3", load("SOTE_SFX_SlimeBlink_3_v1"));
            map.Add("SLIME_BLINK_4", load("SOTE_SFX_SlimeBlink_4_v1"));
            map.Add("SLIME_SPLIT", load("SOTE_SFX_SlimeSplit_v1"));
            map.Add("SNECKO_DEATH", load("STS_SFX_SerpentSneckoDefeat_v2"));
            map.Add("SPHERE_DETECT_VO_1", load("STS_SFX_GuardianOutsiderDetected_1_v1"));
            map.Add("SPHERE_DETECT_VO_2", load("STS_SFX_GuardianOutsiderDetected_2_v1"));
            map.Add("SPLASH", load("SOTE_Logo_Echoing_ShortTail"));
            map.Add("SPORE_CLOUD_RELEASE", load("STS_SFX_SporeCloud"));
            map.Add("STAB_BOOK_DEATH", load("STS_SFX_BookOfStabbingDefeat_v2"));
            map.Add("THUNDERCLAP", load("SOTE_SFX_ThunderclapCard_v1"));
            map.Add("TINGSHA", load("SOTE_SFX_Relic_Tingsha"));
            map.Add("DAMARU", load("damaru"));
            map.Add("TURN_EFFECT", load("SOTE_SFX_PlayerTurn_v4_1"));
            map.Add("UI_CLICK_1", load("SOTE_SFX_UIClick_1_v2"));
            map.Add("UI_CLICK_2", load("SOTE_SFX_UIClick_2_v2"));
            map.Add("UI_HOVER", load("SOTE_SFX_UIHover_v2"));
            map.Add("UNLOCK_SCREEN", load("STS_UnlockScreen_v1"));
            map.Add("UNLOCK_WHIR", load("STS_XPBar_Classic_v1"));
            map.Add("UNLOCK_PING", load("STS_NewUnlock_v1"));
            map.Add("VICTORY", load("SOTE_SFX_Victory_v1"));
            map.Add("WHEEL", load("SOTE_SFX_Wheel_v2"));
            map.Add("WIND", load("SOTE_SFX_WindAmb_v1"));
            map.Add("VO_AWAKENEDONE_1", load("vo/STS_VO_AwakenedOne_1_v2"));
            map.Add("VO_AWAKENEDONE_2", load("vo/STS_VO_AwakenedOne_2_v2"));
            map.Add("VO_AWAKENEDONE_3", load("vo/STS_VO_AwakenedOne_3_v2"));
            map.Add("VO_CULTIST_1A", load("vo/STS_VO_CrowCultist_1a"));
            map.Add("VO_CULTIST_1B", load("vo/STS_VO_CrowCultist_1b"));
            map.Add("VO_CULTIST_1C", load("vo/STS_VO_CrowCultist_1c"));
            map.Add("VO_CULTIST_2A", load("vo/STS_VO_CrowCultist_2a"));
            map.Add("VO_CULTIST_2B", load("vo/STS_VO_CrowCultist_2b"));
            map.Add("VO_CULTIST_2C", load("vo/STS_VO_CrowCultist_2c"));
            map.Add("VO_FLAMEBRUISER_1", load("vo/STS_VO_FlameBruiser_1_v3"));
            map.Add("VO_FLAMEBRUISER_2", load("vo/STS_VO_FlameBruiser_2_v3"));
            map.Add("VO_GIANTHEAD_1A", load("vo/STS_VO_GiantHead_1a"));
            map.Add("VO_GIANTHEAD_1B", load("vo/STS_VO_GiantHead_1b"));
            map.Add("VO_GIANTHEAD_1C", load("vo/STS_VO_GiantHead_1c"));
            map.Add("VO_GIANTHEAD_2A", load("vo/STS_VO_GiantHead_2a"));
            map.Add("VO_GIANTHEAD_2B", load("vo/STS_VO_GiantHead_2b"));
            map.Add("VO_GIANTHEAD_2C", load("vo/STS_VO_GiantHead_2c"));
            map.Add("VO_GREMLINANGRY_1A", load("vo/STS_VO_GremlinAngry_1a"));
            map.Add("VO_GREMLINANGRY_1B", load("vo/STS_VO_GremlinAngry_1b"));
            map.Add("VO_GREMLINANGRY_1C", load("vo/STS_VO_GremlinAngry_1c"));
            map.Add("VO_GREMLINANGRY_2A", load("vo/STS_VO_GremlinAngry_2a"));
            map.Add("VO_GREMLINANGRY_2B", load("vo/STS_VO_GremlinAngry_2b"));
            map.Add("VO_GREMLINCALM_1A", load("vo/STS_VO_GremlinCalm_1a"));
            map.Add("VO_GREMLINCALM_1B", load("vo/STS_VO_GremlinCalm_1b"));
            map.Add("VO_GREMLINCALM_2A", load("vo/STS_VO_GremlinCalm_2a"));
            map.Add("VO_GREMLINCALM_2B", load("vo/STS_VO_GremlinCalm_2b"));
            map.Add("VO_GREMLINDOPEY_1A", load("vo/STS_VO_GremlinDopey_1a"));
            map.Add("VO_GREMLINDOPEY_1B", load("vo/STS_VO_GremlinDopey_1b"));
            map.Add("VO_GREMLINDOPEY_2A", load("vo/STS_VO_GremlinDopey_2a"));
            map.Add("VO_GREMLINDOPEY_2B", load("vo/STS_VO_GremlinDopey_2b"));
            map.Add("VO_GREMLINDOPEY_2C", load("vo/STS_VO_GremlinDopey_2c"));
            map.Add("VO_GREMLINFAT_1A", load("vo/STS_VO_GremlinFat_1a"));
            map.Add("VO_GREMLINFAT_1B", load("vo/STS_VO_GremlinFat_1b"));
            map.Add("VO_GREMLINFAT_1C", load("vo/STS_VO_GremlinFat_1c"));
            map.Add("VO_GREMLINFAT_2A", load("vo/STS_VO_GremlinFat_2a"));
            map.Add("VO_GREMLINFAT_2B", load("vo/STS_VO_GremlinFat_2b"));
            map.Add("VO_GREMLINFAT_2C", load("vo/STS_VO_GremlinFat_2c"));
            map.Add("VO_GREMLINNOB_1A", load("vo/STS_VO_GremlinNob_1a_v3"));
            map.Add("VO_GREMLINNOB_1B", load("vo/STS_VO_GremlinNob_1b_v3"));
            map.Add("VO_GREMLINNOB_1C", load("vo/STS_VO_GremlinNob_1d2b_v3"));
            map.Add("VO_GREMLINNOB_2A", load("vo/STS_VO_GremlinNob_2a_v3"));
            map.Add("VO_GREMLINSPAZZY_1A", load("vo/STS_VO_GremlinSpazzy_1a"));
            map.Add("VO_GREMLINSPAZZY_1B", load("vo/STS_VO_GremlinSpazzy_1b"));
            map.Add("VO_GREMLINSPAZZY_2A", load("vo/STS_VO_GremlinSpazzy_2a"));
            map.Add("VO_GREMLINSPAZZY_2B", load("vo/STS_VO_GremlinSpazzy_2b"));
            map.Add("VO_GREMLINSPAZZY_2C", load("vo/STS_VO_GremlinSpazzy_2c"));
            map.Add("VO_HEALER_1A", load("vo/STS_VO_Healer_1a"));
            map.Add("VO_HEALER_1B", load("vo/STS_VO_Healer_1b"));
            map.Add("VO_HEALER_2A", load("vo/STS_VO_Healer_2a"));
            map.Add("VO_HEALER_2B", load("vo/STS_VO_Healer_2b"));
            map.Add("VO_HEALER_2C", load("vo/STS_VO_Healer_2c"));
            map.Add("VO_IRONCLAD_1A", load("vo/STS_VO_Ironclad_1a"));
            map.Add("VO_IRONCLAD_1B", load("vo/STS_VO_Ironclad_1b"));
            map.Add("VO_IRONCLAD_1C", load("vo/STS_VO_Ironclad_1c"));
            map.Add("VO_IRONCLAD_2A", load("vo/STS_VO_Ironclad_2a"));
            map.Add("VO_IRONCLAD_2B", load("vo/STS_VO_Ironclad_2b"));
            map.Add("VO_IRONCLAD_2C", load("vo/STS_VO_Ironclad_2c"));
            map.Add("VO_LOOTER_1A", load("vo/STS_VO_Looter_1a"));
            map.Add("VO_LOOTER_1B", load("vo/STS_VO_Looter_1b"));
            map.Add("VO_LOOTER_1C", load("vo/STS_VO_Looter_1c"));
            map.Add("VO_LOOTER_2A", load("vo/STS_VO_Looter_2a"));
            map.Add("VO_LOOTER_2B", load("vo/STS_VO_Looter_2b"));
            map.Add("VO_LOOTER_2C", load("vo/STS_VO_Looter_2c"));
            map.Add("VO_MERCENARY_1A", load("vo/STS_VO_Mercenary_1a"));
            map.Add("VO_MERCENARY_1B", load("vo/STS_VO_Mercenary_1b"));
            map.Add("VO_MERCENARY_2A", load("vo/STS_VO_Mercenary_2a"));
            map.Add("VO_MERCENARY_3A", load("vo/STS_VO_Mercenary_3a"));
            map.Add("VO_MERCENARY_3B", load("vo/STS_VO_Mercenary_3b"));
            map.Add("VO_MERCHANT_2A", load("vo/STS_VO_Merchant_2a"));
            map.Add("VO_MERCHANT_2B", load("vo/STS_VO_Merchant_2b"));
            map.Add("VO_MERCHANT_2C", load("vo/STS_VO_Merchant_2c"));
            map.Add("VO_MERCHANT_3A", load("vo/STS_VO_Merchant_3a"));
            map.Add("VO_MERCHANT_3B", load("vo/STS_VO_Merchant_3b"));
            map.Add("VO_MERCHANT_3C", load("vo/STS_VO_Merchant_3c"));
            map.Add("VO_MERCHANT_KA", load("vo/STS_VO_Merchant_Kekeke_a"));
            map.Add("VO_MERCHANT_KB", load("vo/STS_VO_Merchant_Kekeke_b"));
            map.Add("VO_MERCHANT_KC", load("vo/STS_VO_Merchant_Kekeke_c"));
            map.Add("VO_MERCHANT_MA", load("vo/STS_VO_Merchant_Mlyah_a"));
            map.Add("VO_MERCHANT_MB", load("vo/STS_VO_Merchant_Mlyah_b"));
            map.Add("VO_MERCHANT_MC", load("vo/STS_VO_Merchant_Mlyah_c"));
            map.Add("VO_MUGGER_1A", load("vo/STS_VO_Mugger_1a"));
            map.Add("VO_MUGGER_1B", load("vo/STS_VO_Mugger_1b"));
            map.Add("VO_MUGGER_2A", load("vo/STS_VO_Mugger_2a"));
            map.Add("VO_MUGGER_2B", load("vo/STS_VO_Mugger_2b"));
            map.Add("VO_NEMESIS_1A", load("vo/STS_VO_Nemesis_1a"));
            map.Add("VO_NEMESIS_1B", load("vo/STS_VO_Nemesis_1b"));
            map.Add("VO_NEMESIS_1C", load("vo/STS_VO_Nemesis_1c"));
            map.Add("VO_NEMESIS_2A", load("vo/STS_VO_Nemesis_2a"));
            map.Add("VO_NEMESIS_2B", load("vo/STS_VO_Nemesis_2b"));
            map.Add("VO_NEOW_1A", load("vo/STS_VO_Neow_1a"));
            map.Add("VO_NEOW_1B", load("vo/STS_VO_Neow_1b"));
            map.Add("VO_NEOW_2A", load("vo/STS_VO_Neow_2a"));
            map.Add("VO_NEOW_2B", load("vo/STS_VO_Neow_2b"));
            map.Add("VO_NEOW_3A", load("vo/STS_VO_Neow_3a"));
            map.Add("VO_NEOW_3B", load("vo/STS_VO_Neow_3b"));
            map.Add("VO_SILENT_1A", load("vo/STS_VO_Silent_1a"));
            map.Add("VO_SILENT_1B", load("vo/STS_VO_Silent_1b"));
            map.Add("VO_SILENT_2A", load("vo/STS_VO_Silent_2a"));
            map.Add("VO_SILENT_2B", load("vo/STS_VO_Silent_2b"));
            map.Add("VO_SLAVERBLUE_1A", load("vo/STS_VO_SlaverBlue_1a"));
            map.Add("VO_SLAVERBLUE_1B", load("vo/STS_VO_SlaverBlue_1b"));
            map.Add("VO_SLAVERBLUE_2A", load("vo/STS_VO_SlaverBlue_2a"));
            map.Add("VO_SLAVERBLUE_2B", load("vo/STS_VO_SlaverBlue_2b"));
            map.Add("VO_SLAVERLEADER_1A", load("vo/STS_VO_SlaverLeader_1a"));
            map.Add("VO_SLAVERLEADER_1B", load("vo/STS_VO_SlaverLeader_1b"));
            map.Add("VO_SLAVERLEADER_2A", load("vo/STS_VO_SlaverLeader_2a"));
            map.Add("VO_SLAVERLEADER_2B", load("vo/STS_VO_SlaverLeader_2b"));
            map.Add("VO_SLAVERRED_1A", load("vo/STS_VO_SlaverRed_1a"));
            map.Add("VO_SLAVERRED_1B", load("vo/STS_VO_SlaverRed_1b"));
            map.Add("VO_SLAVERRED_2A", load("vo/STS_VO_SlaverRed_2a"));
            map.Add("VO_SLAVERRED_2B", load("vo/STS_VO_SlaverRed_2b"));
            map.Add("VO_SLIMEBOSS_1A", load("vo/STS_VO_SlimeBoss_1a"));
            map.Add("VO_SLIMEBOSS_1B", load("vo/STS_VO_SlimeBoss_1b"));
            map.Add("VO_SLIMEBOSS_1C", load("vo/STS_VO_SlimeBoss_1c"));
            map.Add("VO_SLIMEBOSS_2A", load("vo/STS_VO_SlimeBoss_2a"));
            map.Add("VO_TANK_1A", load("vo/STS_VO_Centurion_1_v2"));
            map.Add("VO_TANK_1B", load("vo/STS_VO_Centurion_2_v2"));
            map.Add("VO_TANK_1C", load("vo/STS_VO_Centurion_3_v2"));
            map.Add("VO_CHAMP_1A", load("vo/STS_VO_TheChamp_1"));
            map.Add("VO_CHAMP_2A", load("vo/STS_VO_TheChamp_2a"));
            map.Add("VO_CHAMP_3A", load("vo/STS_VO_TheChamp_3a"));
            map.Add("VO_CHAMP_3B", load("vo/STS_VO_TheChamp_3b"));
            map.Add("ORB_DARK_CHANNEL", load("orb/STS_SFX_DarkOrb_Channel_v1"));
            map.Add("ORB_DARK_EVOKE", load("orb/STS_SFX_DarkOrb_Evoke_v1"));
            map.Add("ORB_FROST_CHANNEL", load("orb/STS_SFX_FrostOrb_Channel_v1"));
            map.Add("ORB_FROST_DEFEND_1", load("orb/STS_SFX_FrostOrb_GainDefense_1_v1"));
            map.Add("ORB_FROST_DEFEND_2", load("orb/STS_SFX_FrostOrb_GainDefense_2_v1"));
            map.Add("ORB_FROST_DEFEND_3", load("orb/STS_SFX_FrostOrb_GainDefense_3_v1"));
            map.Add("ORB_FROST_EVOKE", load("orb/STS_SFX_FrostOrb_Evoke_v1"));
            map.Add("ORB_LIGHTNING_CHANNEL", load("orb/STS_SFX_LightningOrb_Channel_v1"));
            map.Add("ORB_LIGHTNING_EVOKE", load("orb/STS_SFX_LightningOrb_Evoke_v1"));
            map.Add("ORB_LIGHTNING_PASSIVE", load("orb/STS_SFX_LightningOrb_Passive_v1"));
            map.Add("ORB_PLASMA_CHANNEL", load("orb/STS_SFX_PlasmaOrb_Channel_v1"));
            map.Add("ORB_PLASMA_EVOKE", load("orb/STS_SFX_PlasmaOrb_Evoke_v1"));
            map.Add("ORB_SLOT_GAIN", load("orb/STS_SFX_GainSlot_v1"));
            map.Add("WATCHER_HEART_PUNCH", load("SOTE_SFX_BossGhostFireAtk_3_v1"));
            map.Add("STANCE_ENTER_CALM", load("watcher/STS_SFX_Watcher-Calm_v2"));
            map.Add("STANCE_ENTER_WRATH", load("watcher/STS_SFX_Watcher-Wrath_v2"));
            map.Add("STANCE_ENTER_DIVINITY", load("watcher/STS_SFX_Watcher-Divinity_v3"));
            map.Add("STANCE_LOOP_CALM", load("watcher/STS_SFX_Watcher-CalmLoop_v2"));
            map.Add("STANCE_LOOP_WRATH", load("watcher/STS_SFX_Watcher-WrathLoop_v2"));
            map.Add("STANCE_LOOP_DIVINITY", load("watcher/STS_SFX_Watcher-DivinityLoop_v2"));
            map.Add("SELECT_WATCHER", load("watcher/STS_SFX_Watcher-Select_v2"));
            map.Add("POWER_MANTRA", load("watcher/STS_SFX_Watcher-Mantra_v3"));
            map.Add("CARD_POWER_WOOSH", load("STS_SFX_PowerWoosh_v1"));
            map.Add("CARD_POWER_IMPACT", load("STS_SFX_Power_v1"));
            log("Sound Effect Volume: " + Settings.SOUND_VOLUME);
            log("Loaded " + map.Count + " Sound Effects");
            log("SFX load time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
        }

        Sfx load(string filename) => load(filename, false);

        Sfx load(string filename, bool preload)
        {
            return new Sfx(SFX_DIR + filename, preload);
        }

        public void update(float dt)
        {
            for (var i = fadeOutList.Count - 1; i >= 0; i--)
            {
                var e = fadeOutList[i];
                e.update(dt);
                Sfx sfx = map[e.name];
                if (sfx != null)
                {
                    if (e.isDone)
                    {
                        sfx.stop(e.id);
                        fadeOutList.RemoveAt(i);
                        continue;
                    }

                    sfx.setVolume(e.id, Settings.SOUND_VOLUME * Settings.MASTER_VOLUME * e.volumeMultiplier);
                }
            }
        }

        public void preload(string key)
        {
            if (map.ContainsKey(key))
            {
                log("Preloading: " + key);
                int id = map[key].play(0.0F);
                map[key].stop(id);
            }
            else
            {
                log("Missing: " + key);
            }
        }

        public int play(string key, bool useBgmVolume)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
            {
                if (useBgmVolume)
                    return map[key].play(Settings.MUSIC_VOLUME * Settings.MASTER_VOLUME);

                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME);
            }

            log("Missing: " + key);
            return 0;
        }

        public int play(string key)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            return play(key, false);
        }

        public int play(string key, float pitchVariation)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME, 1.0F + MathUtils.random(-pitchVariation, pitchVariation), 0.0F);

            log("Missing: " + key);
            return 0;
        }

        public int playA(string key, float pitchAdjust)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME, 1.0F + pitchAdjust, 0.0F);

            log("Missing: " + key);
            return 0;
        }

        public int playV(string key, float volumeMod)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME * volumeMod, 1.0F, 0.0F);

            log("Missing: " + key);
            return 0;
        }

        public int playAV(string key, float pitchAdjust, float volumeMod)
        {
            if (Game.MUTE_IF_BG && Settings.isBackgrounded)
                return 0;

            if (map.ContainsKey(key))
                return map[key].play(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME * volumeMod, 1.0F + pitchAdjust, 0.0F);

            log("Missing: " + key);
            return 0;
        }

        public int playAndLoop(string key)
        {
            if (map.ContainsKey(key))
                return map[key].loop(Settings.SOUND_VOLUME * Settings.MASTER_VOLUME);

            log("Missing: " + key);
            return 0;
        }

        public int playAndLoop(string key, float volume)
        {
            if (map.ContainsKey(key))
                return map[key].loop(volume);

            log("Missing: " + key);
            return 0;
        }

        public void adjustVolume(string key, int id, float volume)
        {
            map[key].setVolume(id, volume);
        }

        public void adjustVolume(string key, int id)
        {
            map[key].setVolume(id, Settings.SOUND_VOLUME * Settings.MASTER_VOLUME);
        }

        public void fadeOut(string key, int id)
        {
            fadeOutList.Add(new SoundInfo(key, id));
        }

        public void stop(string key, int id)
        {
            map[key].stop(id);
        }

        public void stop(string key)
        {
            if (map[key] != null)
                map[key].stop();
        }
    }
}