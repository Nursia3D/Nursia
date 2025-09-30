uniform float4x3 cModel;
uniform float4x4 cView;
uniform float4x4 cViewProj;

uniform float3 cCameraPos;

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

float4 GetClipPos(float3 worldPos)
{
    return mul(float4(worldPos, 1.0), cViewProj);
}

#ifdef TRAILFACECAM
float3 GetTrailPos(float4 iPos, float3 iFront, float iScale, float4x3 modelMatrix)
{
    float3 up = normalize(cCameraPos - iPos.xyz);
    float3 left = normalize(cross(iFront, up));
    return (mul(float4((iPos.xyz + left * iScale), 1.0), modelMatrix)).xyz;
}

float3 GetTrailNormal(float4 iPos)
{
    return normalize(cCameraPos - iPos.xyz);
}
#endif

#ifdef TRAILBONE
float3 GetTrailPos(float4 iPos, float3 iParentPos, float iScale, float4x3 modelMatrix)
{
    float3 right = iParentPos - iPos.xyz;
    return (mul(float4((iPos.xyz + right * iScale), 1.0), modelMatrix)).xyz;
}

float3 GetTrailNormal(float4 iPos, float3 iParentPos, float3 iForward)
{
    float3 left = normalize(iPos.xyz - iParentPos);
    float3 up = -normalize(cross(normalize(iForward), left));
    return up;
}
#endif

#if defined(SKINNED)
    #define iModelMatrix GetSkinMatrix(input.BlendWeights, input.BlendIndices)
#elif defined(INSTANCED)
    #define iModelMatrix input.ModelInstance
#else
    #define iModelMatrix cModel
#endif

#if defined(TRAILFACECAM)
    #define GetWorldPos(modelMatrix) GetTrailPos(input.Pos, input.Tangent.xyz, input.Tangent.w, modelMatrix)
#elif defined(TRAILBONE)
    #define GetWorldPos(modelMatrix) GetTrailPos(input.Pos, input.Tangent.xyz, input.Tangent.w, modelMatrix)
#else
    #define GetWorldPos(modelMatrix) mul(input.Pos, modelMatrix)
#endif

#if defined(TRAILFACECAM)
    #define GetWorldNormal(modelMatrix) GetTrailNormal(input.Pos)
#elif defined(TRAILBONE)
    #define GetWorldNormal(modelMatrix) GetTrailNormal(input.Pos, input.Tangent.xyz, input.Normal)
#else
    #define GetWorldNormal(modelMatrix) normalize(mul(input.Normal, (float3x3)modelMatrix))
#endif

#define GetWorldTangent(modelMatrix) float4(normalize(mul(input.Tangent.xyz, (float3x3)modelMatrix)), input.Tangent.w)

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
