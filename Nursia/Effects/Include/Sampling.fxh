uniform float4 cUOffset;
uniform float4 cVOffset;

float2 GetTexCoord(float2 iTexCoord)
{
    return float2(dot(iTexCoord, cUOffset.xy) + cUOffset.w, dot(iTexCoord, cVOffset.xy) + cVOffset.w);
};