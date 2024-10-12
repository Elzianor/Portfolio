﻿using Microsoft.Xna.Framework;
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
            Effect.Parameters["HeightMapTexture"].SetValue(_material.HeightMapTexture);
            Effect.Parameters["RoughnessMapTexture"].SetValue(_material.RoughnessMapTexture);
            Effect.Parameters["MetallicMapTexture"].SetValue(_material.MetallicMapTexture);
            Effect.Parameters["AoMapTexture"].SetValue(_material.AoMapTexture);
            Effect.Parameters["EmissiveMapTexture"].SetValue(_material.EmissiveMapTexture);
            Effect.Parameters["BaseReflectivity"].SetValue(_material.BaseReflectivity);
            Effect.Parameters["InvertGreenChannel"].SetValue(_material.InvertGreenChannel);
            Effect.Parameters["IsDepthMap"].SetValue(_material.IsDepthMap);
            Effect.Parameters["UseSingleDiffuseColor"].SetValue(_material.UseSingleDiffuseColor);
            Effect.Parameters["DiffuseColor"].SetValue(_material.DiffuseColor);
            Effect.Parameters["UseSingleEmissiveColor"].SetValue(_material.UseSingleEmissiveColor);
            Effect.Parameters["EmissiveColor"].SetValue(_material.EmissiveColor);
            Effect.Parameters["ParallaxHeightScale"].SetValue(_material.ParallaxHeightScale);
            Effect.Parameters["ParallaxMinSteps"].SetValue(_material.ParallaxMinSteps);
            Effect.Parameters["ParallaxMaxSteps"].SetValue(_material.ParallaxMaxSteps);
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

    public bool UseSingleDiffuseColor
    {
        get => _material.UseSingleDiffuseColor;
        set
        {
            _material.UseSingleDiffuseColor = value;
            Effect.Parameters["UseSingleDiffuseColor"].SetValue(_material.UseSingleDiffuseColor);
        }
    }
    public Vector3 DiffuseColor
    {
        get => _material.DiffuseColor;
        set
        {
            _material.DiffuseColor = value;
            if (_material.UseSingleDiffuseColor)
                Effect.Parameters["DiffuseColor"].SetValue(_material.DiffuseColor);
        }
    }

    public bool UseSingleEmissiveColor
    {
        get => _material.UseSingleEmissiveColor;
        set
        {
            _material.UseSingleEmissiveColor = value;
            Effect.Parameters["UseSingleEmissiveColor"].SetValue(_material.UseSingleEmissiveColor);
        }
    }
    public Vector3 EmissiveColor
    {
        get => _material.EmissiveColor;
        set
        {
            _material.EmissiveColor = value;
            if (_material.UseSingleEmissiveColor)
                Effect.Parameters["EmissiveColor"].SetValue(_material.EmissiveColor);
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

    public float ParallaxHeightScale
    {
        get => _material.ParallaxHeightScale;
        set
        {
            _material.ParallaxHeightScale = value;
            Effect.Parameters["ParallaxHeightScale"].SetValue(_material.ParallaxHeightScale);
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