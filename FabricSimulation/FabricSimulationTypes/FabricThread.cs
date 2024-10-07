using BMath = Beryllium.Mathematics.Mathematics;

namespace Beryllium.Physics;

public class FabricThread
{
    public MassParticle Mass1 { get; }
    public MassParticle Mass2 { get; }
    public float Length { get; set; }
    public bool Disabled { get; set; }

    public FabricThread(MassParticle mass1, MassParticle mass2)
    {
        Mass1 = mass1;
        Mass2 = mass2;
        Mass1.AddFabricThread(this);
        Mass2.AddFabricThread(this);
    }

    public void Update()
    {
        if (Disabled) return;
        if (Mass1 == null || Mass2 == null) return;
        if (Mass1.IsImmovable && Mass2.IsImmovable) return;

        var length = (Mass1.Position - Mass2.Position).Length();

        if (BMath.IsEqual(length, Length)) return;

        var distanceDelta = length - Length;

        var p1 = Mass1.Position;
        var p2 = Mass2.Position;
        var m1 = Mass1.Mass;
        var m2 = Mass2.Mass;

        var d = distanceDelta * ((p1 - p2) / length);

        var deltaPosition1 = -d * m2 / (m1 + m2);
        var deltaPosition2 = d * m1 / (m1 + m2);

        if (!Mass1.IsImmovable) Mass1.Position += deltaPosition1;
        if (!Mass2.IsImmovable) Mass2.Position += deltaPosition2;
    }
}