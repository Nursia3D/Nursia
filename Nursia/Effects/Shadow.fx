#include "Include/Macros.fxh"
#include "Include/Transform.fxh"

#if defined(ALPHAMASK)
#include "Include/Sampling.fxh"

DECLARE_TEXTURE2D_POINT_CLAMP(DiffMap);
#endif

struct VSInput
{
	float4 Pos : POSITION;
	#if defined(ALPHAMASK)
		float2 TexCoord : TEXCOORD0;
	#endif
	#ifdef SKINNED
		float4 BlendWeights : BLENDWEIGHT;
		int4 BlendIndices : BLENDINDICES;
	#endif
	#ifdef INSTANCED
		float4x3 ModelInstance : TEXCOORD1;
	#endif
};

struct VSOutput
{
	#ifdef ALPHAMASK
		float2 TexCoord : TEXCOORD0;
	#endif
	float4 Pos : OUTPOSITION;
	float4 PosCopy : TEXCOORD1;
};

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;

	float4x3 modelMatrix = iModelMatrix;
	float3 worldPos = GetWorldPos(modelMatrix);

	output.Pos = GetClipPos(worldPos);

	#if ALPHAMASK
		oTexCoord = GetTexCoord(input.TexCoord);
	#endif

	output.PosCopy = output.Pos;

	return output;
}

float4 PS(VSOutput input): OUTCOLOR0
{
	#ifdef ALPHAMASK
		float alpha = Sample2D(DiffMap, input.TexCoord.xy).a;
		if (alpha < 0.5)
			discard;
	#endif

	float depth = input.PosCopy.z / input.PosCopy.w;
	#ifdef VSM
		return float4(depth, depth * depth, 1.0, 1.0);
	#else
		return float4(depth, 0, 0, 0);
	#endif
}

TECHNIQUE(Default, VS, PS);