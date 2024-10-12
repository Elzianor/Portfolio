using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beryllium.Materials;

public class SolidColorProperties
{
    public Vector3 DiffuseColor { get; set; }
    public Vector3 EmissiveColor { get; set; }
    public Vector3 AmbientColor { get; set; }
    public float Roughness { get; set; }
    public float Metallic { get; set; }
}

public class TextureProperties
{
    public string DiffuseTexturePath { get; set; }
    public string NormalTexturePath { get; set; }
    public string HeightTexturePath { get; set; }
    public string RoughnessTexturePath { get; set; }
    public string MetallicTexturePath { get; set; }
    public string AmbientOcclusionTexturePath { get; set; }
    public string EmissiveTexturePath { get; set; }
    public bool InvertNormalYAxis { get; set; }
    public bool IsDepthMap { get; set; }
    public int ParallaxMinSteps { get; set; }
    public int ParallaxMaxSteps { get; set; }
    public float ParallaxHeightScale { get; set; }
}

public class Material
{
    [JsonIgnore]
    public Texture2D DiffuseMapTexture { get; set; }
    [JsonIgnore]
    public Texture2D NormalMapTexture { get; set; }
    [JsonIgnore]
    public Texture2D HeightMapTexture { get; set; }
    [JsonIgnore]
    public Texture2D RoughnessMapTexture { get; set; }
    [JsonIgnore]
    public Texture2D MetallicMapTexture { get; set; }
    [JsonIgnore]
    public Texture2D AmbientOcclusionMapTexture { get; set; }
    [JsonIgnore]
    public Texture2D EmissiveMapTexture { get; set; }

    public string Name { get; set; }
    public SolidColorProperties SolidColorProperties { get; set; }
    public TextureProperties TextureProperties { get; set; }
    public float BaseReflectivity { get; set; }

    public Material(string name = "")
    {
        Name = name;
    }

    public void TryLoadTextures(ContentManager contentManager)
    {
        DiffuseMapTexture = LoadTexture(contentManager, TextureProperties.DiffuseTexturePath);
        NormalMapTexture = LoadTexture(contentManager, TextureProperties.NormalTexturePath);
        HeightMapTexture = LoadTexture(contentManager, TextureProperties.HeightTexturePath);
        RoughnessMapTexture = LoadTexture(contentManager, TextureProperties.RoughnessTexturePath);
        MetallicMapTexture = LoadTexture(contentManager, TextureProperties.MetallicTexturePath);
        AmbientOcclusionMapTexture = LoadTexture(contentManager, TextureProperties.AmbientOcclusionTexturePath);
        EmissiveMapTexture = LoadTexture(contentManager, TextureProperties.EmissiveTexturePath);
    }

    private Texture2D LoadTexture(ContentManager contentManager, string texturePath)
    {
        if (string.IsNullOrEmpty(texturePath)) return null;

        try { return contentManager.Load<Texture2D>(texturePath); }
        catch (Exception) { return null; }
    }

    public void Serialize(string filePath = "")
    {
        filePath = string.IsNullOrEmpty(filePath) ?
            Path.Combine(Environment.CurrentDirectory, $"{Name}.json") :
            filePath;

        File.WriteAllText(filePath, JsonSerializer.Serialize(this));
    }

    public static Material Deserialize(string filePath)
    {
        return JsonSerializer.Deserialize<Material>(File.ReadAllText(filePath));
    }
}