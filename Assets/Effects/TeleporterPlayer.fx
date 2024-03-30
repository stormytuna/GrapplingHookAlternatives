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

float4 TeleporterPlayer(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	// sample noise for vertical and horizontal offsets. we are using uOpacity as an intensity for now
	float2 noiseCoords = coords + (uTime * 0.15, uTime * 0.15) * uOpacity;
	float4 noise1 = tex2D(uImage1, noiseCoords);
	float4 noise2 = tex2D(uImage2, noiseCoords);
	float verticalOffset = lerp(-1, 1, noise1.x) * 0.001;
	float horizontalOffset = lerp(-1, 1, noise2.x) * 0.001;

	// sample player texture
	float2 sampleCoords = coords + (horizontalOffset, verticalOffset);
	float4 originalColor = tex2D(uImage0, sampleCoords);
	float4 color = tex2D(uImage0, sampleCoords);

	// apply green tint and scanlines
	float luminosity = (color.r + color.g + color.b) / 3;
	color.rgb = luminosity * uColor * 15;
	if ((coords.y + uTime * 0.01) % 0.002 < 0.001) 
	{
		color.rgb = luminosity * (uColor + (0.1, 0.1, 0.1)) * 15;
	}

	// allows us to fade the effect in and out during our teleport
	color = lerp(originalColor, color, uOpacity);
	return color * sampleColor;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 TeleporterPlayer();
	}
}
