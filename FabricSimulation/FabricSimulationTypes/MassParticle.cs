using System.Collections.Generic;
using System.Diagnostics;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Beryllium.Physics;

public class MassParticle
{
    private float _timeStep;
    private Vector3 _acceleration = new(0, -9.8f, 0);
    private List<FabricThread> _ownFabricThreads = new();

    public float Mass { get; set; }
    public Vector3 TotalForce { get; set; }
    public Vector3 PrevPosition { get; set; }
    public Vector3 Position { get; set; }
    public float DragCoefficient { get; set; } = 1.5f;
    public float ProjectedArea { get; set; } = 0.01f;
    public bool Pinned { get; set; }

    public Vector3 Normal => GetNormal();
    public Vector3 Velocity => IsImmovable ? Vector3.Zero : (Position - PrevPosition) / (2 * _timeStep);
    public bool IsImmovable => Mass == 0 || Pinned;

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

        //AdditionalConstraints?.Invoke(Position, ref nextPosition, nextVelocity, timeStep);

        PrevPosition = Position;
        Position = nextPosition;
    }

    public void UpdateAcceleration()
    {
        _acceleration = TotalForce / Mass;
    }

    public void AddFabricThread(FabricThread fabricThread)
    {
        _ownFabricThreads.Add(fabricThread);
    }

    public Vector3 GetNormal()
    {
        if (_ownFabricThreads.Count < 2) return Vector3.Zero;

        var normal = Vector3.Zero;
        var index = 0;

        do
        {
            var firstThread = _ownFabricThreads[index++];
            var firstThreadDirection = GetOtherEnd(firstThread).Position - Position;

            if (index == _ownFabricThreads.Count) index = 0;

            var secondThread = _ownFabricThreads[index];
            var secondThreadDirection = GetOtherEnd(secondThread).Position - Position;

            normal += Vector3.Cross(firstThreadDirection, secondThreadDirection);
        } while (index != 0);

        if (normal == Vector3.Zero)
        {
            //return Vector3.UnitY;
            return Vector3.Zero;
        }

        normal.Normalize();

        return normal;
    }

    private MassParticle GetOtherEnd(FabricThread fabricThread)
    {
        return fabricThread.Mass1 == this ? fabricThread.Mass2 : fabricThread.Mass1;
    }
}