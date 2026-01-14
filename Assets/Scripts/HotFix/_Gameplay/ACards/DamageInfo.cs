namespace MarbleHero
{
    public class DamageInfo
    {
        public ACreature owner;
        public string name;
        public DamageType type;
        public int Value;
        public int output;
        public bool isModified;

        public DamageInfo(ACreature damageSource, int value, DamageType type)
        {
            owner = damageSource;
            this.type = type;
            Value = value;
            output = value;
        }

        public DamageInfo(ACreature owner, int value) : this(owner, value, DamageType.NORMAL)
        {
        }

        public void applyPowers(ACreature owner, ACreature target)
        {
            output = Value;
            isModified = false;
            float tmp = output;
            if (!owner.isPlayer)
            {
                if (Settings.isEndless/* && player.hasBlight("DeadlyEnemies")*/)
                {
                    // float mod = player.getBlight("DeadlyEnemies").effectFloat();
                    // tmp *= mod;
                    if (Value != (int)tmp)
                        isModified = true;
                }

                foreach (var p in owner.powers)
                {
                    tmp = p.atDamageGive(tmp, type);
                    if (Value != (int)tmp)
                        isModified = true;
                }

                foreach (var p in target.powers)
                {
                    tmp = p.atDamageReceive(tmp, type);
                    if (Value != (int)tmp)
                        isModified = true;
                }

                // tmp = player.stance.atDamageReceive(tmp, type);
                if (Value != (int)tmp)
                    isModified = true;
                foreach (var p in owner.powers)
                {
                    tmp = p.atDamageFinalGive(tmp, type);
                    if (Value != (int)tmp)
                        isModified = true;
                }

                foreach (var p in target.powers)
                {
                    tmp = p.atDamageFinalReceive(tmp, type);
                    if (Value != (int)tmp)
                        isModified = true;
                }
            }
            else
            {
                foreach (var p in owner.powers)
                {
                    tmp = p.atDamageGive(tmp, type);
                    if (Value != (int)tmp)
                        isModified = true;
                }

                // tmp = player.stance.atDamageGive(tmp, type);
                if (Value != (int)tmp)
                    isModified = true;
                foreach (var p in target.powers)
                {
                    tmp = p.atDamageReceive(tmp, type);
                    if (Value != (int)tmp)
                        isModified = true;
                }

                foreach (var p in owner.powers)
                {
                    tmp = p.atDamageFinalGive(tmp, type);
                    if (Value != (int)tmp)
                        isModified = true;
                }

                foreach (var p in target.powers)
                {
                    tmp = p.atDamageFinalReceive(tmp, type);
                    if (Value != (int)tmp)
                        isModified = true;
                }
            }

            output = MathUtils.floor(tmp);
            if (output < 0)
                output = 0;
        }

        public void applyEnemyPowersOnly(ACreature target)
        {
            output = Value;
            isModified = false;
            float tmp = output;
            foreach (var p in target.powers)
            {
                tmp = p.atDamageReceive(output, type);
                if (Value != output)
                    isModified = true;
            }

            foreach (var p in target.powers)
            {
                tmp = p.atDamageFinalReceive(output, type);
                if (Value != output)
                    isModified = true;
            }

            if (tmp < 0.0F)
                tmp = 0.0F;
            output = MathUtils.floor(tmp);
        }

        // public static int[] createDamageMatrix(int baseDamage)
        // {
        //     return createDamageMatrix(baseDamage, false);
        // }
        //
        // public static int[] createDamageMatrix(int baseDamage, bool isPureDamage)
        // {
        //     int[] retVal = new int[(monsters).monsters.size()];
        //     for (int i = 0; i < retVal.Length; i++)
        //     {
        //         DamageInfo info = new DamageInfo(player, baseDamage);
        //         if (!isPureDamage)
        //             info.applyPowers(player, (monsters).monsters.get(i));
        //         retVal[i] = info.output;
        //     }
        //
        //     return retVal;
        // }

        /*public static int[] createDamageMatrix(int baseDamage, bool isPureDamage, bool isOrbDamage)
        {
            int[] retVal = new int[(monsters).monsters.size()];
            for (int i = 0; i < retVal.Length; i++)
            {
                DamageInfo info = new DamageInfo(player, baseDamage);
                if (isOrbDamage && (monsters).monsters.get(i).hasPower("Lockon"))
                    info.output = (int)(info.Value * 1.5F);
                retVal[i] = info.output;
            }

            return retVal;
        }*/

        public enum DamageType
        {
            NORMAL,
            THORNS,
            HP_LOSS
        }
    }
}