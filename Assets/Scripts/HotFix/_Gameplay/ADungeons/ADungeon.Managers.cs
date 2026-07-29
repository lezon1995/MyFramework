using UnityEngine;

namespace MoreMountains
{
    public partial class ADungeon
    {
        protected void initializeManagers()
        {
            loadCharacterManager();
            loadVolumeManager();
            loadGridManager();
            loadBallManager();
            loadRelicManager();
        }
        
        protected virtual void loadCharacterManager()
        {
            var manager = Object.FindFirstObjectByType<CharacterManager>();
            if (manager)
            {
                characterManager = manager;
                return;
            }
            string path = $"{GAMEPLAY_PATH}/Characters/CharacterManager.prefab";
            var res = resource.loadGameResource<CharacterManager>(path);
            characterManager = Object.Instantiate(res.getResource());
        }
        
        
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

        protected virtual void loadBallManager()
        {
            var manager = Object.FindFirstObjectByType<BallManager>();
            if (manager)
            {
                ballManager = manager;
                return;
            }
            string path = $"{GAMEPLAY_PATH}/Balls/BallManager.prefab";
            var res = resource.loadGameResource<BallManager>(path);
            ballManager = Object.Instantiate(res.getResource());
        }

        protected virtual void loadRelicManager()
        {
            var manager = Object.FindFirstObjectByType<RelicManager>();
            if (manager)
            {
                relicManager = manager;
                return;
            }
            string path = $"{GAMEPLAY_PATH}/Relics/RelicManager.prefab";
            var res = resource.loadGameResource<RelicManager>(path);
            relicManager = Object.Instantiate(res.getResource());
        }

    }
}