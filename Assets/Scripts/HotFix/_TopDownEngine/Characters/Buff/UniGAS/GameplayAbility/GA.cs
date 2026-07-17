// /*
//  *
// GameplayAbility示例:
//     跳跃
//     奔跑
//     射击
//     每X秒被动地阻挡一次攻击
//     使用药剂
//     开门
//     收集资源
//     建造
// 不应该使用GameplayAbility的场景:
//     基础移动输入
//     一些与UI的交互 - 不要使用GameplayAbility从商店中购买物品
//     这些不是规定, 只是我的建议而已, 你的设计和实现可能是多样的.
//  */
//
// namespace MoreMountains
// {
//     public partial class GA
//     {
//         public string Name { get; protected set; }
//
//         public static GASpec CreateGASpec<T>(GA ga, ASC owner) where T : GASpec
//         {
//             var gaSpec = Entity.Acquire<T>();
//             gaSpec.GA = ga;
//             gaSpec.Owner = owner;
//             return gaSpec;
//         }
//
//         public virtual GASpec CreateGASpec(ASC owner)
//         {
//             var gaSpec = CreateGASpec<GASpec>(this, owner);
//             return gaSpec;
//         }
//     }
// }