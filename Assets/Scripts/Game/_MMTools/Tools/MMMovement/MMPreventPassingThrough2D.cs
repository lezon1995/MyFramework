using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Prevents fast moving objects from going through colliders by casting a ray backwards after each movement
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Movement/MMPreventPassingThrough2D")]
    public class MMPreventPassingThrough2D : MonoBehaviour
    {
        public enum Modes
        {
            Raycast,
            BoxCast,
            CircleCast,
        }

        /// whether to cast a ray or a boxcast to look for targets
        public Modes Mode = Modes.Raycast;

        /// the layer mask to search obstacles on
        public LayerMask ObstaclesLayerMask;

        /// the bounds adjustment variable
        public float SkinWidth = 0.1f;

        /// whether to reposition the rb if hitting a trigger collider 
        public bool RepositionRigidbodyIfHitTrigger = true;

        /// whether to reposition the rb if hitting a non trigger collider
        public bool RepositionRigidbodyIfHitNonTrigger = true;

        public RaycastHit2D Hit;

        protected float _smallestBoundsWidth;
        protected float _adjustedSmallestBoundsWidth;
        protected float _squaredBoundsWidth;
        protected Vector2 _positionLastFrame;
        protected Rigidbody2D _rigidbody;
        protected Collider2D _collider;
        protected Vector2 _lastMovement;
        protected float _lastMovementSquared;
        protected RaycastHit2D _hitInfo;
        protected Vector2 _colliderSize;
        bool initialized;

        /// <summary>
        /// On Start, we initialize our object
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Grabs the rigidbody and computes the bounds width
        /// </summary>
        protected virtual void Initialization()
        {
            if (initialized)
                return;
            
            if (TryGetComponent(out _rigidbody))
            {
                _positionLastFrame = _rigidbody.position;
            }
            else
            {
                _positionLastFrame = transform.position;
            }

            TryGetComponent(out _collider);
            switch (_collider)
            {
                case BoxCollider2D box:
                    _colliderSize = box.size;
                    break;
                case CircleCollider2D circle:
                    var radius = circle.radius;
                    _colliderSize = new(radius, radius);
                    break;
            }

            _smallestBoundsWidth = Mathf.Min(Mathf.Min(_collider.bounds.extents.x, _collider.bounds.extents.y), _collider.bounds.extents.z);
            _adjustedSmallestBoundsWidth = _smallestBoundsWidth * (1.0f - SkinWidth);
            _squaredBoundsWidth = _smallestBoundsWidth * _smallestBoundsWidth;
            initialized = true;
        }

        /// <summary>
        /// On Enable, we initialize our last frame position
        /// </summary>
        protected virtual void OnEnable()
        {
            Initialization();
            _positionLastFrame = _rigidbody.position;
        }

        /// <summary>
        /// On fixedUpdate, checks the last movement and if needed casts a ray to detect obstacles
        /// </summary>
        protected virtual void FixedUpdate()
        {
            _lastMovement = _rigidbody.position - _positionLastFrame;
            _lastMovementSquared = _lastMovement.sqrMagnitude;

            // if we've moved further than our bounds, we may have missed something
            if (_lastMovementSquared > _squaredBoundsWidth)
            {
                float movementMagnitude = Mathf.Sqrt(_lastMovementSquared);
                _hitInfo = Mode switch
                {
                    // we cast a ray backwards to see if we should have hit something
                    Modes.Raycast => MMDebug.RayCast(_positionLastFrame, _lastMovement.normalized, movementMagnitude, ObstaclesLayerMask, Color.blue, true),
                    Modes.BoxCast => Physics2D.BoxCast(origin: _positionLastFrame, size: _colliderSize, angle: 0, layerMask: ObstaclesLayerMask, direction: _lastMovement.normalized, distance: movementMagnitude),
                    Modes.CircleCast => Physics2D.CircleCast(origin: _positionLastFrame, radius: _colliderSize.x, layerMask: ObstaclesLayerMask, direction: _lastMovement.normalized, distance: movementMagnitude),
                    _ => _hitInfo
                };

                if (_hitInfo.collider)
                {
                    var position = _hitInfo.point - (_lastMovement / movementMagnitude) * _adjustedSmallestBoundsWidth;
                    if (_hitInfo.collider.isTrigger)
                    {
                        _hitInfo.collider.SendMessage("OnTriggerEnter2D", _collider, SendMessageOptions.DontRequireReceiver);
                        if (RepositionRigidbodyIfHitTrigger)
                        {
                            transform.position = position;
                            if (_rigidbody)
                                _rigidbody.position = position;
                        }
                    }
                    else
                    {
                        Hit = _hitInfo;
                        gameObject.SendMessage("PreventedCollision2D", Hit, SendMessageOptions.DontRequireReceiver);
                        if (RepositionRigidbodyIfHitNonTrigger)
                        {
                            transform.position = position;
                            if (_rigidbody)
                                _rigidbody.position = position;
                        }
                    }
                }
            }

            _positionLastFrame = _rigidbody.position;
        }
    }
}