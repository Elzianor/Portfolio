#ifndef COMMON_VERTEX_SHADERS_FXH
#define COMMON_VERTEX_SHADERS_FXH

#include "CommonUniforms.fxh"
#include "CommonStructures.fxh"
#include "FabricCompute.fxh"

VSOutputPBRSolid VS_PBR_Solid(VSInputPBR input)
{
    VSOutputPBRSolid output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Normal = normalize(mul(input.Normal, (float3x3) WorldViewInverseTranspose));
    output.WorldViewPosition = mul(input.Position, WorldView).xyz;

    return output;
}

VSOutputPBRTextured VS_PBR_Textured(VSInputPBR input)
{
    VSOutputPBRTextured output;

    float3 normal = normalize(mul(input.Normal, (float3x3)WorldViewInverseTranspose));
    float3 tangent = normalize(mul(input.Tangent, (float3x3)WorldViewInverseTranspose));

    // When tangent vectors are calculated on larger meshes that share a considerable
    // amount of vertices, the tangent vectors are generally averaged to give nice
    // and smooth results. A problem with this approach is that the three TBN vectors
    // could end up non-perpendicular, which means the resulting TBN matrix would
    // no longer be orthogonal. So we re-orthogonalize T with respect to N.
    tangent = normalize(tangent - dot(tangent, normal) * normal);

    float3 bitangent = normalize(-cross(normal, tangent));

    output.Position = mul(input.Position, WorldViewProjection);
    output.TextureCoordinates = input.TextureCoordinates;

    float3 worldViewPosition = mul(input.Position, WorldView).xyz;
    float3x3 tbn = float3x3(tangent, bitangent, normal);
    float3x3 inverseTbn = transpose(tbn);

    if (LightType == 0) // directional
    {
        output.TangentLightDirection = mul(-WorldViewLightDirection, inverseTbn);
    }
    else if (LightType == 1) // point
    {
        output.TangentLightPosition = mul(WorldViewLightPosition, inverseTbn);
    }
    else if (LightType == 2) // spot
    {
        output.TangentLightPosition = mul(WorldViewLightPosition, inverseTbn);
        output.TangentLightDirection = mul(-WorldViewLightDirection, inverseTbn);
    }

    output.TangentPosition = mul(worldViewPosition, inverseTbn);

    return output;
}

VSOutputCompute VS_FabricCompute(VSInputCompute input)
{
    VSOutputCompute output;

    FabricParticle p = FabricParticlesReadOnly[input.VertexID];

    float4 position = float4(p.Position, 1.0);
    float3 normal = p.Normal;
    float2 textureCoord = p.TextureCoord;

    output.Position = mul(position, WorldViewProjection);
    output.Normal = normalize(mul(normal, (float3x3) WorldViewInverseTranspose));
    output.WorldViewPosition = mul(position, WorldView).xyz;
    output.TextureCoordinates = textureCoord;

    return output;
}

#endif // COMMON_VERTEX_SHADERS_FXH
