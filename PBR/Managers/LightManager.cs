using PBR.EffectManagers;
using PBR.Utils;

namespace PBR.Managers;
internal class LightManager(
    PbrEffectManager pbrEffectManager,
    LightSourceEffectManager lightSourceEffectManager,
    LightSourceRepresentation representation)
{
    private PbrEffectManager _pbrEffectManager = pbrEffectManager;
    private LightSourceEffectManager _lightSourceEffectManager = lightSourceEffectManager;
    private LightSourceRepresentation _representation = representation;

    public void Update()
    {

    }

    public void Draw()
    {

    }
}
