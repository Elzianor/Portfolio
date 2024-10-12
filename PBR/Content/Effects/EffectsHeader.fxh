// --- CONSTANTS ---

const float PI = 3.14159265359;

// ----- STRUCTURES -----

struct MaterialProperties
{
    float3 DiffuseColor;
    float Roughness;
    float Metallic;
    float Ao;
    float3 EmissiveColor;
    float BaseReflectivity;
};

// ----- OUTPUT COLOR CORRECTION -----

float3 ToneMapping(float3 color, float exposure)
{
    // exposure tone mapping
    return max(float3(1.0, 1.0, 1.0) - exp(-color * exposure), 0.0);
}

float3 GammaCorrection(float3 color, float gamma)
{
    // gamma correction
    float g = 1.0 / gamma;
    return pow(max(color, 0.0), float3(g, g, g));
}

float3 InverseGammaCorrection(float3 color, float gamma)
{
    // inverse gamma correction
    return pow(max(color, 0.0), float3(gamma, gamma, gamma));
}

// ----- SAFE OPERATION -----

float Dot(float3 x, float3 y)
{
    return max(dot(x, y), 0.0);
}

float NonZeroDenominator(float val)
{
    return max(val, 0.000001);
}

// ----- PBR -----

float D(float3 n, float3 h, float roughness)
{
    float NdotH = Dot(n, h);

    float alpha = roughness * roughness;
    float alpha2 = alpha * alpha;
    float denominator = NdotH * NdotH * (alpha2 - 1.0) + 1.0;

    return alpha2 / NonZeroDenominator(PI * denominator * denominator);
}

float G1(float3 n, float3 x, float roughness)
{
    float alpha = roughness * roughness;
    float k = alpha / 2.0;
    float NdotX = Dot(n, x);

    return NdotX / NonZeroDenominator(NdotX * (1.0 - k) + k);
}

float G(float3 n, float3 v, float3 l, float roughness)
{
    return G1(n, l, roughness) * G1(n, v, roughness);
}

float3 F(float3 f0, float3 v, float3 h)
{
    float VdotH = Dot(v, h);

    return f0 + (1.0 - f0) * pow(1.0 - VdotH, 5.0);
}

float3 PBR(float3 n, float3 l, float3 v, float3 h, MaterialProperties material, float3 lightColor, float3 ambientColor)
{
    float f0 = material.BaseReflectivity;
    float3 lerpF0 = lerp(float3(f0, f0, f0), material.DiffuseColor, material.Metallic);

    float d = D(n, h, material.Roughness);
    float g = G(n, v, l, material.Roughness);
    float3 f = F(lerpF0, v, h);

    float NdotV = Dot(n, v);
    float NdotL = Dot(n, l);

    float3 kd = 1.0 - f;
    kd *= (1.0 - material.Metallic);

    float3 numerator = d * g * f;
    float denominator = 4.0 * NdotV * NdotL;
    float3 specular = numerator / NonZeroDenominator(denominator);

    float3 color = material.EmissiveColor * material.DiffuseColor +
    lightColor * (kd * material.DiffuseColor / PI + specular) * NdotL +
    ambientColor * material.DiffuseColor * material.Ao;

    return saturate(color);
}

// ----- PARALLAX OCCLUSION MAPPING -----

float2 ParallaxOcclusionMapping(float2 texCoords,
float3 viewDirTangentSpace,
int parallaxMinSteps,
int parallaxMaxSteps,
float parallaxHeightScale,
sampler2D heightMapTextureSampler,
bool isDepthMap,
int refinmentStepsCount)
{
    viewDirTangentSpace = normalize(viewDirTangentSpace);

    int steps = lerp(parallaxMinSteps, parallaxMaxSteps, length(viewDirTangentSpace.xy));
    const float stepDelta = 1.0 / steps;

    float2 uvDelta = viewDirTangentSpace.xy * parallaxHeightScale / (viewDirTangentSpace.z * steps);

    float2 currentUV = texCoords;
    float currentStepDepth = 0.0;

    for (int i = 0; i < steps; i++)
    {
        currentUV = texCoords - i * uvDelta;
        float currentHeightMapValue = tex2D(heightMapTextureSampler, currentUV).x;
        if (!isDepthMap)
            currentHeightMapValue = 1.0 - currentHeightMapValue;
        currentStepDepth = i * stepDelta;

        if (currentStepDepth >= currentHeightMapValue ||
            i == 100) // HLSL do not like dynamic loops, so we set maximum possible number of cycles
            break;
    }

    // Binary search to refine the intersection point
    float2 prevUV = currentUV + uvDelta;

    // refinement steps
    for (int j = 0; j < refinmentStepsCount; j++)
    {
        float currentDepth = tex2D(heightMapTextureSampler, currentUV).r;
        float prevDepth = tex2D(heightMapTextureSampler, prevUV).r;
        float midDepth = (currentDepth + prevDepth) / 2.0;

        if (midDepth < currentStepDepth)
        {
            currentUV = (currentUV + prevUV) / 2.0;
        }
        else
        {
            prevUV = (currentUV + prevUV) / 2.0;
        }
    }

    return currentUV;
}