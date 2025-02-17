// --- CONSTANTS ---

const float PI = 3.14159265359;

// ----- SAFE OPERATION -----

float Dot(float3 x, float3 y)
{
    return max(dot(x, y), 0.0);
}

float NonZeroDenominator(float val)
{
    return max(val, 0.000001);
}
