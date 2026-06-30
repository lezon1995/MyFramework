    public class Rand
    {
        public RandomXS128 _random;
        public int counter;

        public Rand() : this(MathUtils.random(9999), MathUtils.random(99))
        {
        }

        public Rand(long seed)
        {
            _random = new(seed);
        }

        public Rand(long seed, int counter)
        {
            _random = new(seed);
            for (int i = 0; i < counter; i++)
                random(999);
        }

        public Rand copy()
        {
            var copied = new Rand
            {
                _random = new RandomXS128(_random.getState(0), _random.getState(1)),
                counter = counter
            };
            return copied;
        }

        public void setCounter(int targetCounter)
        {
            if (counter < targetCounter)
            {
                int count = targetCounter - counter;
                for (int i = 0; i < count; i++)
                    randomBool();
            }
            else
            {
                //log("Counter is already higher than target counter!");
            }
        }

        public int random(int range)
        {
            counter++;
            return _random.nextInt(range + 1);
        }

        public int random(int start, int end)
        {
            counter++;
            return start + _random.nextInt(end - start + 1);
        }

        public long random(long range)
        {
            counter++;
            return (long)(_random.nextDouble() * range);
        }

        public long random(long start, long end)
        {
            counter++;
            return start + (long)(_random.nextDouble() * (end - start));
        }
        
        public int randomInt()
        {
            counter++;
            return _random.nextInt();
        }

        public long randomLong()
        {
            counter++;
            return _random.nextLong();
        }
        
        public bool randomBool()
        {
            counter++;
            return _random.nextBool();
        }

        public bool randomBool(float chance)
        {
            counter++;
            return _random.nextFloat() < chance;
        }

        public float random()
        {
            counter++;
            return _random.nextFloat();
        }

        public float random(float range)
        {
            counter++;
            return _random.nextFloat() * range;
        }

        public float random(float start, float end)
        {
            counter++;
            return start + _random.nextFloat() * (end - start);
        }
    }