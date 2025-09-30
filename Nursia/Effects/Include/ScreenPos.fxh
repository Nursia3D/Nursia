uniform float4 cGBufferOffsets;

float4 GetScreenPos(float4 clipPos)
{
    return float4(
        clipPos.x * cGBufferOffsets.z + cGBufferOffsets.x * clipPos.w,
        -clipPos.y * cGBufferOffsets.w + cGBufferOffsets.y * clipPos.w,
        0.0,
        clipPos.w);
}

float2 GetScreenPosPreDiv(float4 clipPos)
{
    return float2(
        clipPos.x / clipPos.w * cGBufferOffsets.z + cGBufferOffsets.x,
        -clipPos.y / clipPos.w * cGBufferOffsets.w + cGBufferOffsets.y);
}

float2 GetQuadTexCoord(float4 clipPos)
{
    return float2(
        clipPos.x / clipPos.w * 0.5 + 0.5,
        -clipPos.y / clipPos.w * 0.5 + 0.5);
}