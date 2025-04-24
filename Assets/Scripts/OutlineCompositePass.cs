using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class OutlineCompositePass:ScriptableRenderPass
{

    private RTHandle m_outlineRT;
    private Material m_mat;

    private RenderTextureDescriptor m_outlineRTDesc;
    private static Vector4 m_ScaleBias = new Vector4(1f, 1f, 0f, 0f);

    private const string m_tempTexture = "TempTexture";
    private const string m_outlineComposePassName = "OutlineCompPass";
    private const string m_outlineCopyPassName = "OutlineCopyPass";


    public OutlineCompositePass(Material mat)
    {
        m_mat = mat;
        m_outlineRTDesc = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
    }

    public void SetOulineTexture(RTHandle outlineRT)
    {
        m_outlineRT = outlineRT;
    }

    private static void ExecutePass(PassData data, RasterGraphContext context, int pass)
    {
        Blitter.BlitTexture(context.cmd, data.src, m_ScaleBias, data.material, pass);
    }

    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), 0, false);
    }

    private class PassData
    {
        internal TextureHandle src;
        internal Material material;
    }

    private class PassData1
    {
        internal TextureHandle src;
    }


    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer)
            return;


        var srcCamColor = resourceData.activeColorTexture;
        var outlineTex = renderGraph.ImportTexture(m_outlineRT);
        TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph,
                m_outlineRTDesc, m_tempTexture, false);


        using (var builder = renderGraph.AddRasterRenderPass<PassData>(m_outlineComposePassName, out var passData))
        {

            passData.src = srcCamColor;
            passData.material = m_mat;

            builder.UseTexture(passData.src);
            builder.SetInputAttachment(outlineTex, 1);
            builder.SetRenderAttachment(dst, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context, 0));
        }

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(m_outlineCopyPassName, out var passData))
        {
            passData.src = dst;
            passData.material = m_mat;

            builder.UseTexture(passData.src);
            builder.SetRenderAttachment(srcCamColor, 0);
            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }
    }
}
