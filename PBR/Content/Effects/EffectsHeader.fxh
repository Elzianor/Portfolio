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