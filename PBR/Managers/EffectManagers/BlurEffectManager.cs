using Beryllium.EffectManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace PBR.EffectManagers;

internal class BlurEffectManager : EffectManagerBase
{
    private Vector2 _texelSize;
    public Vector2 TexelSize
    {
        get => _texelSize;
        set
        {
            _texelSize = value;
            Effect.Parameters["TexelSize"].SetValue(_texelSize);
        }
    }

    private bool _horizontalPass;
    public bool HorizontalPass
    {
        get => _horizontalPass;
        set
        {
            _horizontalPass = value;
            Effect.Parameters["HorizontalPass"].SetValue(_horizontalPass);
        }
    }

    private float[] _gaussianWeights;
    public float[] GaussianWeights
    {
        get => _gaussianWeights;
        set
        {
            _gaussianWeights = value;
            Effect.Parameters["GaussianWeights"].SetValue(_gaussianWeights);
        }
    }

    public BlurEffectManager(ContentManager contentManager,
        string effectPath)
        : base(contentManager, effectPath)
    {
    }

}