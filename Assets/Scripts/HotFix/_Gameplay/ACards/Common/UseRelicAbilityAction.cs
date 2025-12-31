using UnityEngine.Pool;

namespace MarbleHero
{
    public class UseRelicAbilityAction : AGameAction
    {
        AbilityData ability;
        ACreature caster;
        ARelic relic;

        public UseRelicAbilityAction(AbilityData ability, ACreature caster, ARelic relic)
        {
            this.ability = ability;
            this.caster = caster;
            this.relic = relic;
        }

        public override void update(float dt)
        {
            //Debug.Log("Trigger Ability " + ability.id + " : " + card.card_id);

            // OnAbilityStart?.Invoke(ability, card);
            // _battle.ability_trigger = card != null ? card.uid : "";
            // _battle.ability_played.Add(ability.Id);

            // if (IsAbilityRequireSelector(ability, caster, card))
            // return; //Wait for player to select

            UseAbility_Players(ability, caster, relic);
            UseAbility_Monsters(ability, caster, relic);
            UseAbility_Pawns(ability, caster, relic);
            UseAbility_Slots(ability, caster, relic);

            if (caster is APlayer player)
            {
                UseAbility_Cards(ability, player, relic);
                UseAbility_CardDatas(ability, player, relic);
            }

            ability.Do(caster, relic);

            AfterAbilityUsed(ability, caster, relic);
            isDone = true;
        }

        public void UseAbility_Players(AbilityData ability, ACreature caster, ARelic relic)
        {
            using var _ = ListPool<APlayer>.Get(out var targets);
            ability.GetTargetPlayers(caster, relic, ref targets);

            foreach (var t in targets)
                ability.Do_Player(caster, relic, t);
        }

        public void UseAbility_Monsters(AbilityData ability, ACreature caster, ARelic relic)
        {
            using var _ = ListPool<AMonster>.Get(out var targets);
            ability.GetTargetMonsters(caster, relic, ref targets);

            foreach (var t in targets)
                ability.Do_Monster(caster, relic, t);
        }

        public void UseAbility_Pawns(AbilityData ability, ACreature caster, ARelic relic)
        {
            using var _ = ListPool<APawn>.Get(out var targets);
            ability.GetTargetPawns(caster, relic, ref targets);

            foreach (var t in targets)
                ability.Do_Pawn(caster, relic, t);
        }

        public void UseAbility_Cards(AbilityData ability, APlayer caster, ARelic relic)
        {
            using var _ = ListPool<ACard>.Get(out var targets);
            ability.GetTargetCards(caster, relic, ref targets);

            foreach (var t in targets)
                ability.Do_Card(caster, relic, t);
        }

        public void UseAbility_Slots(AbilityData ability, ACreature caster, ARelic relic)
        {
            using var _ = ListPool<Slot>.Get(out var targets);
            ability.GetTargetSlots(caster, relic, ref targets);

            foreach (var t in targets)
                ability.Do_Slot(caster, relic, t);
        }

        public void UseAbility_CardDatas(AbilityData ability, APlayer caster, ARelic relic)
        {
            using var _ = ListPool<CardData>.Get(out var targets);
            ability.GetTargetCardDatas(caster, relic, ref targets);

            foreach (var t in targets)
                ability.Do_CardData(caster, relic, t);
        }

        public void AfterAbilityUsed(AbilityData ability, ACreature caster, ARelic relic)
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