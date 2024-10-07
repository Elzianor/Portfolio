// --- UNIFORMS ---

cbuffer GaussianData
{
    float2 TexelSize;
    bool HorizontalPass;
    float GaussianWeights[5];
}

cbuffer Textures
{
    Texture2D ScreenTexture;
    sampler TextureSampler = sampler_state
    {
        Texture = <ScreenTexture>;
        Filter = Linear;
    };
}

// --- PIXEL SHADER ---

float4 PS(float4 pos : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float3 result = tex2D(TextureSampler, texCoord.xy).rgb * GaussianWeights[0]; // current fragment's contribution

    if (HorizontalPass)
    {
        for (int i = 1; i < 5; ++i)
        {
            result += tex2D(TextureSampler, texCoord.xy + float2(TexelSize.x * i, 0.0)).rgb * GaussianWeights[i];
            result += tex2D(TextureSampler, texCoord.xy - float2(TexelSize.x * i, 0.0)).rgb * GaussianWeights[i];
        }
    }
    else
    {
        for (int i = 1; i < 5; ++i)
        {
            result += tex2D(TextureSampler, texCoord.xy + float2(0.0, TexelSize.y * i)).rgb * GaussianWeights[i];
            result += tex2D(TextureSampler, texCoord.xy - float2(0.0, TexelSize.y * i)).rgb * GaussianWeights[i];
        }
    }

    return float4(result, 1.0);
}

// --- TECHNIQUES ---

technique BlurTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_5_0 PS();
    }
}