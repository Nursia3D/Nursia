#include "Include/Macros.fxh"
#include "Include/Transform.fxh"
#include "Include/Sampling.fxh"
#include "Include/Depth.fxh"
#include "Include/BlinnPhongLighting.fxh"

#ifdef SHADOW
	#include "Include/Shadows.fxh"
#endif

#include "Include/Fog.fxh"

#define PI 3.1415926535897932384626433832795

uniform float4 cMatDiffColor;
uniform float4 cMatSpecColor;
uniform float3 cMatEmissiveColor;
uniform float cMatSpecularPower;

uniform float2 cDetailTiling;
DECLARE_TEXTURE2D_LINEAR_WRAP(WeightMap0);
DECLARE_TEXTURE2D_LINEAR_WRAP(DetailMap1);
DECLARE_TEXTURE2D_LINEAR_WRAP(DetailMap2);
DECLARE_TEXTURE2D_LINEAR_WRAP(DetailMap3);
DECLARE_TEXTURE2D_LINEAR_WRAP(DetailMap4);

#ifdef MARKER

float3 MarkerPosition;
float MarkerRadius;

#endif

#ifdef CLIPPLANE

float4 cClipPlane;

#endif

struct VSInput
{
	float4 Pos : POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
	#ifdef SKINNED
		float4 BlendWeights : BLENDWEIGHT;
		int4 BlendIndices : BLENDINDICES;
	#endif
	#ifdef INSTANCED
		float4x3 ModelInstance : TEXCOORD1;
	#endif
	#if defined(TRAILFACECAM) || defined(TRAILBONE)
		float4 Tangent : TANGENT;
	#endif
};

struct VSOutput
{
	float4 Pos : OUTPOSITION;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float4 WorldPos : TEXCOORD2;
	float2 DetailTexCoord : TEXCOORD3;
	#ifdef SHADOW
		float4 ShadowPos[NUMCASCADES] : TEXCOORD4;
	#endif
	#ifdef SPOTLIGHT
		float4 SpotPos : TEXCOORD5;
	#endif
	#if defined(POINTLIGHT) && defined(CUBEMASK)
		float3 CubeMaskVec : TEXCOORD5;
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
	output.TexCoord = GetTexCoord(input.TexCoord);
	output.DetailTexCoord = cDetailTiling * output.TexCoord;

	#if defined(CLIPPLANE)
		output.Clip = dot(float4(worldPos, 1), cClipPlane);
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
	float4 weights = Sample2D(WeightMap0, input.TexCoord);
	float sumWeights = weights.r + weights.g + weights.b + weights.a;
	weights /= sumWeights;
	float4 diffColor = cMatDiffColor * (
		weights.r * Sample2D(DetailMap1, input.DetailTexCoord) +
		weights.g * Sample2D(DetailMap2, input.DetailTexCoord) +
		weights.b * Sample2D(DetailMap3, input.DetailTexCoord) +
		weights.a * Sample2D(DetailMap4, input.DetailTexCoord)
	);

	// Get material specular albedo
	float3 specColor = cMatSpecColor.rgb;

	// Get normal
	float3 normal = normalize(input.Normal);

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

	#ifdef MARKER
		float dist = distance(MarkerPosition, input.WorldPos.xyz);
		if(dist <= MarkerRadius)
		{
			float gradient = (MarkerRadius - dist + 0.01) / MarkerRadius;
			gradient = 1.0 - clamp(cos(gradient * PI), 0.0, 1.0);
			oColor += float4(0.4 * gradient, 0.4 * gradient, 0.4 * gradient, 0.4 * gradient);
		}
	#endif

/*	#if defined(DIRLIGHT) && defined(SHADOW)
	int res = GetShadowSplit(input.WorldPos.w);
	
	if (res == 0)
	{
		oColor = float4(1, 0, 0, 1);
	} else if (res == 1)
	{
		oColor = float4(0, 1, 0, 1);
	} else if (res == 2)
	{
		oColor = float4(0, 0, 1, 1);
	} else
	{
		oColor = float4(1, 0, 1, 1);
	}
	#endif*/
	
	return oColor;
}

TECHNIQUE(Default, VS, PS);