using UnityEngine;
using UnityEngine.Rendering;

namespace RegRP
{
    public partial class RegRenderer
    {
        private const string CmdName = "Render Camera";
        private static readonly ShaderTagId _unlitShaderTagId = new("SRPDefaultUnlit");
        private Camera _camera;

        private readonly CommandBuffer _cmd = new()
        {
            name = CmdName
        };

        private ScriptableRenderContext _context;
        private CullingResults _cullingResults;

        public void Render(ScriptableRenderContext context, Camera camera)
        {
            _context = context;
            _camera = camera;
            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull())
            {
                return;
            }

            Setup();

            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }

        private void Setup()
        {
            _context.SetupCameraProperties(_camera);
            CameraClearFlags clearFlags = _camera.clearFlags;
            _cmd.ClearRenderTarget(
                clearFlags != CameraClearFlags.Nothing,
                clearFlags == CameraClearFlags.Color,
                clearFlags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
            _cmd.BeginSample(SampleName);
            ExecuteCmd();
        }

        private void DrawVisibleGeometry()
        {
            var sortingSettings = new SortingSettings(_camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
            _context.DrawSkybox(_camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private void Submit()
        {
            _cmd.EndSample(SampleName);
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
            if (_camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
            {
                _cullingResults = _context.Cull(ref cullingParameters);
                return true;
            }

            return false;
        }
    }
}