sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float4 uShaderSpecificData;

float4 DeliciousStrawberry(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
        float alpha = tex2D(uImage0, coords).a * uOpacity;
        return float4(uColor.rgb * alpha, alpha);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 DeliciousStrawberry();
	}
}
