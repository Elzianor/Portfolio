using Beryllium.VertexTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beryllium.Primitives3D;

internal class DrawableTile : DrawableBasePrimitive
{
    public DrawableTile(GraphicsDevice graphicsDevice, float sizeCoefficient)
        : base(graphicsDevice)
    {
        Vertices = new VertexPositionNormalTangentTexture[4];
        Indices = new []
        {
            0, 1, 2,
            0, 2, 3
        };

        Vertices[0].TextureCoordinate = new Vector2(0, 1); // Bottom left
        Vertices[1].TextureCoordinate = new Vector2(0, 0); // Top left
        Vertices[2].TextureCoordinate = new Vector2(1, 0); // Top right
        Vertices[3].TextureCoordinate = new Vector2(1, 1); // Bottom right

        for (var i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].Normal = new Vector3(0, 1, 0);
            Vertices[i].Tangent = new Vector3(1, 0, 0);
        }

        Generate(sizeCoefficient);
    }

    private void Generate(float sizeCoefficient)
    {
        Vertices[0].Position = new Vector3(-1, 0, 1);  // Bottom left
        Vertices[1].Position = new Vector3(-1, 0, -1); // Top left
        Vertices[2].Position = new Vector3(1, 0, -1);  // Top right
        Vertices[3].Position = new Vector3(1, 0, 1);   // Bottom right

        Vertices[0].Position *= sizeCoefficient;
        Vertices[1].Position *= sizeCoefficient;
        Vertices[2].Position *= sizeCoefficient;
        Vertices[3].Position *= sizeCoefficient;
    }
}