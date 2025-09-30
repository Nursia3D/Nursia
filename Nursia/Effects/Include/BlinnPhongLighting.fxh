uniform float3 cAmbientLightColor;
uniform float3 cMatAmbientColor;

uniform float4 cLightColor;

#if defined(DIRLIGHT)
	uniform float3 cLightDir;
#elif defined(POINTLIGHT)
	uniform float4 cLightPos;
	DECLARE_TEXTURE2D_LINEAR_CLAMP(LightRampMap);
#elif defined(SPOTLIGHT)
	uniform float4 cLightPos;
	uniform float4x4 cSpotLightMatrix;
	DECLARE_TEXTURE2D_LINEAR_CLAMP(LightRampMap);
	DECLARE_TEXTURE2D_LINEAR_CLAMP(LightSpotMap);
#endif

float3 GetAmbientColor()
{
	return cAmbientLightColor + cMatAmbientColor;
}

float GetDiffuse(float3 normal, float3 worldPos, out float3 lightDir)
{
	#ifdef DIRLIGHT
		lightDir = cLightDir;
		#ifdef TRANSLUCENT
			return abs(dot(normal, lightDir));
		#else
			return saturate(dot(normal, lightDir));
		#endif
	#else
		float3 lightVec = (cLightPos.xyz - worldPos) * cLightPos.w;
		float lightDist = length(lightVec);
		lightDir = lightVec / lightDist;
		#ifdef TRANSLUCENT
			return abs(dot(normal, lightDir)) * Sample2D(LightRampMap, float2(lightDist, 0.0)).r;
		#else
			return saturate(dot(normal, lightDir)) * Sample2D(LightRampMap, float2(lightDist, 0.0)).r;
		#endif
	#endif
}

float GetDiffuseVolumetric(float3 worldPos)
{
	#ifdef DIRLIGHT
		return 1.0;
	#else
		float3 lightVec = (cLightPos.xyz - worldPos) * cLightPos.w;
		float lightDist = length(lightVec);
		return Sample2D(LightRampMap, float2(lightDist, 0.0)).r;
	#endif
}

float GetSpecular(float3 normal, float3 eyeVec, float3 lightDir, float specularPower)
{
	float3 halfVec = normalize(normalize(eyeVec) + lightDir);
	return saturate(pow(dot(normal, halfVec), specularPower));
}

float GetIntensity(float3 color)
{
	return dot(color, float3(0.299, 0.587, 0.114));
}