using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class OutlineRenderPass : ScriptableRenderPass
{
    private static readonly int m_thicknessId = Shader.PropertyToID("_Thickness");
    private static readonly int m_colorId = Shader.PropertyToID("_Color");
    private static readonly int m_minDepthId = Shader.PropertyToID("_MinDepth");
    private static readonly int m_maxDepthId = Shader.PropertyToID("_MaxDepth");


    private const string m_outlinePassName = "OutlinePass";
    private const string m_outlineTextureName = "OutlineTexture";

    private static Vector4 m_ScaleBias = new Vector4(1f, 1f, 0f, 0f);

    private Material m_mat;
    private OutlineSettings m_defaultSettings;

    private RenderTextureDescriptor outlineTextureDesc;


    public OutlineRenderPass(Material mat, OutlineSettings default_settings)
    {
        m_mat = mat;
        m_defaultSettings = default_settings;

        outlineTextureDesc = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
    }

    private void UpdateOulineSettings()
    {
        if (m_mat == null)
            return;

        var volumeComponent = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();
        float thickness = volumeComponent.thickness.overrideState ?
            volumeComponent.thickness.value : m_defaultSettings.thickness;
        var range = volumeComponent.depthRange.overrideState ?
            volumeComponent.depthRange.value : m_defaultSettings.depthRange;
        Color color = volumeComponent.color.overrideState ?
            volumeComponent.color.value : m_defaultSettings.color;


        m_mat.SetFloat(m_thicknessId, thickness);
        m_mat.SetFloat(m_minDepthId, range.x);
        m_mat.SetFloat(m_maxDepthId, range.y);
        m_mat.SetColor(m_colorId, color);

    }


    private static void ExecutePass(PassData data, RasterGraphContext context, int pass)
    {
        Blitter.BlitTexture(context.cmd, data.src, m_ScaleBias, data.material, pass);
    }


    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        Blitter.BlitTexture(context.cmd, data.src,
            new Vector4(1, 1, 0, 0), 0, false);
    }


    private class PassData
    {
        internal TextureHandle src;
        internal Material material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer)
            return;


        outlineTextureDesc.width = cameraData.cameraTargetDescriptor.width;
        outlineTextureDesc.height = cameraData.cameraTargetDescriptor.height;
        outlineTextureDesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        outlineTextureDesc.depthStencilFormat = GraphicsFormat.None;

        TextureHandle srcCamColor = resourceData.activeColorTexture;

        TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph,
            outlineTextureDesc, m_outlineTextureName, false);

        UpdateOulineSettings();



        using (var builder = renderGraph.AddRasterRenderPass<PassData>(m_outlinePassName, out var passData))
        {
            passData.src = srcCamColor;
            passData.material = m_mat;

            builder.UseTexture(passData.src);
            builder.SetRenderAttachment(dst, 0);
            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context, 0));
        }

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(m_outlinePassName, out var passData))
        {
            passData.src = dst;

            builder.UseTexture(passData.src);
            builder.SetRenderAttachment(srcCamColor, 0);
            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }
    }
}
