using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains
{
    /// <summary>
    /// An example implementation of <see cref="ArcRegionBase"/> with some custom textures.
    /// </summary>
    [ExecuteAlways]
    public class ArcRegion : ArcRegionBase
    {
        /// <summary>
        /// The amount the line is filled.
        /// </summary>
        public float FillProgress
        {
            get => _fillProgress;
            set => _fillProgress = Mathf.Clamp01(value);
        }

        /// <summary>
        /// The dot in the centre
        /// </summary>
        [SerializeField]
        Transform _centreDot;

        /// <summary>
        /// The object on the left side of the arc.
        /// </summary>
        [SerializeField]
        Transform _leftSide;

        /// <summary>
        /// The object on the right side of the arc.
        /// </summary>
        [SerializeField]
        Transform _rightSide;

        /// <summary>
        /// The centre fill of the arc.
        /// </summary>
        [SerializeField]
        Transform _centre;

        /// <summary>
        /// The centre fill of the arc.
        /// </summary>
        [SerializeField]
        Transform _line;

        /// <summary>
        /// The progress of the fill.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [FormerlySerializedAs("_progress")]
        float _fillProgress;

        /// <summary>
        /// All the centre areas that should be masked with the arc.
        /// These should make use of the ArcShader shader.
        /// </summary>
        [SerializeField]
        Renderer[] _maskedCentres;

        /// <summary>
        /// The ID of the <c>_Arc</c> property.
        /// </summary>
        static readonly int ArcShaderID = Shader.PropertyToID("_Arc");

        /// <summary>
        /// The ID of the <c>_Angle</c> property.
        /// </summary>
        static readonly int AngleShaderID = Shader.PropertyToID("_Angle");

        /// <summary>
        /// The ID of the <c>_Progress</c> property.
        /// </summary>
        static readonly int ProgressShaderID = Shader.PropertyToID("_Progress");

        /// <summary>
        /// Updates the transforms of the textures that are placed on the arc.
        /// </summary>
        void Update()
        {
            if (_centreDot == null || _leftSide == null || _rightSide == null || _centre == null)
                return;

            // Centre dot transform configure.
            _centreDot.localPosition = Vector3.up * 0.01f;
            _centreDot.localScale = Vector3.one * (Radius * 0.3f);

            // Centre transform configure.
            _centre.localPosition = Vector3.zero;
            _centre.localScale = Radius * 2 * Vector3.one;

            // Left side transform configure.
            Vector3 original = _leftSide.localEulerAngles;
            _leftSide.localEulerAngles = new Vector3(original.x, 0, LeftAngle - transform.eulerAngles.y);
            _leftSide.localScale = Vector3.one * (Radius * 1.2f);

            // Right side transform configure.
            original = _rightSide.localEulerAngles;
            _rightSide.localEulerAngles = new Vector3(original.x, 0, RightAngle - transform.eulerAngles.y);
            _rightSide.localScale = Vector3.one * (Radius * 1.2f);
            
            if (_line)
            {
                _line.localPosition = Vector3.zero;
                _line.localEulerAngles = new(0, 0, Angle);
            }

            // Apply normalized arc and angle to mask shader.
            for (int i = 0; i < _maskedCentres.Length; i++)
            {
                var material = _maskedCentres[i].sharedMaterial;
                material.SetFloat(ArcShaderID, 1 - Arc / 360);
                material.SetFloat(AngleShaderID, Mathf.Repeat((-Angle - 90) % 360, 360) / 360);
                material.SetFloat(ProgressShaderID, Mathf.Clamp01(_fillProgress));
            }
        }
    }
}