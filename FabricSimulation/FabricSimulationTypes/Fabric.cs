using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beryllium.Physics;

public class Fabric
{
    private readonly Vector3 _gravitationalAcceleration = new (0, -9.8f, 0);

    public List<MassParticle> MassParticles { get; } = [];
    public List<FabricThread> FabricThreads { get; set; } = [];

    public void Update(float timeStep)
    {
        AddGravityForce();

        UpdateMassParticles(timeStep);

        UpdateFabricThreads();

        UpdateAirResistance();

        UpdateAcceleration();
    }

    private void AddGravityForce()
    {
        Parallel.ForEach(MassParticles, massParticle =>
        {
            massParticle.TotalForce = massParticle.Mass * _gravitationalAcceleration;
        });
    }

    private void UpdateMassParticles(float timeStep)
    {
        Parallel.ForEach(MassParticles, massParticle =>
        {
            massParticle.Update(timeStep);
        });
    }

    private void UpdateFabricThreads()
    {
        for (var i = 0; i < 2; i++)
        {
            Parallel.ForEach(FabricThreads, fabricThread =>
            {
                fabricThread.Update();
            });

            /*foreach (var fabricThread in FabricThreads)
            {
                fabricThread.Update();
            }*/
        }
    }

    private void UpdateAirResistance()
    {
        Parallel.ForEach(MassParticles, Air.Resist);
    }

    private void UpdateAcceleration()
    {
        Parallel.ForEach(MassParticles, massParticle =>
        {
            massParticle.UpdateAcceleration();
        });
    }
}