#include "Include/Macros.fxh"
#include "Include/Transform.fxh"
#include "Include/Sampling.fxh"
#include "Include/Depth.fxh"
#include "Include/BlinnPhongLighting.fxh"

#ifdef SHADOW
	#include "Include/Shadows.fxh"
#endif

#include "Include/Fog.fxh"

uniform float4 cMatDiffColor;
uniform float4 cMatSpecColor;
uniform float3 cMatEmissiveColor;
uniform float cMatSpecularPower;

DECLARE_TEXTURE2D_LINEAR_WRAP(DiffMap);
DECLARE_TEXTURE2D_LINEAR_WRAP(SpecMap);

#if defined(NORMALMAP)
	DECLARE_TEXTURE2D_LINEAR_WRAP(NormalMap);
#endif

#ifdef CLIPPLANE

float4 cClipPlane;

#endif

struct VSInput
{
	float4 Pos : POSITION;
	#if !defined(TRAILFACECAM)
		float3 Normal : NORMAL;
	#endif
	float2 TexCoord : TEXCOORD0;
	#ifdef VERTEXCOLOR
		float4 Color : COLOR0;
	#endif
	#if defined(LIGHTMAP) || defined(AO)
		float2 TexCoord2 : TEXCOORD1;
	#endif
	#if (defined(NORMALMAP) || defined(TRAILFACECAM) || defined(TRAILBONE))
		float4 Tangent : TANGENT;
	#endif
	#ifdef SKINNED
		float4 BlendWeights : BLENDWEIGHT;
		int4 BlendIndices : BLENDINDICES;
	#endif
	#ifdef INSTANCED
		float4x3 ModelInstance : TEXCOORD2;
	#endif
};

struct VSOutput
{
	float4 Pos : OUTPOSITION;
	#ifndef NORMALMAP
		float2 TexCoord : TEXCOORD0;
	#else
		float4 TexCoord : TEXCOORD0;
		float4 Tangent : TEXCOORD1;
	#endif
	float3 Normal : TEXCOORD2;
	float4 WorldPos : TEXCOORD3;
	#ifdef SHADOW
		float4 ShadowPos[NUMCASCADES] : TEXCOORD4;
	#endif
	#ifdef SPOTLIGHT
		float4 SpotPos : TEXCOORD5;
	#endif
	#if defined(POINTLIGHT) && defined(CUBEMASK)
		float3 CubeMaskVec : TEXCOORD5;
	#endif
	#ifdef VERTEXCOLOR
		float4 Color : COLOR0;
	#endif
	#if defined(CLIPPLANE)
		float Clip : TEXCOORD8;
	#endif
};

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;
	float4x3 modelMatrix = iModelMatrix;
	float3 worldPos = GetWorldPos(modelMatrix);

	output.Pos = GetClipPos(worldPos);
	output.Normal = GetWorldNormal(modelMatrix);

	output.WorldPos = float4(worldPos, GetDepth(output.Pos));

	#if defined(CLIPPLANE)
		output.Clip = dot(float4(worldPos, 1), cClipPlane);
	#endif

	#ifdef VERTEXCOLOR
		output.Color = input.Color;
	#endif

	#ifdef NORMALMAP
		float4 tangent = GetWorldTangent(modelMatrix);
		float3 bitangent = cross(tangent.xyz, output.Normal) * tangent.w;
		output.TexCoord = float4(GetTexCoord(input.TexCoord), bitangent.xy);
		output.Tangent = float4(tangent.xyz, bitangent.z);
	#else
		output.TexCoord = GetTexCoord(input.TexCoord);
	#endif

	// Per-pixel forward lighting
	float4 projWorldPos = float4(worldPos.xyz, 1.0);

	#ifdef SHADOW
		// Shadow projection: transform from world space to shadow space
		GetShadowPos(projWorldPos, output.Normal, output.ShadowPos);
	#endif

	#ifdef SPOTLIGHT
		// Spotlight projection: transform from world space to projector texture coordinates
		output.SpotPos = mul(projWorldPos, cSpotLightMatrix);
	#endif

	#if defined(POINTLIGHT) && defined(CUBEMASK)
		output.CubeMaskVec = mul(worldPos - cLightPos.xyz, (float3x3)cLightMatrices[0]);
	#endif

	return output;
}

float4 PS(VSOutput input): OUTCOLOR0
{
#ifdef CLIPPLANE
	clip(input.Clip); 
#endif

	// Get material diffuse albedo
	float4 diffInput = Sample2D(DiffMap, input.TexCoord.xy);
	#ifdef ALPHAMASK
		if (diffInput.a < 0.5)
			discard;
	#endif
	float4 diffColor = cMatDiffColor * diffInput;

	#ifdef VERTEXCOLOR
		diffColor *= input.Color;
	#endif

	// Get material specular albedo
	float3 specColor = cMatSpecColor.rgb * Sample2D(SpecMap, input.TexCoord.xy).rgb;

	// Get normal
	#ifdef NORMALMAP
		float3x3 tbn = float3x3(input.Tangent.xyz, float3(input.TexCoord.zw, input.Tangent.w), input.Normal);
		float3 normal = normalize(mul(DecodeNormal(Sample2D(NormalMap, input.TexCoord.xy)), tbn));
	#else
		float3 normal = normalize(input.Normal);
	#endif

	// Get fog factor
	#ifdef HEIGHTFOG
		float fogFactor = GetHeightFogFactor(input.WorldPos.w, input.WorldPos.y);
	#else
		float fogFactor = GetFogFactor(input.WorldPos.w);
	#endif

	// Per-pixel forward lighting
	float3 lightDir;
	float3 lightColor;
	float3 finalColor;

	float diff = GetDiffuse(normal, input.WorldPos.xyz, lightDir);

	#ifdef SHADOW
		float shadow = GetShadow(input.ShadowPos, input.WorldPos.w);
		diff *= (1.0 - shadow);
	#endif

	#if defined(SPOTLIGHT)
		lightColor = input.SpotPos.w > 0.0 ? Sample2DProj(LightSpotMap, input.SpotPos).rgb * cLightColor.rgb : 0.0;
	#elif defined(POINTLIGHT) && defined(CUBEMASK)
		lightColor = SampleCube(LightCubeMap, input.CubeMaskVec).rgb * cLightColor.rgb;
	#else
		lightColor = cLightColor.rgb;
	#endif

	float spec = GetSpecular(normal, cCameraPos - input.WorldPos.xyz, lightDir, cMatSpecularPower);
	finalColor = diff * lightColor * (diffColor.rgb + spec * specColor * cLightColor.a);

	finalColor += GetAmbientColor() * diffColor.rgb;
	finalColor += cMatEmissiveColor;
	float4 oColor = float4(GetFog(finalColor, fogFactor), 1.0);

/*	#if defined(DIRLIGHT) && defined(SHADOW)
		int res = GetShadowSplit(input.WorldPos.w);
		
		if (res == 0)
		{
			oColor.r += 0.2;
		} else if (res == 1)
		{
			oColor.g += 0.2;
		} else if (res == 2)
		{
			oColor.b += 0.2;
		} else
		{
			oColor.r += 0.2;
			oColor.g += 0.2;
			oColor.b += 0.2;
		}
	#endif*/
	
	return oColor;
}

TECHNIQUE(Default, VS, PS);