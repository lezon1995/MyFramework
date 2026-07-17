using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains
{
    public partial class Buff
    {
        StackType _stackType => BuffType.stack.Type;
        int _stackIncrement => BuffType.stack.Delta.Incre;
        int _stackDecrement => BuffType.stack.Delta.Decre;

        Mag _maxStack
        {
            get
            {
                if (BuffType == null)
                    return null;

                return BuffType.stack.MaxStack;
            }
        }

        bool _stackExclusive => BuffType.stack.Exclusive;
        bool _refreshDurationOnStacked => BuffType.stack.RefreshDurationOnStacked;
        bool _resetPeriodOnStacked => BuffType.stack.ResetPeriodOnStacked;
        StackExpirePolicy _stackExpirePolicy => BuffType.stack.StackExpirePolicy;
        bool _isOverrideDecreasingDuration => BuffType.stack.DecreasingDuration.Override;
        float _DecreasingDuration => BuffType.stack.DecreasingDuration.Duration;
        bool _clearOnMaxStacked => BuffType.stack.ClearOnMaxStacked;
        bool _hasExtraStackSources => BuffType.stack.HasExtraStackSources;
        Data[] _maxStackBuffs => BuffType.stack.MaxStackBuffs;
        Mod[] _stackMods => BuffType.stack.Mods;
        StackSource[] _extraStackSources => BuffType.stack.ExtraStackSources;

        public MMFeedbacks FB_Stacked;
        public MMFeedbacks FB_MaxStacked;

        protected virtual void OnStackChange(int oldStack, int newStack)
        {
            LastStack = oldStack;
        }

        protected virtual void OnMaxStacked(int maxStack)
        {
        }

        protected virtual void OnReachMaxStackedClear()
        {
        }

        protected virtual void OnBeforeRemove()
        {
        }

        public int Stack { get; private set; }
        public int LastStack { get; set; }

        public int MaxStack
        {
            get
            {
                if (_maxStack == null)
                    return 0;

                return (int)_maxStack.Value(this);
            }
        }

        public bool IsMaxStacked => Stack == MaxStack;
        public bool IsStackDecreasing => LastStack > Stack;

        void InitializeStack()
        {
            Stack = 0;
            LastStack = 0;
        }

        /// <summary>
        /// 获取本次应用的GE的添加类型，通过本次添加类型来决定该如何处理该GE
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Result GetBuffIncrements(Param param)
        {
            Buff originBuff;
            if (_isStackable)
            {
                switch (_stackType)
                {
                    case StackType.ByTarget:
                        if (!TryGetBuffStackedByTarget(out originBuff))
                            return Result.FirstStack;

                        //如果可以叠加 则判断是否小于最大叠加层数
                        if (originBuff.Stack < MaxStack)
                            return Result.Stacking;

                        if (originBuff.Stack == MaxStack)
                        {
                            if (originBuff.Stack + param.Stack < MaxStack)
                                return Result.Stacking;

                            return Result.StackOverflow;
                        }

                        return Result.Discard;
                    case StackType.BySource:
                        if (!TryGetBuffStackedBySource(out originBuff))
                            return Result.FirstStack;

                        if (originBuff.Stack < MaxStack)
                            return Result.Stacking;

                        if (originBuff.Stack == MaxStack)
                        {
                            if (originBuff.Stack + param.Stack < MaxStack)
                                return Result.Stacking;

                            return Result.StackOverflow;
                        }

                        return Result.Discard;
                }
            }
            else
            {
                switch (_instanceMode)
                {
                    case InstanceModes.Single:
                        if (IfAlreadyExist(out originBuff))
                        {
                            originBuff.RefreshDuration();
                            return Result.Discard;
                        }

                        return Result.FirstAdd;
                    case InstanceModes.Multiple:
                        return Result.FirstAdd;
                }
            }

            return Result.Discard;
        }

        public bool TryGetBuffStackedBySource(out Buff result)
        {
            return Target.StackedBySource.TryGetValue((BuffType, Source), out result);
        }

        public bool TryGetBuffStackedByTarget(out Buff result)
        {
            foreach (var buff in Target.Buffs)
            {
                if (buff.BuffType == BuffType)
                {
                    result = buff;
                    return true;
                }
            }

            result = null;
            return false;
        }

        bool IfAlreadyExist(out Buff originBuff)
        {
            foreach (var buff in Target.Buffs)
            {
                if (buff.BuffType == BuffType)
                {
                    originBuff = buff;
                    return true;
                }
            }

            originBuff = null;
            return false;
        }

        public void IncreaseStack(int delta)
        {
            if (delta == 0)
                delta = _stackIncrement;

            DoIncreaseStack(delta);
        }

        protected void DoIncreaseStack(int increment)
        {
            if (increment == 0)
                return;

            var oldStack = Stack;
            Stack = Mathf.Clamp(Stack + increment, 0, MaxStack);

            if (oldStack == Stack)
                return;

            ExecuteDurationRefreshPolicy();
            ExecuteStackPeriodResetPolicy();

            AddStackMods(oldStack, Stack);

            CheckIfMaxStacked();

            OnStackChange(oldStack, Stack);
        }

        void AddStackMods(int oldStack, int newStack)
        {
            var mods = _stackMods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                AddOrEditMod(mod, mod.ToString(), oldStack, newStack);
            }
        }

        /// <summary>
        /// 检查 是否达到最大叠加层数限制
        /// </summary>
        void CheckIfMaxStacked()
        {
            if (Stack == MaxStack)
            {
                //触发 达到最大叠加层数事件
                ApplyMaxStackBuffs();

                OnMaxStacked(Stack);

                CheckClearStackOnReachStackLimit();
            }
        }

        /// <summary>
        /// 检查 是否清空所有叠加层 当达到最大层数限制时
        /// Example：清空例子 VN打完第3层W被动时，之前叠加的所有W被动被移除
        /// Example：不清空例子 当征服者天赋叠满之后，继续叠加则不会清空征服者效果
        /// </summary>
        void CheckClearStackOnReachStackLimit()
        {
            if (_clearOnMaxStacked)
            {
                OnReachMaxStackedClear();

                RemoveStack();

                Target.RemoveBuff(this, Removal.MaxStacked, true);
            }
        }

        /// <summary>
        /// 应用当该GE达到最大层数时配置的GE
        /// Example：英雄联盟 VN的W被动，当叠加到最大层数三层后会造成一次真实伤害，一次真实伤害也可以配置成一个GE
        /// Example：英雄联盟 小炮的E技能、纳尔的W技能
        /// </summary>
        void ApplyMaxStackBuffs()
        {
            foreach (var data in _maxStackBuffs)
            {
                GetActor(data.ApplyTo).ApplyBuff(data.Buff);
            }
        }

        public bool DecreaseStack(int decrement)
        {
            if (decrement == 0)
                return false;

            var oldStack = Stack;
            Stack = Mathf.Clamp(Stack - decrement, 0, MaxStack);

            if (oldStack == Stack)
                return false;

            OnStackChange(oldStack, Stack);
            RemoveStackMods(oldStack, Stack);

            if (Stack == 0 && _isDuration)
                return RemoveStack();

            return RefreshDuration();
        }

        void RemoveStackMods(int oldStack, int newStack)
        {
            var mods = _stackMods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                RemoveOrEditMod(mod, mod.ToString(), oldStack, newStack);
            }
        }

        /// <summary>
        /// 当成功叠加GE后 执行 持续时间刷新 政策
        /// </summary>
        void ExecuteDurationRefreshPolicy()
        {
            if (_refreshDurationOnStacked)
            {
                //刷新持续时间
                // Example：英雄联盟 EZ技能命中后攻速5秒内提升，当5秒内若再次命中技能，则刷新持续时间为5秒
                RefreshDuration();
            }
            else
            {
                //不刷新持续时间
            }
        }


        /// <summary>
        /// 当成功叠加GE后 执行 周期间隔重置 政策
        /// </summary>
        void ExecuteStackPeriodResetPolicy()
        {
            if (_resetPeriodOnStacked)
            {
                // Example：英雄联盟 蘑菇中毒效果，每0.5秒跳一次伤害，假如当前时间运行到0.4秒的时候
                // 再次踩到蘑菇，则中毒剩余时间会刷新，并且伤害仍会在0.5秒后计算，因为Period重置到了0
                ResetPeriod();
            }
            else
            {
                // Example：英雄联盟 点燃效果，每0.5秒跳一次伤害，假如当前时间运行到0.4秒的时候
                // 再次施加点燃，则点燃剩余时间会刷新，并且伤害仍会在0.1秒后计算，因为Period没有重置
            }
        }


        /// <summary>
        /// 执行 叠加过期 政策
        /// </summary>
        bool DoStackExpirePolicy()
        {
            //根据不同政策执行对应的方法
            return _stackExpirePolicy switch
            {
                //移除全部Stack，并移除buff实例
                //Eg: 英雄联盟 游戏中叠加征服者天赋，不管叠加到多少层，只要当前层过期，直接移除全部层
                StackExpirePolicy.ClearAllStack => RemoveStack(),
                //移除1层Stack 并刷新持续时间，不移除Buff实例
                //Eg: 英雄联盟 武器大师被动，不管叠加到多少层，只要当前层过期，总叠加层数减1，并刷新当前层的持续时间
                StackExpirePolicy.DecreaseStack => DecreaseStack(_stackDecrement),
                //仅刷新持续时间，层数变化可以自定义实现，不移除Buff实例
                StackExpirePolicy.RefreshDuration => RefreshDuration(),
                _ => false
            };
        }

        bool RemoveStack()
        {
            //如果是在Source上聚合，则还需要移除聚合中保存的Buff
            if (IsStackBySource())
                Target.StackedBySource.Remove((BuffType, Source));

            CheckRemoveExclusiveStack();

            CheckRemoveExtraStackSources();

            return true;
        }

        public bool IsStackBySource()
        {
            return _stackType == StackType.BySource;
        }

        void ClearStack()
        {
            Stack = 0;
            LastStack = 0;
        }

        public void CheckAddExclusiveStack()
        {
            if (_stackExclusive)
            {
                var dictionary = BuffType.Exclusive;
                if (dictionary.Remove(Source, out var buff))
                {
                    buff.Target.RemoveBuff(buff, Removal.Exclusively, false);
                }

                dictionary[Source] = this;
            }
        }

        void CheckRemoveExclusiveStack()
        {
            if (_stackExclusive)
            {
                var dictionary = BuffType.Exclusive;
                dictionary.Remove(Source);
            }
        }

        public void CheckAddExtraStackSources()
        {
            if (_hasExtraStackSources)
            {
                foreach (var stackSource in _extraStackSources)
                {
                    switch (stackSource.Source)
                    {
                        case StackSource.Sources.DoAttackHit:
                            Target.Event.addListener<DoAttackEffect>(OnIncreaseStackFrom);
                            break;
                        case StackSource.Sources.DoMove:
                            Target.Event.addListener<DoMove>(OnIncreaseStackFrom);
                            break;
                    }
                }
            }
        }

        void CheckRemoveExtraStackSources()
        {
            if (_hasExtraStackSources)
            {
                foreach (var stackSource in _extraStackSources)
                {
                    switch (stackSource.Source)
                    {
                        case StackSource.Sources.DoAttackHit:
                            Target.Event.removeListener<DoAttackEffect>(OnIncreaseStackFrom);
                            break;
                        case StackSource.Sources.DoMove:
                            Target.Event.removeListener<DoMove>(OnIncreaseStackFrom);
                            break;
                    }
                }
            }
        }

        Dictionary<StackSource, float> stackSourceTriggers = new();

        float IncreStackSourceTrigger(StackSource source, float delta)
        {
            if (!stackSourceTriggers.TryGetValue(source, out var count))
                count = 0;

            var value = count + delta;
            stackSourceTriggers[source] = value;
            return value;
        }

        void ClearStackSourceTrigger(StackSource source)
        {
            stackSourceTriggers.Remove(source);
        }

        protected virtual void OnIncreaseStackFrom(DoAttackEffect e)
        {
            foreach (var source in _extraStackSources)
            {
                if (source.Source == StackSource.Sources.DoAttackHit)
                {
                    var unit = IncreStackSourceTrigger(source, 1);
                    if (unit >= source.Unit)
                    {
                        ClearStackSourceTrigger(source);
                        DoIncreaseStack(source.Increment);
                    }
                }
            }
        }

        protected virtual void OnIncreaseStackFrom(DoMove e)
        {
            var movement = e.Movement;
            var unitDist = movement.magnitude * 100;
            foreach (var source in _extraStackSources)
            {
                if (source.Source == StackSource.Sources.DoMove)
                {
                    var unit = IncreStackSourceTrigger(source, unitDist);
                    var times = (int)(unit / source.Unit);
                    if (times >= 1)
                    {
                        IncreStackSourceTrigger(source, -source.Unit * times);
                        DoIncreaseStack(source.Increment * times);
                    }
                }
            }
        }
    }
}