using System;

namespace MoreMountains
{
    public static class MMLerp
    {
        public static Interpolation.Elastic elastic = new(2.0F, 10.0F, 7, 1.0F);
        public static Interpolation.ElasticIn elasticIn = new(2.0F, 10.0F, 6, 1.0F);
        public static Interpolation.ElasticOut elasticOut = new(2.0F, 10.0F, 7, 1.0F);
        public static Interpolation.Swing swing = new(1.5F);
        public static Interpolation.SwingIn swingIn = new(2.0F);
        public static Interpolation.SwingOut swingOut = new(2.0F);
        public static Interpolation.Bounce bounce = new(4);
        public static Interpolation.BounceIn bounceIn = new(4);
        public static Interpolation.BounceOut bounceOut = new(4);
        public static Interpolation.Exp exp10 = new(2.0F, 10.0F);
        public static Interpolation.ExpIn exp10In = new(2.0F, 10.0F);
        public static Interpolation.ExpOut exp10Out = new(2.0F, 10.0F);
        public static Interpolation.Exp exp5 = new(2.0F, 5.0F);
        public static Interpolation.ExpIn exp5In = new(2.0F, 5.0F);
        public static Interpolation.ExpOut exp5Out = new(2.0F, 5.0F);
        public static Interpolation.Pow pow4 = new(4);
        public static Interpolation.PowIn pow4In = new(4);
        public static Interpolation.PowOut pow4Out = new(4);
        public static Interpolation.Pow pow5 = new(5);
        public static Interpolation.PowIn pow5In = new(5);
        public static Interpolation.PowOut pow5Out = new(5);
        public static Interpolation.Pow pow2 = new(2);
        public static Interpolation.PowIn pow2In = new(2);
        public static Interpolation.PowOut pow2Out = new(2);
        public static Interpolation.Pow pow3 = new(3);
        public static Interpolation.PowIn pow3In = new(3);
        public static Interpolation.PowOut pow3Out = new(3);

        public static Interpolation.Linear linear = new();
        public static Interpolation.Smooth smooth = new();
        public static Interpolation.Smooth2 smooth2 = new();
        public static Interpolation.Smoother smoother = new();
        public static Interpolation fade = smoother;
        public static Interpolation.Pow2InInverse pow2InInverse = new();
        public static Interpolation.Pow2OutInverse pow2OutInverse = new();
        public static Interpolation.Pow3InInverse pow3InInverse = new();
        public static Interpolation.Pow3OutInverse pow3OutInverse = new();
        public static Interpolation.Sine sine = new();
        public static Interpolation.SineIn sineIn = new();
        public static Interpolation.SineOut sineOut = new();
        public static Interpolation.Circle circle = new();
        public static Interpolation.CircleIn circleIn = new();
        public static Interpolation.CircleOut circleOut = new();
    }

    public abstract class Interpolation
    {
        protected abstract float apply(float paramFloat);

        public float apply(float start, float end, float a) => start + (end - start) * apply(a);

        public class Linear : Interpolation
        {
            protected override float apply(float a) => a;
        }

        public class Smooth : Interpolation
        {
            protected override float apply(float a) => a * a * (3.0F - 2.0F * a);
        }

        public class Smooth2 : Interpolation
        {
            protected override float apply(float a)
            {
                a = a * a * (3.0F - 2.0F * a);
                return a * a * (3.0F - 2.0F * a);
            }
        }

        public class Smoother : Interpolation
        {
            protected override float apply(float a)
            {
                return Math.Clamp(a * a * a * (a * (a * 6.0F - 15.0F) + 10.0F), 0.0F, 1.0F);
            }
        }

        public class Pow2InInverse : Interpolation
        {
            protected override float apply(float a)
            {
                return (float)Math.Sqrt(a);
            }
        }

        public class Pow2OutInverse : Interpolation
        {
            protected override float apply(float a)
            {
                return 1.0F - (float)Math.Sqrt(-(a - 1.0F));
            }
        }

        public class Pow3InInverse : Interpolation
        {
            protected override float apply(float a)
            {
                return (float)Math.Cbrt(a);
            }
        }

        public class Pow3OutInverse : Interpolation
        {
            protected override float apply(float a)
            {
                return 1.0F - (float)Math.Cbrt(-(a - 1.0F));
            }
        }

        public class Sine : Interpolation
        {
            protected override float apply(float a)
            {
                return (float)((1.0F - Math.Cos(a * 3.1415927F)) / 2.0F);
            }
        }

