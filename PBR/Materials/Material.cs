using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.Materials;

internal class Material
{
    private ContentManager _contentManager;

    public bool UseSingleDiffuseColor { get; set; }
    public Vector3 DiffuseColor { get; set; }
    public Texture2D DiffuseMapTexture { get; set; }
    public Texture2D NormalMapTexture { get; set; }
    public Texture2D HeightMapTexture { get; set; }
    public Texture2D RoughnessMapTexture { get; set; }
    public Texture2D MetallicMapTexture { get; set; }
    public Texture2D AoMapTexture { get; set; }
    public Texture2D EmissiveMapTexture { get; set; }
    public bool UseSingleEmissiveColor { get; set; }
    public Vector3 EmissiveColor { get; set; }
    public float BaseReflectivity { get; set; }
    public bool InvertGreenChannel { get; set; }
    public bool IsDepthMap { get; set; }
    public float ParallaxHeightScale { get; set; }
    public int ParallaxMinSteps { get; set; }
    public int ParallaxMaxSteps { get; set; }

    public Material(ContentManager contentManager,
        string diffuseTexturePath,
        string normalTexturePath,
        string heightTexturePath,
        string roughnessTexturePath,
        string metallicTexturePath,
        string aoTexturePath,
        string emissiveTexturePath,
        float baseReflectivity,
        bool invertGreenChannel,
        bool isDepthMap,
        float parallaxHeightScale,
        int parallaxMinSteps,
        int parallaxMaxSteps)
    {
        _contentManager = contentManager;

        DiffuseMapTexture = LoadTexture(diffuseTexturePath);
        NormalMapTexture = LoadTexture(normalTexturePath);
        HeightMapTexture = LoadTexture(heightTexturePath);
        RoughnessMapTexture = LoadTexture(roughnessTexturePath);
        MetallicMapTexture = LoadTexture(metallicTexturePath);
        AoMapTexture = LoadTexture(aoTexturePath);
        EmissiveMapTexture = LoadTexture(emissiveTexturePath);
        BaseReflectivity = baseReflectivity;
        InvertGreenChannel = invertGreenChannel;
        IsDepthMap = isDepthMap;
        ParallaxHeightScale = parallaxHeightScale;
        ParallaxMinSteps = parallaxMinSteps;
        ParallaxMaxSteps = parallaxMaxSteps;
    }

    private Texture2D LoadTexture(string texturePath)
    {
        if (string.IsNullOrEmpty(texturePath)) return null;

        try
        {
            return _contentManager.Load<Texture2D>(texturePath);
        }
        catch (Exception)
        {
            return null;
        }
    }
}