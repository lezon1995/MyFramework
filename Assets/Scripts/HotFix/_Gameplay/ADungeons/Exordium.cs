using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero
{
    public class Exordium : ADungeon
    {
        public Exordium(APlayer p, List<string> emptyList) : base(DungeonData.Get("Exordium"), p, emptyList)
        {
            initializeRelicList();
            // if (Settings.isEndless)
            // {
            //     if (floorNum <= 1)
            //     {
            //         blightPool.clear();
            //         blightPool = new();
            //     }
            // }
            // else
            // {
            //     blightPool.clear();
            // }

            // scene?.dispose();
            // scene = new TheBottomScene();
            // scene.randomizeScene();

            fadeColor = new Color32(30, 15, 15, 255);
            sourceFadeColor = new Color32(30, 15, 15, 255);
            initializeSpecialOneTimeEventList();
            Data.initializeLevelSpecificChances();

            if (Settings.seed != null)
                mapRng = new Rand(Settings.seed.Value + actNum);

            generateMap();
            // music.changeBGM(id);
            currMapNode = new MapRoomNode(0, -1);
            if (Settings.isShowBuild || !TipTracker.tips["NEOW_SKIP"])
            {
                room = new EmptyRoom();
            }
            else
            {
                room = new NeowRoom(false);

                if (floorNum > 1)
                    SaveHelper.saveIfAppropriate(SaveType.ENDLESS_NEOW);
                else
                    SaveHelper.saveIfAppropriate(SaveType.ENTER_ROOM);
            }
        }

        public Exordium(APlayer p, SaveFile saveFile) : base(DungeonData.Get("Exordium"), p, saveFile)
        {
            // scene?.dispose();
            // scene = new TheBottomScene();
            fadeColor = new Color32(30, 15, 15, 255);
            sourceFadeColor = new Color32(30, 15, 15, 255);
            // music.changeBGM(id);

            Data.initializeLevelSpecificChances();

            if (Settings.seed != null)
            {
                miscRng = new Rand(Settings.seed.Value + saveFile.floor_num);
                mapRng = new Rand(Settings.seed.Value + saveFile.act_num);
            }

            generateMap();
            firstRoomChosen = true;
            populatePathTaken(saveFile);
            if (isLoadingIntoNeow(saveFile))
                firstRoomChosen = false;
        }

        public override void Initialize(int seed)
        {
            base.Initialize(seed);
        }
    }
}