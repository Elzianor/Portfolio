using Microsoft.Xna.Framework;

namespace Beryllium.Physics;

public class Spring
{
    public MassParticle Mass1 { get; }
    public MassParticle Mass2 { get; }
    public float RestLength { get; set; }
    public float Stiffness { get; set; }
    public float Damping { get; set; }
    public bool Disabled { get; set; }

    public Spring(MassParticle mass1, MassParticle mass2)
    {
        Mass1 = mass1;
        Mass2 = mass2;
    }

    public void Update()
    {
        if (Disabled) return;
        if (Mass1 == null || Mass2 == null) return;

        var length = (Mass2.Position - Mass1.Position).Length();

        var direction = Mass2.Position - Mass1.Position;
        direction.Normalize();

        var springForce = Stiffness * (length - RestLength) * direction;

        var lengthChangeSpeed = Vector3.Dot(Mass2.Velocity - Mass1.Velocity, direction);

        var dampingForce = Damping * lengthChangeSpeed * direction;

        var totalForce = springForce + dampingForce;

        Mass1.TotalForce += totalForce;
        Mass2.TotalForce -= totalForce;
    }
}