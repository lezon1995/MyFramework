using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(fileName = "StageTemplate", menuName = "MoreMountains/StageTemplate")]
    public class StageTemplate : ScriptableObject
    {
        public BrickTemplate[] bricks;
    }
}