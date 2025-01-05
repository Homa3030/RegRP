using UnityEngine;
using UnityEngine.Rendering;

namespace RegRP
{
    [CreateAssetMenu(fileName = "RegRenderPipeline", menuName = "RegRP/RegRenderPipeline")]
    public class RegRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new RegRenderPipeline();
        }
    }
}