using System.Collections.Generic;

namespace MoreMountains
{
    public partial class Brick
    {
        public List<BrickPower> powers = new();

        public bool hasPower<T>()
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                if (powers[i] is T)
                    return true;
            }

            return false;
        }

        public bool tryGetPower<T>(out T power) where T : BrickPower
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                if (powers[i] is T t)
                {
                    power = t;
                    return true;
                }
            }

            power = null;
            return false;
        }

        public T addPower<T>() where T : BrickPower
        {
            var power = CLASS<BrickPower>(typeof(T));
            powers.add(power);
            return power as T;
        }

        public bool removePower<T>() where T : BrickPower
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                if (powers[i] is T t)
                {
                    powers.removeAt(i);
                    UN_CLASS(t);
                    return true;
                }
            }

            return false;
        }
        
        public void addBlock(int amount)
        {
            if (!tryGetPower<BrickBlockPower>(out var power))
            {
                power = addPower<BrickBlockPower>();
                power.with(this, amount);
            }
            else
            {
                power.addBlockAmount(amount);
            }
        }

        public void removeBlock(int amount)
        {
            if (tryGetPower<BrickBlockPower>(out var power))
            {
                power.removeBlockAmount(amount);
            }
        }
    }
}