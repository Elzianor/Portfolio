using System;
using Microsoft.Xna.Framework;

namespace Beryllium.Physics;

public class Air
{
    private const float AirDensity = 1.225f; // kg/m^3

    public static void Resist(MassParticle massParticle)
    {
        var velocity = massParticle.Velocity;

        if (massParticle.IsImmovable || velocity == Vector3.Zero) return;

        // Calculate particle velocity magnitude
        var velocityMagnitude = velocity.Length();

        // Calculate drag force
        var dragForceMagnitude =
            0.5f * massParticle.DragCoefficient * AirDensity * massParticle.ProjectedArea * velocityMagnitude * velocityMagnitude;

        // Heavier the fabric, less the drag force
        dragForceMagnitude /= massParticle.Mass;

        var velocityDirection = velocity;
        velocityDirection.Normalize();

        var normalCoefficient = Math.Abs(Vector3.Dot(massParticle.Normal, velocityDirection));

        // Drag force depends on the fabric particle orientation
        dragForceMagnitude *= normalCoefficient;

        // Calculate drag force vector (opposite direction of velocity)
        var dragForce = -velocityDirection * dragForceMagnitude;

        massParticle.TotalForce += dragForce;

        // add wind
        var windForce = Vector3.UnitX * 3 * normalCoefficient;
        //massParticle.TotalForce += windForce;
    }
}