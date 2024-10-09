using Microsoft.Xna.Framework.Graphics;

namespace PBR.Utils
{
    internal class WireFrameManager
    {
        private GraphicsDevice _graphicsDevice;
        private RasterizerState _wireFrameMode;
        private RasterizerState _defaultMode;

        public bool IsWireFrame { get; private set; }

        public WireFrameManager(GraphicsDevice graphicsDevice, bool isWireFrame = false)
        {
            _graphicsDevice = graphicsDevice;
            IsWireFrame = isWireFrame;
            _wireFrameMode = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };
        }

        public void ToggleWireFrame()
        {
            IsWireFrame = !IsWireFrame;
        }

        public void ApplyWireFrame()
        {
            if (!IsWireFrame) return;

            _defaultMode = _graphicsDevice.RasterizerState;
            _graphicsDevice.RasterizerState = _wireFrameMode;
        }

        public void RestoreDefault()
        {
            if (!IsWireFrame) return;

            _graphicsDevice.RasterizerState = _defaultMode;
        }
    }
}
