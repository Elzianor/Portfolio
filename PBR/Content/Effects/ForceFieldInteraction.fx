#include "EffectHeaders/CommonVertexShaders.fxh"
#include "EffectHeaders/CommonPixelShaders.fxh"

float3 ForceFieldPosition;
float ForceFieldRadius;
float3 ForceFieldHighlightColor;
float ForceFieldHeight;

float Time;

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
    6.0,
    1.0,
    true);
}

float4 BlendIntersectionColor(float3 worldPosition ,float4 color)
{
    if (ForceFieldHeight < -ForceFieldRadius)
        return color;

    float sdfSphere = SdfSphere(worldPosition - ForceFieldPosition, ForceFieldRadius);
    float noise = GetNoiseValue(worldPosition);

    sdfSphere += (noise * 0.2) - 0.09;

    if (sdfSphere > -0.02 && sdfSphere < 0.02 &&
        (ForceFieldPosition.y + ForceFieldHeight) >= worldPosition.y - 0.1)
    {
        float factor = smoothstep(-0.02, 0.02, sdfSphere);
        color = float4(lerp(color.rgb, ForceFieldHighlightColor, factor), color.a);
    }

    return color;
}

float4 PS_FF_Interaction_Solid(VSOutputPBRSolid input) : SV_TARGET
{
    float4 color = PS_PBR_Solid(input);

    return BlendIntersectionColor(input.WorldPosition, color);
}

float4 PS_FF_Interaction_Textured(VSOutputPBRTextured input) : SV_TARGET
{
    float4 color = PS_PBR_Textured(input);

    return BlendIntersectionColor(input.WorldPosition, color);
}

technique SolidFFInteraction
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VS_PBR_Solid();
        PixelShader = compile ps_5_0 PS_FF_Interaction_Solid();
    }
}

technique TexturedFFInteraction
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VS_PBR_Textured();
        PixelShader = compile ps_5_0 PS_FF_Interaction_Textured();
    }
}
