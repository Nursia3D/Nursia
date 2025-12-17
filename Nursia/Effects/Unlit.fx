#include "Include/Macros.fxh"
#include "Include/Transform.fxh"
#include "Include/Sampling.fxh"
#include "Include/Depth.fxh"
#include "Include/Fog.fxh"

uniform float4 cMatDiffColor;

#ifdef DIFFMAP
	DECLARE_TEXTURE2D_LINEAR_WRAP(DiffMap);
#endif

struct VSInput
{
	float4 Pos: POSITION;
	#ifdef DIFFMAP
		float2 TexCoord: TEXCOORD0;
	#endif
	#ifdef VERTEXCOLOR
		float4 Color: COLOR0;
	#endif
	#ifdef SKINNED
		float4 BlendWeights: BLENDWEIGHT;
		int4 BlendIndices: BLENDINDICES;
	#endif
	#ifdef INSTANCED
		float4x3 ModelInstance: BLENDWEIGHT;
	#endif
	#if defined(TRAILBONE)
		float3 Normal: NORMAL;
	#endif
	#if defined(TRAILFACECAM) || defined(TRAILBONE)
		float4 Tangent: TANGENT;
	#endif
};

struct VSOutput
{
	float4 Pos : OUTPOSITION;
	float2 TexCoord : TEXCOORD0;
	float4 WorldPos : TEXCOORD2;
	#ifdef VERTEXCOLOR
		float4 Color : COLOR0;
	#endif
	#if defined(SM4) && defined(CLIPPLANE)
		float Clip : SV_CLIPDISTANCE0;
	#endif
};

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;

	CalculateWorldPos();

	output.Pos = GetClipPos(worldPos);
	output.WorldPos = float4(worldPos, GetDepth(output.Pos));

	#ifdef DIFFMAP
		output.TexCoord = GetTexCoord(input.TexCoord);
	#endif

	#if defined(SM4) && defined(CLIPPLANE)
		output.Clip = dot(output.Pos, cClipPlane);
	#endif

	#ifdef VERTEXCOLOR
		output.Color = input.Color;
	#endif

	return output;
}

float4 PS(VSOutput input): OUTCOLOR0
{
	// Get material diffuse albedo
	#ifdef DIFFMAP
		float4 diffColor = cMatDiffColor * Sample2D(DiffMap, input.TexCoord);
		#ifdef ALPHAMASK
			if (diffColor.a < 0.5)
				discard;
		#endif
	#else
		float4 diffColor = cMatDiffColor;
	#endif

	#ifdef VERTEXCOLOR
		diffColor *= input.Color;
	#endif

	// Get fog factor
	#ifdef HEIGHTFOG
		float fogFactor = GetHeightFogFactor(input.WorldPos.w, input.WorldPos.y);
	#else
		float fogFactor = GetFogFactor(input.WorldPos.w);
	#endif

	return float4(GetFog(diffColor.rgb, fogFactor), diffColor.a);
}

TECHNIQUE(Default, VS, PS);