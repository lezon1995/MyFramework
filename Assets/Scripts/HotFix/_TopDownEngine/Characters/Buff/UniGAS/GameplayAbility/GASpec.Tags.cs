// namespace MoreMountains.TopDownEngine
// {
//     public partial class GASpec
//     {
//         /// <summary>
//         /// 检查Tag要求是否允许激活这种能力
//         /// </summary>
//         /// <returns></returns>
//         public virtual bool CheckTags()
//         {
//             return true;
//         }
//
//         /// <summary>
//         /// 取消带有Ability.ActivationOwnedTags的能力
//         /// </summary>
//         private void CancelAbilitiesWithTags()
//         {
//             var tags = GA.CancelAbilitiesWithTags;
//             foreach (var spec in Owner.GASpecs)
//             {
//                 if (spec.IsActive)
//                 {
//                     var grantedAbilityTags = spec.GA.ActivationOwnedTags;
//                     if (tags.HasAny(grantedAbilityTags))
//                     {
//                         spec.CancelAbility();
//                     }
//                 }
//             }
//         }
//
//
//         /// <summary>
//         /// Checks if an Ability System Character has all the listed tags
//         /// </summary>
//         /// <param name="asc">Ability System Character</param>
//         /// <param name="tags">List of tags to check</param>
//         /// <returns>True, if the Ability System Character has all tags</returns>
//         protected virtual bool HasAllTags(ASC asc, string[] tags)
//         {
//             return asc.GESpecs.HasAllTags(tags);
//         }
//
//         /// <summary>
//         /// Checks if an Ability System Character has none of the listed tags
//         /// </summary>
//         /// <param name="asc">Ability System Character</param>
//         /// <param name="tags">List of tags to check</param>
//         /// <returns>True, if the Ability System Character has none of the tags</returns>
//         protected virtual bool HasNoTags(ASC asc, string[] tags)
//         {
//             return asc.GESpecs.HasNoneTags(tags);
//         }
//     }
// }