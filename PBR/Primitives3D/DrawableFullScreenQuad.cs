using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beryllium.Primitives3D;

internal class DrawableFullScreenQuad
{
    private GraphicsDevice _graphicsDevice;

    private VertexPositionTexture[] _vertices = {
        new(new Vector3(-1, 1, 0), new Vector2(0, 0)),   // Top-left
        new(new Vector3(1, 1, 0), new Vector2(1, 0)),    // Top-right
        new(new Vector3(-1, -1, 0), new Vector2(0, 1)),  // Bottom-left
        new(new Vector3(1, -1, 0), new Vector2(1, 1))    // Bottom-right
    };
    private short[] _indices = { 0, 1, 2, 1, 3, 2 };

    public VertexBuffer VertexBuffer { get; }
    public IndexBuffer IndexBuffer { get; }

    public DrawableFullScreenQuad(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        VertexBuffer = new VertexBuffer(graphicsDevice,
            typeof(VertexPositionTexture),
            _vertices.Length,
            BufferUsage.WriteOnly);
        VertexBuffer.SetData(_vertices);

        IndexBuffer = new IndexBuffer(graphicsDevice,
            typeof(short),
            _indices.Length,
            BufferUsage.WriteOnly);
        IndexBuffer.SetData(_indices);
    }

    public void Draw()
    {
        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
            0,
            0,
            2);
    }
}