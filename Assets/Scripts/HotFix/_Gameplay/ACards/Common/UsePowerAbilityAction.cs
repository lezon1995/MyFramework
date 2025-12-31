using UnityEngine.Pool;

namespace MarbleHero
{
    public class UsePowerAbilityAction : AGameAction
    {
        AbilityData ability;
        ACreature caster;
        APower power;

        public UsePowerAbilityAction(AbilityData ability, ACreature caster, APower power)
        {
            this.ability = ability;
            this.caster = caster;
            this.power = power;
        }

        public override void update(float dt)
        {
            //Debug.Log("Trigger Ability " + ability.id + " : " + card.card_id);

            // OnAbilityStart?.Invoke(ability, card);
            // _battle.ability_trigger = card != null ? card.uid : "";
            // _battle.ability_played.Add(ability.Id);

            // if (IsAbilityRequireSelector(ability, caster, card))
            // return; //Wait for player to select

            UseAbility_Players(ability, caster, power);
            UseAbility_Monsters(ability, caster, power);
            UseAbility_Pawns(ability, caster, power);
            UseAbility_Slots(ability, caster, power);

            if (caster is APlayer player)
            {
                UseAbility_Cards(ability, player, power);
                UseAbility_CardDatas(ability, player, power);
            }

            ability.Do(caster, power);

            AfterAbilityUsed(ability, caster, power);
            isDone = true;
        }

        public void UseAbility_Players(AbilityData ability, ACreature caster, APower power)
        {
            using var _ = ListPool<APlayer>.Get(out var targets);
            ability.GetTargetPlayers(caster, power, ref targets);

            foreach (var t in targets)
                ability.Do_Player(caster, power, t);
        }

        public void UseAbility_Monsters(AbilityData ability, ACreature caster, APower power)
        {
            using var _ = ListPool<AMonster>.Get(out var targets);
            ability.GetTargetMonsters(caster, power, ref targets);

            foreach (var t in targets)
                ability.Do_Monster(caster, power, t);
        }

        public void UseAbility_Pawns(AbilityData ability, ACreature caster, APower power)
        {
            using var _ = ListPool<APawn>.Get(out var targets);
            ability.GetTargetPawns(caster, power, ref targets);

            foreach (var t in targets)
                ability.Do_Pawn(caster, power, t);
        }

        public void UseAbility_Cards(AbilityData ability, APlayer caster, APower power)
        {
            using var _ = ListPool<ACard>.Get(out var targets);
            ability.GetTargetCards(caster, power, ref targets);

            foreach (var t in targets)
                ability.Do_Card(caster, power, t);
        }

        public void UseAbility_Slots(AbilityData ability, ACreature caster, APower power)
        {
            using var _ = ListPool<Slot>.Get(out var targets);
            ability.GetTargetSlots(caster, power, ref targets);

            foreach (var t in targets)
                ability.Do_Slot(caster, power, t);
        }

        public void UseAbility_CardDatas(AbilityData ability, APlayer caster, APower power)
        {
            using var _ = ListPool<CardData>.Get(out var targets);
            ability.GetTargetCardDatas(caster, power, ref targets);

            foreach (var t in targets)
                ability.Do_CardData(caster, power, t);
        }

        public void AfterAbilityUsed(AbilityData ability, ACreature caster, APower power)
        {
            // //Recalculate and clear
            // Refresh_Ongoing();
            //
            // //Chain ability
            // if (ability.Target != AbilityTarget.ChoiceSelector && _battle.phase != BattlePhase.Ended)
            // {
            //     foreach (var choiceAbility in ability.ChoiceAbilities)
            //         TriggerAbility(choiceAbility, caster, card);
            // }
            //
            // if (_resolveQueue.Count() == 0)
            //     CheckBattleWinner();
            //
            // OnAbilityEnd?.Invoke(ability, card);
            // _resolveQueue.ResolveAll(0.5F);
            // RefreshBattle();
        }
    }
}