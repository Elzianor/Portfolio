using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.Materials;

internal class Material
{
    public Texture2D DiffuseMapTexture { get; }
    public Texture2D NormalMapTexture { get; }
    public Texture2D RoughnessMapTexture { get; }
    public Texture2D MetallicMapTexture { get; }
    public Texture2D AoMapTexture { get; }
    public float BaseReflectivity { get; set; }

    public Material(ContentManager contentManager,
        string diffuseTexturePath,
        string normalTexturePath,
        string roughnessTexturePath,
        string metallicTexturePath,
        string aoTexturePath,
        float baseReflectivity)
    {
        DiffuseMapTexture = contentManager.Load<Texture2D>(diffuseTexturePath);
        NormalMapTexture = contentManager.Load<Texture2D>(normalTexturePath);
        RoughnessMapTexture = contentManager.Load<Texture2D>(roughnessTexturePath);
        MetallicMapTexture = contentManager.Load<Texture2D>(metallicTexturePath);
        AoMapTexture = contentManager.Load<Texture2D>(aoTexturePath);
        BaseReflectivity = baseReflectivity;
    }
}