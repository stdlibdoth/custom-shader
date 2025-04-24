using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class OutlineRenderPass : ScriptableRenderPass
{
    private static readonly int m_thicknessId = Shader.PropertyToID("_Thickness");
    private static readonly int m_colorId = Shader.PropertyToID("_Color");
    private static readonly int m_minDepthId = Shader.PropertyToID("_MinDepth");
    private static readonly int m_maxDepthId = Shader.PropertyToID("_MaxDepth");


    private const string m_outlinePassName = "OutlinePass";

    private static Vector4 m_ScaleBias = new Vector4(1f, 1f, 0f, 0f);

    private Material m_mat;
    private OutlineSettings m_defaultSettings;
    private RTHandle m_rtHandle;


    public OutlineRenderPass(Material mat, OutlineSettings default_settings)
    {
        m_mat = mat;
        m_defaultSettings = default_settings;
    }

    public void SetRTHandle(RTHandle rtHandle)
    {
        m_rtHandle = rtHandle;
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


    private class PassData
    {
        internal TextureHandle src;
        internal Material material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        if (resourceData.isActiveTargetBackBuffer)
            return;

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        TextureHandle dst = renderGraph.ImportTexture(m_rtHandle);
        UpdateOulineSettings();

        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;


        RenderGraphUtils.BlitMaterialParameters outlineParams = new(srcCamColor, dst, m_mat, 0);
        renderGraph.AddBlitPass(outlineParams, m_outlinePassName);
    }
}
