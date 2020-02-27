sampler s0;
texture2D lightMask;
float darknessLevel;
float lightLevel;

sampler lightSampler : register(s1) = sampler_state {
	Texture = <lightMask>;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VSOutput
{
	float4 position		: SV_Position;
	float4 color		: COLOR0;
	float2 texCoord		: TEXCOORD0;
};

float4 AddLights(VSOutput input) : SV_TARGET0
{
	float4 color = tex2D(s0, input.texCoord);
	float4 lightColor = tex2D(lightSampler, input.texCoord);

    if (lightColor.r == 245.0f/255.0f && lightColor.g == 245.0/255.0 && lightColor.b == 245.0f/255.0f)
    {
        color.rgb *= darknessLevel;
        return color;
    }
    else
    {
		color.rgb += (lightColor / 10.0f);
    }

    return color;
}


technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_3 AddLights();
	}
}