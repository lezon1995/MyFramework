// using Extensions;
//
// namespace MoreMountains
// {
//     public partial class GASpec
//     {
//         /// <summary>
//         /// 检查是否这个能力的冷却是否已经完毕
//         /// </summary>
//         /// <returns></returns>
//         public virtual bool CheckCooldown()
//         {
//             if (GA.Cooldown == null)
//             {
//                 return true;
//             }
//
//             if (Owner.AbilityCooldown.TryGetValue(this, out var cooldown))
//             {
//                 return cooldown.IsValid == false;
//             }
//
//             return true;
//         }
//
//         public virtual void DoCooldown()
//         {
//             if (GA.Cooldown == null)
//             {
//                 return;
//             }
//
//             var cooldownGE = Owner.ApplyGEToSelf(GA.Cooldown);
//             Owner.AbilityCooldown[this] = cooldownGE;
//         }
//     }
// }