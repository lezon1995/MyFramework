namespace MarbleHero;

public class MonsterBlock : ABlock
{
    AMonster monster;

    public MonsterBlock(AMonster p)
    {
        monster = p;
    }

    public override void addBlock(int blockAmount)
    {
        int tmp = blockAmount;

        foreach (var r in monster.relics)
            tmp = r.onPlayerGainedBlock(tmp);

        if (tmp > 0.0F)
            foreach (var m in monster.powers)
                m.onGainedBlock(tmp);

        bool effect = currentBlock == 0;
        
        foreach (var p in player.powers)
            tmp = p.onMonsterGainedBlock(tmp);

        currentBlock += floor(tmp);
        if (currentBlock > 999)
            currentBlock = 999;

        if (effect && currentBlock > 0)
        {
            // gainBlockAnimation();
        }
        else if (blockAmount > 0)
        {
            // Color tmpCol = Settings.GOLD_COLOR.cpy();
            // tmpCol.a = blockTextColor.a;
            // blockTextColor = tmpCol;
            // blockScale = 5.0F;
        }
    }

    public void loseBlock(int amount, bool noAnimation)
    {
        bool effect = currentBlock != 0;
        currentBlock -= amount;
        if (currentBlock < 0)
            currentBlock = 0;

        if (currentBlock == 0 && effect)
        {
            // if (!noAnimation)
            // ADungeon.effectList.add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
        }
        else if (currentBlock > 0 && amount > 0)
        {
            // Color tmp = Color.white;
            // tmp.a = blockTextColor.a;
            // blockTextColor = tmp;
            // blockScale = 5.0F;
        }
    }

    public override void loseBlock()
    {
        loseBlock(currentBlock);
    }

    public void loseBlock(bool noAnimation)
    {
        loseBlock(currentBlock, noAnimation);
    }

    public override void loseBlock(int amount)
    {
        loseBlock(amount, false);
    }

    public override void brokeBlock()
    {
        foreach (var r in player.relics)
            r.onBlockBroken(monster);

        // ADungeon.effectList.add(new HbBlockBrokenEffect(hb.cX - hb.width / 2.0F + BLOCK_ICON_X, hb.cY - hb.height / 2.0F + BLOCK_ICON_Y));
        // Game.sound.play("BLOCK_BREAK");
    }

    public override void decrementBlock(ref int damageAmount)
    {
        if (currentBlock > 0)
        {
            Game.screenShake.shake(ScreenShake.ShakeIntensity.MED, ScreenShake.ShakeDur.SHORT, false);
            if (damageAmount > currentBlock)
            {
                damageAmount -= currentBlock;
                // if (Settings.SHOW_DMG_BLOCK)
                // ADungeon.effectList.add(new BlockedNumberEffect(hb.cX, hb.cY + hb.height / 2.0F, Integer.toString(currentBlock)));
                loseBlock();
                brokeBlock();
            }
            else if (damageAmount == currentBlock)
            {
                damageAmount = 0;
                loseBlock();
                brokeBlock();
                // ADungeon.effectList.add(new BlockedWordEffect(this, hb.cX, hb.cY, TEXT[1]));
            }
            else
            {
                sound.play("BLOCK_ATTACK");
                loseBlock(damageAmount);
                // for (int i = 0; i < 18; i++)
                // ADungeon.effectList.add(new BlockImpactLineEffect(hb.cX, hb.cY));
                // if (Settings.SHOW_DMG_BLOCK)
                // ADungeon.effectList.add(new BlockedNumberEffect(hb.cX, hb.cY + hb.height / 2.0F, Integer.toString(damageAmount)));
                damageAmount = 0;
            }
        }
    }
}