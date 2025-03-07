#include "EffectHeaders/CommonVertexShaders.fxh"
#include "EffectHeaders/CommonPixelShaders.fxh"
#include "EffectHeaders/FabricCompute.fxh"

technique FabricComputeTechnique
{
    pass VerletPass
    {
        ComputeShader = compile cs_5_0 CS_Verlet();
    }

    pass ConstraintsPass
    {
        ComputeShader = compile cs_5_0 CS_Constraints();
    }

    pass AirCalculations
    {
        ComputeShader = compile cs_5_0 CS_AirCalculations();
    }

    pass UpdateInputBufferPass
    {
        ComputeShader = compile cs_5_0 CS_UpdateInputBuffer();
    }

    pass RenderPass
    {
        VertexShader = compile vs_5_0 VS_FabricCompute();
        PixelShader = compile ps_5_0 PS_PBR_Textured();
    }
}
