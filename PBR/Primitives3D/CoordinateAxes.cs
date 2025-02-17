using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beryllium.Primitives3D;

internal class CoordinateAxes(GraphicsDevice graphicsDevice, float axisLength)
{
    private readonly BasicEffect _basicEffect = new(graphicsDevice)
    {
        VertexColorEnabled = true
    };

    private readonly VertexPositionColor[] _vertices =
    [
        // X
        new (Vector3.Zero, Color.Red),
        new (Vector3.UnitX * axisLength, Color.Red),
        // Y
        new (Vector3.Zero, Color.Green),
        new (Vector3.UnitY * axisLength, Color.Green),
        // Z
        new (Vector3.Zero, Color.Blue),
        new (Vector3.UnitZ * axisLength, Color.Blue)
    ];

    public void Update(Matrix world, Matrix view, Matrix projection)
    {
        _basicEffect.World = world;
        _basicEffect.View = view;
        _basicEffect.Projection = projection;
    }

    public virtual void Draw()
    {
        _basicEffect.CurrentTechnique.Passes[0].Apply();

        graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList,
            _vertices,
            0,
            3);
    }
}