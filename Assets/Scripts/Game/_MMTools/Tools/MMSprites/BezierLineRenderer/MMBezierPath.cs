using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    // [RequireComponent(typeof(LineRenderer))]
    // [AddComponentMenu("More Mountains/Tools/Sprites/MMBezierLineRenderer")]
    public class MMBezierPath : MonoBehaviour
    {
        public Transform Mover;
        public LineRenderer LineRenderer;

        public Transform Start;
        public Transform Mid;
        public Transform End;

        //曲线分割的份数
        public int STEP = 100;

        [Range(0.01F, 5F)]
        public float Duration = 1F;

        public Vector2 P0 => Start.position;
        public Vector2 P1 => Mid.position;
        public Vector2 P2 => End.position;

        //曲线总长度
        double total_length => Len(1);
        double A => 4 * (ax * ax + ay * ay);
        double B => 4 * (ax * bx + ay * by);
        double C => bx * bx + by * by;

        float ax => P0.x - 2 * P1.x + P2.x;
        float ay => P0.y - 2 * P1.y + P2.y;
        float bx => 2 * P1.x - 2 * P0.x;
        float by => 2 * P1.y - 2 * P0.y;

        void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();

            // P0 = new(50, 50);
            // P1 = new(500, 600);
            // P2 = new(800, 200);

            LineRenderer.positionCount = 3;
            LineRenderer.SetPosition(0, P0);
            LineRenderer.SetPosition(1, P1);
            LineRenderer.SetPosition(2, P2);

            Timing.RunCoroutine(Test());
        }

        int nIndex;
        float timeElapsed;

        IEnumerator<float> Test()
        {
            // while (0 <= nIndex && nIndex <= STEP)
            while (true)
            {
                // double t = (double)nIndex / STEP;
                double t = clamp((double)timeElapsed / Duration);
                //如果按照线形增长,此时对应的曲线长度
                double l = t * total_length;
                //根据L函数的反函数，求得l对应的t值
                t = InvertLen(t, l);

                //根据贝塞尔曲线函数，求得取得此时的x,y坐标
                float x = (float)((1 - t) * (1 - t) * P0.x + 2 * (1 - t) * t * P1.x + t * t * P2.x);
                float y = (float)((1 - t) * (1 - t) * P0.y + 2 * (1 - t) * t * P1.y + t * t * P2.y);

                //取整
                var pos = new Vector2(x, y);
                Mover.position = pos;

                // nIndex++;
                timeElapsed += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
                // yield return Timing.WaitForSeconds(0.1F);

                if (pos == P2)
                {
                    Mover.position = P0;
                    Mover.GetComponent<TrailRenderer>().Clear();
                    timeElapsed = 0F;
                }
            }
        }

        void Update()
        {
            LineRenderer.positionCount = 3;
            LineRenderer.SetPosition(0, P0);
            LineRenderer.SetPosition(1, P1);
            LineRenderer.SetPosition(2, P2);
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
                t2 = t1 - (Len(t1) - l) / Speed(t1);
                if (abs(t1 - t2) < 0.000001)
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
            return (temp5 + temp6) / (8 * pow(A, 1.5));
        }


        static double sqrt(double d) => Math.Sqrt(d);
        static double log(double d) => Math.Log(d);
        static double abs(double d) => Math.Abs(d);
        static double clamp(double d) => Math.Clamp(d, 0, 1);
        static double pow(double x, double y) => Math.Pow(x, y);
    }
}