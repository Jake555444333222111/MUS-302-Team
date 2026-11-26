using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

    

public class PainterRendererFeature : ScriptableRendererFeature
{
    [SerializeField] PainterRendererFeatureSettings settings;
    PainterRendererFeaturePass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new PainterRendererFeaturePass(settings);
        if(settings.painterMaterial == null)
        {
            Debug.LogWarning("Painter material is not set in the PainterRendererFeature.");
            return;
        }
        settings.painterMaterial.SetFloat("_Radius", settings.radius);
        settings.painterMaterial.SetFloat("_SampleSteps", settings.sampleSteps);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = settings.injectionPoint;


        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    // Use this class to pass around settings from the feature to the pass
    [Serializable]
    public class PainterRendererFeatureSettings
    {
        public Material painterMaterial;
        [Range(1,8)]public int downscale = 2;
        public int radius = 4;
        [Range(1,16)]public int sampleSteps = 2;
        public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
    }

    class PainterRendererFeaturePass : ScriptableRenderPass
    {
        readonly PainterRendererFeatureSettings settings;

        public PainterRendererFeaturePass(PainterRendererFeatureSettings settings)
        {
            this.settings = settings;
        }

        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class PassData
        {
            public Material painterMaterial;
            public TextureHandle src;
            public TextureHandle tempTexture;
            public TextureHandle dst;
        }


        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if(settings.painterMaterial == null)
            {
                Debug.LogWarning("Painter material is not set in the PainterRendererFeature.");
                return;
            }
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            var cameraDesc = cameraData.cameraTargetDescriptor;
            cameraDesc.depthBufferBits = 0;
            cameraDesc.msaaSamples = 1;
            cameraDesc.width /= settings.downscale;
            cameraDesc.height /= settings.downscale;

            TextureHandle tempTexture = renderGraph.CreateTexture(
                new TextureDesc(cameraDesc)
                {
                    name = "Painter Temp Texture",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                }
            );

            // --------------------
            // Downscale + effect
            // --------------------
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                "Painter Downscale Pass", out var passData))
            {
                passData.src = resourceData.activeColorTexture;
                passData.dst = tempTexture;
                passData.painterMaterial = settings.painterMaterial;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(passData.dst, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.src,
                        new Vector4(1, 1, 0, 0),
                        data.painterMaterial,
                        0
                    );
                });
            }

            // --------------------
            // Upscale back
            // --------------------
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                "Painter Upscale Pass", out var passData))
            {
                passData.src = tempTexture;
                passData.dst = resourceData.activeColorTexture;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(passData.dst, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.src,
                        new Vector4(1, 1, 0, 0),
                        0,
                        true
                    );
                });
            }
        }

    }
}
