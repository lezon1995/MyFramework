using UnityEngine;

namespace TwoDHomingMissiles
{
    public class MissileController : MonoBehaviour
    {
        // The initial target used if missiles are in arc/swarm/swing mode.
        public Transform target;

        public float maxSpeed;

        // Proportional constant - 0f - 1f (set this to tweak performance of missile) lower is more "swingy" higher is more "direct/concise"
        public float kProportionalConst = 0.45f;
        public Transform missileSpriteTransform;

        Vector2 desiredVelocity;
        Vector2 subValue;
        Vector2 error;
        Vector2 currentVelocity;
        Vector2 steeringForce;

        public delegate void MissileDetonatedHandler(Vector2 position);

        public event MissileDetonatedHandler MissileDetonated;

        void Awake()
        {
            missileSpriteTransform = transform.GetChild(0);
        }

        void OnEnable()
        {
            desiredVelocity = Vector2.zero;
            subValue = Vector2.zero;
            error = Vector2.zero;
            currentVelocity = Vector2.zero;
        }

        void Start()
        {
            subValue = Vector2.zero;
            error = Vector2.zero;
            currentVelocity = Vector2.zero;
            steeringForce = Vector2.zero;
        }

        public virtual void GracefullyDestroyMissile(Vector2 position)
        {
            MissileDetonated?.Invoke(transform.position);
        }

        void Update()
        {
            var dt = Time.deltaTime;

            if (target)
            {
                subValue = target.position - transform.position;

                // Normalize the subtracted value
                subValue.Normalize();

                // Multiply the normalized value by the missile's maxSpeed to get the desired Velocity.
                desiredVelocity = subValue * maxSpeed;

                // Calculate missile error
                error = desiredVelocity - currentVelocity;

                // The force we apply to minimize our error is our error times our constant called kProportionalConst
                steeringForce = error * kProportionalConst;

                // Assign the current velocity by adding itself to steering force * deltaTime
                currentVelocity += (steeringForce * dt);

                // Move the missile according to all of the above calculation on the 2D plane
                transform.Translate(new(currentVelocity.x, currentVelocity.y, 0));
                
                if (missileSpriteTransform)
                {
                    // Rotate the sprite to face the target - note: using a missile sprite nested in a parent game-object so that the rotation works correctly. Otherwise, it goes way out and the missile
                    // position is messed up. So we rotate the nested sprite, but move the actual game-object, which also houses the collider2D bounding box
                    float targetAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
                    missileSpriteTransform.rotation = Quaternion.Slerp(missileSpriteTransform.rotation, Quaternion.Euler(0, 0, targetAngle), 100.0f * dt);
                }
                
                if ((target.position - transform.position).sqrMagnitude < 0.01F)
                {
                    GracefullyDestroyMissile(transform.position);
                }
            }
            else
            {
                GracefullyDestroyMissile(transform.position);
            }
        }
    }
}