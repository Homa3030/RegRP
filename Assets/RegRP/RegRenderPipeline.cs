using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RegRP
{
    public class RegRenderPipeline : RenderPipeline
    {
        private readonly RegRenderer _renderer = new();

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            foreach (var camera in cameras)
            {
                _renderer.Render(context, camera);
            }
        }
    }
}