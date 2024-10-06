using Beryllium.VertexTypes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Beryllium.Primitives3D;

public class SphereVertex
{
    private int _trianglesCount;
    private Vector3 _cumulativeTangent;

    public Vector3 Position { get; set; }
    public Vector3 Normal { get; set; }
    public Vector3 Tangent => _cumulativeTangent / _trianglesCount;
    public Vector2 TextureCoordinate { get; set; }

    public void AddTangent(Vector3 tangent)
    {
        _trianglesCount++;
        _cumulativeTangent += tangent;
    }
}

public class Sphere
{
    private List<int> _indices = [];
    private List<SphereVertex> _vertices = [];

    public VertexPositionNormalTangentTexture[] Vertices { get; private set; }
    public int[] Indices { get; private set; }

    public Sphere(int radius, int longitudeSegments, int latitudeSegments)
    {
        GenerateSphere(radius, longitudeSegments, latitudeSegments);
        SetVertices();
    }

    private void GenerateSphere(int radius, int longitudeSegments, int latitudeSegments)
    {
        for (var lat = 0; lat <= latitudeSegments; lat++)
        {
            var theta = MathHelper.Pi * lat / latitudeSegments;
            var sinTheta = (float)Math.Sin(theta);
            var cosTheta = (float)Math.Cos(theta);

            for (var lon = 0; lon <= longitudeSegments; lon++)
            {
                var phi = 2 * MathHelper.Pi * lon / longitudeSegments;
                var sinPhi = (float)Math.Sin(phi);
                var cosPhi = (float)Math.Cos(phi);

                var position = new Vector3(radius * cosPhi * sinTheta,
                    radius * cosTheta,
                    radius * sinPhi * sinTheta);

                var normal = Vector3.Normalize(position);
                var textureCoordinate = new Vector2((float)lon / longitudeSegments, (float)lat / latitudeSegments);

                _vertices.Add(new SphereVertex
                {
                    Position = position,
                    Normal = normal,
                    TextureCoordinate = textureCoordinate
                });
            }
        }

        for (var lat = 0; lat < latitudeSegments; lat++)
        {
            for (var lon = 0; lon < longitudeSegments; lon++)
            {
                var first = lat * (longitudeSegments + 1) + lon;
                var second = first + longitudeSegments + 1;

                _indices.Add(first);
                _indices.Add(second);
                _indices.Add(first + 1);

                AddTangentForVertices(first, second, first + 1);

                _indices.Add(second);
                _indices.Add(second + 1);
                _indices.Add(first + 1);

                AddTangentForVertices(second, second + 1, first + 1);
            }
        }

        Indices = _indices.ToArray();
    }

    private void AddTangentForVertices(int firstIndex, int secondIndex, int thirdIndex)
    {
        var v1 = _vertices[firstIndex];
        var v2 = _vertices[secondIndex];
        var v3 = _vertices[thirdIndex];

        var edge1 = v2.Position - v1.Position;
        var edge2 = v3.Position - v1.Position;

        var deltaUV1 = v2.TextureCoordinate - v1.TextureCoordinate;
        var deltaUV2 = v3.TextureCoordinate - v1.TextureCoordinate;

        var f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

        var tangent = new Vector3(
            f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X),
            f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y),
            f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z)
        );

        v1.AddTangent(tangent);
        v2.AddTangent(tangent);
        v3.AddTangent(tangent);
    }

    private void SetVertices()
    {
        Vertices = new VertexPositionNormalTangentTexture[_vertices.Count];

        for (var i = 0; i < _vertices.Count; i++)
        {
            var vertex = _vertices[i];

            Vertices[i] = new VertexPositionNormalTangentTexture(vertex.Position,
                vertex.Normal,
                vertex.Tangent,
                vertex.TextureCoordinate);
        }
    }
}
