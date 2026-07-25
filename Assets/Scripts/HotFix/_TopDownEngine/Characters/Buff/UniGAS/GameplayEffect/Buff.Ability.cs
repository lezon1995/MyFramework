using System.Collections.Generic;

namespace MoreMountains
{
    public partial class Buff
    {
        Dictionary<CharacterAbility, CharacterAbility> abilityDict = new();

        /// <summary>
        /// 当成功应用该GE时，为该GE的Target赋予配置的Ability
        /// Example：英雄联盟 塞拉斯在使用R技能偷取敌方英雄的大招时，获得了使用对方大招的能力
        /// </summary>
        public void GrantedAbilities()
        {
            foreach (var data in Abilities)
            {
                var character = GetActor(data.ApplyTo).Character;
                if (!abilityDict.TryGetValue(data.Ability, out var ability))
                {
                    ability = Instantiate(data.Ability);
                    ability.InitializeOnAwake = false;
                    abilityDict.Add(data.Ability, ability);
                }

                character.AddAbility(ability);
            }
        }

        // public void ExecuteRemovalPolicy(GASpec gaSpec, RemovalPolicy removalPolicy)
        // {
        //     switch (removalPolicy)
        //     {
        //         case RemovalPolicy.CancelAbilityImmediately:
        //             m_OnGEExpiredRemove = () => CancelAbilityImmediately(gaSpec);
        //             break;
        //         case RemovalPolicy.DoNothing:
        //             break;
        //     }
        // }

        /// <summary>
        /// 当GE过期（自然过期/提前过期）时，所赋予的Ability会被立即Cancel并且被Remove。
        /// </summary>
        /// <param name="gaSpec"></param>
        private void CancelAbilityImmediately()
        {
            foreach (var data in Abilities)
            {
                if (data.RemoveWithBuff)
                {
                    var character = GetActor(data.ApplyTo).Character;
                    if (abilityDict.TryGetValue(data.Ability, out var ability))
                    {
                        character.RemoveAbility(ability);
                    }
                }
            }
        }
    }
}