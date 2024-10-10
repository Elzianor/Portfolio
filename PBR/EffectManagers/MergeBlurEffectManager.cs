using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.EffectManagers;

internal class MergeBlurEffectManager : BaseEffectManager
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

    public MergeBlurEffectManager(ContentManager contentManager,
        string effectPath)
        : base(contentManager, effectPath)
    {
    }
}