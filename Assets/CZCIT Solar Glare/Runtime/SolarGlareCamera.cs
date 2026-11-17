// 在相机渲染前后管理太阳眩光，并负责对应渲染资源的释放。
using UnityEngine;
using UnityEngine.Rendering;

namespace CZCIT.SolarGlare
{
    // A display approximation of optical glare, not a solar atmosphere or an eye model.
    [ExecuteAlways, RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public sealed class SolarGlareCamera : MonoBehaviour
    {
        [Range(0, 1)] public float intensity = .85f;
        [SerializeField] private Light sunOverride;
        [SerializeField] private Transform ignoreOccludersUnder;
        Camera viewer;
        GameObject overlay;
        Mesh mesh;
        Material material;
        MeshRenderer surface;
        readonly RaycastHit[] hits = new RaycastHit[32];
        public bool VisibleLastRender
        {
            get; private set;
        }


        void OnEnable()
        {
            viewer = GetComponent<Camera>();
            var shader = Resources.Load<Shader>("CZCIT/SolarGlare");
            if (!shader)
                return;
            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            mesh = new Mesh { name = "Optical glare quad", hideFlags = HideFlags.HideAndDontSave };
            mesh.vertices = new[] {
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0),
                new Vector3(-1, 1, 0) };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            overlay = new GameObject("Solar optical glare") { hideFlags = HideFlags.HideAndDontSave, layer = 2 };
            overlay.transform.SetParent(transform, false);
            overlay.AddComponent<MeshFilter>().sharedMesh = mesh;
            surface = overlay.AddComponent<MeshRenderer>();
            surface.sharedMaterial = material;
            surface.shadowCastingMode = ShadowCastingMode.Off;
            surface.receiveShadows = false;
            surface.lightProbeUsage = LightProbeUsage.Off;
            surface.reflectionProbeUsage = ReflectionProbeUsage.Off;
            surface.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            surface.enabled = false;
            Camera.onPreCull += BeforeCamera;
            Camera.onPostRender += AfterCamera;
        }

        void BeforeCamera(Camera camera)
        {
            if (!surface)
                return;
            surface.enabled = false;
            VisibleLastRender = false;
            if (camera != viewer || viewer.orthographic || intensity <= 0)
                return;
            var sun = sunOverride ? sunOverride : RenderSettings.sun;
            if (!sun || sun.type != LightType.Directional || !sun.enabled || !sun.gameObject.activeInHierarchy)
                return;
            Vector3 direction = -sun.transform.forward;
            // Hide glare outside the view and behind opaque scene colliders. Ignore our rig.
            if (Vector3.Dot(transform.forward, direction) <= .05f)
                return;
            int count = Physics.RaycastNonAlloc(
                transform.position,
                direction,
                hits,
                viewer.farClipPlane,
                viewer.cullingMask,
                QueryTriggerInteraction.Ignore);
            if (count == hits.Length)
                return;
            for (int i = 0; i < count; i++)
                if (hits[i].collider && (!ignoreOccludersUnder || !hits[i].collider.transform.IsChildOf(ignoreOccludersUnder)))
                    return;
            float distance = Mathf.Max(viewer.nearClipPlane + .02f, .15f);
            overlay.transform.localPosition = Vector3.forward * distance;
            overlay.transform.localScale = Vector3.one * Mathf.Max(2f, distance * 8);
            material.SetVector("_SunDirection", direction);
            material.SetVector("_GlareRight", transform.right);
            material.SetVector("_GlareUp", transform.up);
            material.SetFloat("_Intensity", intensity);
            surface.enabled = true;
            VisibleLastRender = true;
        }

        void AfterCamera(Camera camera)
        {
            if (camera == viewer && surface)
                surface.enabled = false;
        }

        void OnDisable()
        {
            Camera.onPreCull -= BeforeCamera;
            Camera.onPostRender -= AfterCamera;
            Release(overlay);
            Release(mesh);
            Release(material);
            overlay = null;
            mesh = null;
            material = null;
            surface = null;
        }

        static void Release(Object item)
        {
            if (!item)
                return;
            if (Application.isPlaying)
                Destroy(item);
            else
                DestroyImmediate(item);
        }
    }
}
