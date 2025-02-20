using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.EffectManagers;

public abstract class EffectManagerBase
{
    public Effect Effect { get; }

    protected EffectManagerBase(ContentManager contentManager, string effectPath)
    {
        Effect = contentManager.Load<Effect>(effectPath);
    }

    public EffectManagerBase ApplyTechnique(string techniqueName)
    {
        if (!string.IsNullOrEmpty(techniqueName))
            Effect.CurrentTechnique = Effect.Techniques[techniqueName];

        return this;
    }

    public void ApplyPass(string passName = "")
    {
        if (!string.IsNullOrEmpty(passName))
            Effect.CurrentTechnique.Passes[passName].Apply();
        else
            Effect.CurrentTechnique.Passes[0].Apply();
    }
}