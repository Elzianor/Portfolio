using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PBR.Primitives3D;

public class DrawableGeodesicSphere
{
    private struct Triangle(int v1, int v2, int v3)
    {
        public readonly int V1 = v1;
        public readonly int V2 = v2;
        public readonly int V3 = v3;
    }

    private readonly List<Vector3> _vertexPositions = [];
    private readonly List<Triangle> _triangles = [];
    private readonly Dictionary<long, int> _middlePointCache = new ();

    private VertexPositionNormalTexture[] _vertices;
    private int[] _indices;

    public DrawableGeodesicSphere(int subdivisions)
    {
        // Golden ratio
        var t = (1.0f + (float)Math.Sqrt(5.0)) / 2.0f;

        // Create 12 initial vertices of icosahedron
        AddVertex(new Vector3(-1, t, 0));
        AddVertex(new Vector3(1, t, 0));
        AddVertex(new Vector3(-1, -t, 0));
        AddVertex(new Vector3(1, -t, 0));

        AddVertex(new Vector3(0, -1, t));
        AddVertex(new Vector3(0, 1, t));
        AddVertex(new Vector3(0, -1, -t));
        AddVertex(new Vector3(0, 1, -t));

        AddVertex(new Vector3(t, 0, -1));
        AddVertex(new Vector3(t, 0, 1));
        AddVertex(new Vector3(-t, 0, -1));
        AddVertex(new Vector3(-t, 0, 1));

        // Create 20 faces
        _triangles.Add(new Triangle(0, 11, 5));
        _triangles.Add(new Triangle(0, 5, 1));
        _triangles.Add(new Triangle(0, 1, 7));
        _triangles.Add(new Triangle(0, 7, 10));
        _triangles.Add(new Triangle(0, 10, 11));
        _triangles.Add(new Triangle(1, 5, 9));
        _triangles.Add(new Triangle(5, 11, 4));
        _triangles.Add(new Triangle(11, 10, 2));
        _triangles.Add(new Triangle(10, 7, 6));
        _triangles.Add(new Triangle(7, 1, 8));
        _triangles.Add(new Triangle(3, 9, 4));
        _triangles.Add(new Triangle(3, 4, 2));
        _triangles.Add(new Triangle(3, 2, 6));
        _triangles.Add(new Triangle(3, 6, 8));
        _triangles.Add(new Triangle(3, 8, 9));
        _triangles.Add(new Triangle(4, 9, 5));
        _triangles.Add(new Triangle(2, 4, 11));
        _triangles.Add(new Triangle(6, 2, 10));
        _triangles.Add(new Triangle(8, 6, 7));
        _triangles.Add(new Triangle(9, 8, 1));

        // Subdivide triangles
        for (var i = 0; i < subdivisions; i++)
        {
            var newTriangles = new List<Triangle>();

            foreach (var tri in _triangles)
            {
                var a = GetMiddlePoint(tri.V1, tri.V2);
                var b = GetMiddlePoint(tri.V2, tri.V3);
                var c = GetMiddlePoint(tri.V3, tri.V1);

                newTriangles.Add(new Triangle(tri.V1, a, c));
                newTriangles.Add(new Triangle(tri.V2, b, a));
                newTriangles.Add(new Triangle(tri.V3, c, b));
                newTriangles.Add(new Triangle(a, b, c));
            }

            _triangles = newTriangles;
        }

        FillVertices();
        FillIndices();
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

    private int AddVertex(Vector3 position)
    {
        var length = position.Length();
        position /= length; // Normalize to sphere surface

        _vertexPositions.Add(position);

        return _vertexPositions.Count - 1;
    }

    private int GetMiddlePoint(int p1, int p2)
    {
        long smallerIndex = Math.Min(p1, p2);
        long greaterIndex = Math.Max(p1, p2);
        var key = (smallerIndex << 32) + greaterIndex;

        if (_middlePointCache.TryGetValue(key, out var cachedIndex))
            return cachedIndex;

        // Calculate new vertex at midpoint
        var middle = (_vertexPositions[p1] + _vertexPositions[p2]) * 0.5f;
        var newIndex = AddVertex(middle);

        _middlePointCache[key] = newIndex;
        return newIndex;
    }

    private void FillVertices()
    {
        _vertices = new VertexPositionNormalTexture[_vertexPositions.Count];

        for (var i = 0; i < _vertexPositions.Count; i++)
        {
            var normal = Vector3.Normalize(_vertexPositions[i]);
            var uv = new Vector2(
                0.5f + (float)(Math.Atan2(normal.Z, normal.X) / (2.0 * Math.PI)),
                0.5f - (float)(Math.Asin(normal.Y) / Math.PI)
            );

            // Check for seam and duplicate vertices if necessary
            if (uv.X < 0.001f)
            {
                uv.X = 0;
            }
            else if (uv.X > 0.999f)
            {
                uv.X = 1;
            }

            _vertices[i] = new VertexPositionNormalTexture(_vertexPositions[i], normal, uv);
        }
    }

    private void FillIndices()
    {
        var indices = new List<int>();

        foreach (var tri in _triangles)
        {
            indices.Add(tri.V1);
            indices.Add(tri.V3);
            indices.Add(tri.V2);
        }

        _indices = indices.ToArray();
    }
}
