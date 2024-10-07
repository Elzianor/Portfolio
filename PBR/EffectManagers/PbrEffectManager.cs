using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using PBR.EffectManagers;
using PBR.Materials;

namespace PBR.Effects;

internal class PbrEffectManager : BaseEffectManager
{
    private Material _material;
    public Material Material
    {
        get => _material;
        set
        {
            _material = value;
            Effect.Parameters["DiffuseMapTexture"].SetValue(_material.DiffuseMapTexture);
            Effect.Parameters["NormalMapTexture"].SetValue(_material.NormalMapTexture);
            Effect.Parameters["RoughnessMapTexture"].SetValue(_material.RoughnessMapTexture);
            Effect.Parameters["MetallicMapTexture"].SetValue(_material.MetallicMapTexture);
            Effect.Parameters["AoMapTexture"].SetValue(_material.AoMapTexture);
            Effect.Parameters["BaseReflectivity"].SetValue(_material.BaseReflectivity);
        }
    }

    public float BaseReflectivity
    {
        get => _material.BaseReflectivity;
        set
        {
            _material.BaseReflectivity = value;
            Effect.Parameters["BaseReflectivity"].SetValue(_material.BaseReflectivity);
        }
    }

    private Matrix _worldMatrix;
    public Matrix WorldMatrix
    {
        get => _worldMatrix;
        set
        {
            _worldMatrix = value;
            RecalculateMatrices();
        }
    }

    private Matrix _viewMatrix;
    public Matrix ViewMatrix
    {
        get => _viewMatrix;
        set
        {
            _viewMatrix = value;
            RecalculateMatrices();
        }
    }

    private Matrix _projectionMatrix;
    public Matrix ProjectionMatrix
    {
        get => _projectionMatrix;
        set
        {
            _projectionMatrix = value;
            RecalculateMatrices();
        }
    }

    private Vector3 _lightDirection;
    public Vector3 LightDirection
    {
        get => _lightDirection;
        set
        {
            _lightDirection = value;
            Effect.Parameters["LightDirection"].SetValue(_lightDirection);
        }
    }

    private Vector3 _lightColor;
    public Vector3 LightColor
    {
        get => _lightColor;
        set
        {
            _lightColor = value;
            RecalculateLightColor();
        }
    }

    private Vector3 _ambientColor;
    public Vector3 AmbientColor
    {
        get => _ambientColor;
        set
        {
            _ambientColor = value;
            Effect.Parameters["AmbientColor"].SetValue(_ambientColor);
        }
    }

    private Vector3 _emissiveColor;
    public Vector3 EmissiveColor
    {
        get => _emissiveColor;
        set
        {
            _emissiveColor = value;
            Effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
        }
    }

    private float _lightIntensity;
    public float LightIntensity
    {
        get => _lightIntensity;
        set
        {
            _lightIntensity = value;
            RecalculateLightColor();
        }
    }

    public PbrEffectManager(ContentManager contentManager,
        string effectPath)
        : base(contentManager, effectPath)
    {
    }

    private void RecalculateMatrices()
    {
        var wvp = _worldMatrix * _viewMatrix * _projectionMatrix;
        var wv = _worldMatrix * _viewMatrix;
        var wvit = Matrix.Transpose(Matrix.Invert(wv));

        Effect.Parameters["WorldViewProjection"].SetValue(wvp);
        Effect.Parameters["WorldView"].SetValue(wv);
        Effect.Parameters["WorldViewInverseTranspose"].SetValue(wvit);
    }

    private void RecalculateLightColor()
    {
        Effect.Parameters["LightColor"].SetValue(_lightIntensity * _lightColor);
    }
}