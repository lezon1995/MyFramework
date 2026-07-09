using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This class is used to describe spawn properties, to be used by the MMSpawnAround class.
    /// It's meant to be exposed and used by classes that are designed to spawn objects, typically loot systems 
    /// </summary>
    [Serializable]
    public class MMSpawnAroundProperties
    {
        /// the possible shapes objects can be spawned within
        public enum Shapes
        {
            Sphere,
            Cube
        }

        [Title("Shape")]
        [Tooltip("the shape within which objects should spawn")]
        public Shapes Shape = Shapes.Sphere;

        [Title("Position")]
        [Tooltip("the minimum distance to the origin of the spawn at which objects can be spawned")]
        [MMEnumCondition("Shape", (int)Shapes.Sphere)]
        public float MinimumSphereRadius = 1f;

        [Tooltip("the maximum distance to the origin of the spawn at which objects can be spawned")]
        [MMEnumCondition("Shape", (int)Shapes.Sphere)]
        public float MaximumSphereRadius = 2f;

        [Tooltip("the minimum size of the cube's base")]
        [MMEnumCondition("Shape", (int)Shapes.Cube)]
        public Vector3 MinimumCubeBaseSize = Vector3.one;

        [Tooltip("the maximum size of the cube's base")]
        [MMEnumCondition("Shape", (int)Shapes.Cube)]
        public Vector3 MaximumCubeBaseSize = new Vector3(2f, 2f, 2f);

        [Title("Plane")]
        [Tooltip("if this is true, spawn will be constrained to the plane defined by the NormalToSpawnPlane property")]
        public bool ForcePlane = true;

        [Tooltip("a Vector3 that specifies the normal to the plane you want to spawn objects on (if you want to spawn objects on the x/z plane, the normal to that plane would be the y axis (0,1,0)")]
        public Vector3 NormalToSpawnPlane = Vector3.up;

        [Title("NormalAxisOffset")]
        [Tooltip("the minimum offset to apply on the normal axis")]
        public float MinimumNormalAxisOffset;

        [Tooltip("the maximum offset to apply on the normal axis")]
        public float MaximumNormalAxisOffset;

        [Title("NormalAxisOffsetCurve")]
        [Tooltip("whether or not to use a curve to offset the object's spawn position along the spawn plane")]
        public bool UseNormalAxisOffsetCurve;

        [Tooltip("a curve used to define how distance to the origin should be altered (potentially above min/max distance)")]
        [MMCondition("UseNormalAxisOffsetCurve", true)]
        public AnimationCurve NormalOffsetCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 1f));

        [Tooltip("the value to which the curve's zero should be remapped to")]
        [MMCondition("UseNormalAxisOffsetCurve", true)]
        public float NormalOffsetCurveRemapZero;

        [Tooltip("the value to which the curve's one should be remapped to")]
        [MMCondition("UseNormalAxisOffsetCurve", true)]
        public float NormalOffsetCurveRemapOne = 1f;

        [Tooltip("whether or not to invert the curve (horizontally)")]
        [MMCondition("UseNormalAxisOffsetCurve", true)]
        public bool InvertNormalOffsetCurve;

        [Title("Rotation")]
        [Tooltip("the minimum random rotation to apply (in degrees)")]
        public Vector3 MinimumRotation = Vector3.zero;

        [Tooltip("the maximum random rotation to apply (in degrees)")]
        public Vector3 MaximumRotation = Vector3.zero;

        [Title("Scale")]
        [Tooltip("the minimum random scale to apply")]
        public Vector3 MinimumScale = Vector3.one;

        [Tooltip("the maximum random scale to apply")]
        public Vector3 MaximumScale = Vector3.one;
    }

    /// <summary>
    /// This static class is a spawn helper, useful to randomize position, rotation and scale when you need to
    /// instantiate objects  
    /// </summary>
    public static class MMSpawnAround
    {
        public static void ApplySpawnAroundProperties(GameObject o, MMSpawnAroundProperties props, Vector3 origin)
        {
            o.transform.position = SpawnAroundPosition(props, origin);
            o.transform.rotation = SpawnAroundRotation(props);
            o.transform.localScale = SpawnAroundScale(props);
        }

        /// <summary>
        /// Returns the position at which the object should spawn
        /// </summary>
        /// <param name="props"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static Vector3 SpawnAroundPosition(MMSpawnAroundProperties props, Vector3 origin)
        {
            // we get the position of the object based on the defined plane and distance
            var newPosition = Vector3.zero;
            switch (props.Shape)
            {
                case MMSpawnAroundProperties.Shapes.Sphere:
                    float distance = Random.Range(props.MinimumSphereRadius, props.MaximumSphereRadius);
                    newPosition = Random.insideUnitSphere;
                    if (props.ForcePlane)
                        newPosition = Vector3.Cross(newPosition, props.NormalToSpawnPlane);

                    newPosition.Normalize();
                    newPosition *= distance;
                    break;
                case MMSpawnAroundProperties.Shapes.Cube:
                    newPosition = PickPositionInsideCube(props);
                    if (props.ForcePlane)
                        newPosition = Vector3.Cross(newPosition, props.NormalToSpawnPlane);

                    break;
            }

            float randomOffset = Random.Range(props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset);
            // we correct the position based on the NormalOffsetCurve
            if (props.UseNormalAxisOffsetCurve)
            {
                float normalizedOffset = 0f;
                if (randomOffset != 0)
                {
                    if (props.InvertNormalOffsetCurve)
                        normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 1f, 0f);
                    else
                        normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 0f, 1f);
                }

                float offset = props.NormalOffsetCurve.Evaluate(normalizedOffset);
                offset = MMMaths.Remap(offset, 0f, 1f, props.NormalOffsetCurveRemapZero, props.NormalOffsetCurveRemapOne);

                newPosition *= offset;
            }

            // we apply the normal offset
            newPosition += props.NormalToSpawnPlane.normalized * randomOffset;

            // relative position
            newPosition += origin;

            return newPosition;
        }

        public static Vector3 PickPositionInsideCube(MMSpawnAroundProperties props)
        {
            int iterationsCount = 0;
            int maxIterationsCount = 1000;
            while (iterationsCount < maxIterationsCount)
            {
                float randomX = Random.Range(0f, props.MaximumCubeBaseSize.x);
                float randomY = Random.Range(0f, props.MaximumCubeBaseSize.y);
                float randomZ = Random.Range(0f, props.MaximumCubeBaseSize.z);

                if (randomX < props.MinimumCubeBaseSize.x && randomY < props.MinimumCubeBaseSize.y && randomZ < props.MinimumCubeBaseSize.z)
                {
                    iterationsCount++;
                    continue;
                }

                randomX = MMMaths.RollADice(2) > 1 ? -randomX : randomX;
                randomY = MMMaths.RollADice(2) > 1 ? -randomY : randomY;
                randomZ = MMMaths.RollADice(2) > 1 ? -randomZ : randomZ;
                return new Vector3(randomX, randomY, randomZ);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Returns the scale at which the object should spawn
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static Vector3 SpawnAroundScale(MMSpawnAroundProperties props)
        {
            return MMMaths.RandomVector3(props.MinimumScale, props.MaximumScale);
        }

        /// <summary>
        /// Returns the rotation at which the object should spawn
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static Quaternion SpawnAroundRotation(MMSpawnAroundProperties props)
        {
            return Quaternion.Euler(MMMaths.RandomVector3(props.MinimumRotation, props.MaximumRotation));
        }

        /// <summary>
        /// Draws gizmos to show the shape of the spawn area
        /// </summary>
        /// <param name="props"></param>
        /// <param name="origin"></param>
        /// <param name="quantity"></param>
        /// <param name="size"></param>
        public static void DrawGizmos(MMSpawnAroundProperties props, Vector3 origin, int quantity, float size, Color gizmosColor)
        {
            Gizmos.color = gizmosColor;
            for (int i = 0; i < quantity; i++)
            {
                Gizmos.DrawCube(SpawnAroundPosition(props, origin), SpawnAroundScale(props) * size);
            }
        }
    }
}