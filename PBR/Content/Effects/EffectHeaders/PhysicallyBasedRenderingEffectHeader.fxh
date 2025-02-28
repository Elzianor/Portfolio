#ifndef PHYSICALLY_BASED_RENDERING_EFFECT_HEADER_FXH
#define PHYSICALLY_BASED_RENDERING_EFFECT_HEADER_FXH

#include "CommonEffectHeader.fxh"
#include "CommonStructuresEffectHeader.fxh"

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
    // alternatively remove + 1.0 and divide by 2.0 instead of 8.0
    float r = roughness + 1.0;
    float alpha = r * r;
    float k = alpha / 8.0;
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

    return f0 + (1.0 - f0) * pow(clamp(1.0 - VdotH, 0.0, 1.0), 5.0);
}

float3 PBR(float3 n,
    float3 l,
    float3 v,
    float3 h,
    MaterialProperties material,
    float3 lightColor,
    float3 ambientColor,
    float attenuation,
    float spotIntensity)
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

    material.DiffuseColor *= attenuation * spotIntensity;
    specular *= attenuation * spotIntensity;
    ambientColor *= attenuation;

    float3 diffuseSpecular = (kd * material.DiffuseColor / PI + specular) * lightColor * NdotL;
    float3 ambient = ambientColor * material.DiffuseColor * material.Ao;
    float3 emissive = material.EmissiveColor * material.DiffuseColor; // ???

    float3 color = diffuseSpecular + ambient + emissive;

    return saturate(color);
}

#endif // PHYSICALLY_BASED_RENDERING_EFFECT_HEADER_FXH
