using Beryllium.Primitives3D;
using Beryllium.VertexTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.Primitives3D;

public class DrawableCube : DrawableBasePrimitive
{
    private GraphicsDevice _graphicsDevice;

    public DrawableCube(GraphicsDevice graphicsDevice, float edgeSize)
        : base(graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        var hes = edgeSize / 2.0f;

        Vertices = new VertexPositionNormalTangentTexture[]
        {
            // Front Face
            new(new Vector3(-hes, -hes, -hes), Vector3.Forward, Vector3.Zero, Vector2.Zero), // 0
            new(new Vector3(hes, -hes, -hes), Vector3.Forward, Vector3.Zero, Vector2.Zero),  // 1
            new(new Vector3(hes, hes, -hes), Vector3.Forward, Vector3.Zero, Vector2.Zero),   // 2
            new(new Vector3(-hes, hes, -hes), Vector3.Forward, Vector3.Zero, Vector2.Zero),  // 3

            // Back Face
            new(new Vector3(-hes, -hes, hes), Vector3.Backward, Vector3.Zero, Vector2.Zero),  // 4
            new(new Vector3(hes, -hes, hes), Vector3.Backward, Vector3.Zero, Vector2.Zero),   // 5
            new(new Vector3(hes, hes, hes), Vector3.Backward, Vector3.Zero, Vector2.Zero),    // 6
            new(new Vector3(-hes, hes, hes), Vector3.Backward, Vector3.Zero, Vector2.Zero),   // 7

            // Left Face
            new(new Vector3(-hes, -hes, -hes), Vector3.Left, Vector3.Zero, Vector2.Zero),  // 8
            new(new Vector3(-hes, hes, -hes), Vector3.Left, Vector3.Zero, Vector2.Zero),   // 9
            new(new Vector3(-hes, hes, hes), Vector3.Left, Vector3.Zero, Vector2.Zero),    // 10
            new(new Vector3(-hes, -hes, hes), Vector3.Left, Vector3.Zero, Vector2.Zero),   // 11

            // Right Face
            new(new Vector3(hes, -hes, -hes), Vector3.Right, Vector3.Zero, Vector2.Zero),  // 12
            new(new Vector3(hes, hes, -hes), Vector3.Right, Vector3.Zero, Vector2.Zero),   // 13
            new(new Vector3(hes, hes, hes), Vector3.Right, Vector3.Zero, Vector2.Zero),    // 14
            new(new Vector3(hes, -hes, hes), Vector3.Right, Vector3.Zero, Vector2.Zero),   // 15

            // Top Face
            new(new Vector3(-hes, hes, -hes), Vector3.Up, Vector3.Zero, Vector2.Zero),  // 16
            new(new Vector3(hes, hes, -hes), Vector3.Up, Vector3.Zero, Vector2.Zero),   // 17
            new(new Vector3(hes, hes, hes), Vector3.Up, Vector3.Zero, Vector2.Zero),    // 18
            new(new Vector3(-hes, hes, hes), Vector3.Up, Vector3.Zero, Vector2.Zero),   // 19

            // Bottom Face
            new(new Vector3(-hes, -hes, -hes), Vector3.Down, Vector3.Zero, Vector2.Zero),  // 20
            new(new Vector3(hes, -hes, -hes), Vector3.Down, Vector3.Zero, Vector2.Zero),   // 21
            new(new Vector3(hes, -hes, hes), Vector3.Down, Vector3.Zero, Vector2.Zero),    // 22
            new(new Vector3(-hes, -hes, hes), Vector3.Down, Vector3.Zero, Vector2.Zero),   // 23
        };

        Indices = new int[]
        {
            // Front Face
            0, 1, 2, 0, 2, 3,
            // Back Face
            4, 6, 5, 4, 7, 6,
            // Left Face
            8, 9, 10, 8, 10, 11,
            // Right Face
            12, 14, 13, 12, 15, 14,
            // Top Face
            16, 17, 18, 16, 18, 19,
            // Bottom Face
            20, 22, 21, 20, 23, 22
        };
    }

    public override void Draw(Effect effect)
    {
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                Vertices,
                0,
                Vertices.Length,
                Indices,
                0,
                Indices.Length / 3
            );
        }
    }
}