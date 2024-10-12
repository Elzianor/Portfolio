using Beryllium.EffectManagers;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.EffectManagers;

internal class MergeBlurEffectManager : EffectManagerBase
{
    private RenderTarget2D _mainScene;
    public RenderTarget2D MainScene
    {
        get => _mainScene;
        set
        {
            _mainScene = value;
            Effect.Parameters["MainSceneSampler"].SetValue(_mainScene);
        }
    }

    private RenderTarget2D _blur;
    public RenderTarget2D Blur
    {
        get => _blur;
        set
        {
            _blur = value;
            Effect.Parameters["BlurSampler"].SetValue(_blur);
        }
    }

    private float _gamma;
    public float Gamma
    {
        get => _gamma;
        set
        {
            _gamma = value;
            Effect.Parameters["Gamma"].SetValue(_gamma);
        }
    }

    private float _exposure;
    public float Exposure
    {
        get => _exposure;
        set
        {
            _exposure = value;
            Effect.Parameters["Exposure"].SetValue(_exposure);
        }
    }

    private bool _applyGammaCorrection;
    public bool ApplyGammaCorrection
    {
        get => _applyGammaCorrection;
        set
        {
            _applyGammaCorrection = value;
            Effect.Parameters["ApplyGammaCorrection"].SetValue(_applyGammaCorrection);
        }
    }

    private bool _applyToneMapping;
    public bool ApplyToneMapping
    {
        get => _applyToneMapping;
        set
        {
            _applyToneMapping = value;
            Effect.Parameters["ApplyToneMapping"].SetValue(_applyToneMapping);
        }
    }

    public MergeBlurEffectManager(ContentManager contentManager,
        string effectPath)
        : base(contentManager, effectPath)
    {
    }
}