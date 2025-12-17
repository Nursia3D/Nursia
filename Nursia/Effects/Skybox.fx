#include "Include/Macros.fxh"

uniform float4 cMatDiffColor;

// We don't use "Transform.fxh" here, since we need to pass special matrix
uniform float4x4 cCustomTransform;
DECLARE_TEXTURECUBE_LINEAR_CLAMP(DiffCubeMap);

struct VSInput
{
	float4 Pos : POSITION;
};

struct VSOutput
{
	float4 Pos : OUTPOSITION;
	float3 TexCoord : TEXCOORD0;
};

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;
	output.Pos = mul(input.Pos, cCustomTransform);

	// Set z equal to w, so after automatic division by w, z will become 1
	output.Pos.z = output.Pos.w;
	output.TexCoord = input.Pos.xyz;

	return output;
}

float4 PS(VSOutput input): OUTCOLOR0
{
	float4 sky = cMatDiffColor * SampleCube(DiffCubeMap, input.TexCoord);
	#ifdef HDRSCALE
		sky = pow(sky + clamp((cAmbientColor.a - 1.0) * 0.1, 0.0, 0.25), max(cAmbientColor.a, 1.0)) * clamp(cAmbientColor.a, 0.0, 1.0);
	#endif
	return sky;
}

TECHNIQUE(Default, VS, PS);