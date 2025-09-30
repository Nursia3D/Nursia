uniform float4x4 cLightMatrices[4];

uniform float4 cShadowDepthFade;
uniform float4 cShadowSplits;
uniform float2 cShadowMapInvSize;
uniform float3 cShadowParams;

#define cShadowBase cShadowParams.x
#define cShadowIntensity cShadowParams.y
#define cShadowBias cShadowParams.z

DECLARE_TEXTURE2D_LINEAR_CLAMP(ShadowMap);

#ifdef VSMSHADOW
	uniform float2 cVSMShadowParams;
#endif

void GetShadowPos(float4 projWorldPos, float3 normal, out float4 shadowPos[NUMCASCADES])
{
	// Shadow projection: transform from world space to shadow space
	#ifdef NORMALOFFSET
		#ifdef DIRLIGHT
			float cosAngle = saturate(1.0 - dot(normal, cLightDir));
		#else
			float cosAngle = saturate(1.0 - dot(normal, normalize(cLightPos.xyz - projWorldPos.xyz)));
		#endif

		#if defined(DIRLIGHT)
			shadowPos[0] = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScale.x * normal, 1.0), cLightMatrices[0]);
			shadowPos[1] = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScale.y * normal, 1.0), cLightMatrices[1]);
			shadowPos[2] = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScale.z * normal, 1.0), cLightMatrices[2]);
			shadowPos[3] = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScale.w * normal, 1.0), cLightMatrices[3]);
		#elif defined(SPOTLIGHT)
			shadowPos[0] = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScale.x * normal, 1.0), cLightMatrices[1]);
		#else
			shadowPos[0] = float4(projWorldPos.xyz + cosAngle * cNormalOffsetScale.x * normal - cLightPos.xyz, 0.0);
		#endif
	#else
		#if defined(DIRLIGHT)
			shadowPos[0] = mul(projWorldPos, cLightMatrices[0]);
			shadowPos[1] = mul(projWorldPos, cLightMatrices[1]);
			shadowPos[2] = mul(projWorldPos, cLightMatrices[2]);
			shadowPos[3] = mul(projWorldPos, cLightMatrices[3]);
		#elif defined(SPOTLIGHT)
			shadowPos[0] = mul(projWorldPos, cLightMatrices[1]);
		#else
			shadowPos[0] = float4(projWorldPos.xyz - cLightPos.xyz, 0.0);
		#endif
	#endif
}

#ifdef VSMSHADOW

float ReduceLightBleeding(float min, float p_max)  
{  
	return clamp((p_max - min) / (1.0 - min), 0.0, 1.0);  
}

float Chebyshev(float2 Moments, float depth)  
{  
	//One-tailed inequality valid if depth > Moments.x  
	float p = float(depth <= Moments.x);  
	//Compute variance.
	float Variance = Moments.y - (Moments.x * Moments.x); 

	float minVariance = cVSMShadowParams.x;
	Variance = max(Variance, minVariance);  
	//Compute probabilistic upper bound.  
	float d = depth - Moments.x;  
	float p_max = Variance / (Variance + d*d); 
	// Prevent light bleeding
	p_max = ReduceLightBleeding(cVSMShadowParams.y, p_max);

	return max(p, p_max);
}

#endif

float GetShadow(float4 shadowPos)
{
	#if defined(SIMPLESHADOW)
		float currentDepth = shadowPos.z / shadowPos.w - cShadowBias;
		float closestDepth = SampleShadow(ShadowMap, shadowPos).r;

		#ifndef POINTLIGHT
			return cShadowBase + cShadowIntensity * (currentDepth > closestDepth);
		#else
			// TODO: Eventually update point light shadow code
			return cShadowParams.y + cShadowParams.x * (inLight > shadowPos.z);
		#endif
	#elif defined(PCFSHADOW)
		float currentDepth = shadowPos.z / shadowPos.w - cShadowBias;

		// Take four samples and average them
		// Note: in case of sampling a point light cube shadow, we optimize out the w divide as it has already been performed
		#if !defined(POINTLIGHT)
			float2 offsets = cShadowMapInvSize * shadowPos.w;
		#else
			float2 offsets = cShadowMapInvSize;
		#endif
		float4 shadowPos2 = float4(shadowPos.x + offsets.x, shadowPos.yzw);
		float4 shadowPos3 = float4(shadowPos.x, shadowPos.y + offsets.y, shadowPos.zw);
		float4 shadowPos4 = float4(shadowPos.xy + offsets.xy, shadowPos.zw);

		float4 closestDepth = float4(
			SampleShadow(ShadowMap, shadowPos).r,
			SampleShadow(ShadowMap, shadowPos2).r,
			SampleShadow(ShadowMap, shadowPos3).r,
			SampleShadow(ShadowMap, shadowPos4).r
		);
		#ifndef POINTLIGHT
			return cShadowBase + dot(currentDepth > closestDepth, cShadowIntensity) / 4.0;
		#else
			// TODO: Eventually update point light shadow code
			return cShadowParams.y + dot(inLight > shadowPos.z, cShadowParams.x);
		#endif

	#elif defined(VSMSHADOW)
		// TODO: Eventually update VSM shadow code
		float2 samples = Sample2D(ShadowMap, shadowPos.xy / shadowPos.w).rg;
		return cShadowParams.y + cShadowParams.x * Chebyshev(samples, shadowPos.z/shadowPos.w);
	#endif
}

