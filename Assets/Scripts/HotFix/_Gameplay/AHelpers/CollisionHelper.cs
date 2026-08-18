using UnityEngine;

namespace MoreMountains
{
    public static class CollisionHelper
    {
        public static bool isColliderInSector(Vector2 origin, Vector2 direction, float radius, float angleDeg,
            Collider2D collider)
        {
            if (collider == null)
                return false;

            if (radius <= 0f || angleDeg <= 0f)
                return false;

            Vector2 closestPoint = collider.ClosestPoint(origin);
            Vector2 toTarget = closestPoint - origin;
            float distance = toTarget.magnitude;

            if (distance > radius)
                return false;

            if (distance < 0.0001f)
                return true;

            float cosThreshold = Mathf.Cos(angleDeg * 0.5f * Mathf.Deg2Rad);
            float dot = Vector2.Dot(direction.normalized, toTarget / distance);

            return dot >= cosThreshold;
        }
    }
}
