#ifndef COMMON_EFFECT_HEADER_FXH
#define COMMON_EFFECT_HEADER_FXH

// --- CONSTANTS ---

static const float PI = 3.14159265359;

// --- SAFE OPERATION ---

float Dot(float3 x, float3 y)
{
    return max(dot(x, y), 0.0);
}

float NonZeroDenominator(float val)
{
    return max(val, 0.000001);
}

// --- EQUALITY CHECKS ---
bool IsEqual(float a, float b)
{
    return (abs(a - b) < 1e-6);
}

bool IsZero(float2 v)
{
    return all(v == 0);
}

bool IsZero(float3 v)
{
    return all(v == 0);
}

#endif // COMMON_EFFECT_HEADER_FXH
