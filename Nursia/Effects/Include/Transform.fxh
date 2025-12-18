uniform float4x3 cModel;
uniform float4x4 cViewProj;

#ifdef SKINNED
uniform float4x3 cSkinMatrices[MAXBONES];

float4x3 GetSkinMatrix(float4 blendWeights, int4 blendIndices)
{
	return cSkinMatrices[blendIndices.x] * blendWeights.x +
		cSkinMatrices[blendIndices.y] * blendWeights.y +
		cSkinMatrices[blendIndices.z] * blendWeights.z +
		cSkinMatrices[blendIndices.w] * blendWeights.w;
}
#endif

#if defined(INSTANCED)
	#define CALCULATE_WORLD_POS(Name) \
		float3 Name = mul(input.Pos, input.ModelInstance).xyz; \
		Name = mul(float4(Name, 1.0), cModel);

	#define CALCULATE_WORLD_NORMAL(Name) \
		float3 Name = mul(input.Normal, (float3x3)input.ModelInstance); \
		Name = normalize(mul(Name, (float3x3)cModel));

	#define GET_WORLD_TANGENT float4(normalize(mul(mul(input.Tangent.xyz, (float3x3)input.ModelInstance), (float3x3)cModel)), input.Tangent.w)
#elif defined(SKINNED)
	#define CALCULATE_WORLD_POS(Name) \
		float4x3 skinMatrix = GetSkinMatrix(input.BlendWeights, input.BlendIndices); \
		float3 Name = mul(input.Pos, skinMatrix); \
		Name = mul(float4(Name, 1.0), cModel);

	#define CALCULATE_WORLD_NORMAL(Name) \
		float3 Name = mul(input.Normal, (float3x3)skinMatrix); \
		Name = normalize(mul(Name, (float3x3)cModel));

	#define GET_WORLD_TANGENT float4(normalize(mul(mul(input.Tangent.xyz, (float3x3)skinMatrix), (float3x3)cModel)), input.Tangent.w)
#else
	#define CALCULATE_WORLD_POS(Name) \
		float3 Name = mul(input.Pos, cModel);

	#define CALCULATE_WORLD_NORMAL(Name) \
		float3 Name = normalize(mul(input.Normal, (float3x3)cModel));

	#define GET_WORLD_TANGENT float4(normalize(mul(input.Tangent.xyz, (float3x3)cModel)), input.Tangent.w)
#endif

float4 GetClipPos(float3 Name)
{
	return mul(float4(Name, 1.0), cViewProj);
}

float3 DecodeNormal(float4 normalInput)
{
#ifdef PACKEDNORMAL
	float3 normal;
	normal.xy = normalInput.ag * 2.0 - 1.0;
	normal.z = sqrt(max(1.0 - dot(normal.xy, normal.xy), 0.0));
	return normal;
#else
	return normalInput.rgb * 2.0 - 1.0;
#endif
}
