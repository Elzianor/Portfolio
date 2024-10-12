#include "EffectsHeader.fxh"

// --- UNIFORMS ---

cbuffer ExposureGammaData
{
    float Gamma;
    float Exposure;

    bool ApplyGammaCorrection;
    bool ApplyToneMapping;
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
    float4 color = tex2D(MainSceneSampler, input.TextureCoordinates);
    float3 mainColor = color.rgb;
    float3 blurColor = tex2D(BlurSampler, input.TextureCoordinates).rgb;

    // additive blending
    mainColor += blurColor;

    if (ApplyToneMapping)
    {
        mainColor = ToneMapping(mainColor, Exposure);
    }

    if (ApplyGammaCorrection)
    {
        mainColor = GammaCorrection(mainColor, Gamma);
    }

    return float4(mainColor, 1.0);

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