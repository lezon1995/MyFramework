using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An implementation of <see cref="CircleRegionBase"/>.
    /// </summary>
    [ExecuteAlways]
    public class CircleRegion : CircleRegionBase
    {
        /// <summary>
        /// The amount the circle is filled.
        /// </summary>
        public float FillProgress
        {
            get => _fillProgress;
            set => _fillProgress = Mathf.Clamp01(value);
        }

        /// <summary>
        /// The amount the circle is filled.
        /// </summary>
        [Range(0, 1)]
        [SerializeField]
        float _fillProgress;

        /// <summary>
        /// The transform of the object that should adhere to the circle region.
        /// </summary>
        [SerializeField]
        Transform _circle;

        /// <summary>
        /// Renders the circle mesh.
        /// </summary>
        [SerializeField]
        Renderer _circleRenderer;

        /// <summary>
        /// The shader ID for the _FillProgress property.
        /// </summary>
        static readonly int ProgressShaderID = Shader.PropertyToID("_FillProgress");

        /// <summary>
        /// Updates the scale and position of the circle.
        /// </summary>
        void Update()
        {
            if (_circle == null)
                return;

            _circle.localScale = Vector3.one * (2 * Radius);
            _circle.localPosition = new Vector3(Offset.x, Offset.y, 0);

            _circleRenderer.sharedMaterial.SetFloat(ProgressShaderID, _fillProgress);
        }
    }
}