        public class SineIn : Interpolation
        {
            protected override float apply(float a)
            {
                return (float)(1.0F - Math.Cos(a * 3.1415927F / 2.0F));
            }
        }

        public class SineOut : Interpolation
        {
            protected override float apply(float a)
            {
                return (float)Math.Sin(a * 3.1415927F / 2.0F);
            }
        }

        public class Circle : Interpolation
        {
            protected override float apply(float a)
            {
                if (a <= 0.5F)
                {
                    a *= 2.0F;
                    return (1.0F - (float)Math.Sqrt((1.0F - a * a))) / 2.0F;
                }

                a--;
                a *= 2.0F;
                return ((float)Math.Sqrt((1.0F - a * a)) + 1.0F) / 2.0F;
            }
        }

        public class CircleIn : Interpolation
        {
            protected override float apply(float a)
            {
                return 1.0F - (float)Math.Sqrt((1.0F - a * a));
            }
        }

        public class CircleOut : Interpolation
        {
            protected override float apply(float a)
            {
                a--;
                return (float)Math.Sqrt((1.0F - a * a));
            }
        }

        public class Pow : Interpolation
        {
            protected int power;

            public Pow(int power)
            {
                this.power = power;
            }

            protected override float apply(float a)
            {
                if (a <= 0.5F)
                    return (float)Math.Pow((a * 2.0F), power) / 2.0F;
                return (float)Math.Pow(((a - 1.0F) * 2.0F), power) / ((power % 2 == 0) ? -2 : 2) + 1.0F;
            }
        }

        public class PowIn : Pow
        {
            public PowIn(int power) : base(power)
            {
            }

            protected override float apply(float a)
            {
                return (float)Math.Pow(a, power);
            }
        }

        public class PowOut : Pow
        {
            public PowOut(int power) : base(power)
            {
            }

            protected override float apply(float a)
            {
                return (float)Math.Pow((a - 1.0F), power) * ((power % 2 == 0) ? -1 : 1) + 1.0F;
            }
        }

        public class Exp : Interpolation
        {
            protected float value;
            protected float power;
            protected float min;
            protected float scale;

            public Exp(float value, float power)
            {
                this.value = value;
                this.power = power;
                min = (float)Math.Pow(value, -power);
                scale = 1.0F / (1.0F - min);
            }

            protected override float apply(float a)
            {
                if (a <= 0.5F)
                    return ((float)Math.Pow(value, (power * (a * 2.0F - 1.0F))) - min) * scale / 2.0F;
                return (2.0F - ((float)Math.Pow(value, (-power * (a * 2.0F - 1.0F))) - min) * scale) / 2.0F;
            }
        }

        public class ExpIn : Exp
        {
            public ExpIn(float value, float power) : base(value, power)
            {
            }

            protected override float apply(float a)
            {
                return ((float)Math.Pow(value, (power * (a - 1.0F))) - min) * scale;
            }
        }

        public class ExpOut : Exp
        {
            public ExpOut(float value, float power) : base(value, power)
            {
            }

            protected override float apply(float a)
            {
                return 1.0F - ((float)Math.Pow(value, (-power * a)) - min) * scale;
            }
        }

        public class Elastic : Interpolation
        {
            protected float value;
            protected float power;
            protected float scale;
            protected float bounces;

            public Elastic(float value, float power, int bounces, float scale)
            {
                this.value = value;
                this.power = power;
                this.scale = scale;
                this.bounces = bounces * 3.1415927F * ((bounces % 2 == 0) ? 1 : -1);
            }

            protected override float apply(float a)
            {
                if (a <= 0.5F)
                {
                    a *= 2.0F;
                    return (float)((float)Math.Pow(value, (power * (a - 1.0F))) * Math.Sin(a * bounces) * scale / 2.0F);
                }

                a = 1.0F - a;
                a *= 2.0F;
                return (float)(1.0F - (float)Math.Pow(value, (power * (a - 1.0F))) * Math.Sin(a * bounces) * scale / 2.0F);
            }
        }

        public class ElasticIn : Elastic
        {
            public ElasticIn(float value, float power, int bounces, float scale) : base(value, power, bounces, scale)
            {
            }

            protected override float apply(float a)
            {
                if (a >= 0.99D)
                    return 1.0F;
                return (float)((float)Math.Pow(value, (power * (a - 1.0F))) * Math.Sin(a * bounces) * scale);
            }
        }

        public class ElasticOut : Elastic
        {
            public ElasticOut(float value, float power, int bounces, float scale) : base(value, power, bounces, scale)
            {
            }

