struct FabricParticle
{
    float3 PrevPosition;
    float3 Position;
    bool IsPinned;
};

//=============================================================================
// Compute Shader
//=============================================================================
#define GroupSize 256

RWStructuredBuffer<FabricParticle> FabricParticlesInput;
RWStructuredBuffer<FabricParticle> FabricParticlesOutput;

uint FabricParticlesCount;

float FabricParticleMass;
float3 GravitationalAcceleration;
float PhysicsUpdateTimeStep;

uint FabricWidthInParticles;
uint FabricHeightInParticles;
float FabricStructuralRestLength;
float FabricShearingRestLength;

float FabricDamping;
float FabricStiffness;

bool UseShearingConstraints;

float3 ProcessConstraint(FabricParticle currentParticle, FabricParticle neighborParticle, float restLength)
{
    float3 direction = neighborParticle.Position - currentParticle.Position;
    float distance = length(direction);

    float3 correction = float3(0.0, 0.0, 0.0);

    if (abs(distance - restLength) < 1e-6)
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
        return;

    float3 nextPosition = p.Position + (1.0 - FabricDamping) * (p.Position - p.PrevPosition) + GravitationalAcceleration * PhysicsUpdateTimeStep * PhysicsUpdateTimeStep;

    p.PrevPosition = p.Position;
    p.Position = nextPosition;

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
    bool hasRightNeigbor = (uint) (index % FabricWidthInParticles) < (uint) (FabricWidthInParticles - 1.0);
    bool hasDownNeigbor = downIndex >= 0;
    bool hasLeftNeighbor = (index % FabricWidthInParticles) > 0;

    if (hasUpNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[upIndex], FabricStructuralRestLength);

    if (hasRightNeigbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[rightIndex], FabricStructuralRestLength);

    if (hasDownNeigbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[downIndex], FabricStructuralRestLength);

    if (hasLeftNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[leftIndex], FabricStructuralRestLength);

    if (!UseShearingConstraints)
    {
        currentParticle.Position += correction;
        FabricParticlesOutput[index] = currentParticle;
        return;
    }

    // shearing constraints
    uint upRightIndex = upIndex + 1.0;
    uint upLeftIndex = upIndex - 1.0;
    int downRightIndex = downIndex + 1.0;
    int downLeftIndex = downIndex - 1.0;

    bool hasUpRightNeighbor = hasUpNeighbor && hasRightNeigbor;
    bool hasUpLeftNeighbor = hasUpNeighbor && hasLeftNeighbor;
    bool hasDownRightNeighbor = hasDownNeigbor && hasRightNeigbor;
    bool hasDownLeftNeighbor = hasDownNeigbor && hasLeftNeighbor;

    if (hasUpRightNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[upRightIndex], FabricShearingRestLength);

    if (hasUpLeftNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[upLeftIndex], FabricShearingRestLength);

    if (hasDownRightNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[downRightIndex], FabricShearingRestLength);

    if (hasDownLeftNeighbor)
        correction += ProcessConstraint(currentParticle, FabricParticlesInput[downLeftIndex], FabricShearingRestLength);

    currentParticle.Position += correction;

    FabricParticlesOutput[index] = currentParticle;
}

[numthreads(GroupSize, 1, 1)]
void CSSwap(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint index = globalID.x;

    if (index >= FabricParticlesCount)
        return;

    FabricParticlesInput[index] = FabricParticlesOutput[index];
}

//==============================================================================
// Vertex shader
//==============================================================================
StructuredBuffer<FabricParticle> FabricParticlesReadOnly;

float4x4 WorldViewProjection;

struct VertexIn
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    uint VertexID : SV_VertexID;
};

struct VertexOut
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexOut VS(in VertexIn input)
{
    VertexOut output;

    FabricParticle p = FabricParticlesReadOnly[input.VertexID];

    output.Position = mul(float4(p.Position, 1.0), WorldViewProjection);
    output.TexCoord = input.TexCoord;

    return output;
}

//==============================================================================
// Pixel shader 
//==============================================================================
float4 PS(VertexOut input) : SV_TARGET
{
    return float4(1.0, 0.0, 0.0, 1.0);
}

//===============================================================================
// Techniques
//===============================================================================
technique Tech0
{
    pass VerletPass
    {
        ComputeShader = compile cs_5_0 CSVerlet();
    }

    pass ConstraintsPass
    {
        ComputeShader = compile cs_5_0 CSConstraints();
    }

    pass SwapPass
    {
        ComputeShader = compile cs_5_0 CSSwap();
    }

    pass RenderPass
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}
