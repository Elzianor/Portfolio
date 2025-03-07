#include "EffectHeaders/ColorCorrection.fxh"

// Transform
float4x4 WorldViewProjection;
float4x4 WorldView;
float4x4 WorldViewInverseTranspose;

texture GridTexture;
sampler2D GridTextureSampler = sampler_state
{
    Texture = (GridTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture NoiseTexture;
sampler2D NoiseTextureSampler = sampler_state
{
    Texture = (NoiseTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

float Time;
float ShockwaveTime;
float3 FieldColor;
float3 GridLinesColor;
float GlowIntensity;
float2 WaveCenter;
float3 WaveParams;

struct VS_INPUT
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float3 WorldViewPos : TEXCOORD2;
    float2 TextureCoordinates : TEXCOORD3;
};

VS_OUTPUT VS_Main(VS_INPUT input)
{
    VS_OUTPUT output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldViewPos = mul(input.Position, WorldView).xyz;
    output.Normal = normalize(mul(input.Normal, (float3x3) WorldViewInverseTranspose));
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PS_Main(VS_OUTPUT input) : SV_Target
{
    float currentTime = ShockwaveTime;
    float2 inputUV = input.TextureCoordinates;
    float dist = distance(inputUV, WaveCenter);
    float scaleDiff;

    bool distort = ((dist <= ((currentTime) + (WaveParams.z))) && (dist >= ((currentTime) - (WaveParams.z * 0.5))));

    //Only distort the pixels within the parameter distance from the centre
    if (distort)
    {
        //The pixel offset distance based on the input parameters
        float diff = (dist - currentTime);
        scaleDiff = (1.0 - pow(abs(diff * WaveParams.x), WaveParams.y));
        float diffTime = (diff * scaleDiff);
        
        //The direction of the distortion
        float2 diffTexCoord = normalize(inputUV - WaveCenter);
        
        //Perform the distortion and reduce the effect over time
        inputUV += ((diffTexCoord * diffTime) / (currentTime * dist * 40.0));
    }

    float3 viewDirection = normalize(-input.WorldViewPos);
    float3 normal = input.Normal;
    float fresnelPow = 1.5;

    if (dot(normal, viewDirection) < 0)
    {
        normal = -normal;
        fresnelPow = 1.0;
    }

    // Fresnel effect for edge glow
    float fresnel = pow(1.0 - dot(normal, viewDirection), fresnelPow);

    // Scrolling noise for energy effect
    float2 gridUV = inputUV + float2(Time * 0.03, Time * 0.03);
    float3 gridColor = tex2D(GridTextureSampler, gridUV).rgb;

    float2 noiseUV = inputUV - float2(Time * 0.05, Time * 0.05);
    float3 noiseColor = tex2D(NoiseTextureSampler, noiseUV).rgb;

    float noise = noiseColor.r;

    noise *= (sin(Time * 0.75) + cos(Time * 0.93)) * 1.5;

    // Combine effects
    float fieldStrength = fresnel * (0.5 + noise * 0.2);
    float3 color = FieldColor * fieldStrength * GlowIntensity;

    if (gridColor.r < 0.5)
        color += GridLinesColor * (0.5 + abs(sin(Time * 0.59) + cos(Time * 0.43)) / 2.0);

    fieldStrength *= (1.0 + abs(sin(Time * 0.75) + cos(Time * 0.93))) * 2.0;

    if (distort)
    {
        //Blow out the color and reduce the effect over time
        color += (color * scaleDiff) / (currentTime * dist * 30.0);
    }

    color = saturate(color);
    fieldStrength = saturate(fieldStrength);

    // Output final color with transparency
    return saturate(float4(color, fieldStrength));
}

technique ForceFieldTech
{
    pass P0
    {
        VertexShader = compile vs_5_0 VS_Main();
        PixelShader = compile ps_5_0 PS_Main();
    }
}
