using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Beryllium.Primitives3D;

public class Sphere
{
    private List<int> _indices = [];

    public List<(Vector3 Position, Vector3 Normal, Vector2 TextureCoordinate)> Vertices { get; } = [];
    public int[] Indices { get; private set; }

    public Sphere(int radius, int longitudeSegments, int latitudeSegments)
    {
        GenerateSphere(radius, longitudeSegments, latitudeSegments);
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

                Vertices.Add((position, normal, textureCoordinate));
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

                _indices.Add(second);
                _indices.Add(second + 1);
                _indices.Add(first + 1);
            }
        }

        Indices = _indices.ToArray();
    }
}
