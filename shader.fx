// Pixel shader combines the bloom image with the original
// scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

sampler screen : register(s0);
float2 size;
struct VertexInput
{
    float4 Color : COLOR0;
    float2 UVMapping : TEXCOORD0;

};
struct VOut
{
    float4 Color : COLOR0;
    float2 UVMapping : TEXCOORD0;
};
struct PixelInput
{
    float4 Color : COLOR0;
    float2 UVMapping : TEXCOORD0;
    //float2 Position : TEXCOORD1;
};
struct PixelOutput
{
    float4 Color : COLOR0;
    //float Depth : SV_Depth;
};


VOut VertexShaderF(VertexInput input)
{
    VOut output;
	
    output.Color = input.Color;
    output.UVMapping = input.UVMapping;
    //output.Position = float2(1, 1);
	
    return output;
}
PixelOutput PixelShaderInner(PixelInput input)
{
    PixelOutput output;
    
    float4 color = input.Color;
    float2 texCoord = input.UVMapping;
    float4 b = tex2D(screen, texCoord);
	
    if (b.a != 0)
    {

        float2 offset = float2(1 / size.x, 1 / size.y);
		
        if (
            tex2D(screen, texCoord + float2(offset[0], 0)).a == 0 ||
            tex2D(screen, texCoord - float2(offset[0], 0)).a == 0 ||
            tex2D(screen, texCoord + float2(0, offset[1])).a == 0 ||
            tex2D(screen, texCoord - float2(0, offset[1])).a == 0)
                b = color;
        else
        {
            //b = float4(0, 0, 0, 0);
        }
    }
    output.Color = b;
	
    return output;
}

PixelOutput PixelShaderF(PixelInput input)
{
    PixelOutput output;
    
    float4 color = input.Color;
    float2 texCoord = input.UVMapping;
    float4 b = tex2D(screen, texCoord);
	
    if (b.a == 0)
    {
	    
        float2 offset = float2(1 / size.x, 1 / size.y);
		
        if (
            tex2D(screen, texCoord + float2(offset[0], 0)).a != 0 ||
            tex2D(screen, texCoord - float2(offset[0], 0)).a != 0 ||
            tex2D(screen, texCoord + float2(0, offset[1])).a != 0 ||
            tex2D(screen, texCoord - float2(0, offset[1])).a != 0)
                b = color;
        else
        {
            //b = float4(0, 0, 0, 0);
        }
    }
    output.Color = b;
	
    return output;
}

technique InnerOutline
{
    pass Pass1
    {
#if SM4
        PixelShader = compile ps_4_0_level_9_1 PixelShaderInner();
#elif SM3
        PixelShader = compile ps_3_0 PixelShaderInner();
#else
        PixelShader = compile ps_2_0 PixelShaderInner();
#endif
    }
}
technique OuterOutline
{
    pass Pass1
    {
#if SM4
        PixelShader = compile ps_4_0_level_9_1 PixelShaderF();
#elif SM3
        PixelShader = compile ps_3_0 PixelShaderF();
#else
        PixelShader = compile ps_2_0 PixelShaderF();
#endif
    }
}