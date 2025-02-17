float GetAttenuation(float distanceToLightSource, float c, float l, float q)
{
    return 1.0 / (c + l * distanceToLightSource + q * (distanceToLightSource * distanceToLightSource));
}