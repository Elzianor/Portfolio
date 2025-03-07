#ifndef COMMON_UNIFORMS_FXH
#define COMMON_UNIFORMS_FXH

// --- Transform ---
float4x4 WorldViewProjection;
float4x4 WorldView;
float4x4 WorldViewInverseTranspose;

// --- Lighting ---
uint LightType;

float Constant;
float Linear;
float Quadratic;

float CutOffInner;
float CutOffOuter;

float3 WorldViewLightDirection;
float3 WorldViewLightPosition;

float3 LightColor;
float3 AmbientColor;

float BaseReflectivity;

bool InvertNormalYAxis;
bool IsDepthMap;

float ParallaxHeightScale;
int ParallaxMinSteps;
int ParallaxMaxSteps;

float Gamma;
bool ApplyGammaCorrection;

bool IsDoubleSidedMaterial;

// --- PBR solid material properties ---
float3 DiffuseColor;
float3 EmissiveColor;
float Roughness;
float Metallic;

// --- PBR textured material properties ---
texture DiffuseMapTexture;
sampler2D DiffuseMapTextureSampler = sampler_state
{
    Texture = (DiffuseMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture NormalMapTexture;
sampler2D NormalMapTextureSampler = sampler_state
{
    Texture = (NormalMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture HeightMapTexture;
sampler2D HeightMapTextureSampler = sampler_state
{
    Texture = (HeightMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture RoughnessMapTexture;
sampler2D RoughnessMapTextureSampler = sampler_state
{
    Texture = (RoughnessMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture MetallicMapTexture;
sampler2D MetallicMapTextureSampler = sampler_state
{
    Texture = (MetallicMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture AoMapTexture;
sampler2D AoMapTextureSampler = sampler_state
{
    Texture = (AoMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture EmissiveMapTexture;
sampler2D EmissiveMapTextureSampler = sampler_state
{
    Texture = (EmissiveMapTexture);
    Filter = Anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
};

#endif // COMMON_UNIFORMS_FXH
