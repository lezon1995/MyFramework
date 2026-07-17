namespace MoreMountains
{
    public partial class Buff
    {
        /// <summary>
        /// 检查 GE是否满足被应用的要求
        /// </summary>
        /// <returns></returns>
        public bool CheckTagRequirements()
        {
            return true;
            var requiredTags = BuffType.ApplicationRequiredTags;
            var ignoredTags = BuffType.ApplicationIgnoredTags;
            var hasAllTags = Target.Buffs.HasAllTags(requiredTags);
            var hasNoneTags = Target.Buffs.HasNoneTags(ignoredTags);
            if (hasAllTags && hasNoneTags)
            {
                return true;
            }

            var allTags = string.Join(",", Target.Buffs.GetAllTags());
            var requireTags = string.Join(",", requiredTags);
            var blockedTags = string.Join(",", ignoredTags);
            return false;
        }


        /// <summary>
        /// 检查 GE是否满足持续运行的要求
        /// </summary>
        public bool CheckOngoingTagRequirements(Buffable buffable)
        {
            return true;
            /*var requiredTags = BuffType.OngoingRequiredTags;
            var ignoredTags = BuffType.OngoingIgnoredTags;
            var hasAllTags = asc.GESpecs.HasAllTags(requiredTags);
            var hasNoneTags = asc.GESpecs.HasNoneTags(ignoredTags);
            return hasAllTags && hasNoneTags;*/
        }


        /// <summary>
        /// 移除其他所有拥有GE.RemoveGEWithTags的GE
        /// </summary>
        // public void RemoveOtherGEWithTags()
        // {
        //     var removeGEWithTags = BuffType.RemoveGEWithTags;
        //     if (removeGEWithTags == null || removeGEWithTags.Length == 0)
        //         return;
        //
        //     var asc = GetTarget(BuffType.RemoveGEWithTagsFrom);
        //     for (var i = asc.GESpecs.Count - 1; i >= 0; i--)
        //     {
        //         var spec = asc.GESpecs[i];
        //         var grantedTags = spec.BuffType.GrantedTags;
        //         if (removeGEWithTags.HasAny(grantedTags))
        //         {
        //             asc.RemoveGEPrematurely(i);
        //         }
        //     }
        // }
    }
}