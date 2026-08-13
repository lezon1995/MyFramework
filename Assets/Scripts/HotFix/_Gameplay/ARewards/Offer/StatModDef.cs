using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/StatModDef")]
    public class StatModDef : ScriptableObject
    {
        public string statKey;
        public Sprite Icon;
    }
}