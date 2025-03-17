float4x4 WorldViewProjection;
float ResW;
float ResH;

texture SceneTexture;
sampler2D SceneTextureSampler = sampler_state
{
    Texture = (SceneTexture);
};

struct VertexShaderInput
{
    float4 Position : POSITION;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 ClipPos : TEXCOORD0; // Pass clip-space position
};

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.ClipPos = mul(input.Position, WorldViewProjection);
    output.Position = output.ClipPos;

    return output;
}

float4 PS(VertexShaderOutput input) : SV_TARGET
{
    //float2 uv = float2(input.Position.x / ResW, input.Position.y / ResH);

    float2 ndc = input.ClipPos.xy / input.ClipPos.w * 0.5 + 0.5;
    ndc.y = 1.0 - ndc.y;

    return tex2D(SceneTextureSampler, ndc);
    //if (ndc.x < 100.0)
    //    return float4(0, 1, 0, 1);

    //return float4(1, 0, 0, 1);
}

technique ForceFieldTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}