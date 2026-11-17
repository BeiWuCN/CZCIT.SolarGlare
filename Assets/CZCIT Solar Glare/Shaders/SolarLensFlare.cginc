#ifndef CUIT_SOLAR_FLARE_INCLUDED
#define CUIT_SOLAR_FLARE_INCLUDED
float3 SolarLensFlare(float3 ray, float3 sun)
{
    float3 source = mul((float3x3)UNITY_MATRIX_V, sun);
    float3 view = mul((float3x3)UNITY_MATRIX_V, ray);
    if (source.z > -.25 || view.z > -.25)
        return 0;
    float2 origin = source.xy / -source.z;
    float2 pixel = view.xy / -view.z;
    float edge = length(origin);
    float visibility = (1 - smoothstep(.95, 1.45, edge)) * smoothstep(.015, .2, edge);
    float2 axis = origin / max(edge,.001);
    float2 delta = pixel - origin;
    float along = dot(delta, axis);
    float across = dot(delta, float2(-axis.y,axis.x));
    // A soft, short smear follows the lens rather than the ghost axis.
    float2 smear = float2((delta.x + .025) / .13, delta.y / .018);
    float3 flare = exp2(-dot(smear, smear)*3.0) * float3(.072,.048,.038);
    [unroll] for (int i = 0; i < 4; i++)
    {
        float distanceAlong = (i + 1) * .30 * edge;
        float2 offset = float2((along + distanceAlong) / (.012 + i*.009), across / (.006 + i*.009));
        float3 tint = lerp(float3(.65,.34,.25),float3(.21,.18,.44),i/3.0);
        flare += tint * exp2(-dot(offset,offset)*2.5) * (.16/(1+i*.25));
    }
    return flare * visibility;
}
#endif
