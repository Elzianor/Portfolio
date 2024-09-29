using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Beryllium.Physics;

public class MassParticle
{
    private float _timeStep;
    private Vector3 _acceleration = new(0, -9.8f, 0);

    public float Mass { get; set; }
    public Vector3 TotalForce { get; set; }
    public Vector3 PrevPosition { get; set; }
    public Vector3 Position { get; set; }
    public bool Pinned { get; set; }

    public Vector3 Velocity => Mass == 0 || Pinned ? Vector3.Zero : (Position - PrevPosition) / (2 * _timeStep);

    public delegate void AdditionalConstraintsDelegate(Vector3 oldPosition,
        ref Vector3 newPosition,
        Vector3 velocity,
        float timeStep);
    public AdditionalConstraintsDelegate AdditionalConstraints { get; set; }

    public void Update(float timeStep)
    {
        if (Mass == 0 || Pinned) return;

        // Verlet integration (more accurate with stiffness and large time steps)

        _timeStep = timeStep;

        var nextPosition = 2 * Position - PrevPosition + _acceleration * timeStep * timeStep;

        var nextVelocity = (nextPosition - Position) / (2 * _timeStep);

        AdditionalConstraints?.Invoke(Position, ref nextPosition, nextVelocity, timeStep);

        PrevPosition = Position;
        Position = nextPosition;
    }

    public void UpdateAcceleration()
    {
        _acceleration = TotalForce / Mass;
    }
}