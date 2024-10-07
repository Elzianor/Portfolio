using Microsoft.Xna.Framework.Graphics;

namespace PBR.Utils
{
    internal class WireFrameManager
    {
        private GraphicsDevice _graphicsDevice;
        private bool _isWireFrame;
        private RasterizerState _wireFrameMode;
        private RasterizerState _defaultMode;

        public WireFrameManager(GraphicsDevice graphicsDevice, bool isWireFrame = false)
        {
            _graphicsDevice = graphicsDevice;
            _isWireFrame = isWireFrame;
            _wireFrameMode = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };
        }

        public void ToggleWireFrame()
        {
            _isWireFrame = !_isWireFrame;
        }

        public void ApplyWireFrame()
        {
            if (!_isWireFrame) return;

            _defaultMode = _graphicsDevice.RasterizerState;
            _graphicsDevice.RasterizerState = _wireFrameMode;
        }

        public void RestoreDefault()
        {
            if (!_isWireFrame) return;

            _graphicsDevice.RasterizerState = _defaultMode;
        }
    }
}
