using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An implementation of <see cref="LineRegionBase"/> with added textures.
    /// </summary>
    [ExecuteAlways]
    public class LineRegion : LineRegionBase
    {
        /// <summary>
        /// The body of the line.
        /// </summary>
        [SerializeField]
        Renderer _bodyRenderer;

        /// <summary>
        /// The end point, head, of the line.
        /// </summary>
        [SerializeField]
        Renderer _headRenderer;

        /// <summary>
        /// The body of the line.
        /// </summary>
        [SerializeField]
        Transform _bodyTransform;

        /// <summary>
        /// The end point, head, of the line.
        /// </summary>
        [SerializeField]
        Transform _headTransform;

        /// <summary>
        /// The shader ID of the _FillProgress property.
        /// </summary>
        static readonly int ProgressShaderID = Shader.PropertyToID("_FillProgress");

        /// <summary>
        /// Updates the position of the head and the body according to the line data.
        /// </summary>
        void Update()
        {
            if (_bodyTransform == null || _headTransform == null)
                return;

            // Sets body angle and scale.
            _bodyTransform.localEulerAngles = new(_bodyTransform.eulerAngles.x, _bodyTransform.eulerAngles.y, Angle);
            _bodyTransform.localScale = new(Width, Mathf.Max(Length - 2, 0), 1);

            // Sets head angle, scale and position.
            _headTransform.localScale = new(Width, Mathf.Min((Length / 2), 1),_headTransform.localScale.z);
            _headTransform.position = _bodyTransform.position + _bodyTransform.up * _bodyTransform.localScale.y;
            _headTransform.eulerAngles = _bodyTransform.eulerAngles;

            float bodyPart = _bodyTransform.localScale.y / Length;
            _bodyRenderer.sharedMaterial.SetFloat(ProgressShaderID, Mathf.InverseLerp(0, bodyPart, FillProgress));
            _headRenderer.sharedMaterial.SetFloat(ProgressShaderID, Mathf.InverseLerp(bodyPart, 1, FillProgress));
        }
    }
}