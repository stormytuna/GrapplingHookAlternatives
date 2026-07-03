sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float intensity;
float opacity;
float brightness;
float time;
float2 textureSize;

float2 snapToGrid(float2 coords) {
    float2 pixelCoords = floor(coords * textureSize);
    float2 snappedCoords = floor(pixelCoords / 2) * 2;
    float2 snappedUV = (snappedCoords + float2(0.5, 0.5)) / textureSize;
    return snappedUV;
}

float4 Glitch(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords = snapToGrid(coords);

    float2 noiseCoords = float2(0, coords.y * time * 0.15);
    float4 noise = tex2D(uImage1, noiseCoords);
    float horizontalOffset = lerp(-10, 10, noise.x) * 0.002 * intensity;
    horizontalOffset *= step(2 / textureSize.x, abs(horizontalOffset));

    coords.x += horizontalOffset;

    float4 color = tex2D(uImage0, coords) * sampleColor;

    float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float3 green = float3(0.1, 0.95, 0.05);
    float3 greenImage = green * luminance * brightness;

    float pixelY = floor(coords.y * textureSize.y);
    float scanline = 1;
    if (pixelY % 4 == 0) {
      scanline = 0.7;
    }

    color.rgb = lerp(color.rgb, greenImage, opacity) * scanline;
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 Glitch();
    }
}