            protected override float apply(float a)
            {
                if (a == 0.0F)
                    return 0.0F;
                a = 1.0F - a;
                return (float)(1.0F - (float)Math.Pow(value, (power * (a - 1.0F))) * Math.Sin(a * bounces) * scale);
            }
        }

        public class Bounce : BounceOut
        {
            public Bounce(float[] widths, float[] heights) : base(widths, heights)
            {
            }

            public Bounce(int bounces) : base(bounces)
            {
            }

            float Get(float a)
            {
                float test = a + widths[0] / 2.0F;
                if (test < widths[0])
                    return test / widths[0] / 2.0F - 1.0F;
                return base.apply(a);
            }

            protected override float apply(float a)
            {
                if (a <= 0.5F)
                    return (1.0F - Get(1.0F - a * 2.0F)) / 2.0F;
                return Get(a * 2.0F - 1.0F) / 2.0F + 0.5F;
            }
        }

        public class BounceOut : Interpolation
        {
            protected float[] widths;
            protected float[] heights;

            public BounceOut(float[] widths, float[] heights)
            {
                if (widths.Length != heights.Length)
                    throw new ArgumentException("Must be the same number of widths and heights.");
                this.widths = widths;
                this.heights = heights;
            }

            public BounceOut(int bounces)
            {
                if (bounces < 2 || bounces > 5)
                    throw new ArgumentException("bounces cannot be < 2 or > 5: " + bounces);
                widths = new float[bounces];
                heights = new float[bounces];
                heights[0] = 1.0F;
                switch (bounces)
                {
                    case 2:
                        widths[0] = 0.6F;
                        widths[1] = 0.4F;
                        heights[1] = 0.33F;
                        break;
                    case 3:
                        widths[0] = 0.4F;
                        widths[1] = 0.4F;
                        widths[2] = 0.2F;
                        heights[1] = 0.33F;
                        heights[2] = 0.1F;
                        break;
                    case 4:
                        widths[0] = 0.34F;
                        widths[1] = 0.34F;
                        widths[2] = 0.2F;
                        widths[3] = 0.15F;
                        heights[1] = 0.26F;
                        heights[2] = 0.11F;
                        heights[3] = 0.03F;
                        break;
                    case 5:
                        widths[0] = 0.3F;
                        widths[1] = 0.3F;
                        widths[2] = 0.2F;
                        widths[3] = 0.1F;
                        widths[4] = 0.1F;
                        heights[1] = 0.45F;
                        heights[2] = 0.3F;
                        heights[3] = 0.15F;
                        heights[4] = 0.06F;
                        break;
                }

                widths[0] *= 2.0F;
            }

            protected override float apply(float a)
            {
                if (a == 1.0F)
                    return 1.0F;
                a += widths[0] / 2.0F;
                float width = 0.0F, height = 0.0F;
                for (int i = 0, n = widths.Length; i < n; i++)
                {
                    width = widths[i];
                    if (a <= width)
                    {
                        height = heights[i];
                        break;
                    }

                    a -= width;
                }

                a /= width;
                float z = 4.0F / width * height * a;
                return 1.0F - (z - z * a) * width;
            }
        }

        public class BounceIn : BounceOut
        {
            public BounceIn(float[] widths, float[] heights) : base(widths, heights)
            {
            }

            public BounceIn(int bounces) : base(bounces)
            {
            }

            protected override float apply(float a)
            {
                return 1.0F - base.apply(1.0F - a);
            }
        }

        public class Swing : Interpolation
        {
            float scale;

            public Swing(float scale)
            {
                this.scale = scale * 2.0F;
            }

            protected override float apply(float a)
            {
                if (a <= 0.5F)
                {
                    a *= 2.0F;
                    return a * a * ((scale + 1.0F) * a - scale) / 2.0F;
                }

                a--;
                a *= 2.0F;
                return a * a * ((scale + 1.0F) * a + scale) / 2.0F + 1.0F;
            }
        }

        public class SwingOut : Interpolation
        {
            float scale;

            public SwingOut(float scale)
            {
                this.scale = scale;
            }

            protected override float apply(float a)
            {
                a--;
                return a * a * ((scale + 1.0F) * a + scale) + 1.0F;
            }
        }

        public class SwingIn : Interpolation
        {
            float scale;

            public SwingIn(float scale)
            {
                this.scale = scale;
            }

            protected override float apply(float a)
            {
                return a * a * ((scale + 1.0F) * a - scale);
            }
        }
    }
}