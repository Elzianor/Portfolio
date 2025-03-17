float4x4 WorldViewProjection;

float ZNear = 0.1;
float ZFar = 1000.0;

struct VertexShaderInput
{
    float4 Position : POSITION;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
};

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjection);

    return output;
}

float LinearDepth(float depth)
{
    return (2.0f * ZNear * ZFar) / (ZFar + ZNear - depth * (ZFar - ZNear));
}

float PS(VertexShaderOutput input) : SV_TARGET
{
    return LinearDepth(input.Position.z / input.Position.w);
}

technique DepthMap
{
    pass P0
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}