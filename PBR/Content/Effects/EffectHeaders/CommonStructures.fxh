#ifndef COMMON_STRUCTURES_FXH
#define COMMON_STRUCTURES_FXH

struct FabricParticle
{
    float3 PrevPosition;
    float3 Position;
    float3 TotalForce;
    float3 Velocity;
    float3 Acceleration;
    float3 Normal;
    float2 TextureCoord;
    bool IsPinned;
};

struct PBRMaterialProperties
{
    float3 DiffuseColor;
    float Roughness;
    float Metallic;
    float Ao;
    float3 EmissiveColor;
    float BaseReflectivity;
};

struct VSInputCompute
{
    float3 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinates : TEXCOORD0;
    uint VertexID : SV_VertexID;
};

struct VSOutputCompute
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinates : TEXCOORD1;
    float3 WorldViewPosition : TEXCOORD2;
};

struct VSInputPBR
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VSOutputPBRSolid
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float3 WorldViewPosition : TEXCOORD1;
};

struct VSOutputPBRTextured
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float3 TangentLightPosition : TEXCOORD1;
    float3 TangentLightDirection : TEXCOORD2;
    float3 TangentPosition : TEXCOORD3;
    //float3 TangentWorldViewPosition : TEXCOORD3;
    //float3 WorldViewPosition : TEXCOORD1;
    //float3x3 Tbn : TEXCOORD2;
};

#endif // COMMON_STRUCTURES_FXH
