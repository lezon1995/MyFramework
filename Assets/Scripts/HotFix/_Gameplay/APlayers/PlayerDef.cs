using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/PlayerDef")]
    public sealed class PlayerDef : ScriptableObject
    {
        public APlayer.PlayerClass Type;
        public string DisplayName = "Player";
        public string DisplayStats = "Test Stats";
        public string DisplayDesc = "Test Desc";

        [Header("Visual")]
        public Sprite Icon;
    }
}
