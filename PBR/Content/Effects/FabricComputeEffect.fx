#include "EffectHeaders/CommonVertexShadersEffectHeader.fxh"
#include "EffectHeaders/CommonPixelShadersEffectHeader.fxh"

#define GroupSize 256

RWStructuredBuffer<FabricParticle> FabricParticlesInput;
RWStructuredBuffer<FabricParticle> FabricParticlesOutput;

StructuredBuffer<FabricParticle> FabricParticlesReadOnly;

texture FrontTexture;
sampler2D FrontTextureSampler = sampler_state
{
    Texture = (FrontTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture BackTexture;
sampler2D BackTextureSampler = sampler_state
{
    Texture = (BackTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

//=============================================================================
// Compute Shader
//=============================================================================
uint FabricParticlesCount;

float FabricParticleMass;
float3 GravitationalAcceleration;
float PhysicsUpdateTimeStep;

float AirDensity;
float FabricParticleDragCoefficient;
float FabricParticleProjectedArea;
float3 Wind;

uint FabricWidthInParticles;
uint FabricHeightInParticles;
float FabricStructuralRestLength;
float FabricShearingRestLength;

float FabricDamping;
float FabricStiffness;

bool UseShearingConstraints;

float PinnedYShift;

float3 ProcessConstraint(FabricParticle currentParticle, FabricParticle neighborParticle, float restLength)
{
    float3 direction = neighborParticle.Position - currentParticle.Position;
    float distance = length(direction);

    float3 correction = float3(0.0, 0.0, 0.0);

    if (distance > 0.05 && distance <= restLength)
        return correction;

    direction = normalize(direction);

    float distanceDelta = distance - restLength;

    if (neighborParticle.IsPinned)
        correction = direction * distanceDelta;
    else
        correction = direction * (distanceDelta * 0.5);

    return correction * FabricStiffness;
}

[numthreads(GroupSize, 1, 1)]
void CSVerlet(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint index = globalID.x;

    if (index >= FabricParticlesCount)
        return;

    FabricParticle p = FabricParticlesInput[index];

    if (p.IsPinned)
    {
        p.Position.y += PinnedYShift;
        FabricParticlesInput[index] = p;
        return;
    }

    p.TotalForce = FabricParticleMass * GravitationalAcceleration;

    float3 nextPosition = p.Position + (1.0 - FabricDamping) * (p.Position - p.PrevPosition) + p.Acceleration * PhysicsUpdateTimeStep * PhysicsUpdateTimeStep;
    float3 nextVelocity = (nextPosition - p.Position) / (2.0 * PhysicsUpdateTimeStep);

    p.PrevPosition = p.Position;
    p.Position = nextPosition;
    p.Velocity = nextVelocity;

    FabricParticlesInput[index] = p;
}

[numthreads(GroupSize, 1, 1)]
void CSConstraints(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint index = globalID.x;

    if (index >= FabricParticlesCount)
        return;

    FabricParticle currentParticle = FabricParticlesInput[index];

    if (currentParticle.IsPinned)
    {
        FabricParticlesOutput[index] = currentParticle;
        return;
    }

    float3 correction = float3(0.0, 0.0, 0.0);

    // structural constraints
    uint upIndex = index + FabricWidthInParticles;
    uint rightIndex = index + 1.0;
    int downIndex = index - FabricWidthInParticles;
    int leftIndex = index - 1.0;

    bool hasUpNeighbor = upIndex < FabricParticlesCount;
    bool hasRightNeighbor = (uint)(index % FabricWidthInParticles) < (uint)(FabricWidthInParticles - 1.0);
    bool hasDownNeighbor = downIndex >= 0;
    bool hasLeftNeighbor = (index % FabricWidthInParticles) > 0;

    if (hasUpNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[upIndex], FabricStructuralRestLength);

    if (hasRightNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[rightIndex], FabricStructuralRestLength);

    if (hasDownNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[downIndex], FabricStructuralRestLength);

    if (hasLeftNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[leftIndex], FabricStructuralRestLength);

    // calculate normal
    float3 normal = float3(0.0, 0.0, 0.0);

    float3 toDownNeighbor = float3(0.0, 0.0, 0.0);
    float3 toLeftNeighbor = float3(0.0, 0.0, 0.0);
    float3 toUpNeighbor = float3(0.0, 0.0, 0.0);
    float3 toRightNeighbor = float3(0.0, 0.0, 0.0);

    if (hasDownNeighbor)
        toDownNeighbor = FabricParticlesInput[downIndex].Position - currentParticle.Position;
    if (hasLeftNeighbor)
        toLeftNeighbor = FabricParticlesInput[leftIndex].Position - currentParticle.Position;
    if (hasUpNeighbor)
        toUpNeighbor = FabricParticlesInput[upIndex].Position - currentParticle.Position;
    if (hasRightNeighbor)
        toRightNeighbor = FabricParticlesInput[rightIndex].Position - currentParticle.Position;

    if (!IsZero(toDownNeighbor) && !IsZero(toLeftNeighbor))
        normal += cross(toDownNeighbor, toLeftNeighbor);

    else if (!IsZero(toLeftNeighbor) && !IsZero(toUpNeighbor))
        normal += cross(toLeftNeighbor, toUpNeighbor);

    else if (!IsZero(toUpNeighbor) && !IsZero(toRightNeighbor))
        normal += cross(toUpNeighbor, toRightNeighbor);

    else if (!IsZero(toRightNeighbor) && !IsZero(toDownNeighbor))
        normal += cross(toRightNeighbor, toDownNeighbor);

    if (!UseShearingConstraints)
    {
        currentParticle.Position += correction;
        currentParticle.Normal = -normalize(normal);
        FabricParticlesOutput[index] = currentParticle;
        return;
    }

    // shearing constraints
    uint upRightIndex = upIndex + 1.0;
    uint upLeftIndex = upIndex - 1.0;
    int downRightIndex = downIndex + 1.0;
    int downLeftIndex = downIndex - 1.0;

    bool hasUpRightNeighbor = hasUpNeighbor && hasRightNeighbor;
    bool hasUpLeftNeighbor = hasUpNeighbor && hasLeftNeighbor;
    bool hasDownRightNeighbor = hasDownNeighbor && hasRightNeighbor;
    bool hasDownLeftNeighbor = hasDownNeighbor && hasLeftNeighbor;

    if (hasUpRightNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[upRightIndex], FabricShearingRestLength);

    if (hasUpLeftNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[upLeftIndex], FabricShearingRestLength);

    if (hasDownRightNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[downRightIndex], FabricShearingRestLength);

    if (hasDownLeftNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[downLeftIndex], FabricShearingRestLength);

    // correct normal
    float3 toDownLeftNeighbor = float3(0.0, 0.0, 0.0);
    float3 toUpLeftNeighbor = float3(0.0, 0.0, 0.0);
    float3 toUpRightNeighbor = float3(0.0, 0.0, 0.0);
    float3 toDownRightNeighbor = float3(0.0, 0.0, 0.0);

    if (hasDownLeftNeighbor)
        toDownLeftNeighbor = FabricParticlesInput[downLeftIndex].Position - currentParticle.Position;
    if (hasUpLeftNeighbor)
        toUpLeftNeighbor = FabricParticlesInput[upLeftIndex].Position - currentParticle.Position;
    if (hasUpRightNeighbor)
        toUpRightNeighbor = FabricParticlesInput[upRightIndex].Position - currentParticle.Position;
    if (hasDownRightNeighbor)
        toDownRightNeighbor = FabricParticlesInput[downRightIndex].Position - currentParticle.Position;

    if (!IsZero(toDownLeftNeighbor) && !IsZero(toUpLeftNeighbor))
        normal += cross(toDownLeftNeighbor, toUpLeftNeighbor);
    else if (!IsZero(toUpLeftNeighbor) && !IsZero(toUpRightNeighbor))
        normal += cross(toUpLeftNeighbor, toUpRightNeighbor);
    else if (!IsZero(toUpRightNeighbor) && !IsZero(toDownRightNeighbor))
        normal += cross(toUpRightNeighbor, toDownRightNeighbor);
    else if (!IsZero(toDownRightNeighbor) && !IsZero(toDownLeftNeighbor))
        normal += cross(toDownRightNeighbor, toDownLeftNeighbor);

    currentParticle.Position += correction;
    currentParticle.Normal = -normalize(normal);

    FabricParticlesOutput[index] = currentParticle;
}

[numthreads(GroupSize, 1, 1)]
void CSAirCalculations(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint index = globalID.x;

    if (index >= FabricParticlesCount)
        return;

    FabricParticle p = FabricParticlesOutput[index];

    if (p.IsPinned || IsZero(p.Velocity))
        return;

    // Calculate particle velocity magnitude
    float velocityMagnitude = length(p.Velocity);

    // Calculate drag force
    float dragForceMagnitude =
            0.5 * FabricParticleDragCoefficient * AirDensity *
            FabricParticleProjectedArea * velocityMagnitude * velocityMagnitude;

    // Heavier the fabric, less the drag force
    dragForceMagnitude /= FabricParticleMass;

    float3 velocityDirection = normalize(p.Velocity);

    float normalCoefficient = abs(dot(p.Normal, velocityDirection));

    // Drag force depends on the fabric particle orientation
    dragForceMagnitude *= normalCoefficient;

    // Calculate drag force vector (opposite direction of velocity)
    float3 dragForce = -velocityDirection * dragForceMagnitude;

    p.TotalForce += dragForce;

    // add wind
    float3 windForce = Wind * normalCoefficient;
    p.TotalForce += windForce;

    p.Acceleration = p.TotalForce / FabricParticleMass;

    FabricParticlesOutput[index] = p;
}

[numthreads(GroupSize, 1, 1)]
void CSSwapBuffers(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint index = globalID.x;

    if (index >= FabricParticlesCount)
        return;

    FabricParticlesInput[index] = FabricParticlesOutput[index];
}

//===============================================================================
// Vertex and pixel shaders
//===============================================================================

VertexShaderOutputCompute VSSolidCompute(in VertexShaderInputCompute input)
{
    VertexShaderOutputCompute output;

    FabricParticle p = FabricParticlesReadOnly[input.VertexID];

    float4 position = float4(p.Position, 1.0);
    float3 normal = p.Normal;
    float2 textureCoord = p.TextureCoord;

    output.Position = mul(position, WorldViewProjection);
    output.Normal = normalize(mul(normal, (float3x3) WorldViewInverseTranspose));
    output.WorldViewPosition = mul(position, WorldView).xyz;
    output.TextureCoordinates = textureCoord;

    return output;
}

float4 PSSolidCompute(VertexShaderOutputCompute input) : SV_TARGET
{
    float3 viewDirection = normalize(-input.WorldViewPosition);
    float3 lightDirection;

    float spotIntensity = 1.0;

    if (LightType == 0) // directional
    {
        lightDirection = normalize(-WorldViewLightDirection);
    }
    else if (LightType == 1) // point
    {
        lightDirection = normalize(WorldViewLightPosition - input.WorldViewPosition);
    }
    else if (LightType == 2) // spot
    {
        lightDirection = normalize(WorldViewLightPosition - input.WorldViewPosition);

        float theta = dot(lightDirection, normalize(-WorldViewLightDirection));
        float epsilon = CutOffInner - CutOffOuter;
        spotIntensity = smoothstep(0.0, 1.0, (theta - CutOffOuter) / epsilon);
    }

    float3 halfDirection = normalize(viewDirection + lightDirection);
    float3 normal = normalize(input.Normal);

    MaterialProperties material;

    if (dot(normal, viewDirection) < 0)
    {
        normal = -normal;
        float2 texCoord = float2(1.0 - input.TextureCoordinates.x, input.TextureCoordinates.y);
        material.DiffuseColor = tex2D(BackTextureSampler, texCoord).xyz;
    }
    else
    {
        material.DiffuseColor = tex2D(FrontTextureSampler, input.TextureCoordinates).xyz;
    }

    //material.DiffuseColor = tex2D(DiffuseMapTextureSampler, input.TextureCoordinates).xyz;
    //material.DiffuseColor = DiffuseColor;
    material.Roughness = Roughness;
    material.Metallic = Metallic;
    material.Ao = 1.0;
    material.EmissiveColor = EmissiveColor;
    material.BaseReflectivity = BaseReflectivity;

    if (ApplyGammaCorrection)
    {
        material.DiffuseColor = InverseGammaCorrection(material.DiffuseColor, Gamma);
        material.EmissiveColor = InverseGammaCorrection(material.EmissiveColor, Gamma);
        LightColor = InverseGammaCorrection(LightColor, Gamma);
        AmbientColor = InverseGammaCorrection(AmbientColor, Gamma);
    }

    float attenuation = 1.0;

    if (LightType == 1 || LightType == 2)
    {
        float distanceToLightSource = length(WorldViewLightPosition - input.WorldViewPosition);
        attenuation = GetAttenuation(distanceToLightSource, Constant, Linear, Quadratic);
    }

    float3 color = PBR(normal,
        lightDirection,
        viewDirection,
        halfDirection,
        material,
        LightColor,
        AmbientColor,
        attenuation,
        spotIntensity);

    color = SimpleToneMapping(color);

    if (ApplyGammaCorrection)
    {
        color = GammaCorrection(color, Gamma);
    }

    color = saturate(color);

    return float4(color, 1.0);
}

//===============================================================================
// Techniques
//===============================================================================
technique FabricComputeTechnique
{
    pass VerletPass
    {
        ComputeShader = compile cs_5_0 CSVerlet();
    }

    pass ConstraintsPass
    {
        ComputeShader = compile cs_5_0 CSConstraints();
    }

    pass AirCalculations
    {
        ComputeShader = compile cs_5_0 CSAirCalculations();
    }

    pass SwapBuffersPass
    {
        ComputeShader = compile cs_5_0 CSSwapBuffers();
    }

    pass RenderPass
    {
        VertexShader = compile vs_5_0 VSSolidCompute();
        PixelShader = compile ps_5_0 PSSolidCompute();
    }
}
