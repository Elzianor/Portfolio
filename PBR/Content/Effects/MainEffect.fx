#include "EffectHeaders/CommonVertexShadersEffectHeader.fxh"
#include "EffectHeaders/CommonPixelShadersEffectHeader.fxh"

// --- TECHNIQUES ---

technique Solid
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VSSolid();
        PixelShader = compile ps_5_0 PSSolid();
    }
}

technique Textured
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VSTextured();
        PixelShader = compile ps_5_0 PSTextured();
    }
}