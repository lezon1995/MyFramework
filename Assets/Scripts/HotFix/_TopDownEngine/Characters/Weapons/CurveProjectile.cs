using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A thrown object type of projectile, useful for grenades and such
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/CurveProjectile")]
    public class CurveProjectile : Projectile
    {
        public float FlyDuration = 1;
        public Vector2 DivertRange = new(90, 150);
        public Vector2 DivertLength = new(300, 1000);
        public bool RandomDivert;

        public int CurveCount = 1;
        public bool RandomCurveCount;

        float timeElapsed;
        Vector3 divertPoint;

        int curCurveCount;
        float curDivertLength;
        int bezierPathIndex;

        static bool divertFlag;

        List<(Vector3, Vector3, Func<Vector3>, float)> _list = new();

        protected override void Awake()
        {
            base.Awake();

            if (_damageOnTouch)
            {
                _damageOnTouch.TriggerFilter = DamageOnTouch.TriggerMask.IgnoreAll;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            timeElapsed = 0;
            bezierPathIndex = 0;
            _list.Clear();
        }

        public override void SetTarget(Transform target)
        {
            base.SetTarget(target);

            if (_target == null)
                return;
            
            var direction = (_target.position - transform.position).normalized;

            curDivertLength = Random.Range(DivertLength.x, DivertLength.y);

            // 旋转角度（度数）
            float angle = Random.Range(DivertRange.x, DivertRange.y);

            if (RandomDivert)
            {
                if (divertFlag)
                {
                    angle = -angle;
                }

                divertFlag = !divertFlag;
            }

            // 创建旋转四元数
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 旋转向量
            Vector3 rotatedVector = rotation * direction;

            divertPoint = rotatedVector * (curDivertLength / 100) + _startPosition;

            curCurveCount = CurveCount;
            if (RandomCurveCount)
            {
                curCurveCount = MMMaths.Chance(0.5F) ? 1 : 2;
            }

            switch (curCurveCount)
            {
                case 1:
                    _list.Add((_startPosition, divertPoint, () => _target.position, FlyDuration));
                    break;
                case 2:
                    var bezier1 = new MMBezier(_startPosition, divertPoint, (_target.position + _startPosition) / 2F);
                    var mid = (_target.position + _startPosition) / 2F;
                    var bezier2 = new MMBezier(mid, mid + (mid - divertPoint).normalized * (curDivertLength / 100), _target.position);
                    var length1 = bezier1.GetLength();
                    var length2 = bezier2.GetLength();
                    var flyDuration1 = length1 / (length1 + length2) * FlyDuration;
                    var flyDuration2 = length2 / (length1 + length2) * FlyDuration;
                    _list.Add((_startPosition, divertPoint, () => (_target.position + _startPosition) / 2F, flyDuration1));
                    _list.Add((mid, mid + (mid - divertPoint).normalized * (curDivertLength / 100), () => _target.position, flyDuration2));
                    break;
            }
        }

        public override void Movement(float dt)
        {
            if (_target == null)
            {
                _health.Kill();
                return;
            }

            timeElapsed += dt;

            var lastDirection = CurDirection;
            var (begin, mid, end, flyDuration) = _list[bezierPathIndex];
            var t = Mathf.Clamp01(timeElapsed / flyDuration);
            var bezier = new MMBezier(begin, mid, end());
            var position = bezier.GetPoint(t);

            var vector3 = position - transform.position;

            transform.position = position;

            CurDirection = vector3.normalized;

            if (t >= 1F)
            {
                timeElapsed = 0;
                bezierPathIndex++;
                if (bezierPathIndex == curCurveCount)
                {
                    if (_targetHealth == null || _targetHealth.IsDead())
                    {
                        _health.Kill();
                    }
                    else
                    {
                        _damageOnTouch.SetDamageScriptDirection(lastDirection);
                        _damageOnTouch.ForceColliding(_target.gameObject);
                    }
                }
            }
        }
    }
}