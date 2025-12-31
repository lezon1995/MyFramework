using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero
{
    public record struct OnNextRoomTransition;
    public partial class ADungeon
    {
        public static MapRoomNode nextRoom { get; set; }

        public static MapRoomNode currMapNode { get; protected set; }

        public static List<List<MapRoomNode>> map;
        public static bool leftRoomAvailable;
        public static bool centerRoomAvailable;
        public static bool rightRoomAvailable;
        public static bool firstRoomChosen;
        public static int MAP_HEIGHT = 15;
        public static int MAP_WIDTH = 7;
        public static int MAP_DENSITY = 6;
        public static int _ACT_MAP_HEIGHT = 3;
        public static List<(int x, int y)> path = new();
        public static bool isDungeonBeaten;

        public static bool isFadingIn;
        public static bool isFadingOut;
        public static bool waitingOnFadeOut;
        protected static float fadeTimer;
        public static Color fadeColor;
        public static Color sourceFadeColor;

        public static ARoom getCurrRoom() => currMapNode.room;
        public static MapRoomNode getCurrMapNode() => currMapNode;

        public static void setCurrMapNode(MapRoomNode curNode)
        {
            var souls = room.souls;
            room?.Dispose();
            currMapNode = curNode;

            if (room == null)
            {
                logger.Warn("This player loaded into a room that no longer exists (due to a new map gen?)");
                for (int i = 0; i < 5; i++)
                {
                    var node = map[curNode.y][i];
                    if (node.room != null)
                    {
                        currMapNode = node;
                        room = node.room;
                        nextRoom.room = node.room;
                        break;
                    }
                }
            }
            else
            {
                room.souls = souls;
            }
        }

        public static void nextRoomTransitionStart()
        {
            fadeOut();
            waitingOnFadeOut = true;
            // overlayMenu.proceedButton.hide();
            if (ModHelper.isModEnabled("Terminal"))
                player.decreaseMaxHealth(1);
        }

        protected static void generateMap()
        {
            long startTime = TimeUtility.getNowTimeStampMS();
            List<ARoom> roomList = new();
            map = MapGenerator.generateDungeon(MAP_HEIGHT, MAP_WIDTH, MAP_DENSITY, mapRng);
            int count = 0;
            foreach (var nodes in map)
            {
                foreach (var node in nodes)
                {
                    if (!node.hasEdges())
                        continue;

                    if (node.y == map.Count - 2)
                        continue;

                    count++;
                }
            }

            GenerateRoomTypes(ref roomList, count);
            RoomTypeAssigner.AssignRowAsRoomType<RestRoom>(map[^1]);
            RoomTypeAssigner.AssignRowAsRoomType<MonsterRoom>(map[0]);
            if (Settings.isEndless && player.hasBlight("MimicInfestation"))
                RoomTypeAssigner.AssignRowAsRoomType<MonsterRoomElite>(map[8]);
            else
                RoomTypeAssigner.AssignRowAsRoomType<TreasureRoom>(map[8]);

            RoomTypeAssigner.DistributeRoomsAcrossMap(mapRng, ref map, ref roomList);

            log("Generated the following dungeon map:");
            log(MapGenerator.toString(map, true));
            log("Game Seed: " + Settings.seed);
            log("Map generation time: " + (TimeUtility.getNowTimeStampMS() - startTime) + "ms");
            firstRoomChosen = false;
            fadeIn();
            setEmeraldElite();
            new OnDungeonMapGenerated().Trigger();
        }

        static void GenerateRoomTypes(ref List<ARoom> roomList, int availableRoomCount)
        {
            log("Generating Room Types! There are " + availableRoomCount + " rooms:");

            int shopCount = Mathf.RoundToInt(availableRoomCount * shopRoomChance);
            log(" Shop (" + toPct(shopRoomChance) + "): " + shopCount);

            int restCount = Mathf.RoundToInt(availableRoomCount * restRoomChance);
            log(" Rest (" + toPct(restRoomChance) + "): " + restCount);

            int treasureCount = Mathf.RoundToInt(availableRoomCount * treasureRoomChance);
            log(" Treasure (" + toPct(treasureRoomChance) + "): " + treasureCount);

            int eliteCount;
            if (ModHelper.isModEnabled("Elite Swarm"))
                eliteCount = Mathf.RoundToInt(availableRoomCount * eliteRoomChance * 2.5F);
            else if (ascensionLevel >= 1)
                eliteCount = Mathf.RoundToInt(availableRoomCount * eliteRoomChance * 1.6F);
            else
                eliteCount = Mathf.RoundToInt(availableRoomCount * eliteRoomChance);
            log(" Elite (" + toPct(eliteRoomChance) + "): " + eliteCount);

            int eventCount = Mathf.RoundToInt(availableRoomCount * eventRoomChance);
            log(" Event (" + toPct(eventRoomChance) + "): " + eventCount);

            int monsterCount = availableRoomCount - shopCount - restCount - treasureCount - eliteCount - eventCount;
            log(" Monster (" + toPct(1.0F - shopRoomChance - restRoomChance - treasureRoomChance - eliteRoomChance - eventRoomChance) + "): " + monsterCount);

            int i;
            for (i = 0; i < shopCount; i++)
                roomList.Add(new ShopRoom());

            for (i = 0; i < restCount; i++)
                roomList.Add(new RestRoom());

            for (i = 0; i < eliteCount; i++)
                roomList.Add(new MonsterRoomElite());

            for (i = 0; i < eventCount; i++)
                roomList.Add(new EventRoom());

            return;

            string toPct(float n) => string.Format("{0:F0}", n * 100.0F) + "%";
        }

        protected static void setEmeraldElite()
        {
            if (Settings.isFinalActAvailable && !Settings.hasEmeraldKey)
            {
                List<MapRoomNode> eliteNodes = new();
                foreach (var nodes in map)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (node.room is MonsterRoomElite)
                            eliteNodes.Add(node);
                    }
                }

                var index = mapRng.random(0, eliteNodes.Count - 1);
                MapRoomNode chosenNode = eliteNodes[index];
                chosenNode.hasEmeraldKey = true;
                log("[INFO] Elite nodes identified: " + eliteNodes.Count);
                log("[INFO] Emerald Key  placed in: [" + chosenNode.x + "," + chosenNode.y + "]");
            }
        }

        public void populatePathTaken(SaveFile saveFile)
        {
            MapRoomNode node = null;
            switch (saveFile.current_room)
            {
                case nameof(MonsterRoomBoss):
                    node = new MapRoomNode(-1, MAP_HEIGHT, new MonsterRoomBoss());
                    nextRoom = node;
                    break;
                case nameof(TreasureRoomBoss):
                    node = new MapRoomNode(-1, MAP_HEIGHT, new TreasureRoomBoss());
                    nextRoom = node;
                    break;
                default:
                    if (saveFile.room_y == MAP_HEIGHT && saveFile.room_x == -1)
                    {
                        node = new MapRoomNode(-1, MAP_HEIGHT, new VictoryRoom(VictoryRoom.EventType.HEART));
                        nextRoom = node;
                    }
                    else if (saveFile.current_room == nameof(NeowRoom))
                    {
                        nextRoom = null;
                    }
                    else
                    {
                        nextRoom = map[saveFile.room_y][saveFile.room_x];
                    }

                    break;
            }

            for (int i = 0; i < path.Count; i++)
            {
                var (x, y) = path[i];
                var mapRoomNode = map[y][x];
                if (y == 14)
                {
                    foreach (var edge in mapRoomNode.edges)
                        edge?.markAsTaken();
                }

                if (y < MAP_HEIGHT)
                {
                    mapRoomNode.markAsTaken();
                    if (node != null)
                    {
                        MapEdge connectedEdge = node.getEdgeConnectedTo(mapRoomNode);
                        connectedEdge?.markAsTaken();
                    }

                    node = mapRoomNode;
                }
            }

            currMapNode = new MapRoomNode(0, -1, new EmptyRoom());

            if (isLoadingIntoNeow(saveFile))
            {
                log("Loading into Neow");
                nextRoom = null;
            }
            else
            {
                log("Loading into: " + saveFile.room_x + "," + saveFile.room_y);
            }

            nextRoomTransition(saveFile);
            if (isLoadingIntoNeow(saveFile))
            {
                room = new NeowRoom(saveFile.chose_neow_reward);
            }

            if (room is VictoryRoom && (!Settings.isFinalActAvailable || !Settings.hasRubyKey || !Settings.hasEmeraldKey || !Settings.hasSapphireKey))
                Game.stopClock = true;
        }

        protected bool isLoadingIntoNeow(SaveFile saveFile)
        {
            return floorNum == 0 || saveFile.current_room == nameof(NeowRoom);
        }

        static void firstRoomLogic()
        {
            initializeFirstRoom();
            leftRoomAvailable = currMapNode.leftNodeAvailable();
            centerRoomAvailable = currMapNode.centerNodeAvailable();
            rightRoomAvailable = currMapNode.rightNodeAvailable();
        }

        public static void initializeFirstRoom()
        {
            fadeIn();
            floorNum++;
            if (room is MonsterRoom)
            {
                if (!Game.loadingSave)
                {
                    if (SaveHelper.shouldSave())
                    {
                        SaveHelper.saveIfAppropriate(SaveType.ENTER_ROOM);
                    }
                    else
                    {
                        // Metrics metrics = new Metrics();
                        // metrics.setValues(false, false, null, Metrics.MetricRequestType.NONE);
                        // metrics.gatherAllDataAndSave(false, false, null);
                    }
                }

                floorNum--;
            }
            // scene.nextRoom(room);
        }

        public static void fadeIn()
        {
            if (fadeColor.a != 1.0F)
                log("WARNING: Attempting to fade in even though screen is not black");

            isFadingIn = true;

            if (Settings.FAST_MODE)
                fadeTimer = 0.001F;
            else
                fadeTimer = 0.8F;
        }

        public static void fadeOut()
        {
            if (fadeTimer == 0.0F)
            {
                if (fadeColor.a != 0.0F)
                    log("WARNING: Attempting to fade out even though screen is not transparent");

                isFadingOut = true;

                if (Settings.FAST_MODE)
                    fadeTimer = 0.001F;
                else
                    fadeTimer = 0.8F;
            }
        }

        public void updateFading(float dt)
        {
            if (isFadingIn)
            {
                fadeTimer -= dt;
                fadeColor.a = MMLerp.fade.apply(0.0F, 1.0F, fadeTimer / 0.8F);
                if (fadeTimer < 0.0F)
                {
                    isFadingIn = false;
                    fadeColor.a = 0.0F;
                    fadeTimer = 0.0F;
                }
            }
            else if (isFadingOut)
            {
                fadeTimer -= dt;
                fadeColor.a = MMLerp.fade.apply(1.0F, 0.0F, fadeTimer / 0.8F);
                if (fadeTimer < 0.0F)
                {
                    fadeTimer = 0.0F;
                    isFadingOut = false;
                    fadeColor.a = 1.0F;
                    if (!isDungeonBeaten)
                        nextRoomTransition();
                }
            }
        }


        public static void closeCurrentScreen()
        {
            // PeekButton.isPeeking = false;
            if (previousScreen == screen)
                previousScreen = CurrentScreen.NONE;
            /*switch (screen)
            {
                case MASTER_DECK_VIEW:
                    overlayMenu.cancelButton.hide();
                    genericScreenOverlayReset();
                    for (AbstractCard c : player.masterDeck.group)
                    {
                        c.unhover();
                        c.untip();
                    }
                    break;
                case DISCARD_VIEW:
                    overlayMenu.cancelButton.hide();
                    genericScreenOverlayReset();
                    for (AbstractCard c : player.discardPile.group)
                    {
                        c.drawScale = 0.12F;
                        c.targetDrawScale = 0.12F;
                        c.teleportToDiscardPile();
                        c.darken(true);
                        c.unhover();
                    }
                    break;
                case EVENT:
                    genericScreenOverlayReset();
                    break;
                case null:
                    overlayMenu.cancelButton.hide();
                    genericScreenOverlayReset();
                    break;
                case null:
                    overlayMenu.cancelButton.hide();
                    genericScreenOverlayReset();
                    break;
                case null:
                    overlayMenu.cancelButton.hide();
                    genericScreenOverlayReset();
                    settingsScreen.abandonPopup.hide();
                    settingsScreen.exitPopup.hide();
                    break;
                case null:
                    overlayMenu.cancelButton.hide();
                    genericScreenOverlayReset();
                    settingsScreen.abandonPopup.hide();
                    settingsScreen.exitPopup.hide();
                    break;
                case null:
                    genericScreenOverlayReset();
                    CardCrawlGame.sound.stop("UNLOCK_SCREEN", gUnlockScreen.id);
                    break;
                case COMBAT_REWARD:
                    genericScreenOverlayReset();
                    if (!combatRewardScreen.rewards.isEmpty())
                        previousScreen = CurrentScreen.COMBAT_REWARD;
                    break;
                case CARD_REWARD:
                    overlayMenu.cancelButton.hide();
                    dynamicBanner.hide();
                    genericScreenOverlayReset();
                    if (!screenSwap)
                        cardRewardScreen.onClose();
                    break;
                case null:
                    dynamicBanner.hide();
                    genericScreenOverlayReset();
                    break;
                case null:
                    genericScreenOverlayReset();
                    dynamicBanner.hide();
                    break;
                case null:
                    genericScreenOverlayReset();
                    overlayMenu.showCombatPanels();
                    break;
                case MAP:
                    genericScreenOverlayReset();
                    dungeonMapScreen.close();
                    if (!firstRoomChosen && nextRoom != null && !dungeonMapScreen.dismissable)
                    {
                        firstRoomChosen = true;
                        firstRoomLogic();
                    }
                    break;
                case SHOP:
                    CardCrawlGame.sound.play("SHOP_CLOSE");
                    genericScreenOverlayReset();
                    overlayMenu.cancelButton.hide();
                    break;
                case null:
                    CardCrawlGame.sound.play("ATTACK_MAGIC_SLOW_1");
                    genericScreenOverlayReset();
                    overlayMenu.cancelButton.hide();
                    break;
                default:
                    log("UNSPECIFIED CASE: " + screen.name());
                    break;
            }*/

            if (previousScreen == CurrentScreen.NONE)
            {
                screen = CurrentScreen.NONE;
            }
            // else if (screenSwap)
            // {
            //     screenSwap = false;
            // }
            else
            {
                screen = previousScreen;
                previousScreen = CurrentScreen.NONE;
                if (room.rewardTime)
                    previousScreen = CurrentScreen.COMBAT_REWARD;

                isScreenUp = true;
                // openPreviousScreen(screen);
            }
        }

        public void nextRoomTransition(SaveFile saveFile = null)
        {
            // overlayMenu.proceedButton.setLabel(TEXT[0]);
            // combatRewardScreen.clear();

            if (nextRoom is { room: not null })
                nextRoom.room.rewards.Clear();

            switch (room)
            {
                case MonsterRoomElite when eliteMonsterList.Count > 0:
                    log("Removing elite: " + eliteMonsterList[0] + " from monster list.");
                    eliteMonsterList.RemoveAt(0);
                    break;
                case MonsterRoomElite:
                    Data.generateElites(10);
                    break;
                case MonsterRoom when monsterList.Count > 0:
                    log("Removing monster: " + monsterList[0] + " from monster list.");
                    monsterList.RemoveAt(0);
                    break;
                case MonsterRoom:
                    Data.generateStrongEnemies(12);
                    break;
                // case EventRoom when room.evt is NoteForYourself noteForYourself:
                //     var tmpCard = noteForYourself.saveCard;
                //     if (tmpCard != null)
                //     {
                //         Game.playerPref.putString("NOTE_CARD", tmpCard.cardID);
                //         Game.playerPref.putInteger("NOTE_UPGRADE", tmpCard.timesUpgraded);
                //         Game.playerPref.flush();
                //     }
                //
                //     break;
            }

            if (RestRoom.lastFireSoundId != 0L)
                sound.fadeOut("REST_FIRE_WET", RestRoom.lastFireSoundId);

            // if (player.stance.ID != "Neutral")
            // player.stance.stopIdleSfx();

            // gridSelectScreen.upgradePreviewCard = null;
            previousScreen = default;
            // dynamicBanner.hide();
            new OnNextRoomTransition().Trigger();
            // dungeonMapScreen.closeInstantly();
            closeCurrentScreen();
            // topPanel.unhoverHitboxes();
            fadeIn();
            player.resetControllerValues();
            effectList.Clear();
            // topLevelEffects.removeIf(e->e is not ObtainKeyEffect);
            topLevelEffects.Clear();
            topLevelEffectsQueue.Clear();
            effectsQueue.Clear();
            cardInstanceIdGenerator = 0;
            // dungeonMapScreen.dismissable = true;
            // dungeonMapScreen.map.legend.isLegendHighlighted = false;

            resetPlayer();

            if (!Game.loadingSave)
            {
                incrementFloorBasedMetrics();
                floorNum++;

                if (!TipTracker.tips["INTENT_TIP"] && floorNum == 6)
                    TipTracker.neverShowAgain("INTENT_TIP");

                // StatsScreen.incrementFloorClimbed();
                SaveHelper.saveIfAppropriate(SaveType.ENTER_ROOM);
            }

            if (Settings.seed != null)
            {
                var seed = Settings.seed.Value + floorNum;
                monsterHpRng = new(seed);
                aiRng = new(seed);
                shuffleRng = new(seed);
                cardRandomRng = new(seed);
                miscRng = new(seed);
            }

            var isLoadingPostCombatSave = Game.loadingSave && saveFile is { post_combat: true };
            if (nextRoom != null && !isLoadingPostCombatSave)
            {
                foreach (var relic in player.relics)
                    relic.onEnterRoom(nextRoom.room);
            }

            if (actionManager.actions.Count > 0)
            {
                logger.Warn("[WARNING] Action Manager was NOT clear! Clearing");
                actionManager.clear();
            }

            var isLoadingCompletedEvent = false;
            if (nextRoom != null)
            {
                var roomMetricKey = nextRoom.room.getMapSymbol();
                if (nextRoom.room is EventRoom)
                {
                    var eventRngDuplicate = new Rand(Settings.seed.Value, eventRng.counter);
                    var roomResult = EventHelper.roll(eventRngDuplicate);
                    isLoadingCompletedEvent = isLoadingPostCombatSave && roomResult == EventHelper.RoomResult.EVENT;
                    if (!isLoadingCompletedEvent)
                    {
                        eventRng = eventRngDuplicate;
                        nextRoom.room = generateRoom(roomResult);
                    }

                    roomMetricKey = nextRoom.room.getMapSymbol();
                    if (nextRoom.room is MonsterRoom)
                        nextRoom.room.combatEvent = true;

                    nextRoom.room.setMapSymbol("?");
                    // nextRoom.room.setMapImg(ImageMaster.MAP_NODE_EVENT, ImageMaster.MAP_NODE_EVENT_OUTLINE);
                }

                if (!isLoadingPostCombatSave)
                    metricData.path_per_floor.Add(roomMetricKey);

                setCurrMapNode(nextRoom);
            }

            if (room != null && !isLoadingPostCombatSave)
            {
                foreach (var relic in player.relics)
                    relic.justEnteredRoom(room);
            }

            if (isLoadingCompletedEvent)
            {
                room.completeRoom();
                string eventKey = (string)saveFile.metric_event_choices[^1]["event_name"];
                room.evt = EventHelper.getEvent(eventKey);
            }
            else
            {
                if (isAscensionMode)
                    Game.publisherIntegration?.setRichPresenceDisplayPlaying(floorNum, ascensionLevel, player.getLocalizedCharacterName());
                else
                    Game.publisherIntegration?.setRichPresenceDisplayPlaying(floorNum, player.getLocalizedCharacterName());

                room.onPlayerEntry();
            }

            // if (room is MonsterRoom && lastCombatMetricKey == "Shield and Spear")
            // {
            //     player.movePosition(Settings.WIDTH / 2.0F, floorY);
            // }
            // else
            // {
            //     player.movePosition(Settings.WIDTH * 0.25F, floorY);
            //     player.flipHorizontal = false;
            // }

            if (room is MonsterRoom && !isLoadingPostCombatSave)
                player.preBattlePrep();

            // scene.nextRoom(room);
            // if (room is RestRoom)
            //     rs = RenderScene.CAMPFIRE;
            // else if (room.evt is AbstractImageEvent)
            //     rs = RenderScene.EVENT;
            // else
            //     rs = RenderScene.NORMAL;
        }

        public static void resetPlayer()
        {
            // player.orbs.Clear();
            player.animX = 0.0F;
            player.animY = 0.0F;
            player.hideHealthBar();
            player.hand.clear();
            player.powers.Clear();
            player.drawPile.clear();
            player.discardPile.clear();
            player.exhaustPile.clear();
            player.limbo.clear();
            player.loseBlock(true);
            player.damagedThisCombat = 0;

            // if (player.stance.ID != "Neutral")
            // {
            //     player.stance = new NeutralStance();
            //     player.onStanceChange("Neutral");
            // }

            GameActionManager.turn = 0;
        }

        static void incrementFloorBasedMetrics()
        {
            if (floorNum != 0)
            {
                metricData.current_hp_per_floor.Add(player.currentHealth);
                metricData.max_hp_per_floor.Add(player.maxHealth);
                metricData.gold_per_floor.Add(player.gold);
            }
        }

        static ARoom generateRoom(EventHelper.RoomResult roomType)
        {
            log("Generating Room: " + roomType);
            return roomType switch
            {
                EventHelper.RoomResult.ELITE => new MonsterRoomElite(),
                EventHelper.RoomResult.MONSTER => new MonsterRoom(),
                EventHelper.RoomResult.SHOP => new ShopRoom(),
                EventHelper.RoomResult.TREASURE => new TreasureRoom(),
                EventHelper.RoomResult.EVENT => new EventRoom(),
                _ => new EventRoom()
            };
        }

        public static void reset()
        {
            log("Resetting variables...");
            Game.resetScoreVars();
            ModHelper.setModsFalse();
            floorNum = 0;
            actNum = 0;
            if (currMapNode != null && room != null)
            {
                room.Dispose();
                if (room.monsters != null)
                    foreach (var monster in room.monsters.monsters)
                        monster.dispose();
            }

            currMapNode = null;
            shrineList.Clear();
            relicsToRemoveOnStart.Clear();
            previousScreen = default;
            actionManager.clear();
            actionManager.clearNextRoomCombatActions();
            // combatRewardScreen.clear();
            // cardRewardScreen.reset();
            // dungeonMapScreen?.closeInstantly();
            effectList.Clear();
            effectsQueue.Clear();
            topLevelEffectsQueue.Clear();
            topLevelEffects.Clear();
            // cardBlizzRandomizer = cardBlizzStartOffset;
            player?.relics.Clear();
            // rs = RenderScene.NORMAL;
            // blightPool.clear();
            cardInstanceIdGenerator = 0;
        }
    }
}