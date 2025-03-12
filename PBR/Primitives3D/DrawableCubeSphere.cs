using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PBR.Primitives3D;

public class DrawableCubeSphere
{
    private readonly VertexPositionNormalTexture[] _vertices;
    private readonly int[] _indices;

    public DrawableCubeSphere(int subdivisions, float radius)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        var indices = new List<int>();

        var resolution = subdivisions + 2; // How many vertices per face
        var step = 2.0f / (resolution - 1);

        for (var face = 0; face < 6; face++)
        {
            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var cubePos = GetCubePosition(face, x * step - 1, y * step - 1);
                    var spherePos = CubeToSphere(cubePos) * radius;
                    var uv = CubeToSphereUV(face, x, y, resolution);

                    vertices.Add(new VertexPositionNormalTexture(spherePos,
                        Vector3.Normalize(spherePos),
                        uv));
                }
            }

            // Create indices for this face
            for (var y = 0; y < resolution - 1; y++)
            {
                for (var x = 0; x < resolution - 1; x++)
                {
                    var i = face * resolution * resolution + y * resolution + x;

                    // For each face, create two triangles with clockwise winding order
                    indices.Add(i); // Triangle 1: A
                    indices.Add(i + resolution); // C
                    indices.Add(i + 1); // B

                    indices.Add(i + 1); // Triangle 2: A
                    indices.Add(i + resolution); // C
                    indices.Add(i + resolution + 1); // D
                }
            }
        }

        _vertices = vertices.ToArray();
        _indices = indices.ToArray();
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
            _vertices,
            0,
            _vertices.Length,
            _indices,
            0,
            _indices.Length / 3);
    }

    private Vector3 GetCubePosition(int face, float x, float y)
    {
        return face switch
        {
            0 => // +X face
                new Vector3(1, y, -x), // Right side, varies in Y and Z.
            1 => // -X face
                new Vector3(-1, y, x), // Left side, varies in Y and Z.
            2 => // +Y face
                new Vector3(x, 1, -y), // Top side, varies in X and Z.
            3 => // -Y face
                new Vector3(x, -1, y), // Bottom side, varies in X and Z.
            4 => // +Z face
                new Vector3(-y, x, 1), // Front side, varies in X and Y.
            5 => // -Z face
                new Vector3(-y, -x, -1), // Back side, varies in X and Y.
            _ => Vector3.Zero
        };
    }

    private Vector3 CubeToSphere(Vector3 cubePos)
    {
        var x = cubePos.X * MathF.Sqrt(1.0f - (cubePos.Y * cubePos.Y) / 2.0f - (cubePos.Z * cubePos.Z) / 2.0f + (cubePos.Y * cubePos.Y * cubePos.Z * cubePos.Z) / 3.0f);
        var y = cubePos.Y * MathF.Sqrt(1.0f - (cubePos.Z * cubePos.Z) / 2.0f - (cubePos.X * cubePos.X) / 2.0f + (cubePos.Z * cubePos.Z * cubePos.X * cubePos.X) / 3.0f);
        var z = cubePos.Z * MathF.Sqrt(1.0f - (cubePos.X * cubePos.X) / 2.0f - (cubePos.Y * cubePos.Y) / 2.0f + (cubePos.X * cubePos.X * cubePos.Y * cubePos.Y) / 3.0f);

        return new Vector3(x, y, z);
    }

    private Vector2 CubeToSphereUV(int face, int x, int y, int resolution)
    {
        var u = (float)x / (resolution - 1);
        var v = (float)y / (resolution - 1);

        return face switch
        {
            0 => new Vector2(u, v),     // +X
            1 => new Vector2(1 - u, v), // -X
            2 => new Vector2(u, 1 - v), // +Y
            3 => new Vector2(u, v),     // -Y
            4 => new Vector2(v, 1 - u), // +Z
            5 => new Vector2(v, u),     // -Z
            _ => Vector2.Zero
        };
    }
}
