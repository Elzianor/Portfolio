using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Beryllium.Physics.MassSpringSystem;

public class MassSpringSystem
{
    private readonly Vector3 _gravitationalAcceleration = new (0, -9.8f, 0);

    public List<MassParticle> MassParticles { get; } = [];
    public List<Spring> Springs { get; set; } = [];

    public void Update(float timeStep)
    {
        AddGravity();

        UpdateSprings();

        UpdateMassParticles(timeStep);
    }

    private void AddGravity()
    {
        foreach (var massParticle in MassParticles)
        {
            massParticle.TotalForce = massParticle.Mass * _gravitationalAcceleration;
        }
    }

    private void UpdateSprings()
    {
        foreach (var spring in Springs)
        {
            spring.Update();
        }
    }

    private void UpdateMassParticles(float timeStep)
    {
        foreach (var massParticle in MassParticles)
        {
            massParticle.Update(timeStep);
        }
    }
}