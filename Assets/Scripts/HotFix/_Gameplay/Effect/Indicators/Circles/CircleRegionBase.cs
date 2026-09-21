using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// The circle region that can be used to display circular areas.
    /// </summary>
    public abstract class CircleRegionBase : MonoBehaviour
    {
        /// <summary>
        /// The radius of the circle.
        /// </summary>
        public float Radius
        {
            get => _radius;
            set => _radius = value;
        }

        /// <summary>
        /// The offset of the circle from its object.
        /// </summary>
        public Vector2 Offset
        {
            get => _offset;
            set => _offset = value;
        }

        /// <summary>
        /// The offset of the circle from its object.
        /// </summary>
        [SerializeField]
        Vector2 _offset;

        /// <summary>
        /// The radius of the circle.
        /// </summary>
        [SerializeField]
        float _radius;

        /// <summary>
        /// Draws the Gizmos representation of the circle region.
        /// </summary>
        /*void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            GizmosExtensions.DrawCircle(transform.position + new Vector3(_offset.x, 0, _offset.y), _radius);
        }*/
    }
}