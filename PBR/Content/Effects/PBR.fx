#include "EffectHeaders/CommonVertexShaders.fxh"
#include "EffectHeaders/CommonPixelShaders.fxh"

// --- TECHNIQUES ---

technique Solid
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VS_PBR_Solid();
        PixelShader = compile ps_5_0 PS_PBR_Solid();
    }
}

technique Textured
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VS_PBR_Textured();
        PixelShader = compile ps_5_0 PS_PBR_Textured();
    }
}