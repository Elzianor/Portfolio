#ifndef LIGHT_FXH
#define LIGHT_FXH

float GetAttenuation(float distanceToLightSource, float c, float l, float q)
{
    return 1.0 / (c + l * distanceToLightSource + q * (distanceToLightSource * distanceToLightSource));
}

float3 GetDoubleSidedMaterialNormal(float3 normal, float3 viewDirection)
{
    return dot(normal, viewDirection) < 0.0 ? -normal : normal;
}

#endif // LIGHT_FXH
