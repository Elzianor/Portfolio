// --- CONSTANTS ---

const float PI = 3.14159265359;

// --- VARIABLES ---

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
    float3 EmissiveColor;
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
}

// --- SHADER STRUCTURES ---

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
    float3x3 TBN : TEXCOORD2;
};

struct MaterialProperties
{
    float3 DiffuseColor;
    float Roughness;
    float Metallic;
    float Ao;
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

float3 PBR(float3 n, float3 l, float3 v, float3 h, float f0, MaterialProperties material)
{
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

    float3 color = EmissiveColor +
    LightColor * (kd * material.DiffuseColor / PI + specular) * NdotL +
    AmbientColor * material.DiffuseColor * material.Ao;

    return saturate(color);
}

float4 GammaCorrection(float3 color)
{
    float gc = 1.0 / 2.2;
    return float4(pow(color / (color + 1.0), float3(gc, gc, gc)), 1.0);
}

// --- VERTEX AND PIXEL SHADERS ---

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float3 normal = mul(input.Normal, WorldViewInverseTranspose);
    float3 tangent = mul(input.Tangent, WorldViewInverseTranspose);

    tangent = normalize(tangent - dot(tangent, normal) * normal);

    float3 bitangent = cross(normal, tangent);

    output.Position = mul(input.Position, WorldViewProjection);
    output.TextureCoordinates = input.TextureCoordinates;
    output.WorldViewPosition = mul(input.Position, WorldView).xyz;
    output.TBN = float3x3(tangent, bitangent, normal);

    return output;
}

float4 PS(VertexShaderOutput input) : SV_TARGET0
{
    float3x3 tbn = input.TBN;

    float3 normal = tex2D(NormalMapTextureSampler, input.TextureCoordinates);
    normal = normal * 2.0 - 1.0;
    normal.y = -normal.y;
    normal = normalize(mul(normal, tbn));

    float3 viewDirection = -normalize(input.WorldViewPosition);
    float3 lightDirection = -normalize(LightDirection);
    float3 halfDirection = normalize(viewDirection + lightDirection);

    MaterialProperties material;

    material.DiffuseColor = tex2D(DiffuseMapTextureSampler, input.TextureCoordinates).xyz;
    material.Roughness = tex2D(RoughnessMapTextureSampler, input.TextureCoordinates).x;
    material.Metallic = tex2D(MetallicMapTextureSampler, input.TextureCoordinates).x;
    material.Ao = tex2D(AoMapTextureSampler, input.TextureCoordinates).x;

    float3 color = PBR(normal, lightDirection, viewDirection, halfDirection, 0.04, material);

    return saturate(GammaCorrection(color));
}

// --- TECHNIQUES ---

technique BlinnPhongLighting
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}