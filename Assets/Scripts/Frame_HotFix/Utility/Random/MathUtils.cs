using System;

public static class MathUtils
{
    public const float nanoToSec = 1.0E-9F;
    public const float FLOAT_ROUNDING_ERROR = 1.0E-6F;
    public const float PI = 3.1415927F;
    public const float PI2 = 6.2831855F;
    public const float E = 2.7182817F;
    const int SIN_BITS = 14;
    const int SIN_MASK = 16383;
    const int SIN_COUNT = 16384;
    const float radFull = PI2;
    const float degFull = 360.0F;
    const float radToIndex = 2607.5945F;
    const float degToIndex = 45.511112F;
    public const float radiansToDegrees = 57.295776F;
    public const float radDeg = 57.295776F;
    public const float degreesToRadians = 0.017453292F;

    static class Sin
    {
        internal static float[] table = new float[SIN_COUNT];

        static Sin()
        {
            int i;
            for (i = 0; i < SIN_COUNT; i++)
                table[i] = (float)Math.Sin(((i + 0.5F) / 16384.0F * 6.2831855F));
            for (i = 0; i < 360; i += 90)
                table[(int)(i * degToIndex) & 0x3FFF] = (float)Math.Sin((i * degreesToRadians));
        }
    }

    public static float sin(float radians)
    {
        return Sin.table[(int)(radians * radToIndex) & 0x3FFF];
    }

    public static float cos(float radians)
    {
        return Sin.table[(int)((radians + 1.5707964F) * radToIndex) & 0x3FFF];
    }

    public static float sinDeg(float degrees)
    {
        return Sin.table[(int)(degrees * degToIndex) & 0x3FFF];
    }

    public static float cosDeg(float degrees)
    {
        return Sin.table[(int)((degrees + 90.0F) * degToIndex) & 0x3FFF];
    }

    public static float atan2(float y, float x)
    {
        if (x == 0.0F)
        {
            return y switch
            {
                > 0.0F => 1.5707964F,
                0.0F => 0.0F,
                _ => -1.5707964F
            };
        }

        float z = y / x;
        if (Math.Abs(z) < 1.0F)
        {
            float f = z / (1.0F + 0.28F * z * z);
            if (x < 0.0F)
                return f + ((y < 0.0F) ? -PI : PI);
            return f;
        }

        float atan = 1.5707964F - z / (z * z + 0.28F);
        return (y < 0.0F) ? (atan - PI) : atan;
    }

    public static RandomXS128 _random = new();
    const int BIG_ENOUGH_INT = 16384;
    const double BIG_ENOUGH_FLOOR = 16384.0D;
    const double CEIL = 0.9999999D;
    const double BIG_ENOUGH_CEIL = 16384.999999999996D;
    const double BIG_ENOUGH_ROUND = 16384.5D;

    public static int random(int range)
    {
        return _random.nextInt(range + 1);
    }

    public static int random(int start, int end)
    {
        return start + _random.nextInt(end - start + 1);
    }

    public static long random(long range)
    {
        return (long)(_random.nextDouble() * range);
    }

    public static long random(long start, long end)
    {
        return start + (long)(_random.nextDouble() * (end - start));
    }

    public static bool randomBool()
    {
        return _random.nextBool();
    }

    public static bool randomBool(float chance)
    {
        return random() < chance;
    }

    public static float random()
    {
        return _random.nextFloat();
    }

    public static float random(float range)
    {
        return _random.nextFloat() * range;
    }

    public static float random(float start, float end)
    {
        return start + _random.nextFloat() * (end - start);
    }

    public static int randomSign()
    {
        return 0x1 | _random.nextInt() >> 31;
    }

    public static float randomTriangular()
    {
        return _random.nextFloat() - _random.nextFloat();
    }

    public static float randomTriangular(float max)
    {
        return (_random.nextFloat() - _random.nextFloat()) * max;
    }

    public static float randomTriangular(float min, float max, float mode)
    {
        float u = _random.nextFloat();
        float d = max - min;
        if (u <= (mode - min) / d)
            return min + (float)Math.Sqrt((u * d * (mode - min)));
        return max - (float)Math.Sqrt(((1.0F - u) * d * (max - mode)));
    }

    public static int nextPowerOfTwo(int value)
    {
        if (value == 0)
            return 1;
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }

    public static bool isPowerOfTwo(int value)
    {
        return (value != 0 && (value & value - 1) == 0);
    }

    public static short clamp(short value, short min, short max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    public static int clamp(int value, int min, int max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    public static long clamp(long value, long min, long max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    public static float clamp(float value, float min, float max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    public static float clamp01(float value)
    {
        var min = 0F;
        var max = 1F;
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    public static double clamp(double value, double min, double max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    public static float lerp(float fromValue, float toValue, float progress)
    {
        progress = clamp01(progress);
        return fromValue + (toValue - fromValue) * progress;
    }

    public static float lerpAngle(float fromRadians, float toRadians, float progress)
    {
        progress = clamp01(progress);
        float delta = (toRadians - fromRadians + radFull + PI) % radFull - PI;
        return (fromRadians + delta * progress + radFull) % radFull;
    }

    public static float lerpAngleDeg(float fromDegrees, float toDegrees, float progress)
    {
        progress = clamp01(progress);
        float delta = (toDegrees - fromDegrees + degFull + 180.0F) % degFull - 180.0F;
        return (fromDegrees + delta * progress + degFull) % degFull;
    }

    public static int floor(float value)
    {
        return (int)(value + BIG_ENOUGH_FLOOR) - BIG_ENOUGH_INT;
    }

    public static int floorPositive(float value)
    {
        return (int)value;
    }

    public static int ceil(float value)
    {
        return BIG_ENOUGH_INT - (int)(BIG_ENOUGH_FLOOR - value);
    }

    public static int ceilPositive(float value)
    {
        return (int)(value + CEIL);
    }

    public static int round(float value)
    {
        return (int)(value + BIG_ENOUGH_ROUND) - BIG_ENOUGH_INT;
    }

    public static int roundPositive(float value)
    {
        return (int)(value + 0.5F);
    }

    public static bool isZero(float value)
    {
        return (Math.Abs(value) <= FLOAT_ROUNDING_ERROR);
    }

    public static bool isZero(float value, float tolerance)
    {
        return (Math.Abs(value) <= tolerance);
    }

    public static bool isEqual(float a, float b)
    {
        return (Math.Abs(a - b) <= FLOAT_ROUNDING_ERROR);
    }

    public static bool isEqual(float a, float b, float tolerance)
    {
        return (Math.Abs(a - b) <= tolerance);
    }

    public static float log(float a, float value)
    {
        return (float)(Math.Log(value) / Math.Log(a));
    }

    public static float log2(float value)
    {
        return log(2.0F, value);
    }

    public static float remap(float x, float A, float B, float C, float D)
    {
        if (B - A == 0)
            return 0;
        return C + clamp01((x - A) / (B - A)) * (D - C);
    }
}