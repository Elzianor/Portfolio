#ifndef COMMON_VERTEX_SHADERS_EFFECT_HEADER_FXH
#define COMMON_VERTEX_SHADERS_EFFECT_HEADER_FXH

#include "CommonUniformsEffectHeader.fxh"
#include "CommonStructuresEffectHeader.fxh"

VertexShaderOutputSolid VSSolid(VertexShaderInput input)
{
    VertexShaderOutputSolid output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Normal = normalize(mul(input.Normal, (float3x3) WorldViewInverseTranspose));
    output.WorldViewPosition = mul(input.Position, WorldView).xyz;

    return output;
}

VertexShaderOutputTextured VSTextured(VertexShaderInput input)
{
    VertexShaderOutputTextured output;

    float3 normal = normalize(mul(input.Normal, (float3x3) WorldViewInverseTranspose));
    float3 tangent = normalize(mul(input.Tangent, (float3x3) WorldViewInverseTranspose));
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

#endif // COMMON_VERTEX_SHADERS_EFFECT_HEADER_FXH
