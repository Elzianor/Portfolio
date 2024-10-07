using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.EffectManagers;

internal abstract class BaseEffectManager
{
    public Effect Effect { get; }

    protected BaseEffectManager(ContentManager contentManager, string effectPath)
    {
        Effect = contentManager.Load<Effect>(effectPath);
    }

    public void ApplyPass(int pass = 0)
    {
        Effect.CurrentTechnique.Passes[pass].Apply();
    }
}