uniform float3 cAmbientStartColor;
uniform float3 cAmbientEndColor;
uniform float4x3 cZone;

float GetZonePos(float3 worldPos)
{
    return saturate(mul(float4(worldPos, 1.0), cZone).z);
}

float3 GetAmbient(float zonePos)
{
    return cAmbientStartColor + zonePos * cAmbientEndColor;
}
