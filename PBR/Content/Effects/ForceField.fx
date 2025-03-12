#include "EffectHeaders/Common.fxh"

// --- UNIFORMS ---
//samplerCUBE CubeSampler;

float4x4 World;
float4x4 WorldViewProjection;
float4x4 WorldView;
float4x4 WorldViewInverseTranspose;

float Time;

float3 MainColor;
float3 HighlightColor;
float3 LowCapacityColor;
float HighlightThickness;
float GlowIntensity;

float DissolveThreshold;
float Height;

bool IsLowCapacity;

// --- FUNCTIONS ---

float GetNoiseValue(float3 position)
{
    float noiseStep = 64.0;

    float shiftX = Time * 0.1;
    float shiftY = Time * 0.1;
    float shiftZ = Time * 0.1;

    return GetMultioctave3DNoiseValue((position.x + shiftX) * noiseStep,
    (position.y + shiftY) * noiseStep,
    (position.z + shiftZ) * noiseStep,
    1.0,
    5.0,
    2.0,
    true);
}

bool IsBelowGroundLevel(float positionY)
{
    return positionY < 0.0;
}

bool IsOff()
{
    return DissolveThreshold == 0.0 || Height == 0.0;
}

float3 HandleInteriorVisibility(float3 normal, float3 viewDirection)
{
    if (dot(normal, viewDirection) < 0.0)
        return -normal;
    else
        return normal;
}

float CalculateFieldStrength(float3 normal, float3 viewDirection, float noise)
{
    float fresnel = pow(1.0 - dot(normal, viewDirection), 5.0);
    float fieldStrength = fresnel * (0.5 + noise * 0.75);

    return fieldStrength;
}

float3 CalculateBaseColor(float fieldStrength)
{
    float3 color = MainColor;
    color *= fieldStrength * GlowIntensity;

    return color;
}

float4 CalculateFinalColor(float3 position,
                           float heightThreshold,
                           float highlightStep,
                           float fieldStrength,
                           float3 dissolveHighlightColor,
                           float noise,
                           float originalNoise)
{
    float3 color;
    float alpha;

    // draw highlights
    if (position.y <= HighlightThickness * 2.0 ||
        position.y > heightThreshold - HighlightThickness ||
        highlightStep > 0.0)
    {
        color = HighlightColor;

        if (IsLowCapacity)
        {
            color = lerp(color, LowCapacityColor, abs(sin(Time * 0.75)));
        }

        alpha = 0.5;
    }
    // draw main color
    else
    {
        color = CalculateBaseColor(fieldStrength);
        color += dissolveHighlightColor;
        alpha = fieldStrength + 0.15;

        if (noise > 0.0)
        {
            color += MainColor * 0.2;
            alpha += 0.1;
        }

        if (IsLowCapacity)
        {
            color = lerp(color, LowCapacityColor, abs(sin(Time * 0.75)) * 0.2);
            alpha += abs(sin(Time * 0.75)) * 0.2;
        }
    }

    return saturate(float4(color, alpha));
}

// --- STRUCTURES ---

struct VertexShaderInput
{
    float4 Position : POSITION;
    float3 Normal : NORMAL;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float3 WorldViewPosition : TEXCOORD1;
    float3 TextureCoordinates : TEXCOORD2;
    float3 WorldPosition : TEXCOORD3;
};

// --- SHADERS ---

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Normal = normalize(mul(input.Normal, (float3x3)WorldViewInverseTranspose));
    output.WorldViewPosition = mul(input.Position, WorldView).xyz;
    output.TextureCoordinates = normalize(input.Normal);
    output.WorldPosition = mul(input.Position, World).xyz;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    //float3 color = texCUBE(CubeSampler, input.TextureCoordinates).rgb; // Sample from the cube map using the normal as the direction

    if (IsBelowGroundLevel(input.WorldPosition.y))
        discard;

    if (IsOff())
        discard;

    float noise = GetNoiseValue(input.WorldPosition);
    float originalNoise = noise;
    float heightThreshold = Height + noise * 0.15;

    // do not draw dissolved pixels and ones above height threshold
    if (!step(noise, DissolveThreshold) ||
        input.WorldPosition.y > heightThreshold)
        discard;

    float3 viewDirection = normalize(-input.WorldViewPosition);
    float3 normal = normalize(input.Normal);

    // in order to properly see the interior of force field
    normal = HandleInteriorVisibility(normal, viewDirection);

    float halfHighlightThickness = HighlightThickness / 2.0;
    float highlightStep = smoothstep(DissolveThreshold - halfHighlightThickness,
                                     DissolveThreshold + halfHighlightThickness, noise);
    float3 dissolveHighlightColor = HighlightColor * highlightStep;

    // for visual stains on shield surface
    noise = step(frac(noise * 12.0), 0.25);

    // Fresnel effect for edge glow
    float fieldStrength = CalculateFieldStrength(normal, viewDirection, noise);

    return CalculateFinalColor(input.WorldPosition.y,
                               heightThreshold,
                               highlightStep,
                               fieldStrength,
                               dissolveHighlightColor,
                               noise,
                               originalNoise);
}

// --- TECHNIQUES ---

technique ForceFieldTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}