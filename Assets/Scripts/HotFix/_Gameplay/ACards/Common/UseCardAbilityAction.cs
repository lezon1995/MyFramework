using UnityEngine.Pool;

namespace MarbleHero
{
    public class UseCardAbilityAction : AGameAction
    {
        AbilityData ability;
        ACreature caster;
        ACard card;

        public UseCardAbilityAction(AbilityData ability, ACreature caster, ACard card)
        {
            this.ability = ability;
            this.caster = caster;
            this.card = card;
        }

        public override void update(float dt)
        {
            Log.Debug("Trigger Ability " + ability.Id + " : " + card.cardID);

            // OnAbilityStart?.Invoke(ability, card);
            // _battle.ability_trigger = card != null ? card.uid : "";
            // _battle.ability_played.Add(ability.Id);

            // if (IsAbilityRequireSelector(ability, caster, card))
            // return; //Wait for player to select

            UseAbility_Target(ability, caster, card);
            UseAbility_Players(ability, caster, card);
            UseAbility_Monsters(ability, caster, card);
            UseAbility_Pawns(ability, caster, card);
            UseAbility_Slots(ability, caster, card);

            if (caster is APlayer player)
            {
                UseAbility_Cards(ability, player, card);
                UseAbility_CardDatas(ability, player, card);
            }

            ability.Do(caster, card);

            AfterAbilityUsed(ability, caster, card);
            isDone = true;
        }

        public void UseAbility_Target(AbilityData ability, ACreature caster, ACard card)
        {
            if (!ability.IsPlayTarget())
                return;

            var slot = card.slot;
            if (room.getSlotPawn(slot, out var pawn))
            {
                if (ability.CanTarget_Pawn(caster, card, pawn))
                {
                    // _battle.last_targeted_character_uid = pawn.uid;
                    ability.Do_Pawn(caster, card, pawn);
                }
            }
            else
            {
                if (ability.CanTarget_Slot(caster, card, slot))
                    ability.Do_Slot(caster, card, slot);
            }
        }

        public void UseAbility_Players(AbilityData ability, ACreature caster, ACard card)
        {
            using var _ = ListPool<APlayer>.Get(out var targets);
            ability.GetTargetPlayers(caster, card, ref targets);

            foreach (var t in targets)
                ability.Do_Player(caster, card, t);
        }

        public void UseAbility_Monsters(AbilityData ability, ACreature caster, ACard card)
        {
            using var _ = ListPool<AMonster>.Get(out var targets);
            ability.GetTargetMonsters(caster, card, ref targets);

            foreach (var t in targets)
                ability.Do_Monster(caster, card, t);
        }

        public void UseAbility_Pawns(AbilityData ability, ACreature caster, ACard card)
        {
            using var _ = ListPool<APawn>.Get(out var targets);
            ability.GetTargetPawns(caster, card, ref targets);

            foreach (var t in targets)
                ability.Do_Pawn(caster, card, t);
        }

        public void UseAbility_Cards(AbilityData ability, APlayer caster, ACard card)
        {
            using var _ = ListPool<ACard>.Get(out var targets);
            ability.GetTargetCards(caster, card, ref targets);

            foreach (var t in targets)
                ability.Do_Card(caster, card, t);
        }

        public void UseAbility_Slots(AbilityData ability, ACreature caster, ACard card)
        {
            using var _ = ListPool<Slot>.Get(out var targets);
            ability.GetTargetSlots(caster, card, ref targets);

            foreach (var t in targets)
                ability.Do_Slot(caster, card, t);
        }

        public void UseAbility_CardDatas(AbilityData ability, APlayer caster, ACard card)
        {
            using var _ = ListPool<CardData>.Get(out var targets);
            ability.GetTargetCardDatas(caster, card, ref targets);

            foreach (var t in targets)
                ability.Do_CardData(caster, card, t);
        }

        public void AfterAbilityUsed(AbilityData ability, ACreature caster, ACard card)
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