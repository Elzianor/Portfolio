#include "PhysicallyBasedRenderingEffectHeader.fxh"
#include "ParallaxOcclusionMappingEffectHeader.fxh"
#include "ColorCorrectionEffectHeader.fxh"
#include "LightEffectHeader.fxh"

// --- UNIFORMS ---

// Transform
float4x4 WorldViewProjection;
float4x4 WorldView;
float4x4 WorldViewInverseTranspose;

// Lighting
uint LightType;

float Constant;
float Linear;
float Quadratic;

float CutOffInner;
float CutOffOuter;

float3 WorldViewLightDirection;
float3 WorldViewLightPosition;

float3 LightColor;
float3 AmbientColor;

float BaseReflectivity;

bool InvertNormalYAxis;
bool IsDepthMap;

float ParallaxHeightScale;
int ParallaxMinSteps;
int ParallaxMaxSteps;

float Gamma;
bool ApplyGammaCorrection;

// Solid material properties
float3 DiffuseColor;
float3 EmissiveColor;
float Roughness;
float Metallic;

// Textured material properties
texture DiffuseMapTexture;
sampler2D DiffuseMapTextureSampler = sampler_state
{
    Texture = (DiffuseMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture NormalMapTexture;
sampler2D NormalMapTextureSampler = sampler_state
{
    Texture = (NormalMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture HeightMapTexture;
sampler2D HeightMapTextureSampler = sampler_state
{
    Texture = (HeightMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture RoughnessMapTexture;
sampler2D RoughnessMapTextureSampler = sampler_state
{
    Texture = (RoughnessMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture MetallicMapTexture;
sampler2D MetallicMapTextureSampler = sampler_state
{
    Texture = (MetallicMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture AoMapTexture;
sampler2D AoMapTextureSampler = sampler_state
{
    Texture = (AoMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture EmissiveMapTexture;
sampler2D EmissiveMapTextureSampler = sampler_state
{
    Texture = (EmissiveMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

// --- STRUCTURES ---

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutputSolid
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float3 WorldViewPosition : TEXCOORD1;
};

struct VertexShaderOutputTextured
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float3 TangentLightPosition : TEXCOORD1;
    float3 TangentLightDirection : TEXCOORD2;
    float3 TangentPosition : TEXCOORD3;
    //float3 TangentWorldViewPosition : TEXCOORD3;
    //float3 WorldViewPosition : TEXCOORD1;
    //float3x3 Tbn : TEXCOORD2;
};

// --- VERTEX AND PIXEL SHADERS ---

// Solid

VertexShaderOutputSolid VSSolid(VertexShaderInput input)
{
    VertexShaderOutputSolid output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Normal = normalize(mul(input.Normal, (float3x3) WorldViewInverseTranspose));
    output.WorldViewPosition = mul(input.Position, WorldView).xyz;

    return output;
}

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

// Textured

VertexShaderOutputTextured VSTextured(VertexShaderInput input)
{
    VertexShaderOutputTextured output;

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
    if (InvertNormalYAxis) normal.y = -normal.y;
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

// --- TECHNIQUES ---

technique Solid
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VSSolid();
        PixelShader = compile ps_5_0 PSSolid();
    }
}

technique Textured
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VSTextured();
        PixelShader = compile ps_5_0 PSTextured();
    }
}