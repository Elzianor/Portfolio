#ifndef PARALLAX_OCCLUSION_MAPPING_EFFECT_HEADER_FXH
#define PARALLAX_OCCLUSION_MAPPING_EFFECT_HEADER_FXH

float2 ParallaxOcclusionMapping(float2 texCoords,
float3 viewDirTangentSpace,
int parallaxMinSteps,
int parallaxMaxSteps,
float parallaxHeightScale,
sampler2D heightMapTextureSampler,
bool isDepthMap,
int refinmentStepsCount)
{
    if (parallaxMaxSteps == 0.0 || parallaxHeightScale == 0.0)
        return texCoords;

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

#endif // PARALLAX_OCCLUSION_MAPPING_EFFECT_HEADER_FXH
