using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using PBR.EffectManagers;
using PBR.Utils;

namespace PBR.Managers.EffectManagers;

public class ForceFieldEffectManager : EffectManagerBase
{
    #region Matrices
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

    private Matrix _cameraOffsetMatrix;
    public Matrix CameraOffsetMatrix
    {
        get => _cameraOffsetMatrix;
        set
        {
            _cameraOffsetMatrix = value;
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
    #endregion

    #region Time
    private float _time;
    public float Time
    {
        get => _time;
        set
        {
            _time = value;
            Effect.Parameters["Time"].SetValue(_time);
        }
    }
    #endregion

    #region Force field colors
    private Vector3 _mainColor;
    public Vector3 MainColor
    {
        get => _mainColor;
        set
        {
            _mainColor = value;
            Effect.Parameters["MainColor"].SetValue(_mainColor);
        }
    }

    private Vector3 _highlightColor;
    public Vector3 HighlightColor
    {
        get => _highlightColor;
        set
        {
            _highlightColor = value;
            Effect.Parameters["HighlightColor"].SetValue(_highlightColor);
        }
    }

    private Vector3 _lowCapacityColor;
    public Vector3 LowCapacityColor
    {
        get => _lowCapacityColor;
        set
        {
            _lowCapacityColor = value;
            Effect.Parameters["LowCapacityColor"].SetValue(_lowCapacityColor);
        }
    }
    #endregion

    #region Other properties
    private float _highlightThickness;
    public float HighlightThickness
    {
        get => _highlightThickness;
        set
        {
            _highlightThickness = value;
            Effect.Parameters["HighlightThickness"].SetValue(_highlightThickness);
        }
    }

    private float _glowIntensity;
    public float GlowIntensity
    {
        get => _glowIntensity;
        set
        {
            _glowIntensity = value;
            Effect.Parameters["GlowIntensity"].SetValue(_glowIntensity);
        }
    }

    private float _dissolveThreshold;
    public float DissolveThreshold
    {
        get => _dissolveThreshold;
        set
        {
            _dissolveThreshold = value;
            Effect.Parameters["DissolveThreshold"].SetValue(_dissolveThreshold);
        }
    }

    private float _height;
    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            Effect.Parameters["Height"].SetValue(_height);
        }
    }

    private bool _isLowCapacity;
    public bool IsLowCapacity
    {
        get => _isLowCapacity;
        set
        {
            _isLowCapacity = value;
            Effect.Parameters["IsLowCapacity"].SetValue(_isLowCapacity);
        }
    }
    #endregion

    public ForceFieldEffectManager(ContentManager contentManager, string effectPath)
        : base(contentManager, effectPath)
    {
        Perlin3D.SetSeed();

        var pt = Perlin3D.PermutationTable.Select(ptEntry => (int)ptEntry).ToArray();
        var gs = Perlin3D.GradientSet.Select(vec => new Vector3(vec.X, vec.Y, vec.Z)).ToArray();

        Effect.Parameters["mX"].SetValue(Perlin3D.MX);
        Effect.Parameters["mY"].SetValue(Perlin3D.MY);
        Effect.Parameters["mZ"].SetValue(Perlin3D.MZ);
        Effect.Parameters["permutationTable"].SetValue(pt);
        Effect.Parameters["gradientSet"].SetValue(gs);
    }

    #region Recalculations
    private void RecalculateMatrices()
    {
        var wo = _worldMatrix * _cameraOffsetMatrix;
        var wovp = wo * _viewMatrix * _projectionMatrix;
        var wov = wo * _viewMatrix;
        var wovit = Matrix.Transpose(Matrix.Invert(wov));

        Effect.Parameters["World"].SetValue(_worldMatrix);
        Effect.Parameters["WorldViewProjection"].SetValue(wovp);
        Effect.Parameters["WorldView"].SetValue(wov);
        Effect.Parameters["WorldViewInverseTranspose"].SetValue(wovit);
    }
    #endregion
}
