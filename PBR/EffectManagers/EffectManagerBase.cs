using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Beryllium.EffectManagers;

public abstract class EffectManagerBase
{
    public Effect Effect { get; }

    protected EffectManagerBase(ContentManager contentManager, string effectPath)
    {
        Effect = contentManager.Load<Effect>(effectPath);
    }

    public void ApplyPass(string techniqueName = "", int pass = 0)
    {
        var technique = string.IsNullOrWhiteSpace(techniqueName) ?
            Effect.CurrentTechnique :
            Effect.Techniques[techniqueName];

        technique.Passes[pass].Apply();
    }
}