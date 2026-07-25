namespace MoreMountains
{
    public partial class Buff
    {
        bool hasCooldown => main.HasCooldown;
        float cooldownDuration => cooldown.Duration.Value(this);
        Actors cooldownAt => cooldown.CooldownAt;

        Buffable CooldownActor()
        {
            var buffable = cooldownAt switch
            {
                Actors.Target => Target,
                Actors.Source => Source,
                _ => Target
            };
            return buffable;
        }

        public bool CheckCooldown()
        {
            if (hasCooldown)
            {
                var buffable = CooldownActor();
                var list = buffable.BuffCooldown;
                for (var i = 0; i < list.Count; i++)
                {
                    var (buffType, cooldown) = list[i];
                    if (buffType == GetType())
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