using System;
using System.Collections.Generic;
using Drawing;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This decision will return true if an object on its TargetLayer layermask is within its specified radius, false otherwise. It will also set the Brain's Target to that object.
    /// </summary>
    public class EnemyDetectTargetRadiusDecision : EnemyDecision
    {
        public enum ObstaclesDetectionModes
        {
            Boxcast,
            Raycast
        }

        [Tooltip("the radius to search our target in")]
        public float Radius = 3f;

        [Tooltip("the center of the search circle")]
        public Vector3 DetectionOriginOffset;

        [Tooltip("the layer(s) to search our target on")]
        public LayerMask TargetLayer;

        [Tooltip("whether or not to look for obstacles")]
        public bool ObstacleDetection;

        [Tooltip("the layer(s) to look for obstacles on")]
        public LayerMask ObstacleMask = LayerManager.Obstacles_Mask;

        [Tooltip("the method to use to detect obstacles")]
        public ObstaclesDetectionModes ObstaclesDetectionMode = ObstaclesDetectionModes.Raycast;

        [Tooltip("if this is true, this AI will be able to consider itself (or its children) a target")]
        public bool CanTargetSelf;

        [Tooltip("the frequency (in seconds) at which to check for obstacles")]
        public float TargetCheckFrequency = 1f;

        [Tooltip("the maximum amount of targets the overlap detection can acquire")]
        public int OverlapMaximum = 10;

        protected Collider2D _collider;
        protected Vector2 _facingDirection;
        protected Vector2 _raycastOrigin;
        protected CharacterOrientation2D _orientation2D;
        protected Color _gizmoColor = Color.yellow;
        protected bool _init;
        protected Vector2 _boxcastDirection;
        protected Collider2D[] _results;
        protected List<Transform> _potentialTargets = new();
        protected float _lastTargetCheckTimestamp;
        protected bool _lastReturnValue;
        protected RaycastHit2D _hit;
        Comparison<Transform> comparison;

        public EnemyDetectTargetRadiusDecision()
        {
            comparison = Comparison;
        }

        /// <summary>
        /// On init, we grab our Character component
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();
            _potentialTargets.Clear();

            if (_orientation2D == null)
                brick.FindAbility(out _orientation2D);

            if (_collider == null)
                this.TryGetComponentInParent(out _collider);

            _gizmoColor.a = 0.25f;
            _init = true;
            _results ??= new Collider2D[OverlapMaximum];
        }

        /// <summary>
        /// On Decide we check for our target
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
        {
            if (brick.IsDead())
                return false;

            return DetectTarget();
        }

        /// <summary>
        /// Returns true if a target is found within the circle
        /// </summary>
        /// <returns></returns>
        protected virtual bool DetectTarget()
        {
            // we check if there's a need to detect a new target
            if (Time.time - _lastTargetCheckTimestamp < TargetCheckFrequency)
            {
                return _lastReturnValue;
            }

            _lastTargetCheckTimestamp = Time.time;

            ComputeRaycastOrigin();

            if (!GetPotentialTargets())
            {
                return false;
            }

            // we check if there's a target in the list
            if (_potentialTargets.Count == 0)
            {
                _lastReturnValue = false;
                return false;
            }

            SortTargetsByDistance();

            if (FindUnobscuredTarget())
            {
                return true;
            }

            _lastReturnValue = false;
            return false;
        }

        protected virtual bool FindUnobscuredTarget()
        {
            var targets = _potentialTargets;
            if (!ObstacleDetection && targets[0])
            {
                _brain.SetTarget(targets[0]);
                _lastReturnValue = true;
                return true;
            }

            // we return the first unobscured target
            foreach (var t in targets)
            {
                if (t.TryGetComponent(out Collider2D collider2D))
                {
                    _boxcastDirection = (collider2D.bounds.center - _collider.bounds.center);
                }

                if (ObstaclesDetectionMode == ObstaclesDetectionModes.Boxcast)
                {
                    _hit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f, _boxcastDirection.normalized, _boxcastDirection.magnitude, ObstacleMask);
                }
                else
                {
                    _hit = MMDebug.RayCast(_collider.bounds.center, _boxcastDirection, _boxcastDirection.magnitude, ObstacleMask, Color.yellow, true);
                }

                if (!_hit)
                {
                    _brain.SetTarget(t);
                    _lastReturnValue = true;
                    return true;
                }
            }

            return false;
        }

        protected virtual void SortTargetsByDistance()
        {
            _potentialTargets.Sort(comparison);
        }

        int Comparison(Transform a, Transform b)
        {
            if (a == null || b == null)
                return 0;

            var selfPos = transform.position;
            var dist1 = Vector2.SqrMagnitude(selfPos - a.transform.position);
            var dist2 = Vector2.SqrMagnitude(selfPos - b.transform.position);
            return dist1.CompareTo(dist2);
        }

        protected virtual void ComputeRaycastOrigin()
        {
            if (_orientation2D)
            {
                _facingDirection = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
                _raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
                _raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;
            }
            else
            {
                _raycastOrigin = transform.position + DetectionOriginOffset;
            }
        }

        protected virtual bool GetPotentialTargets()
        {
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(TargetLayer);
            int numberOfResults = Physics2D.OverlapCircle(_raycastOrigin, Radius, filter, _results);
            // if there are no targets around, we exit
            if (numberOfResults == 0)
            {
                _lastReturnValue = false;
                return false;
            }

            // we go through each collider found
            _potentialTargets.Clear();
            int min = Mathf.Min(OverlapMaximum, numberOfResults);
            for (int i = 0; i < min; i++)
            {
                if (IsPotentialTarget(_results[i]))
                {
                    _potentialTargets.Add(_results[i].transform);
                }
            }

            return true;
        }

        protected virtual bool IsPotentialTarget(Collider2D c)
        {
            if (c == null)
                return false;

            if (CanTargetSelf)
                return true;

            if (c.gameObject == _brain.Owner || c.transform.IsChildOf(transform))
                return false;
            
            if (c.TryGetComponent(out APlayer _))
                return true;

            return false;
        }

        public override void DrawGizmos()
        {
            using (Draw.InLocalSpace(transform))
            {
                if (GizmoContext.InSelection(this))
                {
                    _raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
                    _raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;

                    Draw.xy.Circle(Vector3.zero, Radius, Color.yellow);
                    if (_init)
                    {
                        Draw.xy.Circle(Vector3.zero, Radius, _gizmoColor);
                    }
                }
                else
                {
                    Draw.xy.Circle(Vector3.zero, Radius, Color.yellow * new Color(1, 1, 1, 0.5f));
                }
            }
        }

        /// <summary>
        /// Draws gizmos for the detection circle
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
        }
    }
}