#ifdef POINTLIGHT

float GetPointShadow(float3 lightVec)
{
	float3 axis = SampleCube(FaceSelectCubeMap, lightVec).rgb;
	float depth = abs(dot(lightVec, axis));

	// Expand the maximum component of the light vector to get full 0.0 - 1.0 UV range from the cube map,
	// and to avoid sampling across faces. Some GPU's filter across faces, while others do not, and in this
	// case filtering across faces is wrong
	const float factor = 1.0 / 256.0;
	lightVec += factor * axis * lightVec;

	// Read the 2D UV coordinates, adjust according to shadow map size and add face offset
	float4 indirectPos = SampleCube(IndirectionCubeMap, lightVec);
	indirectPos.xy *= cShadowCubeAdjust.xy;
	indirectPos.xy += float2(cShadowCubeAdjust.z + indirectPos.z * 0.5, cShadowCubeAdjust.w + indirectPos.w);

	float4 shadowPos = float4(indirectPos.xy, cShadowDepthFade.x + cShadowDepthFade.y / depth, 1.0);
	return GetShadow(shadowPos);
}

#endif

#ifdef DIRLIGHT

float GetDirShadowFade(float shadow, float depth)
{
	return saturate(shadow - saturate((depth - cShadowDepthFade.z) * cShadowDepthFade.w));
}

float GetDirShadow(const float4 iShadowPos[NUMCASCADES], float depth)
{
	float4 shadowPos;

	if (depth < cShadowSplits.x)
		shadowPos = iShadowPos[0];
	else if (depth < cShadowSplits.y)
		shadowPos = iShadowPos[1];
	else if (depth < cShadowSplits.z)
		shadowPos = iShadowPos[2];
	else
		shadowPos = iShadowPos[3];

	return GetDirShadowFade(GetShadow(shadowPos), depth);
}

/*int GetShadowSplit(float depth)
{
	int result;
	if (depth < cShadowSplits.x)
	{
		result = 0;
	}
	else if (depth < cShadowSplits.y)
	{
		result = 1;
	}
	else if (depth < cShadowSplits.z)
	{
		result = 2;
	}
	else 
	{
		result = 3;
	}

	return result;
}*/

float GetDirShadowDeferred(float4 projWorldPos, float3 normal, float depth)
{
	float4 shadowPos;

	#ifdef NORMALOFFSET
		float cosAngle = saturate(1.0 - dot(normal, cLightDir));
		if (depth < cShadowSplits.x)
			shadowPos = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScalePS.x * normal, 1.0), cLightMatrices[0]);
		else if (depth < cShadowSplits.y)
			shadowPos = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScalePS.y * normal, 1.0), cLightMatrices[1]);
		else if (depth < cShadowSplits.z)
			shadowPos = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScalePS.z * normal, 1.0), cLightMatrices[2]);
		else
			shadowPos = mul(float4(projWorldPos.xyz + cosAngle * cNormalOffsetScalePS.w * normal, 1.0), cLightMatrices[3]);
	#else
		if (depth < cShadowSplits.x)
			shadowPos = mul(projWorldPos, cLightMatrices[0]);
		else if (depth < cShadowSplits.y)
			shadowPos = mul(projWorldPos, cLightMatrices[1]);
		else if (depth < cShadowSplits.z)
			shadowPos = mul(projWorldPos, cLightMatrices[2]);
		else
			shadowPos = mul(projWorldPos, cLightMatrices[3]);
	#endif

	return GetDirShadowFade(GetShadow(shadowPos), depth);
}

#endif

float GetShadow(float4 iShadowPos[NUMCASCADES], float depth)
{
	#if defined(DIRLIGHT)
		return GetDirShadow(iShadowPos, depth);
	#elif defined(SPOTLIGHT)
		return GetShadow(iShadowPos[0]);
	#else
		return GetPointShadow(iShadowPos[0].xyz);
	#endif
}

float GetShadowDeferred(float4 projWorldPos, float3 normal, float depth)
{
	#ifdef DIRLIGHT
		return GetDirShadowDeferred(projWorldPos, normal, depth);
	#else
		#ifdef NORMALOFFSET
			float cosAngle = saturate(1.0 - dot(normal, normalize(cLightPos.xyz - projWorldPos.xyz)));
			projWorldPos.xyz += cosAngle * cNormalOffsetScalePS.x * normal;
		#endif

		#ifdef SPOTLIGHT
			float4 shadowPos = mul(projWorldPos, cLightMatrices[1]);
			return GetShadow(shadowPos);
		#else
			float3 shadowPos = projWorldPos.xyz - cLightPos.xyz;
			return GetPointShadow(shadowPos);
		#endif
	#endif
}