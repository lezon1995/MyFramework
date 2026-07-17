using System.Collections.Generic;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    [AddComponentMenu("TopDown Engine/Character/Core/Buffable")]
    [RequireComponent(typeof(Character))]
    public class Buffable : MMMonoBehaviour
    {
        public Transform BuffParent;

        Character _character;
        Health _health;
        Stats _stats;
        Exp _exp;

        public Character Character => _character;
        public Health Health => _health;
        public int Level => _exp ? _exp.Level : 0;
        public int LevelMax => _exp ? _exp.LevelMax : 1;
        public IEventRouter Event => _character.Event;

        public List<Buff> SelfAppliedBuffs = new();

        [ReadOnly]
        public List<Buff> Buffs = new();

        public List<(BuffType, MMCooldown)> BuffCooldown = new();

        //ASC身上被赋予的所有AbilitySpec实例
        // public List<GASpec> GASpecs = new List<GASpec>();


        void Awake()
        {
            _character = GetComponent<Character>();
            _stats = GetComponent<Stats>();
            _health = GetComponent<Health>();
            _exp = GetComponent<Exp>();

            if (BuffParent == null)
            {
                BuffParent = new GameObject("[Buffs]").transform;
                BuffParent.SetParent(_character.transform);
                BuffParent.localPosition = Vector3.zero;
            }
        }

        void Start()
        {
            CheckApplySelfBuff();
        }

        void CheckApplySelfBuff()
        {
            if (SelfAppliedBuffs.Count == 0)
                return;

            foreach (var buff in SelfAppliedBuffs)
            {
                ApplyBuff(buff, this);
            }
        }

        public UniStats.Stat GetStat(string statName)
        {
            return _stats.GetStat(statName);
        }

        public virtual bool CanTakeBuffThisFrame()
        {
            if (!enabled)
                return false;

            return true;
        }

        public void ApplyBuff(Buff buffTemplate)
        {
            ApplyBuff(buffTemplate, this);
        }

        public void ApplyBuff(Buff buffTemplate, GameObject instigator, Buff.Param param)
        {
            Buffable source;
            if (instigator == null)
                source = this;
            else
                source = instigator.GetComponent<Buffable>();

            ApplyBuff(buffTemplate, source, param);
        }

        internal void ApplyBuff(Buff buffTemplate, Buffable source = null, Buff.Param param = default)
        {
            if (buffTemplate == null)
                return;

            if (source == null)
                source = this;

            var buff = buffTemplate.BuffType.Get(buffTemplate, source, this);
            if (!DoApply(buff, param))
            {
                buffTemplate.BuffType.Release(buff);
            }
        }

        public bool HasBuff(BuffType buffType)
        {
            foreach (var buff in Buffs)
            {
                if (buff.BuffType == buffType)
                    return true;
            }

            return false;
        }

        public bool CheckBuffCooldown(Buff buffTemplate)
        {
            for (var i = BuffCooldown.Count - 1; i >= 0; i--)
            {
                var (buffType, cooldown) = BuffCooldown[i];
                if (buffType == buffTemplate.BuffType)
                    return cooldown.Ready();
            }

            return true;
        }

        #region Misc

        public void OnTick(float dt)
        {
            for (var i = Buffs.Count - 1; i >= 0; i--)
            {
                var buff = Buffs[i];
                if (buff.OnUpdate(dt))
                {
                    RemoveBuff(buff, Buff.Removal.Routinely, false);
                }
            }

            if (BuffCooldown.Count > 0)
            {
                for (var i = BuffCooldown.Count - 1; i >= 0; i--)
                {
                    var (buffType, cooldown) = BuffCooldown[i];
                    cooldown.Update(dt);
                    if (cooldown.Ready())
                    {
                        BuffCooldown.RemoveAt(i);
                        MMCooldown.Return(cooldown);
                        // Debug.Log($"{buffType.name} CD结束");
                    }
                }
            }
        }

        #endregion

        #region Buff Application

        static bool DoApply(Buff buff, Buff.Param param)
        {
            if (buff == null)
                return false;

            //检查应用条件
            if (!buff.CheckCooldown())
                return false;

            //检查应用条件
            if (!buff.CheckApplyCondition())
                return false;

            //检查Tag条件
            if (!buff.CheckTagRequirements())
                return false;

            return buff.Execute(param);
        }


        /// <summary>
        /// 应用 持续的GESpec
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="param"></param>
        internal bool DispatchBuff(Buff buff, Buff.Param param)
        {
            //检查Stacking叠加
            return buff.GetBuffIncrements(param) switch
            {
                //添加类型如果是Discard（丢弃）则会忽视本次应用，出现在 多次 应用 非叠加类GE
                Buff.Result.Discard => DiscardBuff(buff),
                //添加类型如果是Addition（增加）则会添加本次应用，出现在 首次 应用 非叠加类GE
                Buff.Result.FirstAdd => FirstAddBuff(buff),
                //添加类型如果是StackAddition（增加叠加）则会添加本次应用，出现在 首次 应用 叠加类GE
                Buff.Result.FirstStack => FirstAddStackBuff(buff, param),
                //添加类型如果是Stacking（叠加）则会叠加本次应用，出现在 非首次 应用 叠加类GE
                Buff.Result.Stacking => StackBuff(buff, param),
                //添加类型如果是StackOverflow（叠加溢出）则会叠加溢出本次应用，出现在 叠加类GE达到最大层数后 再次应用GE时
                Buff.Result.StackOverflow => OverflowBuff(buff),
                _ => false
            };
        }

        #endregion

        #region Buff Execution Type

        /// <summary>
        /// 溢出最大叠加层数
        /// </summary>
        /// <param name="buff"></param>
        static bool OverflowBuff(Buff buff)
        {
            Buff originBuff;
            if (buff.IsStackBySource())
                buff.TryGetBuffStackedBySource(out originBuff);
            else
                buff.TryGetBuffStackedByTarget(out originBuff);

            //如果spec不是原始的GeSpec 就回收spec
            bool success = buff == originBuff;

            //只对原始buff实例做修改
            originBuff.Overflow();

            return success;
        }

        static bool DiscardBuff(Buff buff)
        {
            buff.Discard();
            return false;
        }

        bool StackBuff(Buff buff, Buff.Param param)
        {
            Buff originBuff;
            if (buff.IsStackBySource())
                buff.TryGetBuffStackedBySource(out originBuff);
            else
                buff.TryGetBuffStackedByTarget(out originBuff);

            bool success = buff == originBuff;

            //只对原始Buff实例做修改
            var delta = Mathf.Abs(param.Stack);
            switch (param.Stack)
            {
                case >= 0:
                    originBuff.IncreaseStack(delta);
                    break;
                case < 0:
                    if (originBuff.DecreaseStack(delta))
                    {
                        RemoveBuff(originBuff, Buff.Removal.ApplyStack, true);
                    }

                    break;
            }

            return success;
        }

        bool FirstAddBuff(Buff buff)
        {
            buff.AddMainMods();

            // buff.RemoveOtherGEWithTags();
            // buff.GrantedAbilities();

            Buffs.Add(buff);
            buff.FirstAdd();

            buff.SetParent(BuffParent);
            buff.Owner = this;
            return true;
        }

        //应用在该ASC上的叠加类并且StackingType为AggregateBySource的GE 还需要在这里记录
        //key为这个GESPec的Source，value为这个GESPec
        internal Dictionary<(BuffType, Buffable), Buff> StackedBySource = new();

        bool FirstAddStackBuff(Buff buff, Buff.Param param)
        {
            buff.CheckAddExclusiveStack();

            buff.CheckAddExtraStackSources();

            if (buff.IsStackBySource())
            {
                //只往对应的GE的CAggregateBySource中添加
                StackedBySource.TryAdd((buff.BuffType, buff.Source), buff);
            }

            buff.AddMainMods();
            // buff.RemoveOtherGEWithTags();
            // buff.GrantedAbilities();

            Buffs.Add(buff);
            buff.FirstAdd();

            buff.SetParent(BuffParent);
            buff.Owner = this;
            return StackBuff(buff, param);
        }

        #endregion

        #region GE Removal

        public void RemoveBuffWithTag(string tag, bool cooldown = false)
        {
            for (var i = Buffs.Count - 1; i >= 0; i--)
            {
                if (Buffs[i].BuffType.Tag == tag)
                {
                    RemoveBuff(Buffs[i], Buff.Removal.WithTag, cooldown);
                }
            }
        }

        public void RemoveBuffWithType(BuffType type, bool cooldown = false)
        {
            for (var i = Buffs.Count - 1; i >= 0; i--)
            {
                if (Buffs[i].BuffType == type)
                {
                    RemoveBuff(Buffs[i], Buff.Removal.WithType, cooldown);
                }
            }
        }

        public void RemoveBuff(Buff buff, Buff.Removal removal, bool cooldown)
        {
            buff.BeforeRemove(removal, cooldown);
            InternalRemove(buff);
        }

        void InternalRemove(Buff buff)
        {
            Buffs.Remove(buff);
            Event.trigger(new Buff.AfterRemoved(buff));

            buff.BuffType.Release(buff);
        }

        #endregion

        public void NotifyOnCombat(bool inCombat)
        {
            foreach (var buff in Buffs)
            {
                buff.NotifyOnCombat(inCombat);
            }
        }
    }
}