#include "EffectsHeader.fxh"

// --- UNIFORMS ---

cbuffer ExposureGammaData
{
    float Exposure;
    float Gamma;
}

cbuffer Textures
{
    sampler2D MainSceneSampler : register(s0);
    sampler2D BlurSampler : register(s1);
}

// --- STRUCTURES ---

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
};

// --- VERTEX AND PIXEL SHADERS ---

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PS(VertexShaderOutput input) : SV_TARGET0
{
    float3 mainColor = tex2D(MainSceneSampler, input.TextureCoordinates).rgb;
    float3 blurColor = tex2D(BlurSampler, input.TextureCoordinates).rgb;

    // additive blending
    mainColor += blurColor;

    float avg = (mainColor.r + mainColor.g + mainColor.b) / 3.0;

    float exposure = 1.0;
    float gamma = lerp(1.0, 2.2, avg);

    // tone mapping
    float3 result = ToneMapping(mainColor, exposure);

    // gamma correction
    result = GammaCorrection(result, gamma);

    return float4(result, 1.0);
}

// --- TECHNIQUES ---

technique MergeBlurTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}