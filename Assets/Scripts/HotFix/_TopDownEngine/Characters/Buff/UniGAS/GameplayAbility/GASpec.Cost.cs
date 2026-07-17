// using System.Linq;
// using Extensions;
//
// namespace MoreMountains
// {
//     public partial class GASpec
//     {
//         /// <summary>
//         /// 检查是否有足够的资源来激活能力
//         /// </summary>
//         /// <returns></returns>
//         public virtual bool CheckCost()
//         {
//             //如果该能力没有消耗，则默认可以激活
//             if (GA.Cost == null)
//             {
//                 return true;
//             }
//
//             var geSpec = GA.Cost.CreateGESpec(Owner, Owner, 1);
//             var cAttribute = Owner.Get<AttributeSet>();
//             //由于消耗可以有多个，比如同时耗血耗蓝，或者耗蓝又耗充能，所以这里的Modifiers是一个数组，但是在本例子下其实就一个资源，就是耗蓝
//             var canCost = GA.Cost.Modifiers.All(modifier =>
//             {
//                 //获取本次消耗的实际值，可能会随着能力等级变化，具体值看具体配置
//                 var costValue = modifier.Magnitude.GetMagnitude(geSpec);
//                 //获得需要消耗的资源，在本例子下是魔法值，即蓝量(MP)，其他场景下可能是生命值(HP)
//                 var attributeInfo = cAttribute.GetAttribute(modifier.StatName);
//                 return attributeInfo + costValue >= 0;
//             });
//             Release(geSpec);
//             return canCost;
//         }
//
//         public virtual void DoCost()
//         {
//             if (GA.Cost == null) return;
//             Owner.ApplyGEToSelf(GA.Cost);
//         }
//     }
// }