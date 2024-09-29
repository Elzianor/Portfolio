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
        //AddGravity();

        UpdateMassParticles(timeStep);

        UpdateFabricThreads();

        //UpdateAcceleration();
    }

    private void AddGravity()
    {
        foreach (var massParticle in MassParticles)
        {
            massParticle.TotalForce = massParticle.Mass * _gravitationalAcceleration;
        }
    }

    private void UpdateAcceleration()
    {
        foreach (var massParticle in MassParticles)
        {
            massParticle.UpdateAcceleration();
        }
    }

    private void UpdateFabricThreads()
    {
        for (var i = 0; i < 2; i++)
        {
            /*Parallel.ForEach(FabricThreads, fabricThread =>
            {
                fabricThread.Update();
            });*/

            foreach (var fabricThread in FabricThreads)
            {
                fabricThread.Update();
            }
        }
    }

    private void UpdateMassParticles(float timeStep)
    {
        Parallel.ForEach(MassParticles, massParticle =>
        {
            massParticle.Update(timeStep);
        });
    }
}