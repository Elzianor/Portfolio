#include "EffectsHeader.fxh"

// --- CONSTANTS ---

const float PI = 3.14159265359;

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
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float3 WorldViewPosition : TEXCOORD1;
    float3x3 Tbn : TEXCOORD2;
};

struct PixelShaderOutput
{
    float4 target0 : SV_Target0;
    float4 target1 : SV_Target1;
};

struct MaterialProperties
{
    float3 DiffuseColor;
    float Roughness;
    float Metallic;
    float Ao;
    float3 EmissiveColor;
    float BaseReflectivity;
};

// --- FUNCTIONS ---

float Dot(float3 x, float3 y)
{
    return max(dot(x, y), 0.0);
}

float NonZeroDenominator(float val)
{
    return max(val, 0.000001);
}

float D(float3 n, float3 h, float roughness)
{
    float NdotH = Dot(n, h);

    float alpha = roughness * roughness;
    float alpha2 = alpha * alpha;
    float denominator = NdotH * NdotH * (alpha2 - 1.0) + 1.0;

    return alpha2 / NonZeroDenominator(PI * denominator * denominator);
}

float G1(float3 n, float3 x, float roughness)
{
    float alpha = roughness * roughness;
    float k = alpha / 2.0;
    float NdotX = Dot(n, x);

    return NdotX / NonZeroDenominator(NdotX * (1.0 - k) + k);
}

float G(float3 n, float3 v, float3 l, float roughness)
{
    return G1(n, l, roughness) * G1(n, v, roughness);
}

float3 F(float3 f0, float3 v, float3 h)
{
    float VdotH = Dot(v, h);

    return f0 + (1.0 - f0) * pow(1.0 - VdotH, 5.0);
}

float3 PBR(float3 n, float3 l, float3 v, float3 h, MaterialProperties material)
{
    float f0 = material.BaseReflectivity;
    float3 lerpF0 = lerp(float3(f0, f0, f0), material.DiffuseColor, material.Metallic);

    float d = D(n, h, material.Roughness);
    float g = G(n, v, l, material.Roughness);
    float3 f = F(lerpF0, v, h);

    float NdotV = Dot(n, v);
    float NdotL = Dot(n, l);

    float3 kd = 1.0 - f;
    kd *= (1.0 - material.Metallic);

    float3 numerator = d * g * f;
    float denominator = 4.0 * NdotV * NdotL;
    float3 specular = numerator / NonZeroDenominator(denominator);

    float ao = material.Ao;

    if (ao < 0.85) ao *= 0.5;

    float3 color = material.EmissiveColor * material.DiffuseColor +
    LightColor * (kd * material.DiffuseColor / PI + specular) * NdotL +
    //AmbientColor * material.DiffuseColor * material.Ao;
    AmbientColor * material.DiffuseColor * ao;

    return saturate(color);
}

float2 ParallaxOcclusionMapping(float2 texCoords, float3 viewDirTangentSpace)
{
    viewDirTangentSpace = normalize(viewDirTangentSpace);

    int steps = lerp(ParallaxMinSteps, ParallaxMaxSteps, length(viewDirTangentSpace.xy));
    const float stepDelta = 1.0 / steps;

    float2 uvDelta = viewDirTangentSpace.xy * ParallaxHeightScale / (viewDirTangentSpace.z * steps);

    float2 currentUV = texCoords;
    float currentStepDepth = 0.0;

    for (int i = 0; i < steps; i++)
    {
        currentUV = texCoords - i * uvDelta;
        float currentHeightMapValue = tex2D(HeightMapTextureSampler, currentUV).x;
        if (!IsDepthMap) currentHeightMapValue = 1.0 - currentHeightMapValue;
        currentStepDepth = i * stepDelta;

        if (currentStepDepth >= currentHeightMapValue ||
            i == 100) // HLSL do not like dynamic loops, so we set maximum possible number of cycles
            break;
    }

    // Binary search to refine the intersection point
    float2 prevUV = currentUV + uvDelta;

    for (int j = 0; j < 5; j++)  // 5 refinement steps
    {
        float currentDepth = tex2D(HeightMapTextureSampler, currentUV).r;
        float prevDepth = tex2D(HeightMapTextureSampler, prevUV).r;
        float midDepth = (currentDepth + prevDepth) / 2.0;

        if (midDepth < currentStepDepth)
        {
            currentUV = (currentUV + prevUV) / 2.0;
        }
        else
        {
            prevUV = (currentUV + prevUV) / 2.0;
        }
    }

    return currentUV;
}

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

    return output;
}

PixelShaderOutput PS(VertexShaderOutput input)
{
    PixelShaderOutput output;

    float3x3 tbn = input.Tbn;
    float3x3 inverseTbn = transpose(tbn);

    float3 viewDirection = normalize(-input.WorldViewPosition);
    float3 lightDirection = normalize(-LightDirection);
    float3 halfDirection = normalize(viewDirection + lightDirection);

    float3 viewDirectionTangentSpace = normalize(mul(viewDirection, inverseTbn));

    float2 parallaxUV = ParallaxOcclusionMapping(input.TextureCoordinates, viewDirectionTangentSpace);

    if (parallaxUV.x > 1.0 ||
        parallaxUV.y > 1.0 ||
        parallaxUV.x < 0.0 ||
        parallaxUV.y < 0.0)
        discard;

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

    float3 color = PBR(normal, lightDirection, viewDirection, halfDirection, material);

    output.target0 = float4(color, 1.0);

    //float brightness = dot(color.rgb, float3(0.5, 0.5, 0.5));

    //if (brightness > 1.0)

    if (material.Metallic > 0.7)
    {
        output.target1 = float4(color, 1.0);
    }
    else
    {
        output.target1 = float4(0.0, 0.0, 0.0, 1.0);
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