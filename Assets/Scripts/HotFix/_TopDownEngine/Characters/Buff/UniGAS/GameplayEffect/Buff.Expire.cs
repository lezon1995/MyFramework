// using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    public partial class Buff
    {
        // public MMFeedbacks FB_Expired;
        
        Data[] _expiredBuffs => BuffType.expire.ExpiredBuffs;
        Data[] _preExpiredBuffs => BuffType.expire.PreExpiredBuffs;

        /// <summary>
        /// 当该GE提前过期时（一般是主动被移除），应用该GE配置的提前过期的GE
        /// Example：英雄联盟 提莫被动隐身，当玩家移动后，隐身被主动移除（过期），随后为提莫应用一个攻速加成的Buff（GE）
        /// </summary>
        void ApplyPreExpiredBuff()
        {
            if (_preExpiredBuffs == null)
                return;

            foreach (var data in _preExpiredBuffs)
            {
                GetActor(data.ApplyTo).ApplyBuff(data.Buff);
            }
        }

        /// <summary>
        /// 当该GE自然过期时（一般是被动被移除）应用 该GE配置的自然过期的GE
        /// Example：英雄联盟 余震天赋，定身敌人后获取双抗的加成buff，随后buff自然结束（过期），为周围的敌人施加余震伤害（GE）
        /// </summary>
        void ApplyExpiredBuff()
        {
            if (_expiredBuffs == null)
                return;

            foreach (var data in _expiredBuffs)
            {
                GetActor(data.ApplyTo).ApplyBuff(data.Buff);
            }
        }
    }
}