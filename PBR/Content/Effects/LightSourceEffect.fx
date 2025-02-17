// --- UNIFORMS ---

// Transform
float4x4 WorldViewProjection;

float3 LightColor;

// --- STRUCTURES ---

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
};

// --- VERTEX AND PIXEL SHADERS ---

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjection);

    return output;
}

float4 PS(VertexShaderOutput input) : SV_TARGET
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