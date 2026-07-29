using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    public partial class Exordium : ADungeon
    {
        public Exordium(APlayer p, List<string> emptyList) : base("Exordium", "Exordium", p, emptyList)
        {
            screen = CurrentScreen.NONE;
            isScreenUp = false;
            fadeColor = new(30, 15, 15, 255);
            sourceFadeColor = new(30, 15, 15, 255);
        }

        public Exordium(APlayer p, SaveFile saveFile) : base("Exordium", p, saveFile)
        {
            fadeColor = new Color32(30, 15, 15, 255);
            sourceFadeColor = new Color32(30, 15, 15, 255);
        }

        public override void initialize()
        {
            base.initialize();

            initializeRelicList();
            initializeSpecialOneTimeEventList();
            initializeLevelSpecificChances();

            if (Settings.seed != 0)
                mapRng = new Rand(Settings.seed + actNum);

            generateMap();
            music.changeBGM(id);
            prevMapNode = null;
            currMapNode = new(0, -1);
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
            
            room.onPlayerEntry(player);
        }

        public override void initializeByFile(SaveFile saveFile)
        {
            music.changeBGM(id);

            if (Settings.seed != 0)
            {
                miscRng = new Rand(Settings.seed + saveFile.floor_num);
                mapRng = new Rand(Settings.seed + saveFile.act_num);
            }

            initializeLevelSpecificChances();

            generateMap();
            firstRoomChosen = true;
            populatePathTaken(saveFile);
            if (isLoadingIntoNeow(saveFile))
                firstRoomChosen = false;

            base.initializeByFile(saveFile);
        }
    }
}