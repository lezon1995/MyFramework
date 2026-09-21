using UnityEngine;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(LineRenderer))]
    public class MMLineRendererMaterialFlow : MonoBehaviour
    {
        static int MainTex = Shader.PropertyToID("_MainTex");

        LineRenderer _lineRenderer;

        public Material flowMaterial;
        public Vector2 TextureScale = new(1, 1);

        void Awake()
        {
            if (TryGetComponent(out _lineRenderer))
            {
                flowMaterial = _lineRenderer.material;
            }
        }

        void Update()
        {
            //Material Anim
            float offset = Time.time * -1f;
            flowMaterial.mainTextureScale = TextureScale;
            flowMaterial.SetTextureOffset(MainTex, new(offset, 0f));
        }
    }
}