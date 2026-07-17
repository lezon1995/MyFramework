using UnityEngine;

namespace MoreMountains.ApplyConditions
{
    [CreateAssetMenu(menuName = "TopDown Engine/Character/Buff/ApplyConditions/Cooldown", fileName = "AC-Cooldown")]
    public class ApplyCondition_Cooldown : ApplyCondition
    {
        public BuffType CooldownBuffType;
        public Buff.Actors Actors;

        public override bool CanApply(Buff buff)
        {
            var buffable = Actors switch
            {
                Buff.Actors.Target => buff.Target,
                Buff.Actors.Source => buff.Source,
                _ => buff.Target
            };

            foreach (var (buffType, cooldown) in buffable.BuffCooldown)
            {
                if (buffType == CooldownBuffType)
                {
                    var ready = cooldown.Ready();
                    if (ready==false)
                    {
                        Debug.Log($"{buff.name} 无法添加到 {buff.Target.name} 上，CD中");
                    }
                    return ready;
                }
            }

            return true;
        }
    }
}