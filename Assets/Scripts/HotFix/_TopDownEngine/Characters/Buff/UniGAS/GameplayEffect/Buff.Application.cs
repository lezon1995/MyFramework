namespace MoreMountains.TopDownEngine
{
    public partial class Buff
    {
        protected virtual void OnDiscard()
        {
        }

        protected virtual void OnAfterAdd()
        {
        }

        /// <summary>
        /// 检查应用该GESpec的要求是否满足
        /// </summary>
        /// <returns></returns>
        public bool CheckApplyCondition()
        {
            //检查应用条件
            var conditions = BuffType.condition.ApplyConditions;
            if (conditions == null)
                return true;

            if (conditions.Length == 0)
                return true;

            var canApply = true;
            foreach (var condition in conditions)
                canApply &= condition.CanApply(this);

            return canApply;
        }

        public void Discard()
        {
            OnDiscard();
        }

        public void FirstAdd()
        {
            OnAfterAdd();
        }
    }
}