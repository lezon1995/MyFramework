using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Object Bounds/MMObjectBounds")]
    public class MMObjectBounds : MonoBehaviour
    {
        public enum WaysToDetermineBounds
        {
            Collider,
            Collider2D,
            Renderer,
            Undefined
        }

        [Header("Bounds")]
        public WaysToDetermineBounds BoundsBasedOn;

        public virtual Vector3 Size { get; set; }

        /// <summary>
        /// When this component is added we define its bounds.
        /// </summary>
        protected virtual void Reset()
        {
            DefineBoundsChoice();
        }

        /// <summary>
        /// Tries to determine automatically what the bounds should be based on.
        /// In this order, it'll keep the last found of these : Collider2D, Collider or Renderer.
        /// If none of these is found, it'll be set as Undefined.
        /// </summary>
        protected virtual void DefineBoundsChoice()
        {
            BoundsBasedOn = WaysToDetermineBounds.Undefined;
            
            if (TryGetComponent<Renderer>(out _))
                BoundsBasedOn = WaysToDetermineBounds.Renderer;

            if (TryGetComponent<Collider>(out _))
                BoundsBasedOn = WaysToDetermineBounds.Collider;

            if (TryGetComponent<Collider2D>(out _))
                BoundsBasedOn = WaysToDetermineBounds.Collider2D;
        }

        /// <summary>
        /// Returns the bounds of the object, based on what has been defined
        /// </summary>
        public virtual Bounds GetBounds()
        {
            switch (BoundsBasedOn)
            {
                case WaysToDetermineBounds.Renderer:
                    if (TryGetComponent<Renderer>(out var renderer))
                        return renderer.bounds;

                    throw new Exception("The PoolableObject " + gameObject.name + " is set as having Renderer based bounds but no Renderer component can be found.");
                case WaysToDetermineBounds.Collider:
                    if (TryGetComponent<Collider>(out var collider))
                        return collider.bounds;

                    throw new Exception("The PoolableObject " + gameObject.name + " is set as having Collider based bounds but no Collider component can be found.");
                case WaysToDetermineBounds.Collider2D:
                    if (TryGetComponent<Collider2D>(out var collider2D))
                        return collider2D.bounds;

                    // throw new Exception("The PoolableObject " + gameObject.name + " is set as having Collider2D based bounds but no Collider2D component can be found.");
                    return new(Vector3.zero, Vector3.zero);
                default:
                    return new(Vector3.zero, Vector3.zero);
            }
        }
    }
}