using Beryllium.Camera;
using Beryllium.Primitives3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PBR.Utils;

internal class LightSourceRepresentation
{
    private GraphicsDevice _graphicsDevice;
    private DrawableBasePrimitive _representation;
    private VertexPositionColor[] _projections;
    private BasicEffect _projectionsEffect;

    private Vector3 _lightDirection;
    public Vector3 LightDirection
    {
        get => _lightDirection;
        set
        {
            _lightDirection = value;
            _lightDirection.Normalize();
            UpdateDirectionProjection();
        }
    }

    public Vector3 Position
    {
        get => _representation.Position;
        set
        {
            _representation.Position = value;
            UpdatePositionProjection();
            UpdateDirectionProjection();
        }
    }

    public Matrix WorldMatrix => _representation.WorldMatrix;

    public LightSourceRepresentation(GraphicsDevice graphicsDevice, DrawableBasePrimitive representation)
    {
        _graphicsDevice = graphicsDevice;
        _representation = representation;

        _projections =
        [
            // position projection
            new VertexPositionColor(Position, Color.Red),
            new VertexPositionColor(new Vector3(Position.X, 0, Position.Z), Color.Red),

            // direction projection
            new VertexPositionColor(Position, Color.Green),
            new VertexPositionColor(Position + LightDirection * 0.5f, Color.Green)
        ];

        _projectionsEffect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true
        };
    }

    #region Draw

    public void Draw(Effect lightSourceEffect)
    {
        _representation.Draw(lightSourceEffect);

        _projectionsEffect.CurrentTechnique.Passes[0].Apply();

        _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList,
            _projections,
            0,
            2);
    }
    #endregion

    #region Updaters
    public void Update(Camera camera)
    {
        _projectionsEffect.World = camera.OffsetWorldMatrix;
        _projectionsEffect.View = camera.ViewMatrix;
        _projectionsEffect.Projection = camera.ProjectionMatrix;
    }

    private void UpdatePositionProjection()
    {
        _projections[0].Position = Position;
        _projections[1].Position = new Vector3(Position.X, 0, Position.Z);
    }

    private void UpdateDirectionProjection()
    {
        _projections[2].Position = Position;
        _projections[3].Position = Position + LightDirection * 0.5f;
    }
    #endregion
}
