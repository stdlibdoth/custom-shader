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

    private RenderTextureDescriptor m_outlineTextureDesc;

    private RTHandle m_outlineRT;

    private RenderTexture m_renderTexture;
    private int m_featureState;

    public override void Create()
    {
        if (m_outlineShader == null || m_composeShader == null)
            return;
        m_outlineTextureDesc = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
        m_outlineTextureDesc.depthBufferBits = 0;
        m_renderTexture = new RenderTexture(m_outlineTextureDesc);
        m_renderTexture.filterMode = FilterMode.Bilinear;
        m_outlineRT = RTHandles.Alloc(m_renderTexture);

        m_outlineMat = new Material(m_outlineShader);
        m_composeMat = new Material(m_composeShader);
        m_outlineRenderPass = new OutlineRenderPass(m_outlineMat, m_outlineSettings);
        m_compositePass = new OutlineCompositePass(m_composeMat);

        m_outlineRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        m_compositePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        m_debug = m_renderTexture;
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
                    m_outlineRenderPass.SetRTHandle(m_outlineRT);
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
                    m_compositePass.SetOutlineRT(m_outlineRT);
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
            Destroy(m_composeMat);
            m_outlineRT.Release();
        }
        else if( m_featureState == 0)
        {
            DestroyImmediate(m_outlineMat);
            DestroyImmediate(m_composeMat);
            m_outlineRT.Release();
        }
#else
        Destroy(m_outlineMat);
        Destroy(m_composeMat);
        m_outlineRT.Release();
#endif
    }

}
