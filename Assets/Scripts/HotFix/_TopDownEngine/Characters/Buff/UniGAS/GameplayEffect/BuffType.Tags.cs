using UnityEngine;

namespace MoreMountains
{
    public partial class Buff
    {
        /// <summary>
        /// Tags that the GameplayEffect has.
        /// They do not do any function on their own and serve only the purpose of describing the GameplayEffect.
        /// </summary>
        [HideInInspector]
        public string Tag;

        /// <summary>
        /// Tags that live on the GameplayEffect but are also given to the ASC that the GameplayEffect is applied to.
        /// They are removed from the ASC when the GameplayEffect is removed. This only works for Duration and Infinite GameplayEffects.
        /// </summary>
        [HideInInspector]
        public string[] GrantedTags;

        /// <summary>
        /// 从{RemoveFrom}上移除带有Tags的GEs
        /// </summary>
        [HideInInspector]
        public Actors RemoveGEWithTagsFrom;

        /// <summary>
        /// GameplayEffects on the Target that have any of these tags in their Asset Tags or
        /// Granted Tags will be removed from the Target when this GameplayEffect is successfully applied.
        ///
        /// 当该GE成功Apply到该ASC上的时候，该ASC上所有拥有RemoveGEWithTags的GE将会被移除掉
        /// </summary>
        [HideInInspector]
        public string[] RemoveGEWithTags;

        /// <summary>
        /// Once applied, these tags determine whether the GameplayEffect is on or off.
        /// A GameplayEffect can be off and still be applied.
        /// If a GameplayEffect is off due to failing the Ongoing Tag Requirements,
        /// but the requirements are then met, the GameplayEffect will turn on again and reapply its modifiers.
        /// This only works for Duration and Infinite GameplayEffects.
        ///
        /// 当该GE成功Apply到该ASC上的时候，
        /// 该ASC上的GrantedTags如果满足OngoingTagRequirements，则代表该GE为On状态 
        /// 该ASC上的GrantedTags如果不满足OngoingTagRequirements，则代表该GE为Off状态
        /// Off->On时 apply其modifiers
        /// On->Off时 dis apply其modifiers
        /// 只适用于Duration或Infinite的GE
        /// </summary>
        [HideInInspector]
        public string[] OngoingRequiredTags;

        [HideInInspector]
        public string[] OngoingIgnoredTags;

        /// <summary>
        /// Tags on the Target that determine if a GameplayEffect can be applied to the Target.
        /// If these requirements are not met, the GameplayEffect is not applied.
        ///
        /// 当尝试Apply该GE到该ASC上的时候，如果该ASC的GrantedTags满足ApplicationTagRequirements，则会Apply成功，否则将会Apply失败
        /// </summary>
        [HideInInspector]
        public string[] ApplicationRequiredTags;

        [HideInInspector]
        public string[] ApplicationIgnoredTags;
    }
}