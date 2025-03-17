sampler2D PlaneSampler : register(s0);
sampler2D SphereSampler : register(s1);

float ZNear = 0.1;
float ZFar = 1000.0;

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

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float LinearDepth(float depth)
{
    return (2.0f * ZNear * ZFar) / (ZFar + ZNear - depth * (ZFar - ZNear));
}

float4 PS(VertexShaderOutput input) : SV_TARGET
{
    float planeDepth = tex2D(PlaneSampler, input.TextureCoordinates).r;
    float sphereDepth = tex2D(SphereSampler, input.TextureCoordinates).r;

    //float pdl = LinearDepth(planeDepth);
    //float sdl = LinearDepth(sphereDepth);

    float pdl = planeDepth;
    float sdl = sphereDepth;

    float3 color;

    if (planeDepth == 0.0 && sphereDepth == 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);

    if (planeDepth == 0.0)
        return float4(1.0, 0.0, 0.0, 1.0);

    if (sphereDepth == 0.0)
        return float4(0.0, 1.0, 0.0, 1.0);

    if (pdl < sdl)
    {
        color = float4(1.0, 0.0, 0.0, 1.0);

        if (sdl > 0.203)
        {
            float depthThreshold = 0.0003 * max(pdl, sdl); // Scale threshold based on depth

            if (abs(sdl - pdl) < depthThreshold)
            {
                color = lerp(color, float4(0.0, 0.0, 1.0, 1.0), 1.0 - depthThreshold);
            }
        }
    }
    else
    {
        color = float4(0.0, 1.0, 0.0, 1.0);
    }

    // Intersection highlight

    return float4(color, 1.0);
}

technique DepthMap
{
    pass P0
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}