#include "Macros.fxh"

float4x4 cViewProj              : ViewProjection;
float4x3 cModel                 : World;
uniform float4 cMatDiffColor;

DECLARE_TEXTURE2D_LINEAR_CLAMP(ReflectionMap);

struct VertexInput
{
    float3 pos: POSITION;
};

struct VertexOutput
{
    float4 pos: POSITION;
    float4 posCopy: TEXCOORD0;
    float2 reflectUV: TEXCOORD1;
};

float2 GetQuadTexCoord(float4 clipPos)
{
    return float2(
        clipPos.x / clipPos.w * 0.5 + 0.5,
        -clipPos.y / clipPos.w * 0.5 + 0.5);
}

VertexOutput VS(VertexInput input)
{
	VertexOutput output = (VertexOutput)0;

	float3 worldPos = mul(float4(input.pos, 1.0), cModel);
	output.pos = mul(float4(worldPos, 1.0), cViewProj);
	output.posCopy = output.pos;
	output.reflectUV = GetQuadTexCoord(output.pos) * output.pos.w;

	return output;
}

float4 PS(VertexOutput input) : COLOR
{
	float2 reflectUV = input.reflectUV.xy / input.posCopy.w;
	
	float4 diffColor = cMatDiffColor * Sample2D(ReflectionMap, reflectUV);

	return float4(diffColor.rgb, 1.0);
}

TECHNIQUE(Default, VS, PS);