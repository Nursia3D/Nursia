#include "Include/Macros.fxh"
#include "Include/Transform.fxh"

struct VSInput
{
	float4 Pos : POSITION;
	#ifdef SKINNED
		float4 BlendWeights : BLENDWEIGHT;
		int4 BlendIndices : BLENDINDICES;
	#endif
	#ifdef INSTANCED
		float4x3 ModelInstance : BLENDWEIGHT;
	#endif
};

struct VSOutput
{
	float4 Pos : OUTPOSITION;
	float4 PosCopy : TEXCOORD0;
};

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;

	CALCULATE_WORLD_POS(worldPos);

	output.Pos = GetClipPos(worldPos);

	#if ALPHAMASK
		oTexCoord = GetTexCoord(iTexCoord);
	#endif

	output.PosCopy = output.Pos;

	return output;
}

float4 PS(VSOutput input): OUTCOLOR0
{
	float depth = input.PosCopy.z / input.PosCopy.w;
	return float4(depth, 0, 0, 0);
}

TECHNIQUE(Default, VS, PS);