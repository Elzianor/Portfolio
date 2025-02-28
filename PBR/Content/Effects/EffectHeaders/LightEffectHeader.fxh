#ifndef LIGHT_EFFECT_HEADER_FXH
#define LIGHT_EFFECT_HEADER_FXH

float GetAttenuation(float distanceToLightSource, float c, float l, float q)
{
    return 1.0 / (c + l * distanceToLightSource + q * (distanceToLightSource * distanceToLightSource));
}

#endif // LIGHT_EFFECT_HEADER_FXH
