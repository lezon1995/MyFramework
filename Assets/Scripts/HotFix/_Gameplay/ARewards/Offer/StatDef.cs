using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/StatModDef")]
    public class StatDef : ScriptableObject
    {
        public string statKey;
        public Sprite Icon;
        
        public DisplayConfig DisplayConfig;
    }
}