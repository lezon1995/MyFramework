using System;

public class RandomXS128
{
    const double NORM_DOUBLE = 1.1102230246251565E-16d;
    const double NORM_FLOAT = 5.9604644775390625E-8d;

    long seed0;
    long seed1;

    static readonly Random _random = new();

    public RandomXS128()
    {
        setSeed(((long)_random.Next() << 32) ^ _random.Next());
    }

    public RandomXS128(long seed)
    {
        setSeed(seed);
    }

    public RandomXS128(long seed0, long seed1)
    {
        setState(seed0, seed1);
    }

    public long nextLong()
    {
        ulong s1 = (ulong)seed0;
        ulong s0 = (ulong)seed1;
        seed0 = (long)s0;
        s1 ^= s1 << 23;
        ulong result = s1 ^ s0 ^ (s1 >> 17) ^ (s0 >> 26);
        seed1 = (long)result;
        return (long)(result + s0);
    }

    protected int nextBits(int bits)
    {
        return (int)(nextLong() & ((1L << bits) - 1L));
    }

    public int nextInt()
    {
        return (int)nextLong();
    }

    public int nextInt(int n)
    {
        return (int)nextLong(n);
    }

    public long nextLong(long n)
    {
        if (n <= 0L)
            throw new ArgumentException("n must be positive");

        while (true)
        {
            long bits = (long)((ulong)nextLong() >> 1);
            long value = bits % n;
            if (bits - value + (n - 1L) >= 0L)
                return value;
        }
    }

    public double nextDouble()
    {
        return ((ulong)nextLong() >> 11) * NORM_DOUBLE;
    }

    public float nextFloat()
    {
        return (float)(((ulong)nextLong() >> 40) * NORM_FLOAT);
    }

    public bool nextBool()
    {
        return (nextLong() & 1L) != 0L;
    }

    public void nextBytes(byte[] bytes)
    {
        int i = bytes.Length;
        while (i != 0)
        {
            int n = (i < 8) ? i : 8;
            long bits = nextLong();
            for (; n-- != 0; bits >>= 8)
            {
                bytes[--i] = (byte)bits;
            }
        }
    }

    public void setSeed(long seed)
    {
        long _seed0 = murmurHash3(seed == 0L ? long.MinValue : seed);
        setState(_seed0, murmurHash3(_seed0));
    }

    public void setState(long _seed0, long _seed1)
    {
        seed0 = _seed0;
        seed1 = _seed1;
    }

    public long getState(int seed)
    {
        return seed == 0 ? seed0 : seed1;
    }

    static long murmurHash3(long x)
    {
        unchecked
        {
            x ^= (long)((ulong)x >> 33);
            x *= -49064778989728563L;
            x ^= (long)((ulong)x >> 33);
            x *= -4265267296055464877L;
            x ^= (long)((ulong)x >> 33);
            return x;
        }
    }
}