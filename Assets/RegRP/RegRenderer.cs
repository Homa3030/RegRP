using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

namespace RegRP
{
    public class RegRenderer
    {
        private ScriptableRenderContext _context;
        private Camera _camera;
        private CullingResults _cullingResults;
        private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        private static ShaderTagId[] _legacyShaderTagIds = new ShaderTagId[]
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };
        private static Material _errorMaterial;
        
        private CommandBuffer _cmd = new CommandBuffer()
        {
            name = CmdName,
        };

        private const string CmdName = "Render Camera";

        public void Render(ScriptableRenderContext context, Camera camera)
        {
            _context = context;
            _camera = camera;
            if (!Cull()) return;
            
            Setup();

            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            Submit();
        }

        private void Setup()
        {
            _context.SetupCameraProperties(_camera);
            _cmd.ClearRenderTarget(true, true, Color.clear);
            _cmd.BeginSample(CmdName);
            ExecuteCmd();
        }

        private void DrawVisibleGeometry()
        {
            SortingSettings sortingSettings = new SortingSettings(_camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            DrawingSettings drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
            _context.DrawSkybox(_camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private void DrawUnsupportedShaders()
        {
            if (_errorMaterial == null)
            {
                _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            }
            DrawingSettings drawingSettings = new DrawingSettings(_legacyShaderTagIds[0], new SortingSettings(_camera))
            {
                overrideMaterial = _errorMaterial
            };
            for (int i = 1; i < _legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
            }
            
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private void Submit()
        {
            _cmd.EndSample(CmdName);
            ExecuteCmd();
            _context.Submit();
        }

        private void ExecuteCmd()
        {
            _context.ExecuteCommandBuffer(_cmd);
            _cmd.Clear();
        }

        private bool Cull()
        {
            if (_camera.TryGetCullingParameters(out var cullingParameters))
            {
                _cullingResults = _context.Cull(ref cullingParameters);
                return true;
            }
            return false;
        }
    }
}