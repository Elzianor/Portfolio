// --- CONSTANTS ---

const float PI = 3.14159265359;

// --- VARIABLES ---

    float4x4 World;
    float4x4 View;
    float4x4 Projection;

    float3 ViewPos;

    float3 LightDirection;
    float4 LightColor;

    float3 DiffuseColor;
    float3 AmbientColor;
    float3 EmissiveColor;

    float Roughness;
    float Metallic;

// --- SHADER STRUCTURES ---

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinates : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};

// --- FUNCTIONS ---

float Dot(float3 x, float3 y)
{
    return max(dot(x, y), 0.0);
}

float NonZeroDenominator(float val)
{
    return max(val, 0.000001);
}

float D(float3 n, float3 h)
{
    float NdotH = Dot(n, h);

    float alpha = Roughness * Roughness;
    float alpha2 = alpha * alpha;
    float denominator = NdotH * NdotH * (alpha2 - 1.0) + 1.0;

    return alpha2 / (PI * denominator * denominator);
}

float G1(float3 n, float3 x)
{
    float k = (Roughness * Roughness) / 2.0;
    float NdotX = Dot(n, x);

    return NdotX / (NdotX * (1.0 - k) + k);
}

float G(float3 n, float3 v, float3 l)
{
    return G1(n, l) * G1(n, v);
}

float3 F(float3 f0, float3 v, float3 h)
{
	float VdotH = Dot(v, h);
	return f0 + (1.0 - f0) * pow(1.0 - VdotH, 5.0);
}

float3 Diffuse()
{
    return DiffuseColor;
}



// --- VERTEX AND PIXEL SHADERS ---

VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;

    //output.Position = mul(input.Position, WorldViewProjection);
    //output.Normal = mul(input.Normal, WorldViewInverseTranspose);
    //output.Normal = mul(input.Normal, WorldViewInverseTranspose);
    //output.TextureCoordinates = input.TextureCoordinates;
    //output.WorldViewPosition = mul(input.Position, WorldView).xyz;

    output.Position = mul(mul(mul(input.Position, World), View), Projection);
    output.WorldPosition = mul(input.Position, World).xyz;
    output.Normal = input.Normal;

    //output.Normal = float3x3(transpose(inverse(World))) * input.Normal;

    return output;
}

float4 PS(VertexShaderOutput input) : SV_TARGET0
{
    //float3 viewDirection = -normalize(input.WorldViewPosition);
    //float3 normal = normalize(input.Normal);
    //float3 lightDirection = -normalize(LightDirection);
    //float3 halfDirection = normalize(viewDirection + lightDirection);

    float3 N = normalize(input.Normal);
    float3 L = normalize(-LightDirection);
    float3 V = normalize(ViewPos - input.WorldPosition);
    float3 H = normalize(V + L);

    float3 F0 = float3(0.04, 0.04, 0.04);
    F0 = lerp(F0, DiffuseColor, Metallic);

    float d = D(N, H);
    float g = G(N, V, L);
    float3 f = F(F0, V, H);

    float3 kd = 1.0 - f;
    kd *= (1.0 - Metallic);

    float3 numerator = d * g * f;
    float denominator = 4.0 * Dot(N, V) * Dot(N, L);
    float3 specular = numerator / NonZeroDenominator(denominator);

    float NdotL = Dot(N, L);

    float3 color = LightColor * (kd * DiffuseColor / PI + specular) * NdotL;

    return float4(color, 1.0);
}

// --- TECHNIQUES ---

technique BlinnPhongLighting
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}