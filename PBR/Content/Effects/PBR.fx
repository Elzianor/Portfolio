#include "EffectsHeader.fxh"

// --- UNIFORMS ---

cbuffer Transform
{
    float4x4 WorldViewProjection;
    float4x4 WorldView;
    float4x4 WorldViewInverseTranspose;
}

cbuffer Lighting
{
    float3 LightDirection;
    float3 LightColor;

    float3 AmbientColor;

    bool UseSingleDiffuseColor;
    float3 DiffuseColor;

    bool UseSingleEmissiveColor;
    float3 EmissiveColor;

    float BaseReflectivity;
    bool InvertGreenChannel;
    bool IsDepthMap;
    float ParallaxHeightScale;
    int ParallaxMinSteps;
    int ParallaxMaxSteps;

    bool ApplyGammaCorrection;
}

cbuffer Textures
{
    texture DiffuseMapTexture;
    sampler2D DiffuseMapTextureSampler = sampler_state
    {
        Texture = (DiffuseMapTexture);
        MinFilter = Linear;
        MagFilter = Linear;
        AddressU = Wrap;
        AddressV = Wrap;
    };

    texture NormalMapTexture;
    sampler2D NormalMapTextureSampler = sampler_state
    {
        Texture = (NormalMapTexture);
        MinFilter = Linear;
        MagFilter = Linear;
        AddressU = Wrap;
        AddressV = Wrap;
    };

    texture HeightMapTexture;
    sampler2D HeightMapTextureSampler = sampler_state
    {
        Texture = (HeightMapTexture);
        MinFilter = Linear;
        MagFilter = Linear;
        AddressU = Wrap;
        AddressV = Wrap;
    };

    texture RoughnessMapTexture;
    sampler2D RoughnessMapTextureSampler = sampler_state
    {
        Texture = (RoughnessMapTexture);
        MinFilter = Linear;
        MagFilter = Linear;
        AddressU = Wrap;
        AddressV = Wrap;
    };

    texture MetallicMapTexture;
    sampler2D MetallicMapTextureSampler = sampler_state
    {
        Texture = (MetallicMapTexture);
        MinFilter = Linear;
        MagFilter = Linear;
        AddressU = Wrap;
        AddressV = Wrap;
    };

    texture AoMapTexture;
    sampler2D AoMapTextureSampler = sampler_state
    {
        Texture = (AoMapTexture);
        MinFilter = Linear;
        MagFilter = Linear;
        AddressU = Wrap;
        AddressV = Wrap;
    };
    
    texture EmissiveMapTexture;
    sampler2D EmissiveMapTextureSampler = sampler_state
    {
        Texture = (EmissiveMapTexture);
        MinFilter = Linear;
        MagFilter = Linear;
        AddressU = Wrap;
        AddressV = Wrap;
    };
}

// --- STRUCTURES ---

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float2 TextureCoordinates : TEXCOORD0;
    uint instanceID : SV_InstanceID;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float3 WorldViewPosition : TEXCOORD1;
    float3x3 Tbn : TEXCOORD2;
    float3x3 InverseTbn : TEXCOORD5;
};

struct PixelShaderOutput
{
    float4 TargetGeneral : SV_TARGET0;
    float4 TargetBlur : SV_TARGET1;
};

// --- VERTEX AND PIXEL SHADERS ---

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float3 normal = normalize(mul(input.Normal, (float3x3)WorldViewInverseTranspose));
    float3 tangent = normalize(mul(input.Tangent, (float3x3)WorldViewInverseTranspose));
    tangent = normalize(tangent - dot(tangent, normal) * normal);
    float3 bitangent = normalize(-cross(normal, tangent));

    output.Position = mul(input.Position, WorldViewProjection);
    output.TextureCoordinates = input.TextureCoordinates;
    output.WorldViewPosition = mul(input.Position, WorldView).xyz;
    output.Tbn = float3x3(tangent, bitangent, normal);
    output.InverseTbn = transpose(output.Tbn);

    return output;
}

PixelShaderOutput PS(VertexShaderOutput input)
{
    PixelShaderOutput output;

    float3x3 tbn = input.Tbn;
    float3x3 inverseTbn = input.InverseTbn;

    float3 viewDirection = normalize(-input.WorldViewPosition);
    float3 lightDirection = normalize(-LightDirection);
    float3 halfDirection = normalize(viewDirection + lightDirection);

    float3 viewDirectionTangentSpace = normalize(mul(viewDirection, inverseTbn));

    float2 parallaxUV = ParallaxOcclusionMapping(input.TextureCoordinates,
    viewDirectionTangentSpace,
    ParallaxMinSteps,
    ParallaxMaxSteps,
    ParallaxHeightScale,
    HeightMapTextureSampler,
    IsDepthMap,
    5.0);

    if (parallaxUV.x > 1.0 || parallaxUV.y > 1.0 || parallaxUV.x < 0.0 || parallaxUV.y < 0.0) discard;

    float3 normal = tex2D(NormalMapTextureSampler, parallaxUV).xyz;
    normal = normal * 2.0 - 1.0;
    if (InvertGreenChannel) normal.y = -normal.y;
    normal = normalize(mul(normal, tbn));

    MaterialProperties material;

    if (UseSingleDiffuseColor) material.DiffuseColor = DiffuseColor;
    else material.DiffuseColor = tex2D(DiffuseMapTextureSampler, parallaxUV).xyz;
    material.Roughness = tex2D(RoughnessMapTextureSampler, parallaxUV).x;
    material.Metallic = tex2D(MetallicMapTextureSampler, parallaxUV).x;
    material.Ao = tex2D(AoMapTextureSampler, parallaxUV).x;
    if (UseSingleEmissiveColor) material.EmissiveColor = EmissiveColor;
    else material.EmissiveColor = tex2D(EmissiveMapTextureSampler, parallaxUV).xyz;
    material.BaseReflectivity = BaseReflectivity;

    if (ApplyGammaCorrection)
    {
        material.DiffuseColor = InverseGammaCorrection(material.DiffuseColor, 2.2);
        material.EmissiveColor = InverseGammaCorrection(material.EmissiveColor, 2.2);
        LightColor = InverseGammaCorrection(LightColor, 2.2);
        AmbientColor = InverseGammaCorrection(AmbientColor, 2.2);
    }

    float3 color = PBR(normal, lightDirection, viewDirection, halfDirection, material, LightColor, AmbientColor);

    output.TargetGeneral = float4(color, 1.0);

    //float brightness = dot(color.rgb, float3(0.5, 0.5, 0.5));

    //if (brightness > 1.0)

    if (material.Metallic > 0.7)
    {
        output.TargetBlur = float4(color, 1.0);
    }
    else
    {
        output.TargetBlur = float4(0.0, 0.0, 0.0, 1.0);
    }

    return output;
}

// --- TECHNIQUES ---

technique PBRTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}