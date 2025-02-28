#ifndef COLOR_CORRECTION_EFFECT_HEADER_FXH
#define COLOR_CORRECTION_EFFECT_HEADER_FXH

float3 SimpleToneMapping(float3 color)
{
    return color / (color + 1.0);
}

float3 ExposureToneMapping(float3 color, float exposure)
{
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
    // inverse gamma correction (do it before processing color from texture)
    return pow(max(color, 0.0), float3(gamma, gamma, gamma));
}

#endif // COLOR_CORRECTION_EFFECT_HEADER_FXH
