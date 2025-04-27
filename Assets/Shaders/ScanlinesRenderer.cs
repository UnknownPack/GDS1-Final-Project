// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;

// [CreateAssetMenu(menuName = "Rendering/Scanlines Renderer Feature")]
// public class ScanlinesRenderer : ScriptableRendererFeature
// {
//     public bool enableEffect = true;

//     class ScanlinesRenderPass : ScriptableRenderPass
//     {
//         private Material material;
//         private RTHandle source;
//         private RTHandle destination;
//         private RTHandle lastFrame;
//         private const string ProfilerTag = "Scanlines Effect";
//         private static readonly int LastFrameTexId = Shader.PropertyToID("_LastFrameTex");
//         private ScanlinesEffect scanlinesEffect;

//         public ScanlinesRenderPass(Material material)
//         {
//             this.material = material;
//             profilingSampler = new ProfilingSampler(ProfilerTag);
//             renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
//         }

//         public void Setup(RTHandle source, RTHandle destination)
//         {
//             this.source = source;
//             this.destination = destination;
//         }

//         public void SetupLastFrame(RTHandle lastFrame)
//         {
//             this.lastFrame = lastFrame;
//         }

//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//         {
//             if (material == null) return;

//             // Get the volume settings
//             var stack = VolumeManager.instance.stack;
//             scanlinesEffect = stack.GetComponent<ScanlinesEffect>();

//             if (scanlinesEffect == null || !scanlinesEffect.IsActive())
//                 return;

//             var cmd = CommandBufferPool.Get(ProfilerTag);

//             // Create a temporary RT for intermediate operations
//             var tempRT = RTHandles.Alloc(source);

//             // Update material properties from volume settings
//             material.SetFloat("_Intensity", scanlinesEffect.intensity.value);
//             material.SetFloat("_LineCount", scanlinesEffect.lineCount.value);
//             material.SetColor("_LineColor", scanlinesEffect.lineColor.value);
//             material.SetFloat("_LineWidth", scanlinesEffect.lineWidth.value);
//             material.SetFloat("_ScrollSpeed", scanlinesEffect.animate.value ? scanlinesEffect.scrollSpeed.value : 0);
            
//             material.SetFloat("_EnableFlicker", scanlinesEffect.enableFlicker.value ? 1 : 0);
//             material.SetFloat("_FlickerSpeed", scanlinesEffect.flickerSpeed.value);
//             material.SetFloat("_FlickerAmount", scanlinesEffect.flickerAmount.value);
            
//             material.SetFloat("_EnableJitter", scanlinesEffect.enableJitter.value ? 1 : 0);
//             material.SetFloat("_JitterSpeed", scanlinesEffect.jitterSpeed.value);
//             material.SetFloat("_JitterAmount", scanlinesEffect.jitterAmount.value);
            
//             material.SetFloat("_EnablePulsing", scanlinesEffect.enablePulsing.value ? 1 : 0);
//             material.SetFloat("_PulseSpeed", scanlinesEffect.pulseSpeed.value);
//             material.SetFloat("_PulseAmount", scanlinesEffect.pulseAmount.value);

//             // Handle ghosting effect
//             if (lastFrame != null && scanlinesEffect.enableGhosting.value)
//             {
//                 material.SetTexture(LastFrameTexId, lastFrame);
//                 material.SetFloat("_GhostingAmount", scanlinesEffect.ghostingAmount.value);
//                 Blitter.BlitCameraTexture(cmd, source, lastFrame);
//             }

//             // First blit to temp RT to preserve original
//             Blitter.BlitCameraTexture(cmd, source, tempRT);
            
//             // Apply effect from temp to destination
//             Blitter.BlitCameraTexture(cmd, tempRT, destination, material, 0);

//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
            
//             // Release temporary RT
//             RTHandles.Release(tempRT);
//         }

//         public override void OnCameraCleanup(CommandBuffer cmd)
//         {
//             // Cleanup resources if needed
//         }
//     }

//     private ScanlinesRenderPass m_ScanlinesPass;
//     private RTHandle m_LastFrameRT;
//     public Material material;
//     public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

//     public override void Create()
//     {
//         m_ScanlinesPass = new ScanlinesRenderPass(material);
//         m_ScanlinesPass.renderPassEvent = renderPassEvent;
//     }

//     public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//     {
//         if (material == null || !enableEffect)
//             return;

//         // Setup the last frame RT for ghosting
//         if (m_LastFrameRT == null || !m_LastFrameRT.rt.IsCreated())
//         {
//             var desc = renderingData.cameraData.cameraTargetDescriptor;
//             desc.depthBufferBits = 0;
//             m_LastFrameRT = RTHandles.Alloc(desc, name: "_LastFrameTexture");
//         }

//         m_ScanlinesPass.SetupLastFrame(m_LastFrameRT);
//         m_ScanlinesPass.Setup(renderer.cameraColorTargetHandle, renderer.cameraColorTargetHandle);
//         renderer.EnqueuePass(m_ScanlinesPass);
//     }

//     protected override void Dispose(bool disposing)
//     {
//         if (m_LastFrameRT != null)
//         {
//             m_LastFrameRT.Release();
//             m_LastFrameRT = null;
//         }
//     }
// }