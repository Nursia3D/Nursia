#include "Include/Macros.fxh"
#include "Include/Transform.fxh"
#include "Include/ScreenPos.fxh"
#include "Include/Fog.fxh"
#include "Include/Depth.fxh"

uniform float2 cNoiseSpeed;
uniform float cNoiseTiling;
uniform float cNoiseStrength;
uniform float cFresnelPower;
uniform float3 cWaterTint;
uniform float cElapsedTime;
uniform float cMurkinessStart;
uniform float cMurkinessFactor;
uniform float cNearPlane;
uniform float cFarPlane;
uniform float cEdgeFactor;

DECLARE_TEXTURE2D_LINEAR_CLAMP(DepthMap);
DECLARE_TEXTURE2D_LINEAR_CLAMP(ReflectionMap);
DECLARE_TEXTURE2D_LINEAR_CLAMP(ScreenMap);
DECLARE_TEXTURE2D_LINEAR_WRAP(NormalMap);

struct VSInput
{
	float4 Pos: POSITION;
	float3 Normal: NORMAL;
	float2 TexCoord: TEXCOORD0;
	#ifdef INSTANCED
		float4x3 ModelInstance: TEXCOORD4;
	#endif
};

struct VSOutput
{
	float4 Pos : OUTPOSITION;
	float4 ScreenPos : TEXCOORD0;
	float2 ReflectUV : TEXCOORD1;
	float2 WaterUV : TEXCOORD2;
	float3 Normal : TEXCOORD3;
	float4 EyeVec : TEXCOORD4;
	#if defined(CLIPPLANE)
		float Clip : SV_CLIPDISTANCE0;
	#endif
	float4 PosCopy: TEXCOORD5;
};

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;
	float4x3 modelMatrix = iModelMatrix;
	float3 worldPos = GetWorldPos(modelMatrix);
	output.Pos = GetClipPos(worldPos);
	output.PosCopy = output.Pos;

	output.ScreenPos = GetScreenPos(output.Pos);

	// GetQuadTexCoord() returns a float2 that is OK for quad rendering; multiply it with output W
	// coordinate to make it work with arbitrary meshes such as the water plane (perform divide in pixel shader)
	output.ReflectUV = GetQuadTexCoord(output.Pos) * output.Pos.w;
	output.WaterUV = input.TexCoord * cNoiseTiling + cElapsedTime * cNoiseSpeed;
	output.Normal = GetWorldNormal(modelMatrix);
	output.EyeVec = float4(cCameraPos - worldPos, GetDepth(output.Pos));

	#if defined(CLIPPLANE)
		output.Clip = dot(float4(worldPos, 1), cClipPlane);
	#endif

	return output;
}

float Edge(float depth)
{
	return 2.0 * cNearPlane * cFarPlane / (cFarPlane + cNearPlane - (2.0 * depth - 1.0) * (cFarPlane - cNearPlane));
}

float4 PS(VSOutput input): OUTCOLOR0
{
	#ifdef CLIPPLANE
		clip(input.Clip);
	#endif

	float2 refractUV = input.ScreenPos.xy / input.ScreenPos.w;
	float2 reflectUV = input.ReflectUV.xy / input.ScreenPos.w;

	float depth = Sample2D(DepthMap, refractUV).r;
	float floorDistance = Edge(depth);
	float waterDistance = Edge(input.PosCopy.z / input.PosCopy.w);
	float waterDepth = floorDistance - waterDistance;

	float depthBlend = exp((waterDepth - cMurkinessStart) * -(cMurkinessFactor / 10));
	depthBlend = 1.0 - clamp(depthBlend, 0.0, 1.0);

	float2 noise = (Sample2D(NormalMap, input.WaterUV).rg - 0.5) * cNoiseStrength;
	refractUV += noise;
	// Do not shift reflect UV coordinate upward, because it will reveal the clipping of geometry below water
	if (noise.y < 0.0)
		noise.y = 0.0;
	reflectUV += noise;

	float fresnel = pow(1.0 - saturate(dot(normalize(input.EyeVec.xyz), input.Normal)), cFresnelPower);

	float3 refractColor = Sample2D(ScreenMap, refractUV).rgb;
	refractColor = lerp(refractColor, float3(1, 1, 1), depthBlend) * cWaterTint;

	float3 reflectColor = Sample2D(ReflectionMap, reflectUV).rgb;
	float3 finalColor = lerp(refractColor, reflectColor, fresnel);

	float alpha = clamp(waterDepth / cEdgeFactor, 0.0f, 1.0f);

//	return float4(waterDepth, waterDepth, waterDepth, 1.0);
//	return float4(refractColor, 0.0);

	return float4(GetFog(finalColor, GetFogFactor(input.EyeVec.w)), alpha);
}

TECHNIQUE(Default, VS, PS);