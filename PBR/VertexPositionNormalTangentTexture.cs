using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Beryllium.VertexTypes;

[DataContract]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionNormalTangentTexture : IVertexType
{
    [DataMember]
    public Vector3 Position;
    [DataMember]
    public Vector3 Normal;
    [DataMember]
    public Vector3 Tangent;
    [DataMember]
    public Vector2 TextureCoordinate;

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public VertexPositionNormalTangentTexture(Vector3 position,
        Vector3 normal,
        Vector3 tangent,
        Vector2 textureCoordinate)
    {
        Position = position;
        Normal = normal;
        Tangent = tangent;
        TextureCoordinate = textureCoordinate;
    }

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
        new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    );

    public static int SizeInBytes => sizeof(float) * 11;

    public static bool operator !=(VertexPositionNormalTangentTexture left,
        VertexPositionNormalTangentTexture right)
    {
        return left.GetHashCode() != right.GetHashCode();
    }

    public static bool operator ==(VertexPositionNormalTangentTexture left,
        VertexPositionNormalTangentTexture right)
    {
        return left.GetHashCode() == right.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return this == (VertexPositionNormalTangentTexture)obj;
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode() |
               Normal.GetHashCode() |
               Tangent.GetHashCode() |
               TextureCoordinate.GetHashCode();
    }

    public override string ToString()
    {
        return $"X: {Position.X}, Y: {Position.Y}, Z: {Position.Z}";
    }
}