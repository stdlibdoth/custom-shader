using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[Serializable]
public class OutlineSettings
{
    public Color color;
    public float thickness;
    public Vector2 depthRange;
}



public class OutlineRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private OutlineSettings m_outlineSettings;
    [SerializeField] private Shader m_outlineShader;
    [SerializeField] private Shader m_composeShader;


    [SerializeField] private RenderTexture m_debug;

    private Material m_outlineMat;
    private Material m_composeMat;
    private OutlineRenderPass m_outlineRenderPass;
    private OutlineCompositePass m_compositePass;

    private RTHandle m_tempTexture;
    private int m_featureState;

    public override void Create()
    {
        if (m_outlineShader == null || m_composeShader == null)
            return;
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0,RenderTextureFormat.ARGB32);
        rt.filterMode = FilterMode.Bilinear;
        rt.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        m_tempTexture = RTHandles.Alloc(rt);
        m_outlineMat = new Material(m_outlineShader);
        m_composeMat = new Material(m_composeShader);
        m_outlineRenderPass = new OutlineRenderPass(m_outlineMat, m_outlineSettings);
        m_compositePass = new OutlineCompositePass(m_composeMat);

        m_outlineRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        m_compositePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        m_debug = rt;
        m_featureState = 0;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        if (!cameraData.camera.CompareTag("MainCamera"))
        {
            if (!cameraData.camera.TryGetComponent<Volume>(out var vol))
                return;
            if (vol.profile.TryGet<OutlineVolumeComponent>(out var comp))
            {
                if (cameraData.cameraType == CameraType.Game && comp.active)
                {
                    m_featureState = 1;
                    cameraData.targetTexture = m_tempTexture;
                    renderer.EnqueuePass(m_outlineRenderPass);
                }
            }
        }
        else
        {
            if (cameraData.cameraType == CameraType.Game)
            {
                if (m_featureState == 1)
                {
                    m_compositePass.SetOulineTexture(m_tempTexture);
                    renderer.EnqueuePass(m_compositePass);
                }
                 m_featureState = 0;
            }
        }
    }


    protected override void Dispose(bool disposing)
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            Destroy(m_outlineMat);
            m_tempTexture.Release();
        }
        else
        {
            DestroyImmediate(m_outlineMat);
            m_tempTexture.Release();
        }
#else
        Destroy(m_material);
            m_tempTexture.Release();
#endif
    }

}
