#ifndef COMMON_PIXEL_SHADERS_EFFECT_HEADER_FXH
#define COMMON_PIXEL_SHADERS_EFFECT_HEADER_FXH

#include "CommonUniformsEffectHeader.fxh"
#include "CommonStructuresEffectHeader.fxh"
#include "ColorCorrectionEffectHeader.fxh"
#include "LightEffectHeader.fxh"
#include "PhysicallyBasedRenderingEffectHeader.fxh"
#include "ParallaxOcclusionMappingEffectHeader.fxh"

float4 PSSolid(VertexShaderOutputSolid input) : SV_TARGET
{
    float3 viewDirection = normalize(-input.WorldViewPosition);
    float3 lightDirection;

    float spotIntensity = 1.0;

    if (LightType == 0) // directional
    {
        lightDirection = normalize(-WorldViewLightDirection);
    }
    else if (LightType == 1) // point
    {
        lightDirection = normalize(WorldViewLightPosition - input.WorldViewPosition);
    }
    else if (LightType == 2) // spot
    {
        lightDirection = normalize(WorldViewLightPosition - input.WorldViewPosition);

        float theta = dot(lightDirection, normalize(-WorldViewLightDirection));
        float epsilon = CutOffInner - CutOffOuter;
        spotIntensity = smoothstep(0.0, 1.0, (theta - CutOffOuter) / epsilon);
    }

    float3 halfDirection = normalize(viewDirection + lightDirection);
    float3 normal = normalize(input.Normal);

    MaterialProperties material;

    material.DiffuseColor = DiffuseColor;
    material.Roughness = Roughness;
    material.Metallic = Metallic;
    material.Ao = 1.0;
    material.EmissiveColor = EmissiveColor;
    material.BaseReflectivity = BaseReflectivity;

    if (ApplyGammaCorrection)
    {
        material.DiffuseColor = InverseGammaCorrection(material.DiffuseColor, Gamma);
        material.EmissiveColor = InverseGammaCorrection(material.EmissiveColor, Gamma);
        LightColor = InverseGammaCorrection(LightColor, Gamma);
        AmbientColor = InverseGammaCorrection(AmbientColor, Gamma);
    }

    float attenuation = 1.0;

    if (LightType == 1 || LightType == 2)
    {
        float distanceToLightSource = length(WorldViewLightPosition - input.WorldViewPosition);
        attenuation = GetAttenuation(distanceToLightSource, Constant, Linear, Quadratic);
    }

    float3 color = PBR(normal,
        lightDirection,
        viewDirection,
        halfDirection,
        material,
        LightColor,
        AmbientColor,
        attenuation,
        spotIntensity);

    color = SimpleToneMapping(color);

    if (ApplyGammaCorrection)
    {
        color = GammaCorrection(color, Gamma);
    }

    color = saturate(color);

    return float4(color, 1.0);
}

float4 PSTextured(VertexShaderOutputTextured input) : SV_TARGET
{
    float3 viewDirection = normalize(-input.TangentPosition);
    float3 lightDirection;

    float spotIntensity = 1.0;

    if (LightType == 0) // directional
    {
        lightDirection = normalize(input.TangentLightDirection);
    }
    else if (LightType == 1) // point
    {
        lightDirection = normalize(input.TangentLightPosition - input.TangentPosition);
    }
    else if (LightType == 2) // spot
    {
        lightDirection = normalize(input.TangentLightPosition - input.TangentPosition);

        float theta = dot(lightDirection, normalize(input.TangentLightDirection));
        float epsilon = CutOffInner - CutOffOuter;
        spotIntensity = smoothstep(0.0, 1.0, (theta - CutOffOuter) / epsilon);
    }

    float3 halfDirection = normalize(viewDirection + lightDirection);

    float2 parallaxUV = ParallaxOcclusionMapping(input.TextureCoordinates,
        viewDirection,
        ParallaxMinSteps,
        ParallaxMaxSteps,
        ParallaxHeightScale,
        HeightMapTextureSampler,
        IsDepthMap,
        5.0);

    //if (parallaxUV.x > 1.0 || parallaxUV.y > 1.0 || parallaxUV.x < 0.0 || parallaxUV.y < 0.0) discard;

    float3 normal = tex2D(NormalMapTextureSampler, parallaxUV).xyz;
    normal = normal * 2.0 - 1.0;
    if (InvertNormalYAxis)
        normal.y = -normal.y;
    normal = normalize(normal);

    MaterialProperties material;

    material.DiffuseColor = tex2D(DiffuseMapTextureSampler, parallaxUV).xyz;
    material.Roughness = tex2D(RoughnessMapTextureSampler, parallaxUV).x;
    material.Metallic = tex2D(MetallicMapTextureSampler, parallaxUV).x;
    material.Ao = tex2D(AoMapTextureSampler, parallaxUV).x;
    material.EmissiveColor = tex2D(EmissiveMapTextureSampler, parallaxUV).xyz;
    material.BaseReflectivity = BaseReflectivity;

    if (ApplyGammaCorrection)
    {
        material.DiffuseColor = InverseGammaCorrection(material.DiffuseColor, Gamma);
        material.EmissiveColor = InverseGammaCorrection(material.EmissiveColor, Gamma);
        LightColor = InverseGammaCorrection(LightColor, Gamma);
        AmbientColor = InverseGammaCorrection(AmbientColor, Gamma);
    }

    float attenuation = 1.0;

    if (LightType == 1 || LightType == 2)
    {
        float distanceToLightSource = length(input.TangentLightPosition - input.TangentPosition);
        attenuation = GetAttenuation(distanceToLightSource, Constant, Linear, Quadratic);
    }

    float3 color = PBR(normal,
        lightDirection,
        viewDirection,
        halfDirection,
        material,
        LightColor,
        AmbientColor,
        attenuation,
        spotIntensity);

    color = SimpleToneMapping(color);

    if (ApplyGammaCorrection)
    {
        color = GammaCorrection(color, Gamma);
    }

    color = saturate(color);

    return float4(color, 1.0);
}

#endif // COMMON_PIXEL_SHADERS_EFFECT_HEADER_FXH
