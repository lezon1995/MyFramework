// namespace MoreMountains
// {
//     public partial class GA
//     {
//         public string AbilityTag;
//
//         /// <summary>
//         /// Other GameplayAbilities that have these GameplayTags in their Ability Tags will be canceled when this GameplayAbility is activated.
//         /// 
//         /// 当该Ability激活的时候，该ASC上的GrantedAbilityTags所有满足CancelAbilitiesWithTags条件的Ability将会被cancel
//         /// </summary>
//         public string[] CancelAbilitiesWithTags;
//
//         /// <summary>
//         /// Other GameplayAbilities that have these GameplayTags in their Ability Tags are blocked from activating while this GameplayAbility is active.
//         /// 
//         /// 当该Ability处于Active状态时，该ASC上其他所有满足BlockAbilitiesWithTags条件的Ability将会被阻止激活
//         /// </summary>
//         public string[] BlockAbilitiesWithTags;
//
//         /// <summary>
//         /// These GameplayTags are given to the GameplayAbility's owner while this GameplayAbility is active. Remember these are not replicated.
//         /// 
//         /// 当该Ability处于Active状态时，ActivationOwnedTags会被添加 To该ASC的GrantedTags里面
//         /// 当该Ability处于非Active状态时，ActivationOwnedTags会被移除 From该ASC的GrantedTags里面
//         /// </summary>
//         public string[] ActivationOwnedTags;
//
//         /// <summary>
//         /// This GameplayAbility can only be activated if the owner has all of TagContainer.RequiredTags.
//         /// This GameplayAbility cannot be activated if the owner has any of TagContainer.BlockedTags.
//         /// 
//         /// 当该ASC的GrantedTags满足ActivationRequiredTags条件时 才能被激活
//         /// </summary>
//         public string[] ActivationRequiredTags;
//
//         public string[] ActivationIgnoredTags;
//
//         /// <summary>
//         /// This GameplayAbility can only be activated if the Source has all of these GameplayTags.
//         /// The Source GameplayTags are only set if the GameplayAbility is triggered by an event.
//         /// 
//         /// 当Source的ASC的GrantedTags满足ActivationRequiredTags条件时 才能被激活
//         /// </summary>
//         public string[] SourceRequiredTags;
//
//         public string[] SourceIgnoredTags;
//
//
//         /// <summary>
//         /// This GameplayAbility can only be activated if the Target has all of these GameplayTags.
//         /// The Target GameplayTags are only set if the GameplayAbility is triggered by an event.
//         /// 
//         /// 当Target的ASC的GrantedTags满足ActivationRequiredTags条件时 才能被激活
//         /// </summary>
//         public string[] TargetRequiredTags;
//
//         public string[] TargetIgnoredTags;
//     }
// }