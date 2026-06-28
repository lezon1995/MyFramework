using UnityEngine;

namespace MarbleHero
{
    [CreateAssetMenu(fileName = "StageTemplate", menuName = "MarbleHero/StageTemplate")]
    public class StageTemplate : ScriptableObject
    {
        public BrickTemplate[] bricks;
    }
}