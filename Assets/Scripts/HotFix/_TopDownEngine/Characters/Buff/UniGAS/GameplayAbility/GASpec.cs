// using System;
//
// namespace MoreMountains.TopDownEngine
// {
//     [Serializable]
//     public partial class GASpec
//     {
//         public GA GA;
//         public ASC Owner;
//         public bool IsActive;
//
//         #region Activate Ability
//
//         /// <summary>
//         /// 检查能力是否可以被激活
//         /// </summary>
//         /// <returns></returns>
//         public virtual bool CanActivateAbility()
//         {
//             return !IsActive && CheckTags() && CheckCost() && CheckCooldown();
//         }
//
//         /// <summary>
//         /// 尝试激活能力，由于激活能力可以在某个时间段内完成，所以使用async/await来实现
//         /// </summary>
//         /// <returns></returns>
//         public void TryActivateAbility()
//         {
//             if (CanActivateAbility())
//             {
//                 IsActive = true;
//                 CancelAbilitiesWithTags();
//
//                 PreActivate();
//                 ActivateAbility();
//             }
//         }
//
//         /// <summary>
//         /// 激活能力之前，需要执行的方法
//         /// </summary>
//         protected virtual void PreActivate()
//         {
//             DoCooldown();
//             DoCost();
//         }
//
//         /// <summary>
//         /// 激活能力的具体逻辑，一般在该方法中应用GE
//         /// </summary>
//         /// <returns></returns>
//         protected virtual void ActivateAbility()
//         {
//         }
//
//         protected virtual void EndAbility()
//         {
//         }
//
//         /// <summary>
//         /// 取消能力
//         /// </summary>
//         public void CancelAbility()
//         {
//             IsActive = false;
//             EndAbility();
//         }
//
//         #endregion
//
//         #region Use Ability
//
//         /// <summary>
//         /// 检查能力是否可以被使用
//         /// 默认在能力是激活的情况下是可以被使用
//         /// </summary>
//         /// <returns></returns>
//         public virtual bool CanUseAbility()
//         {
//             return IsActive;
//         }
//
//         public void TryUseAbility()
//         {
//             if (CanUseAbility())
//             {
//                 PreUse();
//                 UseAbility();
//             }
//         }
//
//
//         /// <summary>
//         /// 激活能力之前，需要执行的方法
//         /// </summary>
//         protected virtual void PreUse()
//         {
//             DoCooldown();
//             DoCost();
//         }
//
//
//         /// <summary>
//         /// 使用能力
//         /// Example：如果具有移动能力，那么使用其能力就是移动单位距离
//         /// Example：如果具有普通攻击能力，那么使用其能力就是进行普通攻击
//         /// 可以看出“具有能力”和“使用能力”是有区别的
//         /// 由于使用能力有可能不是一瞬间的事情，比如攻击前摇，施法吟唱，所以使用async修饰
//         /// </summary>
//         /// <returns></returns>
//         protected virtual void UseAbility()
//         {
//         }
//
//         #endregion
//     }
// }