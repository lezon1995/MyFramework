using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    /// <summary>
    /// A scriptable object you can create assets from, to identify damage types
    /// </summary>
    [HideMonoScript]
    [CreateAssetMenu(menuName = "TopDown Engine/Character/Buff/BuffType", fileName = "BuffType")]
    public partial class BuffType : SerializedScriptableObject
    {
        public MainConfig main;

        public bool IsInstant => main.Type == Buff.Types.Instant;
        public bool IsDuration => main.Type == Buff.Types.Duration;
        public bool IsInfinite => main.Type == Buff.Types.Infinite;
        public bool IsPeriodic => main.Periodic;
        public bool IsStackable => main.Stackable;
        public bool HasCooldown => main.HasCooldown;
        public bool WithEvent => main.WithEvent;
        public Buff.InstanceModes InstanceMode => main.InstanceMode;

        const string INSTANT = nameof(IsInstant);
        const string PERIODIC = nameof(IsPeriodic);
        const string STACKABLE = nameof(IsStackable);
        const string COOLDOWN = nameof(HasCooldown);
        const string EVENT = nameof(WithEvent);

        [Serializable, HideLabel]
        [BoxGroup("Main", order: 1)]
        public class MainConfig
        {
            public string Name;
            public string Desc;
            public Buff.Types Type;

            [HideIf(nameof(Type), Buff.Types.Instant)]
            [DisableIf(nameof(Stackable))]
            public Buff.InstanceModes InstanceMode;

            [HideIf(nameof(Type), Buff.Types.Instant)]
            [ToggleLeft]
            public bool Periodic;

            [HideIf(nameof(Type), Buff.Types.Instant)]
            [ToggleLeft, OnValueChanged(nameof(OnStackableChanged))]
            public bool Stackable;

            [ToggleLeft]
            public bool HasCooldown;

            [ToggleLeft]
            public bool WithEvent;

            void OnStackableChanged()
            {
                if (Stackable) InstanceMode = Buff.InstanceModes.Single;
            }

            [ShowIf(nameof(Type), Buff.Types.Duration)]
            [BoxGroup("Duration", false), InlineProperty]
            public Buff.Mag Duration;

            [ShowIf(nameof(Type), Buff.Types.Duration)]
            [ToggleLeft]
            public bool RefreshDurationWhileInCombat;

            [HideIf(nameof(Type), Buff.Types.Instant)]
            public Buff.Mod[] Mods;

            [ShowIf(nameof(Type), Buff.Types.Instant)]
            [ToggleLeft, LabelText("Damage")]
            [BoxGroup("Instant")]
            public bool IsInstantDamage;

            [ShowIf(nameof(ShowInstantDamage)), HideLabel]
            [BoxGroup("Instant")]
            public Buff.DmgMag InstantDamage;
            
            [ShowIf(nameof(IsInstantDamage))]
            [ToggleLeft, LabelText("Alternative")]
            [BoxGroup("Instant")]
            public bool HasAlternativeInstantDamage;
            
            [ShowIf(nameof(HasAlternativeInstantDamage)), HideLabel]
            [BoxGroup("Instant")]
            public Buff.DmgMag AlternativeInstantDamage;

            [PropertySpace(SpaceBefore = 30)]
            [ShowIf(nameof(Type), Buff.Types.Instant)]
            [ToggleLeft, LabelText("Heal")]
            [BoxGroup("Instant")]
            public bool IsInstantHeal;

            [ShowIf(nameof(ShowInstantHeal)), HideLabel]
            [BoxGroup("Instant")]
            public Buff.HealMag InstantHeal;

            [HideInInspector]
            public ConditionalBuff[] ConditionalBuffs;

            bool ShowInstantDamage => Type == Buff.Types.Instant && IsInstantDamage;
            bool ShowInstantHeal => Type == Buff.Types.Instant && IsInstantHeal;
        }

        ObjectPool<Buff> _pool;

        public Buff Get(Buff buffTemplate, Buffable source, Buffable target)
        {
            if (_pool == null)
            {
                var buffPool = source.transform.Find("BuffPools");

                if (buffPool == null)
                    buffPool = new GameObject("BuffPools").transform;

                buffPool.SetParent(source.transform);

                var parent = new GameObject($"[Pool] {buffTemplate.name}").transform;
                parent.SetParent(buffPool.transform);
                parent.localPosition = Vector3.zero;

                _pool = new(() =>
                    {
                        var buff = Instantiate(buffTemplate, parent);
                        buff.hideFlags = HideFlags.None;
                        buff.transform.localPosition = Vector3.zero;
                        buff.DefaultParent = parent;
                        buff.OnNew();
                        return buff;
                    },
                    buff => buff.OnGet(),
                    buff => buff.OnRelease(),
                    buff => Destroy(buff.gameObject),
                    false);
            }

            var buff = _pool.Get();
            buff.Initialize(this, source, target);
            return buff;
        }

        public void Release(Buff buff)
        {
            _pool.Release(buff);
        }

//         [Button]
//         public void AddSubAsset()
//         {
//             //创建
//             var itemAsset = CreateInstance<BuffType>();
//             AssetDatabase.AddObjectToAsset(itemAsset, this);
//             AssetDatabase.SaveAssets();
// //删除
//             // AssetDatabase.RemoveObjectFromAsset(itemAsset);
//         }
//
//         [Button]
//         public void RemoveSubAsset()
//         {
//             AssetDatabase.RemoveObjectFromAsset(this);
//             AssetDatabase.SaveAssets();
//         }
//
//         [Button]
//         public void Rename()
//         {
//             name = main.Name;
//             AssetDatabase.SaveAssets();
//         }
    }
}