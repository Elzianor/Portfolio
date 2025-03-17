using System;
using Beryllium.Camera;
using Beryllium.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace PBR.EffectManagers;

internal enum LightType : uint
{
    Directional,
    Point,
    Spot
}

internal class PbrEffectManager(ContentManager contentManager, string effectPath)
    : EffectManagerBase(contentManager, effectPath)
{
    #region Material
    private Material _material;
    public Material Material
    {
        get => _material;
        set
        {
            if (_material == value) return;
            _material = value;

            if (_material == null) return;

            if (_material.SolidColorProperties != null)
            {
                Effect.Parameters["DiffuseColor"].SetValue(_material.SolidColorProperties.DiffuseColor);
                Effect.Parameters["EmissiveColor"].SetValue(_material.SolidColorProperties.EmissiveColor);
                Effect.Parameters["Roughness"].SetValue(_material.SolidColorProperties.Roughness);
                Effect.Parameters["Metallic"].SetValue(_material.SolidColorProperties.Metallic);
            }

            if (_material.TexturedProperties != null)
            {
                _material.TryLoadTextures(contentManager);

                Effect.Parameters["DiffuseMapTexture"].SetValue(_material.DiffuseMapTexture);
                Effect.Parameters["NormalMapTexture"].SetValue(_material.NormalMapTexture);
                Effect.Parameters["HeightMapTexture"].SetValue(_material.HeightMapTexture);
                Effect.Parameters["RoughnessMapTexture"].SetValue(_material.RoughnessMapTexture);
                Effect.Parameters["MetallicMapTexture"].SetValue(_material.MetallicMapTexture);
                Effect.Parameters["AoMapTexture"].SetValue(_material.AmbientOcclusionMapTexture);
                Effect.Parameters["EmissiveMapTexture"].SetValue(_material.EmissiveMapTexture);
                Effect.Parameters["InvertNormalYAxis"].SetValue(_material.TexturedProperties.InvertNormalYAxis);
                Effect.Parameters["IsDepthMap"].SetValue(_material.TexturedProperties.IsDepthMap);
                Effect.Parameters["ParallaxMinSteps"].SetValue(_material.TexturedProperties.ParallaxMinSteps);
                Effect.Parameters["ParallaxMaxSteps"].SetValue(_material.TexturedProperties.ParallaxMaxSteps);
                Effect.Parameters["ParallaxHeightScale"].SetValue(_material.TexturedProperties.ParallaxHeightScale);
            }

            Effect.Parameters["BaseReflectivity"].SetValue(_material.BaseReflectivity);
        }
    }
    #endregion

    #region Matrices
    private Matrix _lightWorldMatrix;

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
            RecalculateLightDirection();
            RecalculateLightPosition();
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
            RecalculateLightDirection();
            RecalculateLightPosition();
        }
    }
    #endregion

    #region Light properties
    private LightType _lightType;
    public LightType LightType
    {
        get => _lightType;
        set
        {
            _lightType = value;
            Effect.Parameters["LightType"].SetValue((uint)_lightType);
        }
    }

    private float _constant;
    public float Constant
    {
        get => _constant;
        set
        {
            _constant = value;
            Effect.Parameters["Constant"].SetValue(_constant);
        }
    }

    private float _linear;
    public float Linear
    {
        get => _linear;
        set
        {
            _linear = value;
            Effect.Parameters["Linear"].SetValue(_linear);
        }
    }

    private float _quadratic;
    public float Quadratic
    {
        get => _quadratic;
        set
        {
            _quadratic = value;
            Effect.Parameters["Quadratic"].SetValue(_quadratic);
        }
    }

    private float _cutOffInnerDegrees;
    public float CutOffInnerDegrees
    {
        get => _cutOffInnerDegrees;
        set
        {
            _cutOffInnerDegrees = value;
            Effect.Parameters["CutOffInner"].SetValue(
                (float)Math.Cos(MathHelper.ToRadians(_cutOffInnerDegrees / 2.0f)));
        }
    }

    private float _cutOffOuterDegrees;
    public float CutOffOuterDegrees
    {
        get => _cutOffOuterDegrees;
        set
        {
            _cutOffOuterDegrees = value;
            Effect.Parameters["CutOffOuter"].SetValue(
                (float)Math.Cos(MathHelper.ToRadians(_cutOffOuterDegrees / 2.0f)));
        }
    }

    private Vector3 _lightDirection;
    public Vector3 LightDirection
    {
        get => _lightDirection;
        set
        {
            _lightDirection = value;
            _lightDirection.Normalize();
            RecalculateLightDirection();
        }
    }

    private Vector3 _lightPosition;
    public Vector3 LightPosition
    {
        get => _lightPosition;
        set
        {
            _lightPosition = value;
            RecalculateLightPosition();
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
    #endregion

    #region Color correction
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
    #endregion

    #region Update
    public void Update(Camera camera, Matrix objectWorldMatrix)
    {
        WorldMatrix = objectWorldMatrix * camera.OffsetWorldMatrix;
        ViewMatrix = camera.ViewMatrix;
        ProjectionMatrix = camera.ProjectionMatrix;

        _lightWorldMatrix = camera.OffsetWorldMatrix;
        RecalculateLightDirection();
        RecalculateLightPosition();
    }
    #endregion

    #region Recalculations
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

    private void RecalculateLightDirection()
    {
        var wv = _lightWorldMatrix * _viewMatrix;
        var wvit = Matrix.Transpose(Matrix.Invert(wv));

        var ld = Vector3.TransformNormal(_lightDirection, wvit);
        ld.Normalize();

        Effect.Parameters["WorldViewLightDirection"].SetValue(ld);
    }

    private void RecalculateLightPosition()
    {
        var wv = _lightWorldMatrix * _viewMatrix;

        var lp = Vector3.Transform(_lightPosition, wv);

        Effect.Parameters["WorldViewLightPosition"].SetValue(lp);
    }
    #endregion
}