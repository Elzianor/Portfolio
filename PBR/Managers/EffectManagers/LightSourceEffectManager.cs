using Beryllium.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace PBR.EffectManagers;

internal class LightSourceEffectManager(ContentManager contentManager, string effectPath)
    : EffectManagerBase(contentManager, effectPath)
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

    #region Light color
    private Vector3 _lightColor;
    public Vector3 LightColor
    {
        get => _lightColor;
        set
        {
            _lightColor = value;
            Effect.Parameters["LightColor"].SetValue(_lightColor);
        }
    }
    #endregion

    #region Update
    public void Update(Camera camera, Vector3 position)
    {
        WorldMatrix = Matrix.CreateTranslation(position - camera.Offset);
        ViewMatrix = camera.ViewMatrix;
        ProjectionMatrix = camera.ProjectionMatrix;
    }
    #endregion

    #region Recalculations
    private void RecalculateMatrices()
    {
        var wvp = _worldMatrix * _viewMatrix * _projectionMatrix;

        Effect.Parameters["WorldViewProjection"].SetValue(wvp);
    }
    #endregion
}
