using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 自定义Tooltip内容生成器接口
    /// </summary>
    public class TooltipContentGenerator : MonoBehaviour
    {
        public virtual TooltipContent GenerateContent(TooltipTrigger trigger)
        {
            return null;
        }
    }
}