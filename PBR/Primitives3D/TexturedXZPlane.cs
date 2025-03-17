using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Beryllium.VertexTypes;

namespace Beryllium.Primitives3D;

internal class TexturedXZPlane
{
    private GraphicsDevice _graphicsDevice;
    private readonly Point _sizeInTiles;
    private readonly float _tileSize;

    private VertexPositionNormalTangentTexture[] _vertices;
    private int[] _indices;

    public float SizeX => _sizeInTiles.X * _tileSize;
    public float SizeZ => _sizeInTiles.Y * _tileSize;

    private Vector3 _position;
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            WorldMatrix = Matrix.CreateTranslation(_position);
        }
    }

    public Matrix WorldMatrix { get; private set; } = Matrix.Identity;

    public Plane Plane { get; }

    public TexturedXZPlane(GraphicsDevice graphicsDevice, Point sizeInTiles, float tileSize)
    {
        _graphicsDevice = graphicsDevice;
        _sizeInTiles = sizeInTiles;
        _tileSize = tileSize;

        Plane = new Plane(Vector3.Up, 0);

        Generate();
    }

    public void Draw(Effect effect)
    {
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            _graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _vertices,
                0,
                _vertices.Length,
                _indices,
                0,
                _indices.Length / 3
            );
        }
    }

    private void Generate()
    {
        var vertices = new List<VertexPositionNormalTangentTexture>();
        var indices = new List<int>();

        for (var z = 0; z <= _sizeInTiles.Y; z++)
        {
            for (var x = 0; x <= _sizeInTiles.X; x++)
            {
                vertices.Add(new VertexPositionNormalTangentTexture(
                    new Vector3(x * _tileSize, 0, -z * _tileSize),
                    Vector3.Up,
                    Vector3.UnitX,
                    new Vector2(x, -z)
                ));
            }
        }

        for (var z = 0; z < _sizeInTiles.Y; z++)
        {
            for (var x = 0; x < _sizeInTiles.X; x++)
            {
                var bottomLeft = z * (_sizeInTiles.X + 1) + x;
                var bottomRight = bottomLeft + 1;
                var topLeft = (z + 1) * (_sizeInTiles.X + 1) + x;
                var topRight = topLeft + 1;

                indices.Add(bottomLeft);
                indices.Add(topLeft);
                indices.Add(bottomRight);

                indices.Add(bottomRight);
                indices.Add(topLeft);
                indices.Add(topRight);
            }
        }

        _vertices = vertices.ToArray();
        _indices = indices.ToArray();
    }
}