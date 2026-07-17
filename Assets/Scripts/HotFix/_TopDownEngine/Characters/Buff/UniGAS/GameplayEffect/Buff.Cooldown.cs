using UnityEngine;

namespace MoreMountains
{
    public partial class Buff
    {
        bool _hasCooldown => BuffType.main.HasCooldown;
        float _cooldownDuration => BuffType.cooldown.Duration.Value(this);
        Actors _cooldownAt => BuffType.cooldown.CooldownAt;

        Buffable CooldownActor()
        {
            var buffable = _cooldownAt switch
            {
                Actors.Target => Target,
                Actors.Source => Source,
                _ => Target
            };
            return buffable;
        }

        public bool CheckCooldown()
        {
            if (_hasCooldown)
            {
                var buffable = CooldownActor();
                var list = buffable.BuffCooldown;
                for (var i = 0; i < list.Count; i++)
                {
                    var (buffType, cooldown) = list[i];
                    if (buffType == BuffType)
                    {
                        var ready = cooldown.Ready();
                        if (ready == false)
                        {
                            // Debug.Log($"{name}CD中，无法应用于");
                        }

                        return ready;
                    }
                }
            }

            return true;
        }
    }
}