sampler uImage0 : register(s0);
float3 color;
float opacity;

float4 DeliciousStrawberry(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
        float alpha = tex2D(uImage0, coords).a * opacity;
        return float4(color.rgb * alpha, alpha);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 DeliciousStrawberry();
	}
}
