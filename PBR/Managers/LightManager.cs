using Beryllium.Camera;
using Microsoft.Xna.Framework;
using PBR.EffectManagers;
using PBR.Utils;

namespace PBR.Managers;

internal class LightManager
{
    private PbrEffectManager _pbrEffectManager;
    private LightSourceEffectManager _lightSourceEffectManager;
    private LightSourceRepresentation _representation;

    #region Light properties
    public LightType LightType
    {
        get => _pbrEffectManager.LightType;
        set => _pbrEffectManager.LightType = value;
    }

    public Vector3 LightDirection
    {
        get => _pbrEffectManager.LightDirection;
        set
        {
            value.Normalize();

            _pbrEffectManager.LightDirection = value;
            _representation.LightDirection = value;
        }
    }

    public Vector3 LightPosition
    {
        get => _pbrEffectManager.LightPosition;
        set
        {
            _pbrEffectManager.LightPosition = value;
            _representation.Position = value;
        }
    }

    public Vector3 LightColor
    {
        get => _pbrEffectManager.LightColor;
        set
        {
            _pbrEffectManager.LightColor = value;
            _lightSourceEffectManager.LightColor = value;
        }
    }

    public Vector3 AmbientColor
    {
        get => _pbrEffectManager.AmbientColor;
        set => _pbrEffectManager.AmbientColor = value;
    }

    public float LightIntensity
    {
        get => _pbrEffectManager.LightIntensity;
        set => _pbrEffectManager.LightIntensity = value;
    }

    public float Constant
    {
        get => _pbrEffectManager.Constant;
        set => _pbrEffectManager.Constant = value;
    }

    public float Linear
    {
        get => _pbrEffectManager.Linear;
        set => _pbrEffectManager.Linear = value;
    }

    public float Quadratic
    {
        get => _pbrEffectManager.Quadratic;
        set => _pbrEffectManager.Quadratic = value;
    }

    public float CutOffInnerDegrees
    {
        get => _pbrEffectManager.CutOffInnerDegrees;
        set => _pbrEffectManager.CutOffInnerDegrees = value;
    }

    public float CutOffOuterDegrees
    {
        get => _pbrEffectManager.CutOffOuterDegrees;
        set => _pbrEffectManager.CutOffOuterDegrees = value;
    }
    #endregion

    public LightManager(PbrEffectManager pbrEffectManager,
        LightSourceEffectManager lightSourceEffectManager,
        LightSourceRepresentation representation)
    {
        _pbrEffectManager = pbrEffectManager;
        _lightSourceEffectManager = lightSourceEffectManager;
        _representation = representation;

        LightDirection = _pbrEffectManager.LightDirection;
        LightPosition = _pbrEffectManager.LightPosition;
        LightColor = _pbrEffectManager.LightColor;
    }

    public void Update(Camera camera)
    {
        _pbrEffectManager.Update(camera);
        _lightSourceEffectManager.Update(camera, _representation.Position);
        _representation.Update(camera);
    }

    public void Draw()
    {
        _representation.Draw(_lightSourceEffectManager.Effect);
    }
}
