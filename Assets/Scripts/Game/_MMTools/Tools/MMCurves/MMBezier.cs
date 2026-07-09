using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    public struct MMBezier
    {
        Vector3 p0;
        Vector3 p1;
        Vector3 p2;

        public MMBezier(Vector3 _p0, Vector3 _p1, Vector3 _p2)
        {
            p0 = _p0;
            p1 = _p1;
            p2 = _p2;
        }

        //曲线总长度
        double total_length => Len(1);
        double A => 4 * (ax * ax + ay * ay);
        double B => 4 * (ax * bx + ay * by);
        double C => bx * bx + by * by;

        double ax => p0.x - 2 * p1.x + p2.x;
        double ay => p0.y - 2 * p1.y + p2.y;
        double bx => 2 * p1.x - 2 * p0.x;
        double by => 2 * p1.y - 2 * p0.y;

        public float GetLength()
        {
            return (float)Len(1);
        }

        public Vector3 GetPoint(double t)
        {
            t = clamp(t);
            //如果按照线形增长,此时对应的曲线长度
            double l = t * total_length;
            //根据L函数的反函数，求得l对应的t值
            t = InvertLen(t, l);
            //根据贝塞尔曲线函数，求得取得此时的x,y坐标
            var a = (1 - t) * (1 - t);
            var b = 2 * (1 - t) * t;
            var c = t * t;

            float x = (float)(a * p0.x + b * p1.x + c * p2.x);
            float y = (float)(a * p0.y + b * p1.y + c * p2.y);
            float z = (float)(a * p0.z + b * p1.z + c * p2.z);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// 长度函数反函数，使用牛顿切线法求解
        /// X(n+1) = Xn - F(Xn)/F'(Xn)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        double InvertLen(double t, double l)
        {
            double t1 = t, t2;
            do
            {
                var speed = Speed(t1);
                if (speed == 0)
                {
                    t2 = 0;
                    break;
                }

                t2 = t1 - (Len(t1) - l) / speed;
                if (abs(t1 - t2) < 0.000001D)
                    break;
                t1 = t2;
            } while (true);

            return t2;
        }


        /// <summary>
        /// 速度函数
        /// s(t_) = Sqrt[A*t*t+B*t+C]
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        double Speed(double t)
        {
            return sqrt(A * t * t + B * t + C);
        }

        /// <summary>
        /// 长度函数
        /// L(t) = Integrate[s[t], t]
        ///L(t_) = ((2*Sqrt[A]*(2*A*t*Sqrt[C + t*(B + A*t)] + B*(-Sqrt[C] + Sqrt[C + t*(B + A*t)])) +
        ///(B^2 - 4*A*C) (Log[B + 2*Sqrt[A]*Sqrt[C]] - Log[B + 2*A*t + 2 Sqrt[A]*Sqrt[C + t*(B + A*t)]]))
        ///(8* A^(3/2)));
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        double Len(double t)
        {
            double temp1 = sqrt(C + t * (B + A * t));
            double temp2 = 2 * A * t * temp1 + B * (temp1 - sqrt(C));
            double temp3 = log(B + 2 * sqrt(A) * sqrt(C));
            double temp4 = log(B + 2 * A * t + 2 * sqrt(A) * temp1);
            double temp5 = 2 * sqrt(A) * temp2;
            double temp6 = (B * B - 4 * A * C) * (temp3 - temp4);
            var f = 8 * pow(A, 1.5F);
            if (f == 0)
                return 0;

            return (temp5 + temp6) / f;
        }


        static double sqrt(double d) => Math.Sqrt(d);
        static double log(double d) => Math.Log(d);
        static double abs(double d) => Math.Abs(d);
        static double clamp(double d) => Math.Clamp(d, 0, 1);
        static double pow(double x, double y) => Math.Pow(x, y);
    }
}