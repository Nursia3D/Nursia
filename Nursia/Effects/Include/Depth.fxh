uniform float4 cDepthMode;

float GetDepth(float4 clipPos)
{
    return dot(clipPos.zw, cDepthMode.zw);
}
