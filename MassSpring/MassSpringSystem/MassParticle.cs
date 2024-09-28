using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Beryllium.Physics.MassSpringSystem;

public class MassParticle
{
    public float Mass { get; set; }
    public Vector3 TotalForce { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Position { get; set; }
    public bool Pinned { get; set; }

    public delegate void AdditionalConstraintsDelegate(Vector3 oldVelocity,
        Vector3 oldPosition,
        ref Vector3 newVelocity,
        ref Vector3 newPosition,
        float timeStep);
    public AdditionalConstraintsDelegate AdditionalConstraints { get; set; }

    public void Update(float timeStep)
    {
        if (Mass == 0 || Pinned) return;

        var acceleration = TotalForce / Mass;

        var deltaVelocity = timeStep * acceleration;
        var newVelocity = Velocity + deltaVelocity;

        var deltaPosition = timeStep * Velocity;
        var newPosition = Position + deltaPosition;

        AdditionalConstraints?.Invoke(Velocity, Position, ref newVelocity, ref newPosition, timeStep);

        Velocity = newVelocity;
        Position = newPosition;
    }
}