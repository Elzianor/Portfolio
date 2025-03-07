#include "EffectHeaders/CommonUniforms.fxh"

// --- STRUCTURES ---

struct VSInput
{
    float4 Position : POSITION0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
};

// --- VERTEX AND PIXEL SHADERS ---

VSOutput VS(VSInput input)
{
    VSOutput output;

    output.Position = mul(input.Position, WorldViewProjection);

    return output;
}

float4 PS(VSOutput input) : SV_TARGET
{
    return float4(LightColor, 1.0);
}

// --- TECHNIQUES ---

technique Tech0
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}