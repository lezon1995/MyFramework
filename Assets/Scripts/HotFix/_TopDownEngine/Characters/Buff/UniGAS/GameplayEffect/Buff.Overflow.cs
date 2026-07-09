namespace MoreMountains.TopDownEngine
{
    public partial class Buff
    {
        protected virtual void OnStackOverflow()
        {
        }

        protected virtual void OnStackOverflowClear()
        {
        }

        Data[] _overflowBuffs => BuffType.overflow.OverflowBuffs;
        bool _denyOverflowApplication => BuffType.overflow.DenyOverflowApplication;
        bool _clearStackOnOverflow => BuffType.overflow.ClearStackOnOverflow;

        /// <summary>
        /// 当前GE溢出
        /// 溢出场景：当叠加到最大层数限制后，如果再次尝试叠加，则会触发Overflow
        /// </summary>
        public void Overflow()
        {
            ApplyOverflowBuffs();

            //如果DenyOverflowApplication为True，则溢出的Apply不会执行DurationRefreshPolicy
            if (!_denyOverflowApplication)
            {
                ExecuteDurationRefreshPolicy();
            }

            OnStackOverflow();

            CheckClearStackOnOverflow();
        }

        /// <summary>
        /// 应用 该GE溢出时 配置的GE
        /// </summary>
        void ApplyOverflowBuffs()
        {
            foreach (var data in _overflowBuffs)
            {
                GetActor(data.ApplyTo).ApplyBuff(data.Buff);
            }
        }

        /// <summary>
        /// 检查 是否需要清空所有叠加 当GE溢出的时候
        /// 与CheckClearStackOnReachStackLimit的区别：CheckClearStackOnReachStackLimit是达到最大层时检查清空所有叠加
        /// 而CheckClearStackOnOverflow是在达到最大层之后再次应用GE的时候才会检查清空所有叠加，两者有一定区别，但是两种方法可以互通
        /// 但是实际情况中CheckClearStackOnOverflow这种方法用的会更少一点。
        /// Example：
        /// </summary>
        void CheckClearStackOnOverflow()
        {
            //当DenyOverflowApplication为True是才有效，当Overflow时是否直接删除所有层数
            if (_denyOverflowApplication && _clearStackOnOverflow)
            {
                OnStackOverflowClear();

                Target.RemoveBuff(this, Removal.Overflowed, true);
            }
        }
    }
}